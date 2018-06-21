using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class FifthOrderMotionController
    {
        public FifthOrderMotionController()
        {
            this.CurrentPosition = 0;
            this.CurrentVelocity = 0;
            this.CurrentAcceleration = 0;

            Begin = new WayPoint();

            this.stepper = new StepperController();

            this.timer = new Timer(10);

            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (index + 1 < Trajectory.Count)
            {
                stepper.TargetPosition = (int)(GetTrajectory().ElementAt(index));
                index++;
            }
            else
            {
                timer.Stop();
                //index = 0;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">このセグメントの終端条件を規定する点．</param>
        public void SetTarget(WayPoint target)
        {
            // this should be current state
            Begin.Position = CurrentPosition;
            Begin.Velocity = CurrentVelocity;
            Begin.Acceleration = CurrentAcceleration;

            End = target;

            k[0] = (0.5 * (End.Acceleration - Begin.Acceleration)) - (3.0 * (End.Velocity + Begin.Velocity)) + (6.0 * (End.Position - Begin.Position));
            k[1] = (-1.0 * End.Acceleration) + (1.5 * Begin.Acceleration) + (7.0 * End.Velocity) + (2.0 * Begin.Velocity) - (15.0 * (End.Position - Begin.Position));
            k[2] = (0.5 * End.Acceleration) - (1.5 * Begin.Acceleration) - (4.0 * End.Velocity) - (6.0 * Begin.Velocity) + (10 * (End.Position - Begin.Position));
            k[3] = 0.5 * Begin.Acceleration;
            k[4] = Begin.Velocity;
            k[5] = Begin.Position;

            this.Trajectory = GetTrajectory().ToList();
            index = 0;

            timer.Start();
        }

        private IEnumerable<double> GetTrajectory()
        {
            double t = 0;
            const double dt = 0.001;

            while(t < 1.0)
            {
                yield return (k[0] * Math.Pow(t, 5)) + (k[1] * Math.Pow(t, 4)) + (k[2] * Math.Pow(t, 3))
                    + (k[3] * Math.Pow(t, 2)) + (k[4] * t) + k[5];

                t += dt;
            }
        }

        private double[] k = new double[6];

        private int index;

        private Timer timer;

        private StepperController stepper;

        private WayPoint Begin { get; set; }
        private WayPoint End { get; set; }

        public int CurrentPosition { get; set; }
        public int CurrentVelocity { get; set; }
        public int CurrentAcceleration { get; set; }
        public int MaximumVelocity { get; set; }
        public int MaximumAcceleration { get; set; }

        public List<double> Trajectory { get; private set; }
    }
}
