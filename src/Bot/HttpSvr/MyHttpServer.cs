using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.SelfHost;
using System.Web.Http;
using System.Threading;
using System.Web.Http.Cors;
using System.Net.NetworkInformation;
using System.Net;
using System.Web;
using System.Net.Http;
using BotLib;

namespace Bot.HttpSvr
{
    public class MyHttpServer
    {
        public static MyHttpServer HttpSvrInst = null;
        static MyHttpServer()
        {
            if (HttpSvrInst == null) HttpSvrInst = new MyHttpServer();
        }

        private ushort _port;
        public ushort HttpPort
        {
            get
            {
                if (this._port <= 0)
                {
                    this._port = (ushort)FreePort.FindNextAvailableTCPPort(31138);
                }
                return this._port;
            }
        }

        public EventHandler<RecieveMsgEventArgs> OnRecieveMessage;

        public void Start()
        {
            var config = new HttpSelfHostConfiguration(string.Format("http://localhost:{0}", HttpPort));
            //跨域配置
            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Routes.MapHttpRoute(
               "Default", "api/{controller}/{id}",
               new { id = RouteParameter.Optional }
               );

            try
            {
                var server = new HttpSelfHostServer(config);
                server.OpenAsync().Wait();
                
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
            finally
            {
            }
        }
    }

    public class FreePort
    {
        private const string PortReleaseGuid = "8875BD8E-4D5B-11DE-B2F4-691756D89593";

        /// <summary> 
        /// Check if startPort is available, incrementing and 
        /// checking again if it's in use until a free port is found 
        /// </summary> 
        /// <param name="startPort">The first port to check</param> 
        /// <returns>The first available port</returns> 
        public static int FindNextAvailableTCPPort(int startPort)
        {
            int port = startPort;
            bool isAvailable = true;

            var mutex = new Mutex(false,
                string.Concat("Global/", PortReleaseGuid));
            mutex.WaitOne();
            try
            {
                IPGlobalProperties ipGlobalProperties =
                    IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] endPoints =
                    ipGlobalProperties.GetActiveTcpListeners();

                do
                {
                    if (!isAvailable)
                    {
                        port++;
                        isAvailable = true;
                    }

                    foreach (IPEndPoint endPoint in endPoints)
                    {
                        if (endPoint.Port != port) continue;
                        isAvailable = false;
                        break;
                    }

                } while (!isAvailable && port < IPEndPoint.MaxPort);

                if (!isAvailable)
                    throw new ApplicationException("Not able to find a free TCP port.");

                return port;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        /// <summary> 
        /// Check if startPort is available, incrementing and 
        /// checking again if it's in use until a free port is found 
        /// </summary> 
        /// <param name="startPort">The first port to check</param> 
        /// <returns>The first available port</returns> 
        public static int FindNextAvailableUDPPort(int startPort)
        {
            int port = startPort;
            bool isAvailable = true;

            var mutex = new Mutex(false,
                string.Concat("Global/", PortReleaseGuid));
            mutex.WaitOne();
            try
            {
                IPGlobalProperties ipGlobalProperties =
                    IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] endPoints =
                    ipGlobalProperties.GetActiveUdpListeners();

                do
                {
                    if (!isAvailable)
                    {
                        port++;
                        isAvailable = true;
                    }

                    foreach (IPEndPoint endPoint in endPoints)
                    {
                        if (endPoint.Port != port)
                            continue;
                        isAvailable = false;
                        break;
                    }

                } while (!isAvailable && port < IPEndPoint.MaxPort);

                if (!isAvailable)
                    throw new ApplicationException("Not able to find a free TCP port.");

                return port;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
    }

    public class QnController : ApiController
    {
        [HttpGet]
        public async Task<HttpResponseMessage> Get()
        {
            return await Process();
        }


        [HttpPost]
        public async Task<HttpResponseMessage> Post()
        {
            return await Process();
        }

        [NonAction]
        public async Task<HttpResponseMessage> Process()
        {
            try
            {
                var pms = await GetParams();
                string n = pms["n"];
                string v = pms["v"];
                if (MyHttpServer.HttpSvrInst != null && MyHttpServer.HttpSvrInst.OnRecieveMessage != null)
                {
                    MyHttpServer.HttpSvrInst.OnRecieveMessage(this, new RecieveMsgEventArgs(n, v));
                }

                var c = new StringContent("{state:200}", Encoding.GetEncoding("UTF-8"), "application/json");
                var res = new HttpResponseMessage();
                res.Content = c;
                res.StatusCode = HttpStatusCode.OK;
                return res;
            }
            catch (Exception exp)
            {
                Log.Exception(exp);
                var c = new StringContent("{state:500}", Encoding.GetEncoding("UTF-8"), "application/json");
                var res = new HttpResponseMessage();
                res.Content = c;
                res.StatusCode = HttpStatusCode.OK;
                return res;
            }
        }
        [NonAction]
        public async Task<Dictionary<string, string>> GetParams()
        {
            var query = Request.RequestUri.Query;
            var content = Request.Content;
            var pms = new Dictionary<string, string>();
            if (query.Length > 1)
            {
                var qs = query.Substring(1, query.Length - 1).Split('&');
                foreach (var k in qs)
                {
                    if (k.Split('=').Length < 2) continue;
                    pms[k.Split('=')[0]] = HttpUtility.UrlDecode(k.Split('=')[1]);
                }
            }
            if (content.IsFormData())
            {
                var formData = await content.ReadAsFormDataAsync();
                foreach (var key in formData.AllKeys)
                {
                    pms[key] = HttpUtility.UrlDecode(formData[key]);
                }
            }
            var data = await content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(data))
            {
                var qs = data.Split('&');
                foreach (var k in qs)
                {
                    if (k.Split('=').Length < 2) continue;
                    pms[k.Split('=')[0]] = HttpUtility.UrlDecode(k.Split('=')[1]);
                }
            }

            return pms;
        }

    }

    public class RecieveMsgEventArgs : EventArgs
    {
        public string Name { get; private set; }

        public string Value { get; private set; }

        public RecieveMsgEventArgs(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
