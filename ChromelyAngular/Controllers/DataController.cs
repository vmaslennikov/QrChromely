﻿using Chromely.Core.Network;

using ChromelyAngular.Backend.DB;
using ChromelyAngular.Backend.Dto;
using ChromelyAngular.Backend.Models;
using ChromelyAngular.Backend.Utils;

using System;
using System.Data.Common;
using System.Linq;
using System.Text.Json;

namespace ChromelyAngular.Controllers
{
    [ControllerProperty(Name = "DataController", Route = "data")]
    public class DataController : ChromelyController
    {
        static dynamic DbDataSource = null;

        [HttpGet(Route = "/data/ping")]
        public ChromelyResponse Ping(ChromelyRequest request)
        {
            return new ChromelyResponse() { RequestId = request.Id, Data = new { Result = "pong", Time = DateTime.UtcNow } };
        }

        [HttpGet(Route = "/data/get")]
        public ChromelyResponse Get(ChromelyRequest request)
        {
            try
            {
                if (DbDataSource == null)
                {
                    HypermediaClient jsonClient = new HypermediaClient();
                    var Zones = (jsonClient.GetAreasAsync(1, 1000).Result).Select(x => x.Name).ToArray();
                    var Positions = (jsonClient.GetPositionsAsync(1, 1000).Result).Select(x => x.Name).ToArray();
                    var BlockReasons = (jsonClient.GetBlockReasonsAsync(1, 1000).Result).Select(x => x.Name).ToArray();
                    
                    DbDataSource = new
                    {
                        Zones,
                        Positions,
                        BlockReasons,
                    };
                }
                return new ChromelyResponse()
                {
                    RequestId = request.Id,
                    Data = new
                    {
                        Result = DbDataSource,
                        Time = DateTime.UtcNow,
                        Status = "ok",
                        ErrorMessage = Array.Empty<string>()
                    }
                };
            }
            catch (Exception ex)
            {
                return new ChromelyResponse()
                {
                    RequestId = request.Id,
                    Data = new
                    {
                        Time = DateTime.UtcNow,
                        Status = "error",
                        Errors = new string[1] { ex.Message }
                    }
                };
            }
        }
    }
}
