using BotLib;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BotLib.Extensions;
using Bot.Automation.ChatDeskNs;
using Bot.Automation.ChatDeskNs.Automators;

namespace Bot.Common
{
    public class QnHelper
    {
        public class Detected
        {
            public static Dictionary<string, LoginedSeller> _cachedNicks { get; set; }
            static Detected()
            {
                _cachedNicks = new Dictionary<string, LoginedSeller>();
            }

            public static LoginedSeller GetSellerFromCache(string seller)
            {
                return _cachedNicks[seller];
            }

            public static string[] GetNicks()
            {
                List<string> sellers = new List<string>();
                if (_cachedNicks != null)
                {
                    sellers = _cachedNicks.Keys.ToList();
                }
                return sellers.ToArray();
            }

            public static Dictionary<string, LoginedSeller> Update(Dictionary<string, LoginedSeller> allNicks, out HashSet<string> removeNicks)
            {
                removeNicks = new HashSet<string>();
                var newNicks = new Dictionary<string, LoginedSeller>();
                foreach (var nick in allNicks)
                {
                    if (!_cachedNicks.ContainsKey(nick.Key))
                    {
                        var chatDesk = ChatDesk.GetDeskFromCache(nick.Key);
                        if (chatDesk != null)
                        {
                            chatDesk.CheckAlive();
                            removeNicks.Add(nick.Key);
                            Log.Error("Detected, 检测到异常的ChatDesk");
                        }
                        else
                        {
                            newNicks[nick.Key] = nick.Value;
                        }
                    }
                }
                foreach (var oldNick in _cachedNicks)
                {
                    if (!allNicks.ContainsKey(oldNick.Key))
                    {
                        removeNicks.Add(oldNick.Key);
                    }
                }
                _cachedNicks = allNicks;
                return newNicks;
            }

        }

        public static string _qnVer { get; set; }
        public static DateTime _qnVersionCacheTime { get; set; }
        private static int _qnVersionInt;
        public static bool _isUpdatingQnVersionCache { get; set; }
        private static string _QnCurrentVersionRootDir;
        private static string _qnProcessPath;
        public static Dictionary<string, bool> _isQnVersionLessThanCache { get; set; }
        public static string _wwcmdpath { get; set; }
        public static int _qnVersionSection1 { get; set; }
        public static int _qnVersionSection2 { get; set; }
        public static int _qnVersionSection3 { get; set; }
        public static string WwcmdPath
        {
            get
            {
                if (_wwcmdpath == null)
                {
                    string text = null;
                    try
                    {
                        RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey("aliim");
                        registryKey = registryKey.OpenSubKey("Shell");
                        registryKey = registryKey.OpenSubKey("Open");
                        registryKey = registryKey.OpenSubKey("Command");
                        text = registryKey.GetValue("").ToString();
                        int num = text.IndexOf("wwcmd.exe");
                        text = text.Substring(0, num + 9);
                    }
                    catch (Exception e)
                    {
                        Log.Exception(e, "WwcmdPath");
                    }
                    _wwcmdpath = (text ?? "");
                }
                return _wwcmdpath;
            }
        }
        public static string QnVersion
        {
            get
            {
                if (_qnVer == "")
                {
                    UpdateQnVersionCache();
                }
                else
                {
                    if (_qnVersionCacheTime.xIsTimeElapseMoreThanMinute(1.0))
                    {
                        Task.Run(() => { UpdateQnVersionCache(); });
                    }
                }
                return _qnVer;
            }
        }
        public static int QnVersionInt
        {
            get
            {
                if (_qnVersionInt == 0)
                {
                    _qnVersionInt = ParseQnVersionFromDir(QnCurrentVersionRootDir);
                }
                return _qnVersionInt;
            }
        }
        public static string QnCurrentVersionRootDir
        {
            get
            {
                if (string.IsNullOrEmpty(_QnCurrentVersionRootDir))
                {
                    string qnProcessDir = QnProcessDir;
                    if (qnProcessDir != null)
                    {
                        string text = qnProcessDir + QnVersion + "\\";
                        if (Directory.Exists(text))
                        {
                            _QnCurrentVersionRootDir = text;
                        }
                    }
                }
                return _QnCurrentVersionRootDir;
            }
        }
        public static string QnProcessDir
        {
            get
            {
                if (_qnProcessPath == null)
                {
                    _qnProcessPath = CUtil.GetProcessPath("Aliworkbench") + "\\";
                    if (!Directory.Exists(_qnProcessPath))
                    {
                        _qnProcessPath = null;
                    }
                }
                return _qnProcessPath;
            }
        }
        public static bool HasQnRunning
        {
            get
            {
                var pName = Process.GetProcessesByName("AliApp");
                bool rt;
                if (pName.Length > 0)
                {
                    rt = true;
                }
                else
                {
                    pName = Process.GetProcessesByName("AliWorkbench");
                    rt = (pName.Length > 0);
                }
                return rt;
            }
        }

        static QnHelper()
        {
            _wwcmdpath = null;
            _isUpdatingQnVersionCache = false;
            _isQnVersionLessThanCache = new Dictionary<string, bool>();
            _qnVer = "";
            _qnVersionCacheTime = DateTime.MinValue;
            _qnVersionSection3 = -1;
            _qnVersionSection2 = -1;
            _qnVersionSection1 = -1;
        }

        public static DateTime ConvertQianNiuMsTickToDataTime(long mstick)
        {
            DateTime result = new DateTime(1970, 1, 1, 0, 0, 0);
            result = result.AddMilliseconds((double)mstick).ToLocalTime();
            return result;
        }
        public static DateTime ConvertQianNiuMsTickToDataTime(string msTickStr)
        {
            DateTime result = DateTime.MinValue;
            try
            {
                long mstick = Convert.ToInt64(msTickStr);
                result = ConvertQianNiuMsTickToDataTime(mstick);
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("exp={0},msTickStr={1}", ex.Message, msTickStr));
            }
            return result;
        }

        private static int ParseQnVersionFromDir(string dir)
        {
            int result = 0;
            try
            {
                string text = PathEx.GetRightSectionOfPath(dir, true, 1);
                if (text.EndsWith("\\"))
                {
                    text = text.xRemoveLastChar();
                }
                result = ParseQnVersion(text);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return result;
        }
        private static int ParseQnVersion(string vstr)
        {
            int ver = 0;
            try
            {
                if (Regex.IsMatch(vstr, "\\d\\.\\d{2}\\.\\d{2}N"))
                {
                    vstr = vstr.xRemoveLastChar();
                    var vers = vstr.Split('.');
                    Util.Assert(vers.Length == 3);
                    int v1 = Convert.ToInt32(vers[0]) * 10000;
                    int v2 = Convert.ToInt32(vers[1]) * 100;
                    int v3 = Convert.ToInt32(vers[2]);
                    ver = v1 + v2 + v3;
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return ver;
        }
        public static bool IsGreaterV6_07_00N()
        {
            return !IsLessQnVersion(6, 7, 0);
        }
        public static bool IsGreaterV6_02_00N()
        {
            return !IsLessQnVersion(6, 2, 0);
        }

        public static bool IsLessQnVersion(int qv1, int qv2, int qv3)
        {
            string qvKey = string.Format("v={0},s1={1},s2={2},s3={3}", QnVersion, qv1, qv2, qv3);
            if (!_isQnVersionLessThanCache.ContainsKey(qvKey))
            {
                _isQnVersionLessThanCache[qvKey] = IsQnVersionLessThanUnCache(qv1, qv2, qv3);
            }
            return _isQnVersionLessThanCache[qvKey];
        }

        private static bool IsQnVersionLessThanUnCache(int sec1, int sec2, int sec3)
        {
            try
            {
                if (GetQnVersionSection1() < sec1)
                {
                    return true;
                }
                if (GetQnVersionSection1() > sec1)
                {
                    return false;
                }
                if (GetQnVersionSection2() > sec2)
                {
                    return false;
                }
                if (GetQnVersionSection2() < sec2)
                {
                    return true;
                }
                if (GetQnVersionSection3() < sec3)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return false;
        }

        private static int GetQnVersionSection3()
        {
            if (_qnVersionSection3 < 0)
            {
                string[] array = QnVersion.Split('.');
                _qnVersionSection3 = Convert.ToInt32(array[2].Substring(0, array[2].Length - 1));
            }
            return _qnVersionSection3;
        }
        private static int GetQnVersionSection2()
        {
            if (_qnVersionSection2 < 0)
            {
                string[] array = QnVersion.Split('.');
                _qnVersionSection2 = Convert.ToInt32(array[1]);
            }
            return _qnVersionSection2;
        }
        private static int GetQnVersionSection1()
        {
            if (_qnVersionSection1 < 0)
            {
                string[] array = QnVersion.Split('.');
                _qnVersionSection1 = Convert.ToInt32(array[0]);
            }
            return _qnVersionSection1;
        }


        private static void UpdateQnVersionCache()
        {
            if (!_isUpdatingQnVersionCache)
            {
                _isUpdatingQnVersionCache = true;
                string text = "";
                try
                {
                    string text2 = CUtil.GetProcessPath("Aliworkbench");
                    if (!string.IsNullOrEmpty(text2))
                    {
                        string path = text2 + "\\AliWorkbench.ini";
                        string input = File.ReadAllText(path);
                        string pattern = "(?<=Version =)[0-9\\.]+N";
                        Match match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            text = match.ToString();
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
                _qnVer = text.Trim().ToUpper();
                _qnVersionCacheTime = DateTime.Now;
                _isUpdatingQnVersionCache = false;
            }
        }


    }
}
