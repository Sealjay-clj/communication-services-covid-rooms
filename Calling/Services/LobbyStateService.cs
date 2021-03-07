using System;
using System.Collections.Generic;
using Calling.Data;
using Microsoft.Extensions.Configuration;

namespace Calling
{
    public class LobbyStateService
    {
        public Dictionary<Guid, DateTime> ConnectedUsers { get; set; } = new Dictionary<Guid, DateTime> { };
        public LobbyState CurrentState { get; set; } = LobbyState.Lobby;
        public Dictionary<Guid, List<Guid>> UserRoomAssignment { get; set; } = new Dictionary<Guid, List<Guid>> { };
        public DateTime StateStart { get; set; } = DateTime.UtcNow;
        private readonly int _partySize;
        private readonly int _mixerLength;
        private readonly int _lobbyLength;
        private readonly int _mixSize;

        public LobbyStateService(IConfiguration configuration)
        {
            _partySize = Int32.Parse(configuration["PartySize"]);
            _mixerLength = Int32.Parse(configuration["MixerLength"]);
            _lobbyLength = Int32.Parse(configuration["LobbyLength"]);
            _mixSize = _partySize * 2;
        }

        public int CountUsers()
        {
            int numUsers = ConnectedUsers.Count;
            return numUsers;
        }

        public void UpdateUserPing(Guid userGuid)
        {
            if (ConnectedUsers.ContainsKey(userGuid))
            {
                ConnectedUsers[userGuid] = DateTime.UtcNow;
            }
            else
            {
                ConnectedUsers.Add(userGuid, DateTime.UtcNow);
            }
        }

        public void RemoveOldUsers()
        {
            TimeSpan ts = TimeSpan.FromSeconds(5);
            DateTime oldTime = DateTime.UtcNow.Subtract(ts);
            Dictionary<Guid, DateTime> RecentUsers = new Dictionary<Guid, DateTime> { };
            foreach (KeyValuePair<Guid, DateTime> entry in ConnectedUsers)
            {
                if (entry.Value > oldTime)
                {
                    RecentUsers.Add(entry.Key, DateTime.UtcNow);
                }
            }
            ConnectedUsers = RecentUsers;
        }

        public void MixUsers()
        {
            Dictionary<Guid, List<Guid>> NewUserRoomAssignment = new Dictionary<Guid, List<Guid>> { };
            int currentPartySize = 0;
            List<Guid> usersInRoom = new List<Guid>();
            Guid nextRoom = Guid.NewGuid();
            foreach (KeyValuePair<Guid, DateTime> entry in ConnectedUsers)
            {
                if (currentPartySize == 0)
                {
                    nextRoom = Guid.NewGuid();
                    usersInRoom = new List<Guid>();
                }
                currentPartySize = currentPartySize + 1;
                usersInRoom.Add(entry.Key);
                if (NewUserRoomAssignment.ContainsKey(nextRoom))
                {
                    NewUserRoomAssignment[nextRoom] = usersInRoom;
                }
                else
                {
                    NewUserRoomAssignment.Add(nextRoom, usersInRoom);
                }
                if (currentPartySize >= _partySize)
                {
                    currentPartySize = 0;
                }
            }
            UserRoomAssignment = NewUserRoomAssignment;
        }

        public int TimeLeft()
        {
            int timeLeft = 0;
            if (CurrentState == LobbyState.CountdownToMix)
            {
                TimeSpan ts = TimeSpan.FromSeconds(_lobbyLength);
                DateTime switchTime = StateStart.Add(ts);
                TimeSpan diff = DateTime.UtcNow.Subtract(switchTime);
                timeLeft = diff.Seconds;
            }
            else if (CurrentState == LobbyState.Mixed)
            {
                TimeSpan ts = TimeSpan.FromSeconds(_mixerLength);
                DateTime switchTime = StateStart.Add(ts);
                TimeSpan diff = DateTime.UtcNow.Subtract(switchTime);
                timeLeft = diff.Seconds;
            }
            return timeLeft;
        }

        public string GetCurrentState()
        {
            string returnState = "waiting for more users to join";
            switch (CurrentState)
            {
                case LobbyState.Mixed:
                    returnState = "in a mixer";
                    break;
                case LobbyState.CountdownToMix:
                    returnState = "in the breakout lobby";
                    break;
            }
            return returnState;
        }

        public void Heartbeat()
        {
            RemoveOldUsers();
            int numUsers = CountUsers();
            if (numUsers < _mixSize)
            {
                CurrentState = LobbyState.Lobby;
                StateStart = DateTime.UtcNow;
            }
            else if (numUsers >= _mixSize)
            {
                if (CurrentState == LobbyState.Lobby)
                {
                    CurrentState = LobbyState.CountdownToMix;
                    StateStart = DateTime.UtcNow;
                }
                if (CurrentState == LobbyState.CountdownToMix)
                {
                    TimeSpan ts = TimeSpan.FromSeconds(_lobbyLength);
                    DateTime switchTime = StateStart.Add(ts);
                    if (DateTime.UtcNow >= switchTime)
                    {
                        MixUsers();
                        CurrentState = LobbyState.Mixed;
                        StateStart = DateTime.UtcNow;
                    }
                }
                else if (CurrentState == LobbyState.Mixed)
                {
                    TimeSpan ts = TimeSpan.FromSeconds(_mixerLength);
                    DateTime switchTime = StateStart.Add(ts);
                    if (DateTime.UtcNow >= switchTime)
                    {
                        CurrentState = LobbyState.CountdownToMix;
                        StateStart = DateTime.UtcNow;
                    }
                }
            }
        }
    }
}