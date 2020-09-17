using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Collection
{
    public class Cache<KT, VT>
    {
        private ConcurrentDictionary<KT, Data> _dict = new ConcurrentDictionary<KT, Data>();
        private Action<VT> _disposeDealer;
        private int _minCacheSize;
        private int _maxCacheSize;
        private int _timeoutMs;
        private long accessCount = 0L;
        private long hitCount = 0L;
        private object _removingLazyKeySynObj = new object();
        private bool _isRemovingLazykey = false;

        public Cache(int size = 0, int timeoutMs = 0, Action<VT> disposeDealer = null)
        {
            if (size <= 0)
            {
                size = 1073741823;
            }
            this._disposeDealer = disposeDealer;
            this._minCacheSize = size;
            this._maxCacheSize = size * 2;
            this._timeoutMs = timeoutMs;
        }

        public VT this[KT key]
        {
            get
            {
                VT result;
                this.TryGetValue(key, out result, default(VT));
                return result;
            }
            set
            {
                this.AddOrUpdate(key, value);
            }
        }

        public bool ContainsKey(KT key)
        {
            return this._dict.ContainsKey(key);
        }

        public bool TryGetValue(KT key, out VT value, VT defv = default(VT))
        {
            this.accessCount += 1L;
            value = defv;
            bool rt = false;
            if (key != null)
            {
                Data data;
                if (_dict.TryGetValue(key, out data))
                {
                    if (data.IsTimeout(this._timeoutMs))
                    {
                        rt = false;
                        this.Remove(key);
                    }
                    else
                    {
                        value = data.TheData;
                        data.LastAccessTime = DateTime.Now;
                        this.hitCount += 1L;
                    }
                }
            }
            return rt;
        }

        public VT GetValue(KT key, Func<VT> producer, bool useCache = true, Predicate<VT> needCache = null)
        {
            VT vt = default(VT);
            this.accessCount += 1L;
            if (useCache && this.TryGetValue(key, out vt, default(VT)))
            {
                this.hitCount += 1L;
            }
            else
            {
                vt = producer();
                if (needCache == null || needCache(vt))
                {
                    this.AddOrUpdate(key, vt);
                }
            }
            return vt;
        }

        public bool IsCacheTimeElapseMoreThanMs(KT key, int timeoutMs)
        {
            bool rt = true;
            Data data;
            if (_dict.TryGetValue(key, out data))
            {
                rt = data.IsTimeout(timeoutMs);
            }
            return rt;
        }

        public void RemoveItems(Predicate<VT> predict)
        {
            _dict.Where(k => predict(k.Value.TheData)).ToList().ForEach(k => { Remove(k.Key); });
        }

        public double HitRate()
        {
            double rt;
            if (accessCount == 0L)
            {
                rt = 0.0;
            }
            else
            {
                rt = (double)this.hitCount / (double)this.accessCount;
            }
            return rt;
        }

        public void Clear()
        {
            var dict = this._dict;
            this._dict = new ConcurrentDictionary<KT, Data>();
            if (_disposeDealer != null)
            {
                foreach (KT key in dict.Keys)
                {
                    Data data;
                    if (dict.TryRemove(key, out data))
                    {
                        try
                        {
                            this._disposeDealer(data.TheData);
                        }
                        catch (Exception e)
                        {
                            Log.Exception(e);
                        }
                    }
                }
            }
        }

        public void AddOrUpdate(KT key, VT value)
        {
            if (key != null)
            {
                if (_dict.ContainsKey(key))
                {
                    this.DisposeData(this._dict[key].TheData);
                }
                else
                {
                    this.RemoveLazyKeyIfNeed();
                }
                this._dict[key] = new Data(value);
            }
        }

        private void DisposeData(VT v)
        {
            try
            {
                if (_disposeDealer != null)
                {
                    _disposeDealer(v);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public void Remove(KT key)
        {
            Data data;
            if (_dict.TryRemove(key, out data))
            {
                this.DisposeData(data.TheData);
            }
        }

        private void RemoveLazyKeyIfNeed()
        {
            bool isRemovingLazykey = this._isRemovingLazykey;
            if (!isRemovingLazykey)
            {
                lock (_removingLazyKeySynObj)
                {
                    _isRemovingLazykey = true;
                    if (_dict.Count > this._maxCacheSize)
                    {
                        try
                        {
                            var dict = this._dict;
                            this._dict = new ConcurrentDictionary<KT, Data>();
                            dict.OrderByDescending(k => k.Value.LastAccessTime)
                                .Skip(_minCacheSize).Select(k => k.Key).ToList<KT>()
                                .ForEach(k =>
                                {
                                    Data data;
                                    if (dict.TryRemove(k, out data) && this._disposeDealer != null)
                                    {
                                        try
                                        {
                                            this._disposeDealer(data.TheData);
                                        }
                                        catch (Exception e)
                                        {
                                            Log.Exception(e);
                                        }
                                    }
                                });
                            this._dict = dict;
                        }
                        catch (Exception e)
                        {
                            Log.Exception(e);
                        }
                    }
                    _isRemovingLazykey = false;
                }
            }
        }

        private class Data
        {
            public VT TheData;

            public DateTime LastAccessTime;

            public DateTime CacheTime;

            public Data(VT d)
            {
                this.TheData = d;
                this.LastAccessTime = DateTime.Now;
                this.CacheTime = this.LastAccessTime;
            }

            public void UpdateAccessTime()
            {
                this.LastAccessTime = DateTime.Now;
            }

            internal bool IsTimeout(int timeoutMs)
            {
                return timeoutMs > 0 && (DateTime.Now - this.CacheTime).TotalMilliseconds > (double)timeoutMs;
            }

        }
    }
}
