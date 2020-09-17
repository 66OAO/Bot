using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Common.Trivial
{
    public class CommonEventArgs<T> : EventArgs
    {
        public T Data;
    }
}
