using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotLib;
using BotLib.Extensions;
using BotLib.Wpf.Extensions;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Bot.Asset;
using Bot.Net.Api;
using Bot.AssistWindow.Widget;
using DbEntity;

namespace Bot.Common.Trivial
{
    public class SynableImageHelper
    {
        private TransferFileTypeEnum _fileType;
        private readonly string _imageDir;
        private static HashSet<string> validImgExtensions;

        static SynableImageHelper()
        {
            validImgExtensions = new HashSet<string>
			{
				".jpg",
				".png",
				".gif",
				".jpeg",
				".tif",
				".tiff"
			};
        }
        public SynableImageHelper(TransferFileTypeEnum transferFileType)
        {
            this._fileType = transferFileType;
            this._imageDir = PathEx.GetSubDirOfData(transferFileType.ToString());
        }

        public bool CopyTo(string imageName, string path)
        {
            bool rt = false;
            if (!string.IsNullOrEmpty(imageName))
            {
                string srcPath = this.GetFullName(imageName);
                if (!File.Exists(srcPath))
                {
                    BotApi.DownloadImageFile(imageName, srcPath);
                }
                if (File.Exists(srcPath))
                {
                    var destFileName = Path.Combine(path, imageName);
                    File.Copy(srcPath, destFileName);
                    rt = true;
                }
            }
            return rt;
        }

        public string GetFullName(string fileName)
        {
            return this._imageDir + fileName;
        }

        public string DownloadImage(string imageName)
        {
            string text = this.GetFullName(imageName);
            if (!File.Exists(text) && !BotApi.DownloadImageFile(imageName, text))
            {
                text = null;
            }
            return text;
        }

        public void UploadImageIfServerHaveNot(string partFn)
        {
            if (!string.IsNullOrEmpty(partFn))
            {
                string fn = this.GetFullName(partFn);
                var image = BitmapImageEx.CreateFromFile(fn, 3);
                if (image != null)
                {
                    BotApi.UpoadImageFile(fn);
                }
            }
        }

        public async void UseImage(string imageName, Action<BitmapImage> cb)
        {
            if (!string.IsNullOrEmpty(imageName))
            {
                var imagePath = this.GetFullName(imageName);
                BitmapImage img = BitmapImageEx.CreateFromFile(imagePath, 3);
                if (img == null)
                {
                    await Task.Factory.StartNew<bool>(() => BotApi.DownloadImageFile(imageName, imagePath), TaskCreationOptions.LongRunning);
                    img = BitmapImageEx.CreateFromFile(imagePath, 3);
                    if (img == null)
                    {
                        img = AssetImageHelper.GetImageFromWpfCache(AssetImageEnum.imgCantFindImage);
                    }
                }
                cb(img);
                img = null;
            }
        }

        public bool CopyImageIntoCache(string fn)
        {
            bool rt = false;
            try
            {
                string fileName = Path.GetFileName(fn);
                if (!this.ExistsFile(fileName, fn))
                {
                    string fullName = this.GetFullName(fileName);
                    if (File.Exists(fullName))
                    {
                        FileEx.DeleteWithoutException(fullName);
                    }
                    File.Copy(fn, fullName);
                    rt = BotApi.UpoadImageFile(fullName);
                }
                else
                {
                    rt = true;
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
                rt = false;
            }
            return rt;
        }

        private bool ExistsFile(string img1, string img2Path)
        {
            bool rt = false;
            string img1Path = this.DownloadImage(img1);
            if (img1Path != null && File.Exists(img1Path) && FileEx.IsTwoFileEqual(img1Path, img2Path))
            {
                rt = true;
            }
            return rt;
        }

        public async void UseImage(string imageName, System.Windows.Controls.Image imageControl)
        {
            if (!string.IsNullOrEmpty(imageName))
            {
                var fullName = this.GetFullName(imageName);
                BitmapImage img = BitmapImageEx.CreateFromFile(fullName, 3);
                if (img == null)
                {
                    imageControl.Source = WebImageHelper.ImgLoading;
                    await Task.Factory.StartNew<bool>(() => BotApi.DownloadImageFile(imageName, fullName), TaskCreationOptions.LongRunning);
                    img = BitmapImageEx.CreateFromFile(fullName, 3);
                }
                imageControl.Source = img;
                img = null;
            }
        }

        public void DeleteImage(string imgName)
        {
            if (!string.IsNullOrEmpty(imgName))
            {
                var fn = this.GetFullName(imgName);
                FileEx.DeleteWithoutException(fn);
                //BotSvrApi.Delete(fn, this._fileType);
            }
        }

        public string AddNewImage(string imageName, string parnFnOld)
        {
            if (parnFnOld != null)
            {
                this.DeleteImage(parnFnOld);
            }
            return AddNewImage(imageName);
        }


        public string AddNewImage(string fullFn)
        {
            string imagePath = null;
            string extension = Path.GetExtension(fullFn);
            if (!string.IsNullOrEmpty(extension))
            {
                extension = extension.ToLower();
            }
            Util.Assert(validImgExtensions.Contains(extension));
            for (int i = 0; i < 2; i++)
            {
                imagePath = StringEx.xGenGuidB32Str() + extension;
                var fullName = this.GetFullName(imagePath);
                File.Copy(fullFn, fullName);
                if (BotApi.UpoadImageFile(fullName))
                {
                    break;
                }
                FileEx.DeleteWithoutException(fullName);
                imagePath = null;
            }
            return imagePath;
        }

    }

}
