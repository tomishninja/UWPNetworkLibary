using System;
using System.Collections.Generic;
using System.IO;

namespace NetworkTestingApplication
{
    class NetworkTestingAlgorithms
    {

        private static NetworkTestingAlgorithms Instance = null;

        /// <summary>
        /// remove access to the network
        /// </summary>
        private NetworkTestingAlgorithms() { }

        /// <summary>
        /// provides access to the class
        /// </summary>
        /// <returns>A instance of this object</returns>
        public static NetworkTestingAlgorithms GetInstance()
        {
            if (Instance == null)
            {
                Instance = new NetworkTestingAlgorithms();
            }
            return Instance;
        }

        /// <summary>
        /// 
        /// </summary>
        private List<string> ConsoleLogList = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string[] RetreveLog()
        {
            string[] array = this.ConsoleLogList.ToArray();
            this.ConsoleLogList = new List<string>();
            return array;
        }

        /// <summary>
        /// 
        /// </summary>
        public void RunTests()
        {
            //this.StartServerTCP();
            //this.StartClientTCP();
            //this.StartServerUDP("");
            //this.StartClientUDP("","","");
        }

        /// <summary>
        /// 
        /// </summary>
        public async void StartServerUDP(string serverPortNumber)
        {
            try
            {
                var serverDatagramSocket = new Windows.Networking.Sockets.DatagramSocket();

                // The ConnectionReceived event is raised when connections are received.
                serverDatagramSocket.MessageReceived += (sender, e) => ServerDatagramSocket_MessageReceived(sender, e, serverPortNumber);

                this.ConsoleLogList.Add("server is about to bind...");

                // Start listening for incoming TCP connections on the specified port. You can specify any port that's not currently in use.
                await serverDatagramSocket.BindServiceNameAsync(serverPortNumber);

                this.ConsoleLogList.Add(string.Format("server is bound to port number {0}", serverPortNumber));
            }
            catch (Exception ex)
            {
                Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
                this.ConsoleLogList.Add(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void ServerDatagramSocket_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender, Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args, string portNumber)
        {
            string request;
            using (Windows.Storage.Streams.DataReader dataReader = args.GetDataReader())
            {
                request = dataReader.ReadString(dataReader.UnconsumedBufferLength).Trim();
            }

            this.ConsoleLogList.Add(string.Format("server received the request: \"{0}\"", request));

            // Echo the request back as the response.
            using (Stream outputStream = (await sender.GetOutputStreamAsync(args.RemoteAddress, portNumber)).AsStreamForWrite())
            {
                using (var streamWriter = new StreamWriter(outputStream))
                {
                    await streamWriter.WriteLineAsync(request);
                    await streamWriter.FlushAsync();
                }
            }

            this.ConsoleLogList.Add(string.Format("server sent back the response: \"{0}\"", request));

            sender.Dispose();

            this.ConsoleLogList.Add("server closed its socket");
        }

        public async void StartClientUDP(string hostName, string serverPortNumber, string clientPortNumber, string request)
        {
            try
            {
                // Create the DatagramSocket and establish a connection to the echo server.
                var clientDatagramSocket = new Windows.Networking.Sockets.DatagramSocket();

                clientDatagramSocket.MessageReceived += ClientDatagramSocket_MessageReceived;

                // The server hostname that we will be establishing a connection to. In this example, the server and client are in the same process.
                var _hostName = new Windows.Networking.HostName(hostName); //"10.160.99.69"

                this.ConsoleLogList.Add("client is about to bind...");

                await clientDatagramSocket.BindServiceNameAsync(clientPortNumber);

                this.ConsoleLogList.Add(string.Format("client is bound to port number {0}", clientPortNumber));

                // Send a request to the echo server.
                using (var serverDatagramSocket = new Windows.Networking.Sockets.DatagramSocket())
                {
                    using (Stream outputStream = (await serverDatagramSocket.GetOutputStreamAsync(_hostName, serverPortNumber)).AsStreamForWrite())
                    {
                        using (var streamWriter = new StreamWriter(outputStream))
                        {
                            await streamWriter.WriteLineAsync(request);
                            await streamWriter.FlushAsync();
                        }
                    }
                }

                this.ConsoleLogList.Add(string.Format("client sent the request: \"{0}\"", request));
            }
            catch (Exception ex)
            {
                Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
                this.ConsoleLogList.Add(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            }
        }

        private void ClientDatagramSocket_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender, Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
        {
            string response;
            using (Windows.Storage.Streams.DataReader dataReader = args.GetDataReader())
            {
                response = dataReader.ReadString(dataReader.UnconsumedBufferLength).Trim();
            }

            this.ConsoleLogList.Add(string.Format("client received the response: \"{0}\"", response));

            sender.Dispose();

            this.ConsoleLogList.Add("client closed its socket");
        }

        public async void StartServerTCP(string portNumber)
        {
            try
            {
                var streamSocketListener = new Windows.Networking.Sockets.StreamSocketListener();

                // The ConnectionReceived event is raised when connections are received.
                streamSocketListener.ConnectionReceived += this.StreamSocketListener_ConnectionReceived;

                // Start listening for incoming TCP connections on the specified port. You can specify any port that's not currently in use.
                await streamSocketListener.BindServiceNameAsync(portNumber);

                this.ConsoleLogList.Add("server is listening...");
            }
            catch (Exception ex)
            {
                Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
                this.ConsoleLogList.Add(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            }
        }

        public async void StreamSocketListener_ConnectionReceived(Windows.Networking.Sockets.StreamSocketListener sender, Windows.Networking.Sockets.StreamSocketListenerConnectionReceivedEventArgs args)
        {
            string request;
            using (var streamReader = new StreamReader(args.Socket.InputStream.AsStreamForRead()))
            {
                request = await streamReader.ReadLineAsync();
            }

            this.ConsoleLogList.Add(string.Format("server received the request: \"{0}\"", request));

            // Echo the request back as the response.
            using (Stream outputStream = args.Socket.OutputStream.AsStreamForWrite())
            {
                using (var streamWriter = new StreamWriter(outputStream))
                {
                    await streamWriter.WriteLineAsync(request);
                    await streamWriter.FlushAsync();
                }
            }

            this.ConsoleLogList.Add(string.Format("server sent back the response: \"{0}\"", request));

            sender.Dispose();

            this.ConsoleLogList.Add("server closed its socket");
        }

        public async void StartClientTCP(string hostName, string portNumber, string request)
        {
            try
            {
                // Create the StreamSocket and establish a connection to the echo server.
                using (var streamSocket = new Windows.Networking.Sockets.StreamSocket())
                {
                    // The server hostname that we will be establishing a connection to. In this example, the server and client are in the same process.
                    var _hostName = new Windows.Networking.HostName(hostName);//"10.160.98.35"

                    this.ConsoleLogList.Add("client is trying to connect...");

                    await streamSocket.ConnectAsync(_hostName, portNumber);

                    this.ConsoleLogList.Add("client connected");

                    // Send a request to the echo server.
                    for(int i = 0; i < 3; i++)
                    {
                        using (Stream outputStream = streamSocket.OutputStream.AsStreamForWrite())
                        {
                            using (var streamWriter = new StreamWriter(outputStream))
                            {
                                await streamWriter.WriteLineAsync(request);
                                await streamWriter.FlushAsync();
                            }
                        }
                    }

                    this.ConsoleLogList.Add(string.Format("client sent the request: \"{0}\"", request));

                    // Read data from the echo server.
                    string response;
                    using (Stream inputStream = streamSocket.InputStream.AsStreamForRead())
                    {
                        using (StreamReader streamReader = new StreamReader(inputStream))
                        {
                            response = await streamReader.ReadLineAsync();
                        }
                    }

                    this.ConsoleLogList.Add(string.Format("client received the response: \"{0}\" ", response));
                    this.ConsoleLogList.Add("client closed its socket");
                }
                
            }
            catch (Exception ex)
            {
                Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
                this.ConsoleLogList.Add(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            }
        }
    }
}
