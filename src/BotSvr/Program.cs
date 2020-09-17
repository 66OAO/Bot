using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Web.Http.Cors;
using BotSvr.Security;
using Newtonsoft.Json.Serialization;
using Microsoft.Owin.StaticFiles;
using BotLib;

namespace BotSvr
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            var port = ReadPort();
            var config = new HttpSelfHostConfiguration(string.Format("http://localhost:{0}",port));

            //跨域配置
            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
            config.MaxReceivedMessageSize = 2147483647;
            //config.Filters.Add(new AuthenticationFilter());
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();


            config.Routes.MapHttpRoute(
               "Default", "api/{controller}/{action}/{id}",
               new { id = RouteParameter.Optional }
               );

            try
            {
                using (var server = new HttpSelfHostServer(config))
                {
                    Log.Info(string.Format("开始监听端口:{0}",port));
                    server.OpenAsync().Wait();
                    var t = new Thread(() =>
                    {
                        while (true)
                        {
                            Thread.Sleep(30 * 60 * 1000);
                            GC.Collect();
                        }
                    });
                    t.IsBackground = true;
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                    t.Join();
                }
            } catch (Exception ex)
            {
                Log.Exception(ex);
            } finally
            {
            }
        }

        private static int ReadPort()
        {
            int port = 30006;
            try
            {
                var portStr = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "port.ini"));
                port = int.Parse(portStr.Replace("port=", ""));
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return port;
        }
    }
}
