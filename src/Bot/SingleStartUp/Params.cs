using BotLib;
using BotLib.Db.Sqlite;
using BotLib.Extensions;
using BotLib.Misc;
using Bot.Common;
using Bot.Options.HotKey;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bot.Common.Account;
using DbEntity;

namespace Bot
{
    public class Params
    {
        public static bool ForceActiveIME
        {
            get
            {
                return PersistentParams.GetParam("ForceActiveIME", true);
            }
            set
            {
                PersistentParams.TrySaveParam("ForceActiveIME", value);
            }
        }

        public static bool SetIsFirstLogin(string nick)
        {
            bool param2Key;
            if (param2Key = PersistentParams.GetParam2Key("IsFirstLogin", nick, true))
            {
                PersistentParams.TrySaveParam2Key("IsFirstLogin", nick, false);
            }
            return param2Key;
        }


        public static bool IsFirstLogin(string nick)
        {
            bool param2Key;
            if (param2Key = PersistentParams.GetParam2Key("IsFirstLogin", nick, true))
            {
                PersistentParams.TrySaveParam2Key("IsFirstLogin", nick, false);
            }
            return param2Key;
        }

        public static string Server
        {
            get
            {
                return _server;
            }
        }
        
        public static string RealServer
        {
            get
            {
                return Params.Server;
            }
        }
        public static bool IsDevoloperClient
        {
            get
            {
                if (Params._isDevoloperClient == null)
                {
                    Params._isDevoloperClient = new bool?(File.Exists("DevoloperClientMark.txt"));
                    bool? isDevoloperClient = Params._isDevoloperClient;
                    if (!isDevoloperClient.GetValueOrDefault() & isDevoloperClient != null)
                    {
                        string filenameUnderAppDataDir = PathEx.GetFilenameUnderAppDataDir("DevoloperClientMark.txt");
                        Params._isDevoloperClient = new bool?(File.Exists(filenameUnderAppDataDir));
                    }
                }
                return Params._isDevoloperClient.Value;
            }
        }

        public static bool IsShowGoodsKnowledgeWhenBuyerTalkIt
        {
            get
            {
                return PersistentParams.GetParam("IsShowGoodsKnowledgeWhenBuyerTalkIt", true);
            }
            set
            {
                PersistentParams.TrySaveParam("IsShowGoodsKnowledgeWhenBuyerTalkIt", value);
            }
        }

        public static void SaveLatestCheckDbDeleteTime(string dbAccount)
        {
            PersistentParams.TrySaveParam2Key("LatestCheckDbDeleteTime", dbAccount, BatTime.Now);
        }

        public static DateTime GetLatestCheckDbDeleteTime(string dbAccount)
        {
            return PersistentParams.GetParam2Key("LatestCheckDbDeleteTime", dbAccount, DateTime.MinValue);
        }

        public static DateTime GetLatestSynOkTime(string dbAccount)
        {
            return PersistentParams.GetParam2Key("LatestSynOkTime", dbAccount, DateTime.MinValue);
        }

        public static void SetLatestSynOkTime(string dbAccount)
        {
            PersistentParams.TrySaveParam2Key("LatestSynOkTime", dbAccount, BatTime.Now);
        }

        public static string SystemInfo
        {
            get
            {
                return string.Format("{4} {0},千牛版本={1}，PcId={2},{3}", new object[]
				{
					Params.VersionStr,
					QnHelper.QnVersion,
					Params.PcId,
					ComputerInfo.SysInfoForLog,
					"软件"
				});
            }
        }

        public static bool TestParam
        {
            get
            {
                return PersistentParams.GetParam("TestParam", Params.TestParamDefault);
            }
            set
            {
                PersistentParams.TrySaveParam("TestParam", true);
            }
        }

        public static string PcId
        {
            get
            {
                if (Params._pcGuid == null)
                {
                    Params._pcGuid = ComputerInfo.GetCpuID();
                }
                return Params._pcGuid;
            }
        }

        public static string InstanceGuid
        {
            get
            {
                if (Params._instanceGuid == null)
                {
                    string text = PersistentParams.GetParam("InstanceGuid", "");
                    string param = PersistentParams.GetParam("PcId4InstanceGuid", "");
                    if (string.IsNullOrEmpty(text) || param != Params.PcId)
                    {
                        text = StringEx.xGenGuidB64Str();
                        PersistentParams.TrySaveParam("InstanceGuid", text);
                        PersistentParams.TrySaveParam("PcId4InstanceGuid", Params.PcId);
                    }
                    Params._instanceGuid = text;
                }
                return Params._instanceGuid;
            }
        }


        public static bool IsAppStartMoreThan10Second
        {
            get
            {
                return (DateTime.Now - Params.AppStartTime).TotalSeconds > 10.0;
            }
        }

        public static bool IsAppStartMoreThan20Second
        {
            get
            {
                return (DateTime.Now - Params.AppStartTime).TotalSeconds > 20.0;
            }
        }

        public static bool HadUniformShortcutCode
        {
            get
            {
                return PersistentParams.GetParam("HadUniformShortcutCode", false);
            }
            set
            {
                PersistentParams.TrySaveParam("HadUniformShortcutCode", value);
            }
        }

        public static bool NeedClearDb
        {
            get
            {
                return PersistentParams.GetParam("NeedClearDb", false);
            }
            set
            {
                PersistentParams.TrySaveParam("NeedClearDb", value);
            }
        }

        public static void SetProcessPath(string processName, string processPath)
        {
            string key = Params.GetProcessPathKey(processName);
            PersistentParams.TrySaveParam(key, processPath);
        }

        private static string GetProcessPathKey(string processName)
        {
            return "ProcessPath#" + processName;
        }

        public static string GetProcessPath(string processName)
        {
            string key = Params.GetProcessPathKey(processName);
            return PersistentParams.GetParam(key, "");
        }


        static Params()
        {
            Params.VersionStr = ShareUtil.ConvertVersionToString(80000);
            Params.TestVersionDesc = "";
            Params.TestParamDefault = false;
            Params.AppSecret = "asfda";
            Params.HelpRoot = "https://github.com/renchengxiaofeixia";
            Params._isDevoloperClient = null;
            Params.BottomPannelAnswerCount = 5;
            Params.RulePatternMatchStrict = true;
            Params.AppStartTime = DateTime.Now;
            Params.IsAppClosing = false;
        }

        public const int Version = 80205;

        public static string VersionStr;

        public static string TestVersionDesc;

        public const string CreateDateStr = "2018.09.19";

        public const bool UseHook = false;

        public static bool TestParamDefault;

        public static string AppSecret;

        private static string _server = "http://localhost:30006/api/bot/";

        private const bool _isUseLocalTestServer = false;

        public static string HelpRoot;

        public const int KeepInstalledVersionsCount = 3;

        public const string AppName = "千牛客服";

        private static bool? _isDevoloperClient;

        public const int CtlGoodsListGoodsPerPageMin = 3;

        public const int CtlGoodsListGoodsPerPageMax = 20;

        public const int CtlGoodsListGoodsPerPageDefault = 4;

        public const int MaxQACountForChatRecordManager = 2000;

        public const int MaxSynableQuestionAnswersCount = 2000;

        public const int MaxAddQaCountForQuestionAndAnswersCiteTableManager = 30000;

        public const int MaxSynableQuestionTimeoutDays = 10;

        public const bool UseChaJianMode = false;

        public static int BottomPannelAnswerCount;

        public static bool RulePatternMatchStrict;

        private static string _pcGuid;

        private static string _instanceGuid;

        public static readonly DateTime AppStartTime;


        public static bool IsAppClosing;

        public class Other
        {
            public static int FontSize
            {
                get
                {
                    int num = PersistentParams.GetParam("Other.FontSize", 12);
                    if (num < 12)
                    {
                        num = 12;
                        Params.Other.FontSize = 12;
                    }
                    else if (num > 14)
                    {
                        num = 14;
                        Params.Other.FontSize = 14;
                    }
                    return num;
                }
                set
                {
                    PersistentParams.TrySaveParam("Other.FontSize", value);
                }
            }

            public const int FontSizeDefault = 12;
        }

        public class BuyerNote
        {
            static BuyerNote()
            {
                Params.BuyerNote.IsPreferSelfNoteDefault = true;
                Params.BuyerNote.IsShowDetailAsTooltipDefault = true;
            }
            public static bool GetSetIsPreferSelfNote(string nick)
            {
                return PersistentParams.GetParam2Key("BuyerNote.IsPreferSelfNote", nick, Params.BuyerNote.IsPreferSelfNoteDefault);
            }

            public static void SetIsPreferSelfNote(string nick, bool isPreferSelfNote)
            {
                PersistentParams.TrySaveParam2Key("BuyerNote.IsPreferSelfNote", nick, isPreferSelfNote);
            }

            public static bool GetIsShowDetailAsTooltip(string nick)
            {
                return PersistentParams.GetParam2Key("BuyerNote.IsShowDetailAsTooltip", nick, Params.BuyerNote.IsShowDetailAsTooltipDefault);
            }

            public static void SetIsShowDetailAsTooltip(string nick, bool isShowDetailAsTooltip)
            {
                PersistentParams.TrySaveParam2Key("BuyerNote.IsShowDetailAsTooltip", nick, isShowDetailAsTooltip);
            }


            public static bool IsPreferSelfNoteDefault;

            public static bool IsShowDetailAsTooltipDefault;
        }

        public class Auth
        {
            public static bool IsAllAccountEditShortCutDefault;
            public static bool IsAllAccountEditKnowledgeDefault;
            public static bool IsAllAccountEditRobotDefault;

            static Auth()
            {
                Params.Auth.IsAllAccountEditShortCutDefault = true;
                Params.Auth.IsAllAccountEditKnowledgeDefault = true;
                Params.Auth.IsAllAccountEditRobotDefault = true;
            }

            public static string GetSuperAccounts(string mainnick)
            {
                //TbNickHelper.AssertMainNick(mainnick);
                return PersistentParams.GetParam2Key<string>("Auth.SuperAccounts", mainnick, string.Empty);
            }

            public static void SetSuperAccounts(string nick, string accounts)
            {
                PersistentParams.TrySaveParam2Key<string>("Auth.SuperAccounts", nick, accounts);
            }

            public static bool GetIsAllAccountEditShortCut(string nick)
            {
                return PersistentParams.GetParam2Key<bool>("IsAllAccountEditShortCut", AccountHelper.GetPubDbAccount(nick), Params.Auth.IsAllAccountEditShortCutDefault);
            }

            public static void SetIsAllAccountEditShortCut(string nick, bool isAllAccountEditShortCut)
            {
                PersistentParams.TrySaveParam2Key<bool>("IsAllAccountEditShortCut", AccountHelper.GetPubDbAccount(nick), isAllAccountEditShortCut);
            }

            public static bool GetIsAllAccountEditKnowledge(string nick)
            {
                return PersistentParams.GetParam2Key<bool>("IsAllAccountEditKnowledge", AccountHelper.GetPubDbAccount(nick), Params.Auth.IsAllAccountEditKnowledgeDefault);
            }

            public static void SetIsAllAccountEditKnowledge(string nick, bool isAllAccountEditKnowledge)
            {
                PersistentParams.TrySaveParam2Key<bool>("IsAllAccountEditKnowledge", AccountHelper.GetPubDbAccount(nick), isAllAccountEditKnowledge);
            }

            public static bool GetIsAllAccountEditRobot(string nick)
            {
                return PersistentParams.GetParam2Key<bool>("IsAllAccountEditRobot", AccountHelper.GetPubDbAccount(nick), Params.Auth.IsAllAccountEditRobotDefault);
            }

            public static void SetIsAllAccountEditRobot(string nick, bool isAllAccountEditRobot)
            {
                PersistentParams.TrySaveParam2Key<bool>("IsAllAccountEditRobot", AccountHelper.GetPubDbAccount(nick), isAllAccountEditRobot);
            }


        }


        public class Shortcut
        {
            public enum ShowType
            {
                ShopOnly,
                ShopAndSelf,
                SelfOnly
            }

            static Shortcut()
            {
                Params.Shortcut.ShowTypeDefault = Params.Shortcut.ShowType.ShopAndSelf;
            }

            public static Params.Shortcut.ShowType ShowTypeDefault;
            public const bool IsShowTitleButtonsDefault = true;


            public static bool GetIsShowTitleButtons(string nick)
            {
                return PersistentParams.GetParam2Key("Robot.IsShowTitleButtons", nick, false);
            }

            public static void SetIsShowTitleButtons(string nick, bool isShowTitleButtons)
            {
                PersistentParams.TrySaveParam2Key("Robot.IsShowTitleButtons", nick, isShowTitleButtons);
            }


            public static Params.Shortcut.ShowType GetShowType(string nick)
            {
                return PersistentParams.GetParam2Key<Params.Shortcut.ShowType>("Shortcut.ShowType", nick, Params.Shortcut.ShowTypeDefault);
            }

            public static void SetShowType(string nick, Params.Shortcut.ShowType showType)
            {
                PersistentParams.TrySaveParam2Key<Params.Shortcut.ShowType>("Shortcut.ShowType", nick, showType);
            }
        }

        public class Robot
        {
            public const bool BanRobotForTest2020_1_7 = false;
            public static bool CanUseRobot;
            public const int AutoModeBringForegroundIntervalSecond = 5;
            public const int AutoModeCloseUnAnsweredBuyerIntervalSecond = 10;
            public const bool RuleIncludeExceptDefault = false;
            public const int AutoModeReplyDelaySecDefault = 0;
            public const int SendModeReplyDelaySecDefault = 0;
            public const bool QuoteModeSendAnswerWhenFullMatchDefault = false;
            public const double AutoModeAnswerMiniScore = 0.5;
            public const double QuoteOrSendModeAnswerMiniScore = 0.5;
            public const bool CancelAutoOnResetDefault = true;
            public const string AutoModeNoAnswerTipDefault = "亲,目前是机器人值班.这个问题机器人无法回答,等人工客服回来后再回复您.";
            public enum OperationEnum
            {
                None,
                Auto,
                Send,
                Quote
            }
            static Robot()
            {
                Params.Robot.CanUseRobot = true;
            }

            public static bool CanUseRobotReal
            {
                get
                {
                    return Params.Robot.CanUseRobot;
                }
            }

            public static bool IsBanJsInjector
            {
                get
                {
                    return PersistentParams.GetParam("IsBanJsInjector", true);
                }
                set
                {
                    PersistentParams.TrySaveParam("IsBanJsInjector", value);
                }
            }

            public static bool GetRuleIncludeExcept(string nick)
            {
                return PersistentParams.GetParam2Key<bool>("RuleIncludeExcept", AccountHelper.GetPubDbAccount(nick), false);
            }

            public static void SetRuleIncludeExcept(string nick, bool ruleIncludeExcept)
            {
                PersistentParams.TrySaveParam2Key<bool>("RuleIncludeExcept", AccountHelper.GetPubDbAccount(nick), ruleIncludeExcept);
            }


            public static int GetAutoModeReplyDelaySec(string nick)
            {
                return PersistentParams.GetParam2Key<int>("AutoModeReplyDelaySec", nick, 0);
            }

            public static void SetAutoModeReplyDelaySec(string nick, int autoModeReplyDelaySec)
            {
                PersistentParams.TrySaveParam2Key<int>("AutoModeReplyDelaySec", nick, autoModeReplyDelaySec);
            }

            public static int GetSendModeReplyDelaySec(string nick)
            {
                return PersistentParams.GetParam2Key<int>("SendModeReplyDelaySec", nick, 0);
            }

            public static void SetSendModeReplyDelaySec(string nick, int sendModeReplyDelaySec)
            {
                PersistentParams.TrySaveParam2Key<int>("SendModeReplyDelaySec", nick, sendModeReplyDelaySec);
            }

            public static bool GetQuoteModeSendAnswerWhenFullMatch(string nick)
            {
                return PersistentParams.GetParam2Key<bool>("QuoteModeSendAnswerWhenFullMatch", nick, false);
            }

            public static void SetQuoteModeSendAnswerWhenFullMatch(string nick, bool quoteModeSendAnswerWhenFullMatch)
            {
                PersistentParams.TrySaveParam2Key<bool>("QuoteModeSendAnswerWhenFullMatch", nick, quoteModeSendAnswerWhenFullMatch);
            }

            public static bool CancelAutoOnReset
            {
                get
                {
                    return PersistentParams.GetParam("CancelAutoOnReset", true);
                }
                set
                {
                    PersistentParams.TrySaveParam("CancelAutoOnReset", value);
                }
            }

            public static Params.Robot.OperationEnum GetOperation(string nick)
            {
                return PersistentParams.GetParam2Key<Params.Robot.OperationEnum>("Robot.Operation", nick, Params.Robot.OperationEnum.None);
               
            }

            public static void SetOperation(string nick, Params.Robot.OperationEnum operation)
            {
                PersistentParams.GetParam2Key<Params.Robot.OperationEnum>("Robot.Operation", nick, operation);
            }

            public static string GetAutoModeNoAnswerTip(string nick)
            {
                return PersistentParams.GetParam2Key<string>("Robot.AutoModeNoAnswerTip", nick, "亲,目前是机器人值班.这个问题机器人无法回答,等人工客服回来后再回复您.");
            }

            public static void SetAutoModeNoAnswerTip(string nick, string autoModeNoAnswerTip)
            {
                PersistentParams.GetParam2Key<string>("Robot.AutoModeNoAnswerTip", nick, autoModeNoAnswerTip);
            }

        }

        public class HotKey
        {
            public static bool GetHotKey(HotKeyHelper.HotOp op, out Keys k)
            {
                k = Keys.BrowserFavorites;
                bool rt = false;
                string defv = "";
                switch (op)
                {
                    case HotKeyHelper.HotOp.QinKong:
                        defv = "327755";
                        break;
                    case HotKeyHelper.HotOp.ZhiSi:
                        defv = "327757";
                        break;
                }
                string hotOp = PersistentParams.GetParam("HotOp" + ((int)op).ToString(), defv);
                if (hotOp != "" && hotOp != "171")
                {
                    try
                    {
                        k = (Keys)Convert.ToUInt32(hotOp);
                        rt = true;
                    }
                    catch (Exception e)
                    {
                        Log.Exception(e);
                    }
                }
                return rt;
            }

            public static void SaveHotKey(HotKeyHelper.HotOp hotOp, string value)
            {
                Keys keys = Keys.BrowserFavorites;
                if (value == "无")
                {
                    Params.HotKey.SaveHotKey(hotOp, true, keys);
                }
                else
                {
                    try
                    {
                        keys = (Keys)Convert.ToUInt32(value);
                    }
                    catch (Exception e)
                    {
                        Log.Exception(e);
                    }
                    Params.HotKey.SaveHotKey(hotOp, false, keys);
                }
            }

            public static void SaveHotKey(HotKeyHelper.HotOp op, bool isclear, Keys keys)
            {
                PersistentParams.TrySaveParam("HotOp" + ((int)op).ToString(), isclear ? "" : ((uint)keys).ToString());
            }

            public const string HotKeyQingKongDefault = "327755";

            public const string HotKeyZhiSiDefault = "327757";
        }

        public class InputSuggestion
        {
            static InputSuggestion()
            {
                Params.InputSuggestion.SourceTypeDefault = Params.InputSuggestion.SourceTypeEnum.All;
            }

            public static int BanUseDownKeyCount
            {
                get
                {
                    return PersistentParams.GetParam("InputSuggestion.BanUseDownKeyCount", 0);
                }
                set
                {
                    PersistentParams.TrySaveParam("InputSuggestion.BanUseDownKeyCount", value);
                }
            }

            public static bool IsUseDownKey
            {
                get
                {
                    return PersistentParams.GetParam("InputSuggestion.IsUseDownKey", true);
                }
                set
                {
                    PersistentParams.TrySaveParam("InputSuggestion.IsUseDownKey", value);
                }
            }

            public static bool IsUseLearnedAnswer
            {
                get
                {
                    Params.InputSuggestion.SourceTypeEnum sourceType = Params.InputSuggestion.SourceType;
                    return sourceType == Params.InputSuggestion.SourceTypeEnum.All;
                }
            }

            public static bool IsUseRuleAnswer
            {
                get
                {
                    Params.InputSuggestion.SourceTypeEnum sourceType = Params.InputSuggestion.SourceType;
                    return sourceType == Params.InputSuggestion.SourceTypeEnum.All || sourceType == Params.InputSuggestion.SourceTypeEnum.ScAndRuleAnswer;
                }
            }

            public static bool IsUseShortcut
            {
                get
                {
                    Params.InputSuggestion.SourceTypeEnum sourceType = Params.InputSuggestion.SourceType;
                    return sourceType == Params.InputSuggestion.SourceTypeEnum.All || sourceType == Params.InputSuggestion.SourceTypeEnum.ScAndRuleAnswer || sourceType == Params.InputSuggestion.SourceTypeEnum.Shortcut;
                }
            }

            public static Params.InputSuggestion.SourceTypeEnum SourceType
            {
                get
                {
                    Params.InputSuggestion.SourceTypeEnum sourceTypeEnum = PersistentParams.GetParam<Params.InputSuggestion.SourceTypeEnum>("InputSuggestion.SourceType", Params.InputSuggestion.SourceTypeDefault);
                    if (sourceTypeEnum == Params.InputSuggestion.SourceTypeEnum.Unknown)
                    {
                        sourceTypeEnum = Params.InputSuggestion.SourceTypeEnum.All;
                    }
                    return sourceTypeEnum;
                }
                set
                {
                    PersistentParams.TrySaveParam<Params.InputSuggestion.SourceTypeEnum>("InputSuggestion.SourceType", value);
                }
            }

            public static bool UseSingleQuote
            {
                get
                {
                    return true;
                }
                set
                {
                    PersistentParams.TrySaveParam("UseSingleQuote", value);
                }
            }
            public const bool IsUseDownKeyDefault = true;

            public static Params.InputSuggestion.SourceTypeEnum SourceTypeDefault;

            public const bool UseSingleQuoteDefault = false;

            public enum SourceTypeEnum
            {
                Unknown,
                Shortcut,
                ScAndRuleAnswer,
                All
            }
        }

        public static int BottomPanelAnswerCount { get; set; }

        public static bool NeedTipReSynDataOk
        {
            get
            {
                return PersistentParams.GetParam("NeedTipReSynDataOk", false);
            }
            set
            {
                PersistentParams.TrySaveParam("NeedTipReSynDataOk", value);
            }
        }

    }

}
