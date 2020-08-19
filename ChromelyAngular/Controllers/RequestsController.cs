using Chromely.Core.Network;

using ChromelyAngular.Backend.DB;
using ChromelyAngular.Backend.Dto;
using ChromelyAngular.Backend.Models;

using Microsoft.EntityFrameworkCore;

using OfficeOpenXml;

using System;
using System.IO;
using System.IO.Compression;
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

        [Chromely.Core.Network.HttpPost(Route = "/requests/zip")]
        public ChromelyResponse Zip(ChromelyRequest request)
        {
            AppDbContext db = null;
            try
            {
                byte[] archiveFile = null;
                byte[] archiveFile2 = null;
                string eventRplId = null;

                var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };
                var postDataJson = request.PostData.ToJson();
                var objInfo = JsonSerializer.Deserialize<EntityIdDto>(postDataJson, options);

                if (objInfo != null && objInfo.id != Guid.Empty)
                {
                    var id = objInfo.id;

                    db = new AppDbContext();
                    var filerequest = db.FileRequests.FirstOrDefault(x => x.Id == id);
                    var eventData = db.Events.FirstOrDefault(x => x.Id == filerequest.EventId);
                    eventRplId = eventData?.ExternalId;
                    var personsIds = db.PersonRequests.Where(x => x.RequestId == id).DefaultIfEmpty().Select(x => x.PersonId).ToList();
                    var persons = db.Persons.Where(x => personsIds.Contains(x.Id)).OrderBy(x => x.FullName).ToList();
                    byte[] excelBytes = null;
                    using (ExcelPackage excel = new ExcelPackage())
                    {
                        /*

                         */

                        var ws = excel.Workbook.Worksheets.Add("Лист 1");
                        ws.Cells[1, 1].Value = "№";
                        ws.Cells[1, 2].Value = "организация";
                        ws.Cells[1, 3].Value = "фамилия, имя, отчество";
                        ws.Cells[1, 4].Value = "должность";
                        ws.Cells[1, 5].Value = "фотография";
                        ws.Cells[1, 6].Value = "мобильный телефон";
                        ws.Cells[1, 7].Value = "электронная почта";
                        ws.Cells[1, 8].Value = "разрешенные зоны доступа";
                        ws.Cells[1, 9].Value = "Наличие заполненной декларации по форме РФС";
                        ws.Cells[1, 10].Value = "Наличие результата анализа на ПЦР";
                        ws.Cells[1, 11].Value = "uip";
                        ws.Cells[1, 12].Value = "статус";
                        ws.Cells[1, 13].Value = "причина статуса";

                        for (int i = 0, row = 2; i < persons.Count; i++, row++)
                        {
                            ws.Cells[row, 1].Value = (i + 1).ToString();
                            ws.Cells[row, 2].Value = persons[i].Company;
                            ws.Cells[row, 3].Value = persons[i].FullName;
                            ws.Cells[row, 4].Value = persons[i].Position;
                            ws.Cells[row, 5].Value = persons[i].FullName;
                            ws.Cells[row, 6].Value = persons[i].Phone;
                            ws.Cells[row, 7].Value = persons[i].Email;
                            ws.Cells[row, 8].Value = persons[i].Zone;
                            ws.Cells[row, 9].Value = persons[i].HasDeclaration ? "1" : "0";
                            ws.Cells[row, 10].Value = persons[i].HasPcr ? "1" : "0";
                            ws.Cells[row, 11].Value = persons[i].Id.ToString();
                            ws.Cells[row, 12].Value = persons[i].Status;
                            ws.Cells[row, 13].Value = persons[i].BlockReason;
                        }
                        excelBytes = excel.GetAsByteArray();
                    }

                    var startIndex = "data:image/png;base64,".Length;
                    using (var archiveStream = new MemoryStream())
                    using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true))
                    {
                        persons.ForEach(x =>
                        {
                            var img = System.Convert.FromBase64String(x.Photo.Substring(startIndex));
                            var zipArchiveEntryImg = archive.CreateEntry($"{x.FullName}.jpg", System.IO.Compression.CompressionLevel.NoCompression);
                            using (var zipStream = zipArchiveEntryImg.Open())
                                zipStream.Write(img, 0, img.Length);

                        });

                        using (var archiveStreamXlsx = new MemoryStream())
                        using (var archiveXlsx = new ZipArchive(archiveStreamXlsx, ZipArchiveMode.Create, true))
                        {
                            var zipArchiveEntry = archiveXlsx.CreateEntry("users.xlsx", System.IO.Compression.CompressionLevel.NoCompression);
                            using (var zipStream = zipArchiveEntry.Open())
                                zipStream.Write(excelBytes, 0, excelBytes.Length);

                            var excelZip = archiveStreamXlsx.ToArray();

                            var zipArchiveEntryExcel = archive.CreateEntry("users.xlsx", System.IO.Compression.CompressionLevel.NoCompression);
                            using (var zipStream = zipArchiveEntryExcel.Open())
                                zipStream.Write(excelZip, 0, excelZip.Length);
                        }
                        archiveFile = archiveStream.ToArray();
                    }

                    //using (var archiveStream = new MemoryStream())
                    //using (ZipFile zip = new ZipFile())
                    //{
                    //    persons.ForEach(x =>
                    //    {
                    //        var img = System.Convert.FromBase64String(x.Photo.Substring(startIndex));
                    //        var zipArchiveEntryImg = zip.AddEntry($"{x.FullName}.jpg", img);

                    //    });
                    //    var zipArchiveEntry = zip.AddEntry("users.xlsx", excelBytes);
                    //    zip.Save(archiveStream);
                    //    archiveFile2 = archiveStream.ToArray();
                    //}
                }
                File.WriteAllBytes(System.IO.Path.Combine(Environment.CurrentDirectory, $"{eventRplId}_{DateTime.UtcNow.Ticks}.zip"), archiveFile);
                //return new FileContentResult(archiveFile, System.Net.Mime.MediaTypeNames.Application.Zip);
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
                string error = ex.Message;
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
