using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Cyborier.Tools.Logging.RemoteLogViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        UdpClient udpClient;
        int port;
        Thread reciveTask;
        private bool IsRunning;

        /// <summary>
        /// Start Listinging to Given Port.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                port = int.Parse(txtPort.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Enter Valid Port Number");
                return;

            }
            byte[] buffer;
            string loggingEvent;

            try
            {
                this.Title = "Cyborier Remote Log Viewer | Listening on " + port.ToString();
                udpClient = new UdpClient(port);

                reciveTask = new Thread(new ThreadStart(() =>
                    {
                        while (IsRunning)
                        {
                            try
                            {
                                buffer = udpClient.Receive(ref remoteEndPoint);
                                loggingEvent = System.Text.Encoding.ASCII.GetString(buffer);
                                this.WriteToLogScreen(loggingEvent);
                            }
                            catch (ThreadAbortException the)
                            {
                                this.btnStart.IsEnabled = true;
                                this.btnStop.IsEnabled = false;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Exception occured , " + ex.Message);
                                WriteToLogScreen("Exception Occured , \nExceptio Details.\n" + ex.ToString());
                            }
                        }
                    }));

                this.IsRunning = true;
                this.reciveTask.IsBackground = true;
                this.reciveTask.Start();
                this.btnStop.IsEnabled = true;
                this.btnStart.IsEnabled = false;
            }
            catch (Exception e2)
            {
                WriteToLogScreen("Exception Occured , \nExceptio Details.\n" + e2.ToString());
            }
        }

        private void WriteToLogScreen(string loggingEvent)
        {
            if (!CheckAccess())
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    WriteToLogScreen(loggingEvent);
                }));
                return;
            }

            if (rtbLog.Document.Blocks.Count > 200)
            {
                rtbLog.Document.Blocks.Remove(rtbLog.Document.Blocks.First());
            }

            Paragraph p = new Paragraph(new Run(loggingEvent));
            p.Margin = new Thickness(0,1,0,1);
            p.BorderBrush = Brushes.Gray;
            p.BorderThickness = new Thickness(0, 0, 0, 1);

            p.Foreground = Brushes.White;
            if (loggingEvent.ToLower().Contains("[warn]"))
            {
                p.Foreground = Brushes.Yellow;
            }
            if (loggingEvent.ToLower().Contains("[error]"))
            {
                p.Foreground = Brushes.Red;
            }
            if (loggingEvent.ToLower().Contains("[fatal]"))
            {
                p.Foreground = Brushes.DarkRed;
            }
            if (loggingEvent.ToLower().Contains("[debug]"))
            {
                p.Foreground = Brushes.Green;
            }
            if (loggingEvent.ToLower().Contains("[info]"))
            {
                p.Foreground = Brushes.LightBlue;
            }


            this.rtbLog.Document.Blocks.Add(p);
            if (this.chkAutoScroll.IsChecked == true)
            {
                this.rtbLog.ScrollToEnd();
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            this.IsRunning = false;
            this.reciveTask.Abort();
            this.reciveTask = null;
            this.btnStop.IsEnabled = false;
            this.btnStart.IsEnabled = true;
        }

        private void btnStop_Copy_Click(object sender, RoutedEventArgs e)
        {
            rtbLog.Document.Blocks.Clear();
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            //var brush = new Brush();
            

            //this.rtbLog.Background;
            //this.colorPickerBackgorun.SelectedColor.

            var convertor = new BrushConverter();
            rtbLog.Background =  (Brush)convertor.ConvertFromString(colorPickerBackgorun.SelectedColorText);
        }
    }
}
