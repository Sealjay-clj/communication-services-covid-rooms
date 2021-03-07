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
        public Dictionary<Guid, Guid> UserRoomAssignment { get; set; } = new Dictionary<Guid, Guid> { };
        public DateTime StateStart { get; set; } = DateTime.UtcNow;
        private readonly int _partySize;
        private readonly int _mixerLength;
        private readonly int _lobbyLength;
        private readonly int _mixSize;
        private readonly Guid _lobbyGuid;

        public LobbyStateService(IConfiguration configuration)
        {
            _partySize = Int32.Parse(configuration["PartySize"]);
            _mixerLength = Int32.Parse(configuration["MixerLength"]);
            _lobbyLength = Int32.Parse(configuration["LobbyLength"]);
            _mixSize = _partySize * 2;
            _lobbyGuid = Guid.Parse(configuration["MainLobbyGuid"]);
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

        public Guid GetUserRoom(Guid userGuid)
        {
            Guid userRoom = _lobbyGuid;
            if (CurrentState != LobbyState.Lobby)
            {
                if (UserRoomAssignment.ContainsKey(userGuid) && CurrentState == LobbyState.Mixed)
                {

                    userRoom = UserRoomAssignment[userGuid];

                }

            }
            return userRoom;
        }

        public void RemoveOldUsers()
        {
            TimeSpan ts = TimeSpan.FromSeconds(5);
            DateTime oldTime = DateTime.UtcNow.Subtract(ts);
            Dictionary<Guid, DateTime> RecentUsers = new Dictionary<Guid, DateTime> { };
            foreach (KeyValuePair<Guid, DateTime> entry in ConnectedUsers)
            {
                if (entry.Value > oldTime && !RecentUsers.ContainsKey(entry.Key))
                {
                    RecentUsers.Add(entry.Key, DateTime.UtcNow);
                }
            }
            ConnectedUsers = RecentUsers;
        }

        public void MixUsers()
        {
            Dictionary<Guid, Guid> NewUserRoomAssignment = new Dictionary<Guid, Guid> { };
            int currentPartySize = 0;
            Guid nextRoom = Guid.NewGuid();
            foreach (KeyValuePair<Guid, DateTime> entry in ConnectedUsers)
            {
                if (currentPartySize == 0)
                {
                    nextRoom = Guid.NewGuid();
                }
                NewUserRoomAssignment.Add(entry.Key, nextRoom);
                currentPartySize = currentPartySize + 1;
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
                timeLeft = Convert.ToInt32(diff.TotalSeconds);
            }
            else if (CurrentState == LobbyState.Mixed)
            {
                TimeSpan ts = TimeSpan.FromSeconds(_mixerLength);
                DateTime switchTime = StateStart.Add(ts);
                TimeSpan diff = DateTime.UtcNow.Subtract(switchTime);
                timeLeft = Convert.ToInt32(diff.TotalSeconds);
            }
            return timeLeft;
        }

        public string GetCurrentState()
        {
            string returnState = "Waiting for more users to join before mixing... at least " + _mixSize + " people required.";
            switch (CurrentState)
            {
                case LobbyState.Mixed:
                    returnState = "You are currently in a mixer.";
                    break;
                case LobbyState.CountdownToMix:
                    returnState = "You are currently in the breakout lobby.";
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