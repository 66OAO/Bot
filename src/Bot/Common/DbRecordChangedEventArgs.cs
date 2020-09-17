using DbEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Common
{
    public class DbRecordChangedEventArgs<T> : EventArgs where T : EntityBase
    {
        public T Entity { get; private set; }
        public EntityChangedType ChangedType { get; private set; }
        public T PreState { get; private set; }
        public DbRecordChangedEventArgs(T newData, T oldData)
		{
			this.Entity = newData;
			this.PreState = oldData;
			if (newData.IsDeleted)
			{
				this.ChangedType = EntityChangedType.Delete;
			}
			else if (oldData == null)
			{
				this.ChangedType = EntityChangedType.New;
			}
			else
			{
				this.ChangedType = EntityChangedType.Modify;
			}
		}
    }

    public class DbRecordChangedEventArgs : DbRecordChangedEventArgs<EntityBase>
    {
        public DbRecordChangedEventArgs(EntityBase EntityBase_0, EntityBase EntityBase_1)
			: base(EntityBase_0, EntityBase_1)
		{
		}
    }

    public enum EntityChangedType
    {
        Unknown,
        New,
        Delete,
        Modify
    }

}
