using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MyUniversalApp.Helpers;
using Windows.Networking;
using System.Diagnostics;
using Windows.UI.Core;
using System.Threading.Tasks;
using TomsUWPNetworkingLibrary;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NetworkTestingApplication
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }
        // Every protocol typically has a standard port number. For example, HTTP is typically 80, FTP is 20 and 21, etc.
        // For this example, we'll choose an arbitrary port number.
        static string TCPPortNumber = "1337";



        // Every protocol typically has a standard port number. For example, HTTP is typically 80, FTP is 20 and 21, etc.
        // For this example, we'll choose different arbitrary port numbers for client and server, since both will be running on the same machine.
        static string ClientPortNumber = "1336";
        static string ServerPortNumber = "1337";

        public NetworkClient networkingClient = null;

        public NetworkServer networkingServer = null;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

        private void outputToUI(string[] logStrings)
        {
            if (logStrings == null) return;

            foreach(string logString in logStrings)
            {
                this.DebugListBox.Items.Add(logString);
            }
        }

        private async void StartServerUDP()
        {
            try
            {
                var serverDatagramSocket = new Windows.Networking.Sockets.DatagramSocket();

                // The ConnectionReceived event is raised when connections are received.
                serverDatagramSocket.MessageReceived += ServerDatagramSocket_MessageReceived;

                this.serverListBox.Items.Add("server is about to bind...");

                // Start listening for incoming TCP connections on the specified port. You can specify any port that's not currently in use.
                await serverDatagramSocket.BindServiceNameAsync(MainPage.ServerPortNumber);

                this.serverListBox.Items.Add(string.Format("server is bound to port number {0}", MainPage.ServerPortNumber));
            }
            catch (Exception ex)
            {
                Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
                this.serverListBox.Items.Add(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            }
        }

        private async void ServerDatagramSocket_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender, Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
        {
            string request;
            using (Windows.Storage.Streams.DataReader dataReader = args.GetDataReader())
            {
                request = dataReader.ReadString(dataReader.UnconsumedBufferLength).Trim();
            }

            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add(string.Format("server received the request: \"{0}\"", request)));

            // Echo the request back as the response.
            using (Stream outputStream = (await sender.GetOutputStreamAsync(args.RemoteAddress, MainPage.ClientPortNumber)).AsStreamForWrite())
            {
                using (var streamWriter = new StreamWriter(outputStream))
                {
                    await streamWriter.WriteLineAsync(request);
                    await streamWriter.FlushAsync();
                }
            }

            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add(string.Format("server sent back the response: \"{0}\"", request)));

            sender.Dispose();

            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add("server closed its socket"));
        }

        private async void StartClientUDP()
        {
            try
            {
                // Create the DatagramSocket and establish a connection to the echo server.
                var clientDatagramSocket = new Windows.Networking.Sockets.DatagramSocket();

                clientDatagramSocket.MessageReceived += ClientDatagramSocket_MessageReceived;

                // The server hostname that we will be establishing a connection to. In this example, the server and client are in the same process.
                var hostName = new Windows.Networking.HostName("localhost");

                this.clientListBox.Items.Add("client is about to bind...");

                await clientDatagramSocket.BindServiceNameAsync(MainPage.ClientPortNumber);

                this.clientListBox.Items.Add(string.Format("client is bound to port number {0}", MainPage.ClientPortNumber));

                // Send a request to the echo server.
                string request = "Hello, World!";
                using (var serverDatagramSocket = new Windows.Networking.Sockets.DatagramSocket())
                {
                    using (Stream outputStream = (await serverDatagramSocket.GetOutputStreamAsync(hostName, MainPage.ServerPortNumber)).AsStreamForWrite())
                    {
                        using (var streamWriter = new StreamWriter(outputStream))
                        {
                            await streamWriter.WriteLineAsync(request);
                            await streamWriter.FlushAsync();
                        }
                    }
                }

                this.clientListBox.Items.Add(string.Format("client sent the request: \"{0}\"", request));
            }
            catch (Exception ex)
            {
                Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
                this.clientListBox.Items.Add(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            }
        }

        private async void ClientDatagramSocket_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender, Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
        {
            string response;
            using (Windows.Storage.Streams.DataReader dataReader = args.GetDataReader())
            {
                response = dataReader.ReadString(dataReader.UnconsumedBufferLength).Trim();
            }

            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.clientListBox.Items.Add(string.Format("client received the response: \"{0}\"", response)));

            sender.Dispose();

            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.clientListBox.Items.Add("client closed its socket"));
        }

        private async void StartServerTCP()
        {
            try
            {
                var streamSocketListener = new Windows.Networking.Sockets.StreamSocketListener();

                // The ConnectionReceived event is raised when connections are received.
                streamSocketListener.ConnectionReceived += this.StreamSocketListener_ConnectionReceived;

                // Start listening for incoming TCP connections on the specified port. You can specify any port that's not currently in use.
                await streamSocketListener.BindServiceNameAsync(MainPage.TCPPortNumber);

                this.serverListBox.Items.Add("server is listening...");
            }
            catch (Exception ex)
            {
                Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
                this.serverListBox.Items.Add(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            }
        }

        private async void StreamSocketListener_ConnectionReceived(Windows.Networking.Sockets.StreamSocketListener sender, Windows.Networking.Sockets.StreamSocketListenerConnectionReceivedEventArgs args)
        {
            string request;
            using (var streamReader = new StreamReader(args.Socket.InputStream.AsStreamForRead()))
            {
                request = await streamReader.ReadLineAsync();
            }

            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add(string.Format("server received the request: \"{0}\"", request)));

            // Echo the request back as the response.
            using (Stream outputStream = args.Socket.OutputStream.AsStreamForWrite())
            {
                using (var streamWriter = new StreamWriter(outputStream))
                {
                    await streamWriter.WriteLineAsync(request);
                    await streamWriter.FlushAsync();
                }
            }

            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add(string.Format("server sent back the response: \"{0}\"", request)));

            sender.Dispose();

            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add("server closed its socket"));
        }

        private async void StartClientTCP()
        {
            try
            {
                // Create the StreamSocket and establish a connection to the echo server.
                using (var streamSocket = new Windows.Networking.Sockets.StreamSocket())
                {
                    // The server hostname that we will be establishing a connection to. In this example, the server and client are in the same process.
                    var hostName = new Windows.Networking.HostName("localhost");

                    this.clientListBox.Items.Add("client is trying to connect...");

                    await streamSocket.ConnectAsync(hostName, MainPage.TCPPortNumber);

                    this.clientListBox.Items.Add("client connected");

                    // Send a request to the echo server.
                    string request = "Hello, World!";
                    using (Stream outputStream = streamSocket.OutputStream.AsStreamForWrite())
                    {
                        using (var streamWriter = new StreamWriter(outputStream))
                        {
                            await streamWriter.WriteLineAsync(request);
                            await streamWriter.FlushAsync();
                        }
                    }

                    this.clientListBox.Items.Add(string.Format("client sent the request: \"{0}\"", request));

                    // Read data from the echo server.
                    string response;
                    using (Stream inputStream = streamSocket.InputStream.AsStreamForRead())
                    {
                        using (StreamReader streamReader = new StreamReader(inputStream))
                        {
                            response = await streamReader.ReadLineAsync();
                        }
                    }

                    this.clientListBox.Items.Add(string.Format("client received the response: \"{0}\" ", response));
                }

                this.clientListBox.Items.Add("client closed its socket");
            }
            catch (Exception ex)
            {
                Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
                this.clientListBox.Items.Add(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            }
        }

        private async Task<string> InputTextDialogAsync(string title)
        {
            TextBox inputTextBox = new TextBox();
            inputTextBox.AcceptsReturn = false;
            inputTextBox.Height = 32;
            ContentDialog dialog = new ContentDialog();
            dialog.Content = inputTextBox;
            dialog.Title = title;
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Ok";
            dialog.SecondaryButtonText = "Cancel";
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                return inputTextBox.Text;
            else
                return "";
        }

        private void RefreashButton_Click(object sender, RoutedEventArgs e)
        {
            this.outputToUI(NetworkTestingAlgorithms.GetInstance().RetreveLog());
        }

        private void Button_BasicDemoUDP_Click(object sender, RoutedEventArgs e)
        {
            this.StartServerUDP();
            this.StartClientUDP();
        }

        private void Button_BasicDemoTCP_Click(object sender, RoutedEventArgs e)
        {
            this.StartServerTCP();
            this.StartClientTCP();
        }

        private async void Button_BasicDemoUDP2_Click(object sender, RoutedEventArgs e)
        {
            // show populated dialog
            ContentDialog1 dialog = new ContentDialog1();
            dialog.SetTextboxes(
                "localhost", ClientPortNumber,
                ServerPortNumber, "HelloWorld");

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Secondary)
            {
                string[] parameters = dialog.GetTextBoxContent();
                // user didn't cancel
                NetworkTestingAlgorithms networkTesting = NetworkTestingAlgorithms.GetInstance();

                // start the networks
                networkTesting.StartServerUDP(parameters[2]);
                networkTesting.StartClientUDP(parameters[0], parameters[2], parameters[3], parameters[1]);
            }
        }

        private async void Button_BasicDemoTCP2_Click(object sender, RoutedEventArgs e)
        {
            // show populated dialog
            ContentDialog1 dialog = new ContentDialog1();
            dialog.SetTextboxes(
                "localhost", TCPPortNumber,
                TCPPortNumber, "HelloWorld");

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Secondary)
            {
                string[] parameters = dialog.GetTextBoxContent();
                // user didn't cancel
                NetworkTestingAlgorithms networkTesting = NetworkTestingAlgorithms.GetInstance();

                // start the networks
                networkTesting.StartServerTCP(parameters[2]);
                networkTesting.StartClientTCP(parameters[0], parameters[2], parameters[1]);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.outputToUI(NetworkTestingAlgorithms.GetInstance().RetreveLog());
        }

        private async void Button_StartUDPClient_Click(object sender, RoutedEventArgs e)
        {
            // show populated dialog
            ContentDialog1 dialog = new ContentDialog1();
            dialog.SetTextboxes(
                "localhost", ClientPortNumber,
                ServerPortNumber, "HelloWorld");

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Secondary)
            {
                string[] parameters = dialog.GetTextBoxContent();

                UDPClient client = new UDPClient(parameters[0], parameters[2]);
                this.networkingClient = client;
                await client.Connect();
                await client.SendAsync(parameters[1]);
            }
        }

        private async void Button_StartUDPServer_Click(object sender, RoutedEventArgs e)
        {
            // show populated dialog
            ContentDialog1 dialog = new ContentDialog1();
            dialog.SetTextboxes(
                "localhost", ClientPortNumber,
                ServerPortNumber, "HelloWorld");

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Secondary)
            {
                string[] parameters = dialog.GetTextBoxContent();

                UDPServer server = new UDPServer(parameters[2]);
                this.networkingServer = server;
                server.StartAsync();
            }
        }

        private void Button_Libary_Refresh_Click(object sender, RoutedEventArgs e)
        {
            // remove all of the current items so things don't double up
            for (int index = 0; index < serverListBoxLibaryTesting.Items.Count; index++)
            {
                serverListBoxLibaryTesting.Items.RemoveAt(index);
            }

            for (int index = 0; index < clientListBoxLibaryTesting.Items.Count; index++)
            {
                clientListBoxLibaryTesting.Items.RemoveAt(index);
            }

            // populate each list with the new feilds
            KeyValuePair<DateTime, string>[] clientLog;
            if (this.networkingClient != null)
            {
                clientLog = networkingClient.DegbugLogMessages;
            }
            else
            {
                // create a empty array if the item dosn't exist
                clientLog = new KeyValuePair<DateTime, string>[0];
            }

            KeyValuePair<DateTime, string>[] serverLog;
            if (this.networkingServer != null)
            {
                serverLog = networkingServer.DegbugLogMessages;
            }
            else
            {
                // create a empty array if the item dosn't exist
                serverLog = new KeyValuePair<DateTime, string>[0];
            }

            for (int index = 0; index < serverLog.Length; index++)
            {
                serverListBoxLibaryTesting.Items.Add(serverLog[index].Key.ToString() + " : " + serverLog[index].Value);
            }

            for (int index = 0; index < clientLog.Length; index++)
            {
                clientListBoxLibaryTesting.Items.Add(serverLog[index].Key.ToString() + " : " + clientLog[index].Value);
            }
        }

        private void ButtonStartClientServer_Click(object sender, RoutedEventArgs e)
        {
            networkingServer = new UDPServer(this.Server_Port_Address_Textbox.Text);
            networkingServer.StartAsync();
        }

        private void ButtonCloseClientServer_Click(object sender, RoutedEventArgs e)
        {
            networkingServer.Close();
        }

        private void ButtonStartClientClient_Click(object sender, RoutedEventArgs e)
        {
            networkingClient = new UDPClient(this.Client_IP_Textbox.Text, this.Client_Port_Address_Textbox.Text);
            networkingClient.Connect();
        }

        private void ButtonCloseClientClient_Click(object sender, RoutedEventArgs e)
        {
            networkingClient.Close();
        }

        private void ButtonSendMessageClient_Click(object sender, RoutedEventArgs e)
        {
            networkingClient.SendAsync(this.Client_Port_Address_Textbox.Text);
        }

        private void RefreshConsoleMessages()
        {
            for (int i = 0; i < MessageConsoleClientListBox.Items.Count(); i++)
            {
                this.MessageConsoleClientListBox.Items.RemoveAt(i);
            }

            for (int i = 0; i < DebugConsoleClientListBox.Items.Count(); i++)
            {
                this.DebugConsoleClientListBox.Items.RemoveAt(i);
            }

            for (int i = 0; i < DebugConsoleServerListBox.Items.Count(); i++)
            {
                this.DebugConsoleServerListBox.Items.RemoveAt(i);
            }

            for (int i = 0; i < MessageConsoleServerListBox.Items.Count(); i++)
            {
                this.MessageConsoleServerListBox.Items.RemoveAt(i);
            }

            KeyValuePair<DateTime, string>[] messagesRecivedFromClient;
            KeyValuePair<DateTime, string>[] debugLogFromClient;
            if (networkingClient != null)
            {
                messagesRecivedFromClient = networkingClient.MessageRecivedLog;
                debugLogFromClient = networkingClient.DegbugLogMessages;
            }
            else
            {
                messagesRecivedFromClient = new KeyValuePair<DateTime, string>[0];
                debugLogFromClient = new KeyValuePair<DateTime, string>[0];
            }

            KeyValuePair<DateTime, string>[] debugLogFromServer;
            KeyValuePair<DateTime, string>[] messagesRecivedFromServer;
            if (networkingServer != null)
            {
                debugLogFromServer = networkingServer.DegbugLogMessages;
                messagesRecivedFromServer = networkingServer.MessageRecivedLog;
            }
            else
            {
                debugLogFromServer = new KeyValuePair<DateTime, string>[0];
                messagesRecivedFromServer = new KeyValuePair<DateTime, string>[0];
            }

            foreach(KeyValuePair<DateTime, string> valuePair in messagesRecivedFromClient)
            {
                this.MessageConsoleClientListBox.Items.Add(valuePair);
            }
            
            foreach (KeyValuePair<DateTime, string> valuePair in debugLogFromClient)
            {
                this.DebugConsoleClientListBox.Items.Add(valuePair);
            }

            foreach (KeyValuePair<DateTime, string> valuePair in debugLogFromServer)
            {
                this.DebugConsoleServerListBox.Items.Add(valuePair);
            }

            foreach (KeyValuePair<DateTime, string> valuePair in messagesRecivedFromServer)
            {
                this.MessageConsoleServerListBox.Items.Add(valuePair);
            }
        }

        private void ButtonRefreashMessageBoxes_Click(object sender, RoutedEventArgs e)
        {
            this.RefreshConsoleMessages();
        }
    }
}
