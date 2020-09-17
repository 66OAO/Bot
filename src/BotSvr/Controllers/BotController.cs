using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using DbEntity;
using System.Net.Http;
using System.Reflection;
using System.Net;
using System.Net.Http.Headers;
using System.Windows.Forms;
using System.Web;
using BotLib.Extensions;
using BotLib.Db.Sqlite;
using BotLib.FileSerializer;
using BotLib;

namespace BotSvr.Controllers
{
    public class BotController : ApiController
    {
        private List<Type> _tableTypes;

        public SQLiteHelper Db
        {
            get
            {
                if (_Db == null)
                {
                    var dbPath = PathEx.StartUpPathOfExe + "botsvr.db";
                    _tableTypes = new List<Type>();
                    _tableTypes.Add(typeof(HybridEntity));
                    _tableTypes.Add(typeof(ShortcutEntity));
                    _tableTypes.Add(typeof(ShortcutCatalogEntity));
                    _tableTypes.Add(typeof(BuyerNoteEntity));
                    _tableTypes.Add(typeof(FavoriteNoteEntity));
                    _tableTypes.Add(typeof(OptionEntity));
                    _tableTypes.Add(typeof(UpdateDownloadEntity));
                    _Db = new SQLiteHelper(dbPath, _tableTypes);
                }
                return _Db;
            }
        }
        private SQLiteHelper _Db;

        protected IHttpActionResult Success(object data, string message = "成功")
        {
            return Ok(new Result
            {
                Code = 200,
                Message = message,
                Data = Util.SerializeWithTypeName(data)
            });
        }

        protected IHttpActionResult Error(string message = "错误")
        {
            return Ok(new Result
            {
                Code = 500,
                Message = message
            });
        }

        protected IHttpActionResult Bad(string message = "无效的请求")
        {
            return Ok(new Result
            {
                Code = 201,
                Message = message
            });
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IHttpActionResult> Login()
        {
            var pms = await GetParams();
            var nicks = pms["nicks"];
            var appver = pms["appver"];
            try
            {
                var accounts = Db.Select<AccountEntity>(string.Format("select * from AccountEntity where nick in ('{0}')", string.Join("','", nicks.Split(','))));
                var firstLoginNicks = nicks.Split(',').Where(k => !accounts.Any(j => j.Nick == k)).ToList();
                //添加第一次登陆的用户
                Db.SaveRecordsInTransaction(firstLoginNicks.Select(k =>
                    new AccountEntity
                    {
                        EntityId = StringEx.xGenGuidB64Str(),
                        Nick = k,
                        DbAccount = k,
                        FisrtTime = DateTime.Now,
                        LoginTime = DateTime.Now,
                        LogoutTime = DateTime.Now,
                        IsDeleted = false,
                        ModifyTick = DateTime.Now.Ticks,
                        Tag = null
                    }).ToList<object>());
                //查询有没有新版本
                var newApps = Db.Select<UpdateDownloadEntity>(string.Format("select * from UpdateDownloadEntity where PatchVersion > {0} order by PatchVersion desc", appver));
                return Success(new LoginDownloadEntity
                {
                    UpdateEntity = newApps != null && newApps.Count > 0 ? newApps.First() : null,
                    NickDatas = nicks.Split(',').ToList()
                });
            }
            catch (Exception e)
            {
                Log.Exception(e);
                return Bad(e.Message);
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> Logout()
        {
            return Success(string.Empty);
        }

        public async Task<IHttpActionResult> LatestVesion()
        {
            return await Task.Run<IHttpActionResult>(() =>
            {
                try
                {
                    var ets = Db.Select<UpdateDownloadEntity>("select * from UpdateDownloadEntity order by PatchVersion desc ");
                    return Success(ets.First());
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                    return Error(e.Message);
                }
            });
        }

        [HttpPost]
        public async Task<IHttpActionResult> TransferData()
        {
            try
            {
                var pms = await GetParams();
                var uploadDataJson = pms["uploaddata"];
                if (!string.IsNullOrEmpty(uploadDataJson))
                {
                    //保存上传的数据
                    var uploadDatas = Util.DeserializeWithTypeName<List<SynUploadEntity>>(uploadDataJson);
                    var ets = uploadDatas.SelectMany(k => k.DataList).ToList<object>();
                    Db.SaveRecordsInTransaction(ets);

                    //获取需要同步到客户端的数据
                    var downEts = new List<SynDownloadEntity>();
                    uploadDatas.ForEach(k =>
                    {
                        var downEt = new SynDownloadEntity();
                        downEt.DbAccount = k.DbAccount;
                        _tableTypes.ForEach(t =>
                        {
                            if (downEt.DataList == null)
                            {
                                downEt.DataList = new List<EntityBase>();
                            }
                            var dts = Db.Select(t, " where DbAccount = ? and ModifyTick > ? ", k.DbAccount,k.ServerSynTick);
                            downEt.DataList.AddRange(dts.ConvertAll<EntityBase>(d => d as EntityBase));
                        });
                        downEt.ServerSynTick = downEt.DataList.Count < 1 ? DateTime.Now.Ticks : downEt.DataList.Max(d => d.ModifyTick);
                        downEts.Add(downEt);
                    });
                    return Success(downEts);
                }
                return Success(new List<SynDownloadEntity>());

            }
            catch (Exception e)
            {
                Log.Exception(e);
                return Bad(e.Message);
            }
        }

        #region[上传下载]
        [HttpPost]
        public async Task<HttpResponseMessage> UpoadPatchFile()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            var svrpath = Path.Combine(Application.StartupPath, "Files");
            if (!Directory.Exists(svrpath))
            {
                Directory.CreateDirectory(svrpath);
            }
            var pms = await GetParams();
            var forceupdate = pms["forceupdate"];
            var tip = pms["tip"];
            var patchver = pms["patchver"];

            var provider = new CustomMultipartFormDataStreamProvider(svrpath);
            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);

                foreach (var file in provider.FileData)
                {
                    try
                    {
                        var fileinfo = new FileInfo(file.LocalFileName);
                        var svrfilepath = Path.Combine(svrpath, "BotPatchs", fileinfo.Name);
                        var svrdir = Path.Combine(svrpath, "BotPatchs");
                        if (!Directory.Exists(svrdir))
                        {
                            Directory.CreateDirectory(svrdir);
                        }
                        fileinfo.CopyTo(svrfilepath, true);
                        Db.Execute("delete from UpdateDownloadEntity where PatchVersion = ?", Convert.ToInt32(patchver));
                        Db.SaveOneRecord(new UpdateDownloadEntity
                        {
                            EntityId = StringEx.xGenGuidB32Str(),
                            DbAccount = "admin",
                            IsForceUpdate = forceupdate == "true",
                            PatchSize = (int)fileinfo.Length,
                            PatchVersion = Convert.ToInt32(patchver),
                            Tip = tip,
                            PatchFileName = fileinfo.Name,
                            ModifyTick = DateTime.Now.Ticks,
                        });
                        fileinfo.Delete();
                    }
                    catch (Exception ex)
                    {
                        Log.Exception(ex);
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<HttpResponseMessage> DownloadPatchFile()
        {
            var pms = await GetParams();
            if (pms.ContainsKey("fn"))
            {
                var fn = pms["fn"];
                try
                {
                    var dir = Path.Combine(Application.StartupPath, "Files", "BotPatchs");
                    var fileName = Path.Combine(dir, pms["fn"]);
                    var res = Request.CreateResponse(HttpStatusCode.OK);
                    res.Content = new StreamContent(new FileStream(fileName, FileMode.Open, FileAccess.Read));
                    res.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                        {
                            FileName = fn
                        };
                    return res;
                }
                catch (Exception e)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> UploadImageFile()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            var svrpath = Path.Combine(Application.StartupPath, "Files");
            if (!Directory.Exists(svrpath))
            {
                Directory.CreateDirectory(svrpath);
            }

            var provider = new CustomMultipartFormDataStreamProvider(svrpath);
            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);

                // This illustrates how to get the file names.
                foreach (var file in provider.FileData)
                {
                    try
                    {
                        var fileinfo = new FileInfo(file.LocalFileName);
                        var svrfilepath = Path.Combine(svrpath, "Images", fileinfo.Name);
                        var svrdir = Path.Combine(svrpath, "Images");
                        if (!Directory.Exists(svrdir))
                        {
                            Directory.CreateDirectory(svrdir);
                        }
                        fileinfo.CopyTo(svrfilepath, true);
                        fileinfo.Delete();
                    }
                    catch (Exception ex)
                    {
                        Log.Exception(ex);
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<HttpResponseMessage> DownloadImageFile()
        {
            var pms = await GetParams();
            if (pms.ContainsKey("fn"))
            {
                var fn = pms["fn"];
                try
                {
                    var dir = Path.Combine(Application.StartupPath, "Files", "Images");
                    var fileName = Path.Combine(dir, pms["fn"]);
                    var res = Request.CreateResponse(HttpStatusCode.OK);
                    res.Content = new StreamContent(new FileStream(fileName, FileMode.Open, FileAccess.Read));
                    res.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                    {
                        FileName = fn
                    };
                    return res;
                    //return Request.CreateResponse<string>(HttpStatusCode.OK, File.ReadAllText(fileName), "application/json");//application/octet-stream
                }
                catch (Exception e)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        public class CustomMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
        {
            public CustomMultipartFormDataStreamProvider(string path) : base(path) { }
            public override string GetLocalFileName(HttpContentHeaders headers)
            {
                return headers.ContentDisposition.FileName.Replace("\"", string.Empty);
            }
        }
        #endregion

        public async Task<Dictionary<string, string>> GetParams()
        {
            var req = this.Request;
            var pms = new Dictionary<string, string>();
            if (req.RequestUri.Query.Length > 1)
            {
                var qs = req.RequestUri.Query.Substring(1, req.RequestUri.Query.Length - 1).Split('&');
                foreach (var k in qs)
                {
                    if (k.Split('=').Length < 2) continue;
                    pms.Add(k.Split('=')[0].ToLower(), HttpUtility.UrlDecode(k.Split('=')[1]));
                }
            }
            if (req.Content.IsFormData())
            {
                var formData = await req.Content.ReadAsFormDataAsync();
                foreach (var key in formData.AllKeys)
                {
                    pms.Add(key.ToLower(), HttpUtility.UrlDecode(formData[key]));
                }
            }
            return pms;
        }


    }

    public class Result
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("data")]
        public string Data { get; set; }
    }

}
