using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BotLib.Misc;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using BotLib.Extensions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Web;
using System.Text.RegularExpressions;
using System.Net;

namespace BotUpdator
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        string _url = "http://localhost:30006/api/bot/upoadpatchfile";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtNewVer.Text))
            {
                MessageBox.Show("[版本]必填！！！！");
                return;
            }
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;	//set to false if need to select files
            dialog.Title = "选择文件夹:";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                txtFile.Text = dialog.FileName;
            }
        }

        private void ZipFolder()
        {
            var dir = txtFile.Text.Trim();
            if (string.IsNullOrEmpty(txtFile.Text) || string.IsNullOrEmpty(txtNewVer.Text))
            {
                MessageBox.Show("[文件][版本]必填！！！！");
                return;
            }
            var zipfile = System.IO.Path.Combine(dir.Substring(0, dir.LastIndexOf("\\")), txtNewVer.Text.Trim() + "_Patch.zip");
            if (Directory.Exists(dir))
            {
                Zip.ZipDir(dir, zipfile, 5);
            }
        }

        private void btnFileUpload_Click(object sender, RoutedEventArgs e)
        {
            var richText = new TextRange(txtTip.Document.ContentStart, txtTip.Document.ContentEnd);
            if (string.IsNullOrEmpty(txtFile.Text) || string.IsNullOrEmpty(txtNewVer.Text) || string.IsNullOrEmpty(richText.Text))
            {
                MessageBox.Show("[文件][版本][提示]必填！！！！");
                return;
            }
            if(!Regex.IsMatch(txtNewVer.Text.Trim(), "^v[0-9].[0-9].[0-9]"))
            {
                MessageBox.Show("版本格式不正确，格式必须为[v8.0.0]！！！！");
                return;
            }
            
            //压缩选择的文件夹
            ZipFolder();

            //上传到服务器
            var dir = txtFile.Text.Trim();
            var patchFile = System.IO.Path.Combine(dir.Substring(0, dir.LastIndexOf("\\")), txtNewVer.Text.Trim() + "_Patch.zip");
            var patchVer = ConvertStringToVersion(txtNewVer.Text.Trim());
            var forceUpdate = chkForceUpdate.IsChecked.Value;
            var tip = richText.Text.Trim();

            var pms = new Dictionary<string, string>() {
                {"forceupdate",forceUpdate.ToString().ToLower()},
                {"tip",tip},
                {"patchver",patchVer.ToString()}
            };

            var svrUrl = BuildGetUrl(_url, pms);
            if(UploadPatchFile(svrUrl, patchFile))
            {
                MessageBox.Show("上传成功!!");
            }
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            var fileUrl = "http://localhost:30006/api/bot/DownloadPatchFile?fn=v8.0.1_Patch.zip";

            using (WebClient client = new WebClient())
            {
                client.DownloadFile(fileUrl,System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"test.zip"));
            }
        }

        public static string ConvertVersionToString(int v)
        {
            int v1 = v / 10000;
            int v2 = v % 10000 / 100;
            int v3 = v % 100;
            return string.Format("v{0}.{1}.{2}", v1, v2, v3);
        }

        public static int ConvertStringToVersion(string vstr)
        {
            vstr = vstr.Trim().ToLower();
            var vs = vstr.Split('.');
            int v1 = Convert.ToInt32(vs[0].Substring(1));
            int v2 = Convert.ToInt32(vs[1]);
            int v3 = Convert.ToInt32(vs[2]);
            return v1 * 10000 + v2 * 100 + v3;
        }


        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="dbdir"></param>
        /// <returns></returns>
        public bool UploadPatchFile(string url, string patchFn)
        {
            int BufferSize = 1024;
            HttpClient client = new HttpClient();
            using (var fileStream = new FileStream(patchFn, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, useAsync: true))
            {
                // Create a stream content for the file
                StreamContent content = new StreamContent(fileStream, BufferSize);
                content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Octet);
                // Create Multipart form data content, add our submitter data and our stream content
                MultipartFormDataContent formData = new MultipartFormDataContent();
                formData.Add(new StringContent(patchFn), "newfilename");//DataBase
                formData.Add(content, "filename", patchFn);

                // Post the MIME multipart form data upload with the file
                var rm = client.PostAsync(url, formData).Result;
                return rm.IsSuccessStatusCode;
            }
        }

        /// <summary>
        /// 组装GET请求URL。
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="parameters">请求参数</param>
        /// <returns>带参数的GET请求URL</returns>
        public String BuildGetUrl(string url, Dictionary<string, string> parameters)
        {
            if (parameters != null && parameters.Count > 0)
            {
                if (url.Contains("?"))
                {
                    url = url + "&" + BuildQuery(parameters);
                }
                else
                {
                    url = url + "?" + BuildQuery(parameters);
                }
            }
            return url;
        }

        /// <summary>
        /// 组装普通文本请求参数。
        /// </summary>
        /// <param name="parameters">Key-Value形式请求参数字典</param>
        /// <returns>URL编码后的请求数据</returns>
        public static string BuildQuery(Dictionary<string, string> parameters)
        {
            var postData = new StringBuilder();
            bool hasParam = false;

            parameters.Keys.ToList().ForEach(key =>
            {
                var name = key;
                var value = parameters[key];
                // 忽略参数名或参数值为空的参数
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                {
                    if (hasParam)
                    {
                        postData.Append("&");
                    }

                    postData.Append(name);
                    postData.Append("=");
                    postData.Append(HttpUtility.UrlEncode(value));
                    hasParam = true;
                }
            });
            return postData.ToString();
        }
    }
}
