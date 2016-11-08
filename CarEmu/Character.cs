using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace CarEmu
{
    public class Character : PropertyChangedBase
    {
        public Path Path { get; set; }

        public bool IsMovingForward { get; set; }
        public int RotateDirection { get; set; }
        public int RotationSpeed { get; set; }
        public double MovementSpeed { get; set; }

        public int WantedAngle { get; set; }

        public static Random random = new Random();

        public Timer MoveTimer;

        public int ForwardStopWatch { get; set; }


        private String m_MsgContent;

        public String MsgContent
        {
            get { return m_MsgContent; }
            set
            {
                m_MsgContent = value;
                OnPropertyChanged("MsgContent");
            }
        }



        public Character()
        {
            MoveTimer = new Timer(x => Move(), null, 0,1);
        }

        private Point _location;
        public Point Location
        {
            get { return _location; }
            set
            {
                _location = value;
                OnPropertyChanged("Location");
                Console.WriteLine(value.X + "," + value.Y);
            }
        }

        private double _angle;
        public double Angle
        {
            get { return _angle; }
            set
            {
                _angle = value;
                OnPropertyChanged("Angle");
            }
        }


        private void Move()
        {
            if (Angle > 359)
            {
                Angle = 0;
            }
            if (Angle < 0)
            {
                Angle = 359;
            }

            if (RotateDirection < 0)
                Angle = Angle - RotationSpeed;
            else if (RotateDirection > 0)
                Angle = Angle + RotationSpeed;

            //Console.WriteLine(String.Format("WantedAngle={0}   , Angle={1}",WantedAngle,Angle));

            if (WantedAngle > 358)
            {
                WantedAngle = 358;
            }

            if (WantedAngle == Angle)
            {
                RotateDirection = 0;
            }



            //Console.WriteLine(Angle.ToString() + "," + WantedAngle.ToString());

            if (IsMovingForward || ForwardStopWatch > 0)
            {
                var radians = (Math.PI / 180) * Angle;

                var vector = new Vector
                {
                    X = Math.Sin(radians),
                    Y = -Math.Cos(radians)
                };

                Location += (vector * MovementSpeed);

                if (Path != null && Application.Current != null)
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        Canvas.SetLeft(Path, Location.X);
                        Canvas.SetTop(Path, Location.Y);
                    });
                }

                --ForwardStopWatch;
            }

            
        }
    }
}
