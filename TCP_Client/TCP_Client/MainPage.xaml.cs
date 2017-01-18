using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Vorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 dokumentiert.

namespace TCP_Client
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            lbl_status.Text = "Bereit";
        }

        private async Task sendToServer()
        {
            StreamSocket socket = new StreamSocket();

            socket.Control.KeepAlive = false;

            HostName host = new HostName("192.168.188.45");

            try
            {
                await socket.ConnectAsync(host, "8083");

                lbl_status.Text = "Verbunden";

                // SENDEN
                DataWriter writer = new DataWriter(socket.OutputStream);
                Random rdm = new Random();
                string request = txt_senddata.Text; //"Hallo" + rdm.Next(1000, 9999).ToString();
                writer.WriteUInt32(writer.MeasureString(request));
                writer.WriteString(request);

                await writer.StoreAsync();

                writer.DetachStream();
                writer.Dispose();

                // ECHO/ANTWORT EMPFANGEN
                DataReader reader = new DataReader(socket.InputStream);
                uint sizeFieldCount = await reader.LoadAsync(sizeof(uint));
                if (sizeFieldCount != sizeof(uint))
                {
                    return;
                }

                uint stringLength = reader.ReadUInt32();
                uint actualStringLength = await reader.LoadAsync(stringLength);

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () =>
                    {
                        //UI Updates
                        lbl_result.Text = reader.ReadString(actualStringLength);
                    });

                lbl_status.Text = "Echo empfangen";
            }
            catch (Exception ex)
            {
                lbl_status.Text = "Fail. Reason: " + ex.Message;
            }

            socket.Dispose();

            /*try
            {
                //Create the StreamSocket and establish a connection to the echo server.
                StreamSocket socket = new StreamSocket();

                //The server hostname that we will be establishing a connection to. We will be running the server and client locally,
                //so we will use localhost as the hostname.
                Windows.Networking.HostName serverHost = new Windows.Networking.HostName("192.168.188.45");

                //Every protocol typically has a standard port number. For example HTTP is typically 80, FTP is 20 and 21, etc.
                //For the echo server/client application we will use a random port 1337.
                string serverPort = "8083";
                await socket.ConnectAsync(serverHost, serverPort);
                lbl_status.Text = "Verbunden";

                //Write data to the echo server.
                Stream streamOut = socket.OutputStream.AsStreamForWrite();
                StreamWriter writer = new StreamWriter(streamOut);
                string request = txt_senddata.Text;
                await writer.WriteLineAsync(request);
                await writer.FlushAsync();

                //Read data from the echo server.
                /*Stream streamIn = socket.InputStream.AsStreamForRead();
                StreamReader reader = new StreamReader(streamIn);
                string response = await reader.ReadLineAsync();
                lbl_result.Text =  response;
                lbl_result.Text = "send";
            }
            catch (Exception e)
            {
                //Handle exception here.            
                lbl_status.Text = "Fail. Reason: " + e.ToString();
            }*/
        }

        private async void btn_send_Click(object sender, RoutedEventArgs e)
        {
            await sendToServer();
        }
    }
}
