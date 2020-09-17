using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using BotLib.Db.Sqlite;
using BotLib.Extensions;
using Bot;
using BotLib;
using System.Linq;
using DbEntity;

namespace Bot.Common
{
    public class DbHelper
    {
        public static SQLiteHelper Db;
        public static EventHandler<DbRecordChangedEventArgs> EvSaveRecord;
        private static List<Type> DbEntityTypeList;

        static DbHelper()
        {
            var dbPath = PathEx.DataDir + "bot.db";
            var tableTypes = new List<Type>();
            tableTypes.Add(typeof(HybridEntity));
            tableTypes.Add(typeof(ShortcutEntity));
            tableTypes.Add(typeof(ShortcutCatalogEntity));
            tableTypes.Add(typeof(BuyerNoteEntity));
            tableTypes.Add(typeof(FavoriteNoteEntity));
            tableTypes.Add(typeof(OptionEntity));
            DbEntityTypeList = tableTypes;
            Db = new SQLiteHelper(dbPath, tableTypes);
            DbTable.InitTableType(tableTypes);
        }

        public static long GetMaxModifyTick()
        {
            return DbTable.GetMaxModifyTick();
        }

        public static List<EntityBase> Fetch(string dbAccount, long startTicks, long endTicks)
        {
            return DbTable.Fetch(dbAccount, startTicks, endTicks);
        }

        public static void Fetch<T>(Action<T> act, string dbAccount = null) where T : EntityBase
        {
            DbHelper.DbTable db = DbHelper.DbTable.AddTableType(typeof(T));
            db.Fetch<T>(act, dbAccount);
        }

        public static void Delete<T>(T et) where T : EntityBase
        {
            var db = DbTable.AddTableType(typeof(T));
            db.Delete<T>(et);
        }

        public static void Delete<T>(List<T> ets) where T : EntityBase
        {
            var db = DbTable.AddTableType(typeof(T));
            db.Delete<T>(ets);
        }

        public static List<T> Fetch<T>(Type t, string dbAccount, Predicate<T> pred) where T : EntityBase
        {
            var db = DbTable.AddTableType(t);
            return db.Fetch<T>(dbAccount, pred);
        }

        public static List<EntityBase> Fetch<T>()
        {
            var db = DbTable.AddTableType(typeof(T));
            return db.Fetch(false);
        }

        public static List<T> Fetch<T>(string dbAccount, Predicate<T> pred = null) where T : EntityBase
        {
            var db = DbTable.AddTableType(typeof(T));
            return db.Fetch<T>(dbAccount, pred);
        }

        public static List<EntityBase> Fetch(string dbAccount, Type ty, Predicate<EntityBase> pred = null)
        {
            var db = DbTable.AddTableType(ty);
            return db.Fetch<EntityBase>(dbAccount, pred);
        }

        public static T FirstOrDefault<T>(string dbAccount, Predicate<T> pred) where T : EntityBase
        {
            var db = DbTable.AddTableType(typeof(T));
            return db.FirstOrDefault<T>(dbAccount, pred, false);
        }


        public static int Count<DataType>(Predicate<DataType> pred = null, string dbAccount = null) where DataType : EntityBase
        {
            var db = DbTable.AddTableType(typeof(DataType));
            return db.Count<DataType>(pred, dbAccount);
        }

        public static EntityBase FirstOrDefault(Type t, string dbAccount, Predicate<EntityBase> pred)
        {
            var db = DbTable.AddTableType(t);
            return db.FirstOrDefault<EntityBase>(dbAccount, pred, false);
        }

        public static T FirstOrDefault<T>(Type t, string dbAccount, Predicate<T> pred) where T : EntityBase
        {
            var db = DbTable.AddTableType(t);
            return db.FirstOrDefault<T>(dbAccount, pred, false);
        }

        public static EntityBase FirstOrDefault(Type t, string dbAccount, string entityId)
        {
            var db = DbTable.AddTableType(t);
            return db.FirstOrDefault<EntityBase>(dbAccount, entityId, false);
        }

        public static T FirstOrDefault<T>(string entityId, string dbAccount = null) where T : EntityBase
        {
            var db = DbTable.AddTableType(typeof(T));
            return string.IsNullOrEmpty(dbAccount) ?
                db.FirstOrDefault<T>(entityId, false) : db.FirstOrDefault<T>(dbAccount, entityId, false);
        }

        private static void ClearDataForDelete(EntityBase et)
        {
            string entityId = et.EntityId;
            string dbAccount = et.DbAccount;
            bool isDeleted = et.IsDeleted;
            long modifyTick = et.ModifyTick;
            try
            {
                var props = et.GetType().GetProperties(BindingFlags.Public);
                for (int i = 0; i < props.Length; i++)
                {
                    var prop = props[i];
                    var t = prop.GetType();
                    prop.SetValue(et, TypeEx.xGetDefaultValue(t), null);
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
            et.EntityId = entityId;
            et.DbAccount = dbAccount;
            et.IsDeleted = isDeleted;
            et.ModifyTick = modifyTick;
        }

        private static void DeleteDbAndBackUpIfNeed(string src)
        {
            if (Params.NeedClearDb)
            {
                try
                {
                    var dbnewFn = PathEx.AppendBackupTime(src);
                    FileEx.ReName(src, dbnewFn);
                    Log.Info(string.Format("删除数据库:{0},\r\n并备份为:{1}", src, dbnewFn));
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
                Params.NeedClearDb = false;
            }
        }

        private static void PhisicalDeleteAccountRec(string dbAccount)
        {
            try
            {
                foreach (var t in DbEntityTypeList)
                {
                    var dbTable = DbHelper.DbTable.AddTableType(t);
                    dbTable.Delete(dbAccount);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public static void PhisicalDeleteAccountsRec(List<string> dbAccs)
        {
            try
            {
                foreach (string dbAccount in dbAccs)
                {
                    DbHelper.PhisicalDeleteAccountRec(dbAccount);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public static void SaveToDb(EntityBase et, bool isModify = true)
        {
            if (et != null)
            {
                if (isModify)
                {
                    et.ModifyTick = DateTime.Now.Ticks;
                }
                if (et.IsDeleted)
                {
                    DbHelper.ClearDataForDelete(et);
                }
                DbHelper.Db.SaveOneRecord(et);
                DbTable.AddTableType(et.GetType()).AddOrUpdateCache(et);
            }
        }

        public static void BatchSaveOrUpdateToDb(params EntityBase[] lst)
        {
            DbHelper.SaveToDbInTransaction(true, lst);
        }

        public static void BatchSaveToDb(params EntityBase[] lst)
        {
            DbHelper.SaveToDbInTransaction(false, lst);
        }

        public static void BatchSaveToDb(IEnumerable<EntityBase> lst)
        {
            DbHelper.SaveToDbInTransaction(false, lst);
        }

        public static void BatchSaveToDb(IEnumerable<string> serializeLst)
        {
            if (serializeLst != null)
            {
                //var ets = SynDownloadEntity.DeserializeEntities(serializeLst);
                //if (ets.xCount() > 0)
                //{
                //    DbHelper.BatchSaveToDb(ets);
                //}
            }
        }

        private static void SaveToDbInTransaction(bool isModify, IEnumerable<EntityBase> lst)
        {
            if (!lst.xIsNullOrEmpty())
            {
                var ets = new List<object>();
                foreach (var et in lst)
                {
                    if (et != null)
                    {
                        if (isModify)
                        {
                            et.ModifyTick = DateTime.Now.Ticks;
                        }
                        if (et.IsDeleted)
                        {
                            DbHelper.ClearDataForDelete(et);
                        }
                        ets.Add(et);
                    }
                }
                DbHelper.Db.SaveRecordsInTransaction(ets);
                var dictionary = new Dictionary<EntityBase, EntityBase>();
                foreach (var obj in ets)
                {
                    var newEt = obj as EntityBase;
                    var oldEt = DbHelper.DbTable.AddTableType(newEt.GetType()).AddOrUpdateCache(newEt);
                    dictionary[newEt] = oldEt;
                }
                foreach (var obj in ets)
                {
                    var newEt = obj as EntityBase;
                    var oldEt = dictionary[newEt];
                    DbHelper.DbRecordChanged(newEt, oldEt);
                }
            }
        }

        private static void DbRecordChanged(EntityBase newEt, EntityBase oldEt)
        {
            if (EvSaveRecord != null)
            {
                EvSaveRecord("DbHelper", new DbRecordChangedEventArgs(newEt, oldEt));
            }
        }


        private class DbTable
        {
            private static Dictionary<string, DbTable> _tableDict;
            private Type _t;
            private ConcurrentDictionary<string, ConcurrentDictionary<string, EntityBase>> _dict;
            static DbTable()
            {
                _tableDict = new Dictionary<string, DbTable>();
            }
            public static void InitTableType(List<Type> tableTypes)
            {
                foreach (Type tbType in tableTypes)
                {
                    DbTable.AddTableType(tbType);
                }
            }

            public static DbTable AddTableType(Type t)
            {
                var assemblyQualifiedName = t.AssemblyQualifiedName;
                if (!DbTable._tableDict.ContainsKey(assemblyQualifiedName))
                {
                    DbTable._tableDict[assemblyQualifiedName] = new DbTable(t);
                }
                return DbTable._tableDict[assemblyQualifiedName];
            }

            public DbTable(Type t)
            {
                this._dict = new ConcurrentDictionary<string, ConcurrentDictionary<string, EntityBase>>();
                this._t = t;
                this.ReadAllRecordFromDbAndDeleteTooOldDeletedRec();
            }

            private void ReadAllRecordFromDbAndDeleteTooOldDeletedRec()
            {
                var ds = DbHelper.Db.ReadTable(this._t);
                List<EntityBase> ets = null;
                if (ds != null)
                {
                    ets = ds.ConvertAll<EntityBase>(k => k as EntityBase);
                }
                if (ets != null)
                {
                    try
                    {
                        var delDatas = new List<object>();
                        var delDict = new Dictionary<string, bool>();
                        bool isOk = BatTime.IsOk;
                        foreach (var et in ets)
                        {
                            if (isOk && et.IsDeleted && this.DeleteIfNeed(et, delDict))
                            {
                                delDatas.Add(et);
                            }

                            if (!et.IsDeleted)
                            {
                                et.SetReadOnly(true);
                                if (!this._dict.ContainsKey(et.DbAccount))
                                {
                                    this._dict[et.DbAccount] = new ConcurrentDictionary<string, EntityBase>();
                                }
                                this._dict[et.DbAccount][et.EntityId] = et;
                            }
                        }
                        int delCnt = DbHelper.Db.DeleteInTransaction(delDatas);
                        if (delCnt > 0)
                        {
                            Log.Info("物理删除记录数=" + delCnt);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Exception(ex);
                    }
                }
            }

            private bool DeleteIfNeed(EntityBase et, Dictionary<string, bool> delDict)
            {
                bool rt = false;
                bool needDel = false;
                if (!delDict.ContainsKey(et.DbAccount))
                {
                    DateTime dateTime = Params.GetLatestCheckDbDeleteTime(et.DbAccount);
                    needDel = (BatTime.Diff(Params.GetLatestSynOkTime(et.DbAccount)).TotalDays < 1.0 && (BatTime.Diff(dateTime).TotalDays > 2.0 || BatTime.Diff(dateTime).TotalMinutes < 10.0));
                    delDict[et.DbAccount] = needDel;
                    Params.SaveLatestCheckDbDeleteTime(et.DbAccount);
                }
                else
                {
                    needDel = delDict[et.DbAccount];
                }
                if (needDel)
                {
                    DateTime d = new DateTime(et.ModifyTick);
                    rt = ((BatTime.Now - d).TotalDays > 3.0);
                }
                return rt;
            }

            public static List<EntityBase> Fetch(string dbAccount, long startTicks, long endTicks)
            {
                var ets = new List<EntityBase>();
                foreach (var table in DbTable._tableDict)
                {
                    table.Value.Fetch(dbAccount, startTicks, endTicks, ets);
                }
                return ets;
            }

            public static long GetMaxModifyTick()
            {
                long maxTick = long.MinValue;
                try
                {
                    foreach (var tk in DbTable._tableDict)
                    {
                        if (tk.Value.MaxModifyTick() > maxTick)
                        {
                            maxTick = tk.Value.MaxModifyTick();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
                return maxTick;
            }

            private long MaxModifyTick()
            {
                long tick = long.MinValue;
                foreach (var kv in _dict.xSafeForEach())
                {
                    if (kv.Value != null && kv.Value.Values != null)
                    {
                        foreach (var et in kv.Value.Values)
                        {
                            if (et.ModifyTick > tick)
                            {
                                tick = et.ModifyTick;
                            }
                        }
                    }
                }
                return tick;
            }

            private List<EntityBase> Fetch(string dbAccount, long startTicks, long endTicks, List<EntityBase> rtList)
            {
                if (this._dict.ContainsKey(dbAccount))
                {
                    foreach (var et in this._dict[dbAccount])
                    {
                        long modifyTick = et.Value.ModifyTick;
                        if (modifyTick > startTicks && modifyTick <= endTicks)
                        {
                            rtList.Add(et.Value);
                        }
                    }
                }
                return rtList;
            }

            public void Fetch<T>(Action<T> act, string dbAccount) where T : EntityBase
            {
                if (!string.IsNullOrEmpty(dbAccount))
                {
                    if (!this._dict.ContainsKey(dbAccount)) return;
                    foreach (var et in this._dict[dbAccount].Values)
                    {
                        act((T)et);
                    }
                }
                else
                {
                    foreach (var kv in this._dict)
                    {
                        foreach (var et in kv.Value.Values)
                        {
                            act((T)et);
                        }
                    }
                }
            }

            public List<T> Fetch<T>(string dbAccount, Predicate<T> pred) where T : EntityBase
            {
                if (!this._dict.ContainsKey(dbAccount)) return new List<T>();
                var ets = this._dict[dbAccount].Select(k => k.Value.Clone<T>(false)).ToList();
                if (pred != null)
                {
                    ets = ets.Where(k => pred(k) && !k.IsDeleted).ToList();
                }
                return ets;
            }

            public int Count<DataType>(Predicate<DataType> pred, string dbAccount = null) where DataType : EntityBase
            {
                var rt = 0;
                this.Fetch<DataType>((k) =>
                {
                    if(pred(k))
                        rt += (k == null ? 0 : 1);
                }, dbAccount);
                return rt;
            }

            public T FirstOrDefault<T>(string dbAccount, Predicate<T> pred, bool notDelete, bool isreadonly = false) where T : EntityBase
            {
                if (!this._dict.ContainsKey(dbAccount)) return default(T);
                var et = this._dict[dbAccount].Select(k => k.Value as T).FirstOrDefault(k => pred(k));
                return et == null ? default(T) : et.Clone<T>(false);

            }

            public T FirstOrDefault<T>(string dbAccount, string entityId, bool notDelete, bool isreadonly = false) where T : EntityBase
            {
                T rt = default(T);
                if (this._dict.ContainsKey(dbAccount) && this._dict[dbAccount].ContainsKey(entityId))
                {
                    T t = (T)this._dict[dbAccount][entityId];
                    if (!t.IsDeleted)
                    {
                        rt = (isreadonly ? t : t.Clone<T>(false));
                        return rt;
                    }
                }
                return rt;
            }

            public T FirstOrDefault<T>(string entityId, bool notDelete, bool isreadonly = false) where T : EntityBase
            {
                foreach (var current in this._dict)
                {
                    var value = current.Value;
                    if (value.ContainsKey(entityId))
                    {
                        T t = (T)((object)value[entityId]);
                        if (!t.IsDeleted)
                        {
                            t = (isreadonly ? t : t.Clone<T>(false));
                            return t;
                        }
                    }
                }
                return default(T);
            }

            public EntityBase AddOrUpdateCache(EntityBase et)
            {
                et = et.Clone(false);
                if (!this._dict.ContainsKey(et.DbAccount))
                {
                    this._dict[et.DbAccount] = new ConcurrentDictionary<string, EntityBase>();
                }

                EntityBase oldEt = null;
                if (this._dict[et.DbAccount].ContainsKey(et.EntityId))
                {
                    oldEt = this._dict[et.DbAccount][et.EntityId];
                }
                this._dict[et.DbAccount][et.EntityId] = et;
                return oldEt;
            }

            public List<EntityBase> Fetch(bool isreadonly = false)
            {
                var ets = new List<EntityBase>();
                foreach (var current in this._dict.xSafeForEach())
                {
                    ets.AddRange(current.Value.Values.Where(k => !k.IsDeleted));
                }
                if (!isreadonly)
                {
                    ets = ets.Select(k => k.Clone<EntityBase>(isreadonly)).ToList();
                }
                return ets;
            }

            public void Delete(string dbAccount)
            {
                if (this._dict.ContainsKey(dbAccount))
                {
                    var data = this._dict[dbAccount];
                    if (!data.xIsNullOrEmpty())
                    {
                        this._dict[dbAccount] = new ConcurrentDictionary<string, EntityBase>();
                        List<object> olist = data.Values.ToList<EntityBase>().ConvertAll<object>((EntityBase x) => x);
                        DbHelper.Db.DeleteInTransaction(olist);
                    }
                }
            }

            public void Delete<T>(T et) where T : EntityBase
            {
                DbHelper.Db.Delete(et);
            }

            public void Delete<T>(List<T> ets) where T : EntityBase
            {
                DbHelper.Db.DeleteInTransaction(ets.ConvertAll<object>(k => k));
            }

        }
    }
}
