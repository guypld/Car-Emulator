using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace CarEmu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MovingCharacter : Window
    {
        private const int INCOMMING_PORT = 11001;
        private const int OUTGOING_PORT = 1510;
        private const String DEST_IP = "127.0.0.1";

        UdpClient sendUdpClient = new UdpClient(DEST_IP, OUTGOING_PORT);
        UdpClient receivingUdpClient = new UdpClient(INCOMMING_PORT);
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

        Timer timer;

        Timer udp_timer;

        private List<System.Windows.Shapes.Path> m_Cars = new List<System.Windows.Shapes.Path>();

        public List<System.Windows.Shapes.Path> Cars
        {
            get { return m_Cars; }
            set { m_Cars = value; }
        }

        private Dictionary<System.Windows.Shapes.Path, Character> CharacterDic = 
            new Dictionary<System.Windows.Shapes.Path, Character>();



        public Character Character { get; set; }

        public MovingCharacter()
        {
            InitializeComponent();
            DataContext = Character = new Character()
            {
                Location = new Point(70, 340),
                RotationSpeed = 1,
                MovementSpeed = 1
            };

            Thread t = new Thread(UDP_REC);
            t.Start();
        }

        private void TimerTick(object state)
        {
            Character.IsMovingForward = false;
        }

        int TotalCounter = 0;

        private void UDP_TimerTick(object state)
        {
            lock (this)
            {
                StringBuilder str = new StringBuilder();

                //str.Append
                TotalCounter++;
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.Title = TotalCounter.ToString();
                    });
                }

                

                //String str = String.Empty;

                var cars = Cars.ToList();

                foreach (var car in cars)
                {
                    var ch = CharacterDic[car];
                    double x = 0, y = 0;

                    x = ch.Location.X;
                    y = ch.Location.Y;

                    str.Append(String.Format("{0:0.00}", x));
                    str.Append(",");
                    str.Append(String.Format("{0:0.00}", y));
                    str.Append(",");
                    str.Append(ch.Angle);
                    str.Append("|");

                    //str += x + "," + y + "," + ch.Angle + "|";
                }

                Byte[] sendBytes = Encoding.ASCII.GetBytes(str.ToString());
                try
                {
                    sendUdpClient.Send(sendBytes, sendBytes.Length);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        lbl.Content = DateTime.Now.TimeOfDay + ": '" + str + "'";
                    });
                }
                catch (Exception ex)
                {
                    ShowMSG(ex.Message);
                    if (Application.Current != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            lbl.Content = "ERR: " + ex.Message;
                        });
                    }
                }
            }
        }


        private void UDP_REC()
        {
            try
            {
                while (true)
                {
                    Byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.ASCII.GetString(receiveBytes);
                    string msg = returnData.ToString();

                    ParseMSG(msg);
                }
            }
            catch (Exception e)
            {
                ShowMSG(e.ToString());
            }
        }

        private void ParseMSG(String msg)
        {
            // mas = angle=360|angle=4|f=1|

            Console.WriteLine(msg);

            if (msg == "s")
            {
                foreach (var car in CharacterDic.ToList())
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        car.Value.ForwardStopWatch = 0;
                        car.Value.RotateDirection = 0;
                    });
                }

                return;
            }

            try
            {

                var args = msg.Split('|').Where(m => !String.IsNullOrEmpty(m)).ToList();

                if (args.Count() != Cars.Count)
                {

                    ShowMSG("Error: Cars number not synced well... (Parse Message)");
                    return;
                }

                for (int i = 0; i < args.Count() ; i++)
                {
                    var car = CharacterDic[Cars[i]];
                    var vals = args[i].Split('=');

                    if (vals.Count() != 2)
                    {
                        ShowMSG("Error: Vals.Count != 2 ('A=B')");
                        return;
                    }

                    if (vals[0] == "angle")
                    {
                        car.ForwardStopWatch = 0;

                        var angle = vals[1];

                        //var angle = int.Parse(vals[1]);
                        
                        //car.WantedAngle = angle;


                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (angle == "r")
                            {
                                car.RotateDirection = 1;
                            }
                            else
                            {
                                car.RotateDirection = -1;
                            }

                            return;

                            //if (angle == car.Angle)
                            //{

                            //}
                            //else if (angle > car.Angle)
                            //{
                            //    if (angle - car.Angle < 179)
                            //    {
                            //        car.RotateDirection = 1;
                            //    }
                            //    else
                            //    {
                            //        car.RotateDirection = -1;
                            //    }
                            //}
                            //else
                            //{
                            //    if (car.Angle - angle < 179)
                            //    {
                            //        car.RotateDirection = -1;
                            //    }
                            //    else
                            //    {
                            //        car.RotateDirection = 1;
                            //    }
                            //}
                            //car.WantedAngle = angle;
                            ////timer = new Timer(TimerTick, null, 0, 3);
                        });


                    }
                    else if (vals[0] == "f")
                    {
                        Application.Current.Dispatcher.Invoke(() => 
                        {
                            car.ForwardStopWatch = 1;
                            car.RotateDirection = 0;
                        });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() => 
                        {
                            car.ForwardStopWatch = 0;
                        });
                    }
                }

                //case Key.Left: Character.RotateDirection = -1; break;
                //case Key.Right: Character.RotateDirection = 1; break;
                //case Key.Up: Character.IsMovingForward = true; break;

                if (msg.StartsWith("G"))
                {
                    int value = int.Parse(msg.TrimStart('G'));
                    if (value <= 0)
                    {
                        value = 1;
                    }
                    else if(value >= 360)
                    {
                        value = 359;
                    }
                    
                    Application.Current.Dispatcher.Invoke(() => 
                    {
                        if (value == Character.Angle)
                        {

                        }
                        else if ( value > Character.Angle)
                        {
                            if (value-Character.Angle < 179)
                            {
                                Character.RotateDirection = 1;
                            }
                            else
                            {
                                Character.RotateDirection = -1;
                            }
                        }
                        else
                        {
                            if (Character.Angle - value < 179)
                            {
                                Character.RotateDirection = -1;
                            }
                            else
                            {
                                Character.RotateDirection = 1;
                            }
                        }
                        Character.WantedAngle = value;
                        timer = new Timer(TimerTick, null, 0, 3);
                    });
                }
                else if (msg.StartsWith("F"))
                {
                    int value = int.Parse(msg.TrimStart('F'));
                    Application.Current.Dispatcher.Invoke(() => { Character.ForwardStopWatch = value*10; });
                }

            }
            catch (Exception ex)
            {
                ShowMSG("Error in MSG Parser: " + ex.Message);
            }
        }

        private void ShowMSG(String msg)
        {
            Character.MsgContent = msg;
            Console.WriteLine(msg);
        }
        
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.Key)
                {
                    case Key.Left: Character.RotateDirection = -1; break;
                    case Key.Right: Character.RotateDirection = 1; break;
                    case Key.Up: Character.IsMovingForward = true; break;

                    case Key.R :
                        (DataContext as Character).Location = new Point(70, 340);
                        (DataContext as Character).Angle = 0;
                        break;

                    case Key.D1:
                        if (Cars.Count >= 1)
                            Character = CharacterDic[Cars[0]];
                        break;
                    case Key.D2:
                        if (Cars.Count >= 2)
                            Character = CharacterDic[Cars[1]];
                        break;
                    case Key.D3:
                        if (Cars.Count >= 3)
                            Character = CharacterDic[Cars[2]];
                        break;
                    case Key.D4:
                        if (Cars.Count >= 4)
                            Character = CharacterDic[Cars[3]];
                        break;
                    case Key.D5:
                        if (Cars.Count >= 5)
                            Character = CharacterDic[Cars[4]];
                        break;
                    case Key.D6:
                        if (Cars.Count >= 6)
                            Character = CharacterDic[Cars[5]];
                        break;
                    case Key.D7:
                        if (Cars.Count >= 7)
                            Character = CharacterDic[Cars[6]];
                        break;
                }

            }
            catch {}

        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (Character == null)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Left:
                case Key.Right:
                    Character.RotateDirection = 0;
                    break;
                case Key.Up:
                    Character.IsMovingForward = false;
                    break;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            receivingUdpClient.Close();
            Application.Current.Shutdown();
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                Point p = Mouse.GetPosition(MainCanvas);
                
                string xaml = XamlWriter.Save(CarObject);

                StringReader stringReader = new StringReader(xaml);
                XmlReader xmlReader = XmlReader.Create(stringReader);
                var car = (System.Windows.Shapes.Path)XamlReader.Load(xmlReader);

                var left = p.X - (car.Data.Bounds.Width / 4);
                var top = p.Y - car.Data.Bounds.Height / 2;

                Canvas.SetLeft(car, left);
                Canvas.SetTop(car, top);
                car.Fill = Utils.PickBrush();
                car.Visibility = System.Windows.Visibility.Visible;

                Cars.Add(car);

                var ch = new Character()
                {
                    RotationSpeed = 1,
                    MovementSpeed = 1
                };
                CharacterDic.Add(car, ch);

                var rt = new RotateTransform();

                Binding b = new Binding();
                b.Source = ch;
                b.Path = new PropertyPath("Angle");
                BindingOperations.SetBinding(rt, RotateTransform.AngleProperty, b);

                car.RenderTransform = rt;
                
                //Binding bX = new Binding();
                //bX.Source = ch.Location;
                //bX.Path = new PropertyPath("X");
                //BindingOperations.SetBinding(car, Canvas.LeftProperty, bX);
                
                //Binding bY = new Binding();
                //bY.Source = ch.Location;
                //bY.Path = new PropertyPath("Y");
                //BindingOperations.SetBinding(car, Canvas.TopProperty, bY);

                ch.Path = car;

                ch.Location = new Point(left, top);

                MainCanvas.Children.Add(car);

                ch.Angle = 1;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            udp_timer = new Timer(UDP_TimerTick, null, 0,10);
        }

        private void Reset_Clicked(object sender, RoutedEventArgs e)
        {
            Cars.ForEach(c => MainCanvas.Children.Remove(c));
            Cars.Clear();
            CharacterDic.Clear();
            Character = null;
        }
    }
}
