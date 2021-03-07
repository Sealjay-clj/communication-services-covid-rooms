// © Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Calling.Data;

namespace Calling
{
    [Route("/lobbyStatus")]
    public class LobbyController : Controller
    {
        private LobbyStateService _lobbyStateService;

        public LobbyController(LobbyStateService lobbyStateService)
        {
            this._lobbyStateService = lobbyStateService;
        }

        [HttpGet]
        public ActionResult Get([FromQuery] Guid userGuid)
        {
            _lobbyStateService.UpdateUserPing(userGuid);
            int timeLeft = _lobbyStateService.TimeLeft() * -1;
            string stateString = _lobbyStateService.GetCurrentState();
            if (timeLeft > 0)
            {
                stateString += " " + timeLeft + " seconds to go.";
            }
            int totalUsers = _lobbyStateService.CountUsers();
            Guid userRoom = _lobbyStateService.GetUserRoom(userGuid);
            Dictionary<String, String> apiResonse = new Dictionary<String, String>(){
                {"totalUsers",Convert.ToString(totalUsers)},
                {"stateString",stateString},
                {"userRoom",Convert.ToString(userRoom)}
            };
            return Ok(apiResonse);
        }
    }
}