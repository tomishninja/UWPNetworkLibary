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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace NetworkTestingApplication
{
    public sealed partial class ContentDialog1 : ContentDialog
    {
        public ContentDialog1()
        {
            this.InitializeComponent();
        }

        public void SetTextboxes(string ipaddress, string serverPortNumber, string clientPortNumber, string message)
        {
            TextBox_IP_Address.Text = ipaddress;
            TextBox_Message.Text = message;
            TextBox_Server_Port_Number.Text = serverPortNumber;
            TextBox_Client_Port_Number.Text = clientPortNumber;
        }

        public string[] GetTextBoxContent()
        {
            return new string[]
            {
                TextBox_IP_Address.Text,
                TextBox_Message.Text,
                TextBox_Server_Port_Number.Text,
                TextBox_Client_Port_Number.Text
            };
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
