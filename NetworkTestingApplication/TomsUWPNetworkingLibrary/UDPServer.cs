using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace TomsUWPNetworkingLibrary
{
    public class UDPServer : NetworkServer
    {
        private Windows.Networking.Sockets.DatagramSocket socket = null;

        public UDPServer(string portNumber): base(portNumber)
        {
            socket = new Windows.Networking.Sockets.DatagramSocket();

            // The ConnectionReceived event is raised when connections are received.
            socket.MessageReceived += (sender, e) => ServerDatagramSocket_MessageReceived(sender, e, this.portNumber);
        }

        public override async Task StartAsync()
        {
            try
            {
                this.AddEntryToDebugLog("server is about to bind...");

                // Start listening for incoming TCP connections on the specified port. You can specify any port that's not currently in use.
                await socket.BindServiceNameAsync(this.portNumber);

                this.AddEntryToDebugLog("server is bound to port number");
            }
            catch (Exception ex)
            {
                Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
                this.AddEntryToDebugLog(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            }
        }

        private async void ServerDatagramSocket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs e, object serverPortNumber)
        {
            string request;
            using (Windows.Storage.Streams.DataReader dataReader = e.GetDataReader())
            {
                request = dataReader.ReadString(dataReader.UnconsumedBufferLength).Trim();
            }

            this.AddEntryToDebugLog("server received the request");
            this.AddEntryToMessageLog(request);

            // respond appropriately depending on the request
            await Responce(request, sender, e);

            this.AddEntryToDebugLog("server sent back the response");

            sender.Dispose();

            this.AddEntryToDebugLog("server closed its socket");
        }

        internal virtual async Task Responce(string request, DatagramSocket sender, DatagramSocketMessageReceivedEventArgs e)
        {
            // Echo the request back as the response.
            using (Stream outputStream = (await socket.GetOutputStreamAsync(e.RemoteAddress, portNumber)).AsStreamForWrite())
            {
                using (var streamWriter = new StreamWriter(outputStream))
                {
                    await streamWriter.WriteLineAsync(request);
                    await streamWriter.FlushAsync();
                }
            }
        }
    }
}
