using Bot.Common.Account;
using Bot.Version;
using BotLib;
using DbEntity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Net.Api
{
    public static class BotApi
    {
        static string _svrurl = Params.Server;

        public static bool UpoadImageFile(string fn)
        {
            return UploadFile(_svrurl + "uploadimagefile", fn);
        }

        public static bool DownloadImageFile(string fn, string toPath)
        {
            var rt = false;
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(_svrurl + "downloadimagefile?fn=" + fn, toPath);
                }
                rt = true;
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return rt;
        }

        private static string GetPatchFileDownloadUrl(string fn)
        {
            return _svrurl + "downloadpatchfile?fn=" + fn;
        }

        public static bool UploadFile(string url, string fn)
        {
            try
            {
                int BufferSize = 1024;
                HttpClient client = new HttpClient();
                using (var fileStream = new FileStream(fn, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, useAsync: true))
                {
                    // Create a stream content for the file
                    StreamContent content = new StreamContent(fileStream, BufferSize);
                    content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Octet);
                    // Create Multipart form data content, add our submitter data and our stream content
                    MultipartFormDataContent formData = new MultipartFormDataContent();
                    formData.Add(new StringContent(fn), "newfilename");//DataBase
                    formData.Add(content, "filename", fn);

                    // Post the MIME multipart form data upload with the file
                    var res = client.PostAsync(url, formData).Result;
                    return res.IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return false;
        }

        public async static Task<UpdateDownloadEntity> GetLatestVesion()
        {
            var url = _svrurl + "latestvesion";
            var et = await DoPostForOne<UpdateDownloadEntity>(url, null);
            if (et != null)
            {
                et.PatchUrl = GetPatchFileDownloadUrl(et.PatchFileName);
            }
            return et;
        }

        public async static Task<LoginDownloadEntity> Login(List<string> nicks)
        {
            var firstLoginMainNicks = GetFirstLoginNicks(nicks);
            var newAppver = InstalledVersionManager.GetNewestVersion();
            var url = _svrurl + "login";
            var loginInfo = await DoPostForOne<LoginDownloadEntity>(url
                    , new Dictionary<string, string>() { 
                        {"nicks",string.Join(",",nicks)},
                        {"firstLoginMainNicks",string.Join(",",firstLoginMainNicks)},
                        {"appver",newAppver==null ? "0" : newAppver.Version.ToString()},
                        {"instanceGuid",Params.InstanceGuid},
                    }
                );
            if (loginInfo != null && loginInfo.UpdateEntity != null)
            {
                loginInfo.UpdateEntity.PatchUrl = GetPatchFileDownloadUrl(loginInfo.UpdateEntity.PatchFileName);
            }
            return loginInfo;
        }

        public static async Task<List<SynDownloadEntity>> TransferData(List<SynUploadEntity> upDatas)
        {
            try
            {
                var url = _svrurl + "transferdata";
                var downEts = await DoPost<SynDownloadEntity>(url
                        , new Dictionary<string, string>() { 
                            {"uploaddata",Util.SerializeWithTypeName(upDatas)},
                        }
                    );
                return downEts;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return null;
            }
        }


        private static List<string> GetFirstLoginNicks(List<string> lst)
        {
            var mainAccounts = TbNickHelper.GetMainAccounts(lst);
            mainAccounts.RemoveWhere((string x) => !Params.IsFirstLogin(x));
            return mainAccounts.ToList();
        }

        private async static Task<List<T>> DoPost<T>(string url, Dictionary<string, string> parameters, string token = "")
        {
            var client = new HttpClient();
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            var res = await client.PostAsync(url, parameters == null ? null : new FormUrlEncodedContent(parameters));
            var rt = await res.Content.ReadAsStringAsync();
            Result svrDt = null;
            var ds = new List<T>();
            if (!string.IsNullOrEmpty(rt))
            {
                svrDt = Util.DeserializeWithTypeName<Result>(rt);
                if (svrDt != null && svrDt.Code == 200)
                {
                    ds = Util.DeserializeWithTypeName<List<T>>(svrDt.Data);
                }
                else
                {
                    Log.Error("请求接口出错:" + svrDt.Message);
                }
            }
            return ds;
        }

        private async static Task<T> DoPostForOne<T>(string url, Dictionary<string, string> parameters, string token = "")
        {
            var client = new HttpClient();
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            var res = await client.PostAsync(url, parameters == null ? null : new FormUrlEncodedContent(parameters));
            var rt = await res.Content.ReadAsStringAsync();
            Result svrDt = null;
            var t = default(T);
            if (!string.IsNullOrEmpty(rt))
            {
                svrDt = Util.DeserializeWithTypeName<Result>(rt);
                if (svrDt != null && svrDt.Code == 200)
                {
                    t = Util.DeserializeWithTypeName<T>(svrDt.Data);
                }
                else
                {
                    Log.Error("请求接口出错:" + svrDt.Message);
                }
            }
            return t;
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
