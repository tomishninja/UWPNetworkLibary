using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomsUWPNetworkingLibrary
{
    public abstract class NetworkServer : NetworkingObject
    {
        public readonly string portNumber;

        public NetworkServer(string portNumber)
        {
            this.portNumber = portNumber;
        }

        public abstract Task StartAsync();

        public abstract void Close();

    }
}
