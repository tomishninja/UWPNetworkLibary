using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomsUWPNetworkingLibrary
{
    public class TCPClient : NetworkClient
    {
        private Windows.Networking.HostName _hostName;
        private Windows.Networking.Sockets.StreamSocket socket;

        public TCPClient(string ipaddress, string portNumber) : base(ipaddress, portNumber)
        {
            // The server hostname that we will be establishing a connection to. In this example, the server and client are in the same process.
            _hostName = new Windows.Networking.HostName(ipaddress);//"10.160.98.35"

            socket = new Windows.Networking.Sockets.StreamSocket();
        }

        ~TCPClient()
        {
            socket.Dispose();
        }

        public override void Close()
        {
            socket.Dispose();

            this.AddEntryToDebugLog("client closed its socket");
        }

        public override async Task Connect()
        {
            try
            {
                this.AddEntryToDebugLog("client is trying to connect...");

                await socket.ConnectAsync(_hostName, portNumber);

                this.AddEntryToDebugLog("client connected");

            }
            catch (Exception ex)
            {
                Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
                this.AddEntryToDebugLog(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            }
        }

        public override async Task SendAsync(string message)
        {
            // Send a request to the echo server.
            for (int i = 0; i < 3; i++)
            {
                using (Stream outputStream = socket.OutputStream.AsStreamForWrite())
                {
                    using (var streamWriter = new StreamWriter(outputStream))
                    {
                        await streamWriter.WriteLineAsync(message);
                        await streamWriter.FlushAsync();
                    }
                }
            }

            this.AddEntryToDebugLog("client sent the request");

            await this.Read();
        }

        internal async virtual Task Read()
        {
            // Read data from the echo server.
            string response;
            using (Stream inputStream = socket.InputStream.AsStreamForRead())
            {
                using (StreamReader streamReader = new StreamReader(inputStream))
                {
                    response = await streamReader.ReadLineAsync();
                }
            }
            this.AddEntryToDebugLog("client received the response");
        }

    }
}
