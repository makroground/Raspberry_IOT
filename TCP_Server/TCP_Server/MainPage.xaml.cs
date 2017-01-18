using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace TCP_Server
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            startTCPListener();
        }

        private async void startTCPListener()
        {
            try
            {
                //Create a StreamSocketListener to start listening for TCP connections.
                StreamSocketListener socketListener = new StreamSocketListener();

                //Hook up an event handler to call when connections are received.
                socketListener.ConnectionReceived += SocketListener_ConnectionReceived;

                socketListener.Control.KeepAlive = true;

                //Start listening for incoming TCP connections on the specified port. You can specify any port that' s not currently in use.
                await socketListener.BindServiceNameAsync("8083");

                lbl_status.Text = "Running";
            }
            catch (Exception e)
            {
                if (SocketError.GetStatus(e.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }
                //Handle exception.
                lbl_status.Text = "Fail. Reason: " + e.ToString();
            }
        }

        private async void SocketListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            try
            {
                //Read line from the remote client.
                /*Stream inStream = args.Socket.InputStream.AsStreamForRead();
                StreamReader reader = new StreamReader(inStream);
                string request = await reader.ReadLineAsync();
                lbl_result.Text = request;*/
                DataReader reader = new DataReader(args.Socket.InputStream);
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

                DataWriter writer = new DataWriter(args.Socket.OutputStream);
                //Random rdm = new Random();
                string request = "N/A";
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () =>
                    {
                        //UI Updates
                        request = lbl_result.Text;
                    });
                //"Test" + rdm.Next(1000, 9999).ToString();
                writer.WriteUInt32(writer.MeasureString(request));
                writer.WriteString(request);

                await writer.StoreAsync();

                writer.DetachStream();
                writer.Dispose();

                //Send the line back to the remote client.
                /*Stream outStream = args.Socket.OutputStream.AsStreamForWrite();
                StreamWriter writer = new StreamWriter(outStream);
                await writer.WriteLineAsync(request);
                await writer.FlushAsync();*/
            } catch (Exception ex)
            {
                lbl_status.Text = "Fail. Reason: " + ex.Message;
            }
        }

        private async void btn_sendhello_Click(object sender, RoutedEventArgs e)
        {
            StreamSocket socket = new StreamSocket();

            socket.Control.KeepAlive = false;

            HostName host = new HostName("localhost");

            try
            {
                await socket.ConnectAsync(host, "8083");

                DataWriter writer = new DataWriter(socket.OutputStream);
                Random rdm = new Random();
                string request = "Hallo" + rdm.Next(1000, 9999).ToString();
                writer.WriteUInt32(writer.MeasureString(request));
                writer.WriteString(request);

                await writer.StoreAsync();

                writer.DetachStream();
                writer.Dispose();
            } catch (Exception ex)
            {
                lbl_status.Text = "Fail. Reason: " + ex.Message;
            }

            socket.Dispose();
        }
    }
}
