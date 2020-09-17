using BotLib.Misc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotLib
{
    public class LogWriter
    {
        public LogWriter(string fn, bool saveLogByDay, int maxByte)
        {
            this._start = DateTime.Now;
            this._file = new LoopSaveFile(fn, maxByte, saveLogByDay);
            this.WriteLine(string.Format("\r\n============  日志启动({0})  ============", DateTime.Now.ToString()));
        }

        public void WriteEnvironmentString(string env)
        {
            this._environmentStr = env;
            this.WriteLine(string.Format("{0}：程序版本={1}\r\n-------------------------------------\r\n", DateTime.Now.ToString(), env));
        }

        public void Close(string reason)
        {
            this.WriteLine(string.Format("日志关闭({1})：原因={3},持续时间={0},托管内存占用={4}MB\r\n程序版本={2}\r\n===============================\r\n", new object[]
			{
				(DateTime.Now - this._start).TotalSeconds,
				DateTime.Now.ToString(),
				this._environmentStr,
				reason,
				((double)GC.GetTotalMemory(true) / Math.Pow(2.0, 20.0)).ToString("0.0")
			}));
            this._file.Close();
        }

        public void Clear()
        {
            this._file.Clear();
        }

        public void Write(string text, string tag, bool writeStackTrace = false, string stackTrace = null)
        {
            bool limitSameStringWriteCount = this.LimitSameStringWriteCount;
            if (limitSameStringWriteCount)
            {
                int cnt = this.UpdateWriteCount(text);
                if (cnt > 0 && cnt < 20 && cnt % 10 == 0)
                {
                    text = string.Concat(new object[]
					{
						"第",
						cnt,
						"次发生该写入,超出20次将不再提示",
						Environment.NewLine,
						text
					});
                }
                if (cnt > 20)
                {
                    return;
                }
            }
            text = string.Concat(tag,"(",DateTime.Now.ToString(),"):",text,	Environment.NewLine);
            if (writeStackTrace)
            {
                if (string.IsNullOrEmpty(stackTrace))
                {
                    stackTrace = GetStackTrace(4);
                }
                text = text + stackTrace + Environment.NewLine;
            }
            this._file.WriteLine(text);
        }

        public static string GetStackTrace(int skipFrames = 1)
        {
            var stackTrace = new StackTrace(skipFrames);
            var builder = new StringBuilder();
            foreach (var stackFrame in stackTrace.GetFrames())
            {
                string fullName = stackFrame.GetMethod().ReflectedType.FullName;
                builder.AppendLine(string.Format("{0}:   {1}", fullName, stackFrame.GetMethod().ToString()));
            }
            return builder.ToString();
        }


        public void Error(string text)
        {
            this.Write(text, "ERROR");
        }

        public void Info(string text)
        {
            this.Write(text, "Info");
        }

        public void Debug(string text)
        {
            this.Write(text, "Debug");
        }

        public void Exception(string msg)
        {
            this.Write(msg, "Exception");
        }

        private int UpdateWriteCount(string text)
        {
            int num = 0;
            if (this._wcache.ContainsKey(text))
            {
                num = this._wcache[text];
                num++;
            }
            this._wcache[text] = num;
            return num;
        }

        public void Assert(string msg)
        {
            this.Write(msg, "Assert", false, null);
        }

        public void Show()
        {
            try
            {
                if (File.Exists(this._file.FileName))
                {
                    Process.Start(this._file.FileName);
                }
            }
            catch
            {
            }
        }

        public void TimeElapse(string title, DateTime t0)
        {
            this.Info(title + ",ms=" + (DateTime.Now - t0).TotalMilliseconds);
        }

        public void WriteLine(string msg)
        {
            this._file.WriteLine(msg);
        }

        public void StackTrace()
        {
            this.Write("", "StackTrace", true, null);
        }

        public void CopyTo(string dest)
        {
            this.Close("复制日志");
            if (File.Exists(dest))
            {
                File.Delete(dest);
            }
            File.Copy(this._file.FileName, dest);
        }

        private LoopSaveFile _file;

        private string _environmentStr = "未命名版本";

        private DateTime _start;

        public bool LimitSameStringWriteCount = true;

        private Dictionary<string, int> _wcache = new Dictionary<string, int>();

        private class LoopSaveFile
        {
            public string FileName { get; set; }
            private bool _saveLogByDay;
            private NoReEnterTimer _timer;
            private int _limitFileSize = 0;
            private DateTime _checkFileSizeTime = DateTime.MinValue;
            private ConcurrentQueue<string> _cache = new ConcurrentQueue<string>();

            public LoopSaveFile(string fn, int maxFileByte, bool saveLogByDay)
            {
                this.FileName = fn;
                this._limitFileSize = maxFileByte;
                this._saveLogByDay = saveLogByDay;
                this.KeepFileSizeOrBackupFileByDay();
                this._timer = new NoReEnterTimer(WriteLoop, 1000, 0);
            }

            ~LoopSaveFile()
            {
                this.Close();
            }

            private void WriteLoop()
            {
                if (this._cache.Count > 0)
                {
                    try
                    {
                        this.KeepFileSizeOrBackupFileByDay();
                        using (StreamWriter streamWriter = this.OpenStream(true))
                        {
                            string value;
                            while (this._cache.Count > 0 && this._cache.TryDequeue(out value))
                            {
                                streamWriter.WriteLine(value);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }


            public string GetFileNameFromDate(DateTime date)
            {
                FileInfo fileInfo = new FileInfo(this.FileName);
                int length = this.FileName.LastIndexOf(fileInfo.Extension);
                return this.FileName.Substring(0, length) + date.ToString("yyyy-MM-dd") + fileInfo.Extension;
            }

            private StreamWriter OpenStream(bool append)
            {
                FileStream stream = new FileStream(this.FileName, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                return new StreamWriter(stream, Encoding.GetEncoding("gb2312"));
            }

            private void KeepFileSizeOrBackupFileByDay()
            {
                if ((DateTime.Now - this._checkFileSizeTime).TotalMinutes >= 5.0)
                {
                    this._checkFileSizeTime = DateTime.Now;
                    this.BackOldLogIfNeed();
                    this.ClearFileIfNeed();
                }
            }

            private bool BackOldLogIfNeed()
            {
                bool rt = false;
                try
                {
                    if (this._saveLogByDay && File.Exists(this.FileName))
                    {
                        var fi = new FileInfo(this.FileName);
                        if (fi.CreationTime.Date != DateTime.Now.Date && fi.Length > 0L)
                        {
                            string fileNameFromDate = this.GetFileNameFromDate(DateTime.Now.AddDays(-1.0));
                            File.Copy(this.FileName, fileNameFromDate);
                            File.Delete(this.FileName);
                            rt = true;
                        }
                    }
                }
                catch (Exception)
                {
                }
                return rt;
            }

            private void ClearFileIfNeed()
            {
                try
                {
                    if (this._limitFileSize > 0 && this.IsFileTooBig())
                    {
                        this.Clear();
                    }
                }
                catch
                {
                }
            }

            private bool IsFileTooBig()
            {
                bool rt = false;
                if (File.Exists(this.FileName))
                {
                    var fi = new FileInfo(this.FileName);
                    rt = (fi.Length > (long)this._limitFileSize);
                }
                return rt;
            }

            public void Clear()
            {
                try
                {
                    File.Delete(this.FileName);
                    using (this.OpenStream(false))
                    {
                    }
                }
                catch
                {
                }
            }

            public void WriteLine(string text)
            {
                this._cache.Enqueue(text);
            }

            public void Close()
            {
                this.WriteLoop();
            }

        }
    }
}
