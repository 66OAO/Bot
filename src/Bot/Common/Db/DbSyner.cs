using BotLib;
using BotLib.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BotLib.Extensions;
using Bot.Common.Account;
using BotLib.Db.Sqlite;
using Bot.Net.Api;
using Bot.AssistWindow;
using DbEntity;

namespace Bot.Common.Db
{
    public class DbSyner
    {
        private static NoReEnterTimer _timer;
        private static int _autoSynInterval;
        private static DateTime _preSynTime;
        private static object _synObj;
        private static ManualResetEventSlim _synWaiter;


        public static EventHandler<SynFinishedEventArgs> EvSynFinished;
        public static EventHandler EvHasShortcutDowned;
        public static EventHandler EvHasRobotRuleDowned;
        public class SynFinishedEventArgs : EventArgs
        {
            public bool IsOk
            {
                get
                {
                    return string.IsNullOrEmpty(this.Error);
                }
            }
            public string Error;
        }

        public static bool IsSyning
        {
            get
            {
                return !_synWaiter.IsSet;
            }
        }

        static DbSyner()
        {
            _autoSynInterval = 600000;
            _preSynTime = DateTime.MinValue;
            _synObj = new object();
            _synWaiter = new ManualResetEventSlim(true);
        }

        public static void LoopSyn()
        {
            if (_timer == null)
            {
                _timer = new NoReEnterTimer(() =>
                {
                    if (_preSynTime.xIsTimeElapseMoreThanMs((int)(_autoSynInterval * 0.6)))
                    {
                        _preSynTime = DateTime.Now;
                        Syn(false, false, false);
                    }
                }, _autoSynInterval / 2, 500);
            }
        }

        public static void SynData(bool mustSyn = false)
		{
			Task.Factory.StartNew(()=>{
                if (mustSyn) Syn(mustSyn);
                else Syn(false,false,false);
            }, TaskCreationOptions.LongRunning);
		}

        public static void Syn(bool mustSyn = false)
        {
            if (IsSyning)
            {
                Log.Info("SynOrWaitSynFinished,wait start.");
                _synWaiter.Wait();
                Log.Info("SynOrWaitSynFinished,wait end.");
                if (mustSyn)
                {
                    Syn(false, false, false);
                }
            }
            else
            {
                Syn(false, false, false);
            }
        }

        private static async void Syn(bool isCallByRecursive = false, bool hasShortcutEver = false, bool hasRobotRuleEver = false)
        {
	        try
	        {
                lock (_synObj)
		        {
			        if (!isCallByRecursive && IsSyning)
			        {
				        return;
			        }
			        _synWaiter.Reset();
		        }
		        if (!isCallByRecursive)
		        {
			        _preSynTime = DateTime.Now;
		        }
		        Dictionary<string, long> dictEndTicks;
		        var upDatas = UploadDataProducer.GetUploadData(out dictEndTicks);
		        if (!upDatas.xIsNullOrEmpty())
		        {
			        var downDatas = await BotApi.TransferData(upDatas);
			        if (downDatas == null )
			        {
				        throw new Exception("同步失败");
			        }
			        LogSyncDatas(upDatas, downDatas);
			        SavePreUploadSynTick(dictEndTicks);
			        SaveToDb(downDatas, ref hasShortcutEver, ref hasRobotRuleEver);
			        if (HasNewData(upDatas, downDatas))
			        {
				        Syn(true, hasShortcutEver, hasRobotRuleEver);
			        }
			        else
			        {
				        _synWaiter.Set();
				        SyncFinished(null);
				        if (hasShortcutEver)
				        {
					        RaiseEventHasShortcutDowned();
				        }
				        if (hasRobotRuleEver)
				        {
					        RaiseEventHasRobotRuleDowned();
				        }
				        foreach (var synUploadEntity in upDatas.xSafeForEach<SynUploadEntity>())
				        {
                            Params.SetLatestSynOkTime(synUploadEntity.DbAccount);
				        }
				        if (Params.NeedTipReSynDataOk)
				        {
                            MsgBox.ShowTip("已重新下载全部数据", "提示");
					        Params.NeedTipReSynDataOk = false;
				        }
			        }
		        }
		        else
		        {
			        _synWaiter.Set();
		        }
	        }
	        catch (Exception ex)
	        {
		        _synWaiter.Set();
		        SyncFinished(ex.Message);
		        Log.Error("同步数据出错，err=" + ex.Message);
	        }
        }

        private static bool HasNewData(List<SynUploadEntity> upDatas, List<SynDownloadEntity> downDatas)
        {
            int upCount = 0;
            if (!upDatas.xIsNullOrEmpty())
            {
                upCount = upDatas.Sum(k => k.DataList.Count);
            }
            int downCount =0;
            if (!downDatas.xIsNullOrEmpty())
            {
                downCount = downDatas.Sum(k => k.DataList.Count);
            }
            return upCount >= 1000 || downCount >= 1000;
        }

        private static void SyncFinished(string err = null)
        {
            WndAssist.UpdateAllBuyerNote();
            if (EvSynFinished != null)
            {
                EvSynFinished(null, new SynFinishedEventArgs
                {
                    Error = err
                });
            }
        }

        private static void LogSyncDatas(List<SynUploadEntity> upDatas, List<SynDownloadEntity> downDatas)
        {
            int upCount = 0;
            foreach (var synUploadEntity in upDatas.xSafeForEach())
            {
                upCount += synUploadEntity.DataList.xCount();
            }
            int downCount = 0;
            foreach (var synDownloadEntity in downDatas)
            {
                downCount += synDownloadEntity.DataList.xCount();
            }
            Log.Info(string.Format("同步数据，上传={0},下载={1}", upCount, downCount));
        }

        private static void SavePreUploadSynTick(Dictionary<string, long> dictTicks)
        {
            foreach (var kv in dictTicks)
            {
                SynParams.SetPreUploadSynTick(kv.Key, kv.Value);
            }
        }

        private static void SaveToDb(List<SynDownloadEntity> downDatas, ref bool hasShortcutEver, ref bool hasRobotRuleEver)
        {
            if (downDatas != null)
            {
                foreach (var dt in downDatas)
                {
                    hasShortcutEver = dt.DataList.Any(k => k is ShortcutEntity || k is ShortcutCatalogEntity);
                    DbHelper.BatchSaveToDb(dt.DataList);
                    SynParams.SetPreServerSynTick(dt.DbAccount, dt.ServerSynTick);
                }
            }
        }

        private static void RaiseEventHasShortcutDowned()
        {
            try
            {
                if (EvHasShortcutDowned != null)
                {
                    EvHasShortcutDowned(null, null);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        private static void RaiseEventHasRobotRuleDowned()
        {
            try
            {
                if (EvHasRobotRuleDowned != null)
                {
                    EvHasRobotRuleDowned(null, null);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }


        public class SynParams
        {
            private static bool _hadLogSetPreClientUploadSynTick;
            static SynParams()
            {
                SynParams._hadLogSetPreClientUploadSynTick = false;
            }

            public static long PreUploadSynTick(string dbAccount)
            {
                return PersistentParams.GetParam2Key("PreUploadSynTick", dbAccount, 0L);
            }

            public static void SetPreUploadSynTick(string dbAccount, long synTicks)
            {
                DateTime now = DateTime.Now;
                PersistentParams.TrySaveParam2Key("PreUploadSynTick", dbAccount, synTicks);
                if (!SynParams._hadLogSetPreClientUploadSynTick)
                {
                    SynParams._hadLogSetPreClientUploadSynTick = true;
                    Log.Info(string.Format("SetPreClientUploadSynTick,timeElapse={0}ms", now.xElapse().TotalMilliseconds));
                }
            }

            public static long PreServerSynTick(string dbAccount)
            {
                return PersistentParams.GetParam2Key("PreServerSynTick", dbAccount, 0L);
            }

            public static void SetPreServerSynTick(string dbAccount, long synTicks)
            {
                PersistentParams.TrySaveParam2Key("PreServerSynTick", dbAccount, synTicks);
            }

        }


        private class UploadDataProducer
        {
            public static List<SynUploadEntity> GetUploadData(out Dictionary<string, long> dictEndTicks)
            {
                dictEndTicks = new Dictionary<string, long>();
                var dbAccounts = AccountHelper.GetDbAccounts();
                var ets = new List<SynUploadEntity>();
                var maxCnt = 1000;
                int dtCnt = 0;
                foreach (string dbAccount in dbAccounts)
                {
                    long endTicks;
                    var uploadEt = UploadDataProducer.GetUploadData(dbAccount, maxCnt - dtCnt, out endTicks);
                    dictEndTicks[dbAccount] = endTicks;
                    dtCnt += uploadEt.DataList.Count;
                    ets.Add(uploadEt);
                    if (dtCnt >= maxCnt)
                    {
                        break;
                    }
                }
                return ets;
            }

            private static SynUploadEntity GetUploadData(string dbAccount, int dataCnt, out long endTicks)
            {
                long startTicks = SynParams.PreUploadSynTick(dbAccount);
                endTicks = DateTime.Now.Ticks;
                var dts = DbHelper.Fetch(dbAccount, startTicks, endTicks);
                if (dts.Count > dataCnt)
                {
                    dts = dts.OrderBy(k => k.ModifyTick).ToList();
                    endTicks = dts[dataCnt - 1].ModifyTick;
                    int idx = dataCnt;
                    while (idx < dts.Count && dts[idx].ModifyTick == endTicks)
                    {
                        idx++;
                    }
                    dts.xRemoveFromIndex(idx);
                }
                return new SynUploadEntity
                {
                    DataList = dts,
                    DbAccount = dbAccount,
                    ServerSynTick =SynParams.PreServerSynTick(dbAccount)
                };
            }
        }
    }
}
