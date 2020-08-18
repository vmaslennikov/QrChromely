using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

using Chromely.Core.Network;
using ChromelyAngular.Backend.Dto;

namespace ChromelyAngular.Controllers
{
    [ControllerProperty(Name = "AuthController", Route = "auth")]
    public class AuthController : ChromelyController
    {
        [HttpGet(Route = "/auth/ping")]
        public ChromelyResponse Ping(ChromelyRequest request)
        {
            return new ChromelyResponse() { RequestId = request.Id, Data = new { Result = "pong", Time = DateTime.UtcNow } };
        }

        [HttpPost(Route = "/auth/login")]
        public ChromelyResponse Login(ChromelyRequest request)
        {
            try
            {
                var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };

                var config = File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory, "config.json"));
                Dictionary<string, string> dic = JsonSerializer.Deserialize<Dictionary<string, string>>(config, options);

                var postDataJson = request.PostData.ToJson();
                var loginInfo = JsonSerializer.Deserialize<LoginDto>(postDataJson, options);

                return new ChromelyResponse()
                {
                    RequestId = request.Id,
                    Data = new
                    {
                        Result = string.Equals(loginInfo.username, dic[nameof(LoginDto.username)]) && string.Equals(loginInfo.password, dic[nameof(LoginDto.password)]),
                        Time = DateTime.UtcNow,
                        Status = "ok",
                        ErrorMessage = ""
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
                        Result = false,
                        Time = DateTime.UtcNow,
                        Status = "error",
                        ErrorMessage = ex.Message
                    }
                };
            }
        }
    }
}
