using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomsUWPNetworkingLibrary
{
    public class TCPServer : NetworkServer
    {

        private Windows.Networking.Sockets.StreamSocketListener socket = null;

        public TCPServer(string portNumber) : base(portNumber)
        {
            socket = new Windows.Networking.Sockets.StreamSocketListener();

            // The ConnectionReceived event is raised when connections are received.
            socket.ConnectionReceived += this.StreamSocketListener_ConnectionReceived;
        }

        public override void Close()
        {
            socket.Dispose();

            this.AddEntryToDebugLog("server closed its socket");
        }

        public override async Task StartAsync()
        {
            try
            {

                // Start listening for incoming TCP connections on the specified port. You can specify any port that's not currently in use.
                await socket.BindServiceNameAsync(portNumber);

                this.AddEntryToDebugLog("server is listening...");
            }
            catch (Exception ex)
            {
                Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
                this.AddEntryToDebugLog(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            }
        }

        public async void StreamSocketListener_ConnectionReceived(Windows.Networking.Sockets.StreamSocketListener sender, Windows.Networking.Sockets.StreamSocketListenerConnectionReceivedEventArgs args)
        {
            string request;
            using (var streamReader = new StreamReader(args.Socket.InputStream.AsStreamForRead()))
            {
                request = await streamReader.ReadLineAsync();
            }

            this.AddEntryToDebugLog(string.Format("server received the request: \"{0}\"", request));

            // Echo the request back as the response.
            using (Stream outputStream = args.Socket.OutputStream.AsStreamForWrite())
            {
                using (var streamWriter = new StreamWriter(outputStream))
                {
                    await streamWriter.WriteLineAsync(request);
                    await streamWriter.FlushAsync();
                }
            }

            this.AddEntryToDebugLog("server sent back the response");

            this.Close();
        }
    }
}
