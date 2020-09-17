using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MasterDevs.ChromeDevTools
{
    public class ErrorOrClosedEventArgs : EventArgs
    {
        public ErrorOrClosedEventArgs()
		{
		}

        public string Reason;
    }
}
