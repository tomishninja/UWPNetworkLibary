using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomsUWPNetworkingLibrary
{
    public abstract class NetworkClient : NetworkingObject
    {
        public readonly string portNumber;

        public readonly string ipaddress;

        public NetworkClient(string ipaddress, string portNumber)
        {
            this.portNumber = portNumber;
            this.ipaddress = ipaddress;
        }

        public abstract Task SendAsync(string message);

        public abstract Task Connect();
    }
}
