using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DbEntity
{
	public class SynDownloadEntity
	{
		public string DbAccount { get; set; }

        public List<EntityBase> DataList { get; set; }

		public long ServerSynTick { get; set; }

        //public List<EntityBase> DeserializeEntities()
        //{
        //    return SynDownloadEntity.DeserializeEntities(this.DataList);
        //}

        //public static List<EntityBase> DeserializeEntities(IEnumerable<string> jsons)
        //{
        //    List<EntityBase> list = new List<EntityBase>();
        //    foreach (string s in jsons)
        //    {
        //        EntityBase EntityBase = Newtonsoft.Json.JsonConvert.DeserializeObject<EntityBase>(s,
        //            new JsonSerializerSettings
        //            {
        //                TypeNameHandling = TypeNameHandling.All,
        //                DefaultValueHandling = DefaultValueHandling.Ignore,
        //                NullValueHandling = NullValueHandling.Ignore
        //            });
        //        list.Add(EntityBase);
        //    }
        //    return list;
        //}
	}
}
