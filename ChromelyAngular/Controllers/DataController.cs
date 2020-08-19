using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

using Chromely.Core.Network;

using ChromelyAngular.Backend.DB;
using ChromelyAngular.Backend.Utils;

using OfficeOpenXml;

namespace ChromelyAngular.Controllers
{
    [ControllerProperty(Name = "DataController", Route = "data")]
    public class DataController : ChromelyController
    {
        static dynamic DbDataSource = null;

        [Chromely.Core.Network.HttpGet(Route = "/data/ping")]
        public ChromelyResponse Ping(ChromelyRequest request)
        {
            return new ChromelyResponse() { RequestId = request.Id, Data = new { Result = "pong", Time = DateTime.UtcNow } };
        }

        [Chromely.Core.Network.HttpGet(Route = "/data/get")]
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
                    var Events = (jsonClient.GetEventsAsync(1, 1000).Result).Select(x => new { x.Id, x.Name, x.Date }).ToArray();

                    DbDataSource = new
                    {
                        Zones,
                        Positions,
                        BlockReasons,
                        Events,
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

      
        //public FileResult Download()
        //{
        //    byte[] fileBytes = System.IO.File.ReadAllBytes(@"c:\folder\myfile.ext");
        //    string fileName = "myfile.ext";
        //    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        //}
    }
}
