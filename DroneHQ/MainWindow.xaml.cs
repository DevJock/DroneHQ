using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TelloLib;

namespace DroneHQ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        static float sensitivity = 0.5f;

        public MainWindow()
        {
            InitializeComponent();
            EventManager.RegisterClassHandler(typeof(Window), Keyboard.KeyDownEvent, new KeyEventHandler(keyDown), true);

            Tello.onConnection += (Tello.ConnectionState newState) =>
            {
                if (newState != Tello.ConnectionState.Connected)
                {
                }
                if (newState == Tello.ConnectionState.Connected)
                {
                    
                }
                Console.WriteLine("Tello " + newState.ToString());
            };

            var logPath = "logs/";
            System.IO.Directory.CreateDirectory(Path.Combine("../", logPath));
            var logStartTime = DateTime.Now;
            var logFilePath = Path.Combine("../", logPath + logStartTime.ToString("yyyy-dd-M--HH-mm-ss") + ".csv");

            Tello.onUpdate += (cmdId) =>
            {
                if (cmdId == 86)//ac update
                {
                    //write update to log.
                    var elapsed = DateTime.Now - logStartTime;
                    File.AppendAllText(logFilePath, elapsed.ToString(@"mm\:ss\:ff\,") + Tello.state.getLogLine());
                    this.Dispatcher.Invoke(() =>
                    {
                        batpercent.Content = "Battery: " + Tello.state.batteryPercentage + "%";
                        altitude.Content = "Altitude: " + Tello.state.height + "m";
                        speed.Content = "Speed: " + Tello.state.flySpeed + "m/s";
                        wifi.Content = "WiFi: " + Tello.state.wifiStrength.ToString();
                        disturbance.Content = "Attenuation: " + Tello.state.wifiDisturb + "dB";
                        batteryV.Content = "Battery Voltage: " + Tello.state.droneBatteryLeft + "mV";
                        sensi.Content = "Sensitivity: " + sensitivity;
                    });
                }
            };


            var videoClient = UdpUser.ConnectTo("127.0.0.1", 7038);

            //subscribe to Tello video data
            Tello.onVideoData += (byte[] data) =>
            {
                try
                {
                    videoClient.Send(data.Skip(2).ToArray());//Skip 2 byte header and send to ffplay. 
                    this.Dispatcher.Invoke(() =>
                    {
                        
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in Video Feed: " + ex.ToString());
                }
            };

        }


        void keyDown(object sender, KeyEventArgs e)
        {
            float[] axis = new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
            switch (e.Key)
            {
                case Key.W:
                    {
                        axis[3] = sensitivity;
                    }
                    break;
                case Key.A:
                    {
                        axis[2] = -sensitivity;
                    }
                    break;
                case Key.S:
                    {
                        axis[3] = -sensitivity;
                    }
                    break;
                case Key.D:
                    {
                        axis[2] = sensitivity;
                    }
                    break;
                case Key.Q:
                    {
                        axis[0] = sensitivity;
                    }
                    break;
                case Key.E:
                    {
                        axis[0] = -sensitivity;
                    }
                    break;
                case Key.Z:
                    {
                        axis[1] = sensitivity;
                    }
                    break;
                case Key.X:
                    {
                        axis[1] = -sensitivity;
                    }
                    break;
                case Key.D1:
                    {
                        if (sensitivity + 0.25f <= 1f)
                            sensitivity += 0.25f;
                    }
                    break;
                case Key.D2:
                    {
                        if (sensitivity - 0.25f >= 0.0f)
                            sensitivity -= 0.25f;
                    }
                    break;

                case Key.Tab:
                    {
                        if (Tello.connected)
                        {
                            if (Tello.state.flying)
                            {
                                Tello.land();
                            }
                            else
                            {
                                Tello.takeOff();
                            }
                        }
                    }
                    break;
            }
            Tello.controllerState.setAxis(axis[0], axis[1], axis[2], axis[3]);
        }



        private void StreamPlayerControl_Loaded(object sender, RoutedEventArgs e)
        {
            player.StartPlay(new Uri("udp://127.0.0.1:7038"));
        }

        private void connect_Click(object sender, RoutedEventArgs e)
        {
            if (connect.Content.ToString() == "Connect")
            {
                Tello.startConnecting();
                connect.Content = "Disconnect";
            }
            else
            {
                connect.Content = "Connect";
            }
        }
    }
}
