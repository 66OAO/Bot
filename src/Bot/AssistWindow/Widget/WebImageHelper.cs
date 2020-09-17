using BotLib;
using BotLib.Extensions;
using BotLib.Wpf.Extensions;
using Bot.Asset;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Bot.AssistWindow.Widget
{
	public class WebImageHelper
	{
		private static BitmapImage _imgLoading;
		private static object _synobj;
		static WebImageHelper()
		{
			WebImageHelper._synobj = new object();
		}

		public static async void GetImageFromUrl(string url, Image image, bool useCache = false)
		{
			BitmapImage img = useCache ? null : WebImageHelper.ReadImageFromCachedFile(url);
			if (img == null)
			{
				image.Source = WebImageHelper.ImgLoading;
                await Task.Factory.StartNew(() => {
                    DownImageAndSave(url);
                }, TaskCreationOptions.LongRunning);
				img = WebImageHelper.ReadImageFromCachedFile(url);
			}
			image.Source = img;
		}

		public static BitmapImage ImgLoading
		{
			get
			{
				if (WebImageHelper._imgLoading == null)
				{
					try
					{
						WebImageHelper._imgLoading = AssetImageHelper.GetImageFromWpfCache(AssetImageEnum.imgLoading);
					}
					catch (Exception e)
					{
						Log.Exception(e);
					}
				}
				return WebImageHelper._imgLoading;
			}
		}

		private static string GetImageFileName(string url)
		{
			string subDirOfData = PathEx.GetSubDirOfData("imgcache");
			if (!Directory.Exists(subDirOfData))
			{
				Directory.CreateDirectory(subDirOfData);
			}
			return Path.Combine(subDirOfData, url.GetHashCode().ToString());
		}

		private static void DownImageAndSave(string url)
		{
			if (!string.IsNullOrEmpty(url))
			{
				string text = WebImageHelper.GetImageFileName(url);
				FileEx.DeleteWithoutException(text);
				using (WebClient webClient = new WebClient())
				{
					try
					{
						webClient.DownloadFile(url, text);
					}
					catch (Exception e)
					{
						Log.Exception(e);
						Log.Info("无法下载图片，url=" + url);
					}
				}
				try
				{
					BitmapImageEx.ResizeImageAndSave(text, 300);
				}
				catch (Exception e2)
				{
					Log.Exception(e2);
				}
			}
		}

		private static BitmapImage ReadImageFromCachedFile(string url)
		{
			BitmapImage img = null;
            lock (_synobj)
			{
				try
				{
					string filename = WebImageHelper.GetImageFileName(url);
					img = BitmapImageEx.CreateFromFile(filename, 3);
				}
				catch (Exception e)
				{
					Log.Exception(e);
				}
			}
			return img;
		}

	}
}
