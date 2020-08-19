using Chromely.Core.Network;

using ChromelyAngular.Backend.DB;
using ChromelyAngular.Backend.Dto;
using ChromelyAngular.Backend.Models;

using Microsoft.EntityFrameworkCore;

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
                        Result = db.FileRequests.Include(o => o.Event).OrderByDescending(p => p.Created).ToList(),
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

        [HttpPost(Route = "/requests/new")]
        public ChromelyResponse New(ChromelyRequest request)
        {
            AppDbContext db = null;
            try
            {
                var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };

                var postDataJson = request.PostData.ToJson();
                var personRequestInfo = JsonSerializer.Deserialize<PersonRequestDto>(postDataJson, options);

                db = new AppDbContext();
                var eventObj = db.Events.FirstOrDefault(o => o.ExternalId == personRequestInfo.eventId);
                if (eventObj == null)
                {
                    eventObj = new ChromelyAngular.Backend.Models.Event();
                    eventObj.ExternalId = personRequestInfo.eventId;
                    eventObj.Name = personRequestInfo.eventName;
                    eventObj.Deleted = false;
                }
                db.SaveChanges();

                FileRequest requestObj = new FileRequest();
                requestObj.Event = eventObj;
                requestObj.Deleted = false;
                db.FileRequests.Add(requestObj);
                db.SaveChanges();

                foreach (var personId in personRequestInfo.ids)
                {
                    var personRequestObj = new PersonRequest();
                    personRequestObj.RequestId = requestObj.Id;
                    personRequestObj.PersonId = personId;
                    db.PersonRequests.Add(personRequestObj);
                }
                db.SaveChanges();

                return new ChromelyResponse()
                {
                    RequestId = request.Id,
                    Data = new
                    {
                        Result = new
                        {
                            Event = eventObj,
                            FileRequest = requestObj,
                            AddCount = personRequestInfo.ids.Length
                        },
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
                var objInfo = JsonSerializer.Deserialize<EntityDeleteDto>(postDataJson, options);

                db = new AppDbContext();
                var requests = objInfo.ids == null ? Array.Empty<FileRequest>() : db.FileRequests.Where(p => objInfo.ids.Contains(p.Id)).ToArray();
                foreach (var item in requests)
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
