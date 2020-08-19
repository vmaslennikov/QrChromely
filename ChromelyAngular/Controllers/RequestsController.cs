using Chromely.Core.Network;

using ChromelyAngular.Backend.DB;
using ChromelyAngular.Backend.Dto;
using ChromelyAngular.Backend.Models;

using System;
using System.Linq;
using System.Text.Json;

namespace ChromelyAngular.Controllers
{
    [ControllerProperty(Name = "RequestController", Route = "requests")]
    public class RequestsController : ChromelyController
    {
        [HttpGet(Route = "/requests/ping")]
        public ChromelyResponse Ping(ChromelyRequest request)
        {
            return new ChromelyResponse() { RequestId = request.Id, Data = new { Result = "pong", Time = DateTime.UtcNow } };
        }

        [HttpGet(Route = "/requests/all")]
        public ChromelyResponse Get(ChromelyRequest request)
        {
            AppDbContext db = null;
            try
            {
                db = new AppDbContext();
                return new ChromelyResponse()
                {
                    RequestId = request.Id,
                    Data = new
                    {
                        Result = db.FileRequests.OrderBy(p => p.Id).ToList(),
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
                        ErrorMessage = new string[1] { ex.Message }
                    }
                };
            }
            finally
            {
                db?.Dispose();
            }
        }

        [HttpPost(Route = "/requests/modify")]
        public ChromelyResponse Modify(ChromelyRequest request)
        {
            AppDbContext db = null;
            try
            {
                var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };

                var postDataJson = request.PostData.ToJson();
                var personInfo = JsonSerializer.Deserialize<PersonDto>(postDataJson, options);

                db = new AppDbContext();

                Person person = null;
                if (personInfo?.id != null)
                {
                    person = db.Persons.FirstOrDefault(o => o.Id == personInfo.id.Value);
                }
                else
                {
                    person = new Person();
                    db.Persons.Add(person);
                }

                person.FullName = personInfo?.fullname;
                person.Company = personInfo?.company;
                person.Position = personInfo?.position;
                person.Email = personInfo?.email;
                person.Phone = personInfo?.phone;
                person.HasDeclaration = personInfo?.hasdeclaration ?? false;
                person.HasPcr = personInfo?.haspcr ?? false;
                person.Status = personInfo?.status;
                person.BlockReason = personInfo?.blockreason;
                person.Zone = personInfo?.zone;
                person.Deleted = personInfo?.deleted ?? false;
                person.Photo = personInfo?.photo;
                db.SaveChanges();

                return new ChromelyResponse()
                {
                    RequestId = request.Id,
                    Data = new
                    {
                        Result = person,
                        Time = DateTime.UtcNow,
                        Status = "ok",
                        Errors = Array.Empty<string>()
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
            finally
            {
                db?.Dispose();
            }
        }

        [HttpPost(Route = "/requests/delete")]
        public ChromelyResponse Delete(ChromelyRequest request)
        {
            AppDbContext db = null;
            try
            {
                var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };

                var postDataJson = request.PostData.ToJson();
                var personInfo = JsonSerializer.Deserialize<PersonDeleteDto>(postDataJson, options);

                db = new AppDbContext();
                var persons = personInfo.ids == null ? Array.Empty<Person>() : db.Persons.Where(p => personInfo.ids.Contains(p.Id)).ToArray();
                foreach (var item in persons)
                {
                    item.Deleted = true;
                }
                db.SaveChanges();

                return new ChromelyResponse()
                {
                    RequestId = request.Id,
                    Data = new
                    {
                        Time = DateTime.UtcNow,
                        Status = "ok",
                        Errors = Array.Empty<string>()
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
            finally
            {
                db?.Dispose();
            }
        }

    }
}
