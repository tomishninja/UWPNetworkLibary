using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomsUWPNetworkingLibrary
{

    
    public class UDPClient : NetworkClient
    {
    
        private Windows.Networking.Sockets.DatagramSocket socket = null;

        private Windows.Networking.HostName _hostName = null;

        public UDPClient(string ipaddress, string portNumber) : base(ipaddress, portNumber)
        {
            // Create the DatagramSocket and establish a connection to the echo server.
            socket = new Windows.Networking.Sockets.DatagramSocket();

            // set the listener
            socket.MessageReceived += ClientDatagramSocket_MessageReceived;

            // The server hostname that we will be establishing a connection to. In this example, the server and client are in the same process.
            _hostName = new Windows.Networking.HostName(this.ipaddress); //"10.160.99.69"
        }

        ~UDPClient()
        {
            socket.Dispose();
        }

        public void Close()
        {
            socket.Dispose();

            this.AddEntryToDebugLog("client closed its socket");
        }

        public override async Task Connect()
        {
            try
            {
                this.AddEntryToDebugLog("client is about to bind...");

                await socket.BindServiceNameAsync(portNumber);

                this.AddEntryToDebugLog(string.Format("client is bound to port number"));
            }
            catch (Exception ex)
            {
                this.AddEntryToDebugLog(ex.Message);
                Debug.Write(ex.Message);
            }
        }

        public override async Task SendAsync(string message)
        {
            try
            {
                // Send a request to the echo server.
                using (var serverDatagramSocket = new Windows.Networking.Sockets.DatagramSocket())
                {
                    using (Stream outputStream = (await serverDatagramSocket.GetOutputStreamAsync(_hostName, this.portNumber)).AsStreamForWrite())
                    {
                        using (var streamWriter = new StreamWriter(outputStream))
                        {
                            await streamWriter.WriteLineAsync(message);
                            await streamWriter.FlushAsync();
                        }
                    }
                }

                this.AddEntryToDebugLog("client sent the request");
                this.AddEntryToMessageLog(message);
            }
            catch (Exception ex)
            {
                Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
                this.AddEntryToDebugLog(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            }
        }
        
        private void ClientDatagramSocket_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender, Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
        {
            string response;
            using (Windows.Storage.Streams.DataReader dataReader = args.GetDataReader())
            {
                response = dataReader.ReadString(dataReader.UnconsumedBufferLength).Trim();
            }

            this.AddEntryToMessageLog(response);
            this.AddEntryToDebugLog("client received the response");
        }
    }
}
