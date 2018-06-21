using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class StepperController
    {
        static StreamWriter sw = new StreamWriter(".\\plot.csv", false, Encoding.ASCII);

        public StepperController()
        {
            this.CurrentPosition = 0;
            this.timer = new Timer(timer_Callback, null, 0, 1);
        }

        double _t = 0;
        const double dt = 0.01;
        double last_pos = 0.0;
        double last_vel = 0.0;
        double last_acc = 0.0;
        double last_jerk = 0.0;


        private int i;

        private void timer_Callback(object state)
        {
            i++;
            if(i > 10)
            {
                i = 0;
                slope = 0;
            }

            subPoint += slope * 0.001;
            CurrentPosition = (int)(subPoint);
            //subPoint -= (int)subPoint;

            double p = subPoint;

            // t, 0, N/A, a, v, vt, as, vs
            double vel = ((p - last_pos) / dt);
            double acc = ((vel - last_vel) / dt);
            double jerk = ((acc - last_acc) / dt);
            double snap = ((jerk - last_jerk) / dt);

            string line =
                _t.ToString("F7") + ", " +
                p.ToString("F7") + ", " +
                vel.ToString("F7") + ", " +
                acc.ToString("F7") + ", " +
                jerk.ToString("F7") + ", " +
                snap.ToString("F7") + ", ";

            last_jerk = jerk;
            last_acc = acc;
            last_vel = vel;
            last_pos = p;
            _t += dt;

            sw.WriteLine(line);
            Console.WriteLine(line);
            sw.Flush();
        }

        public int CurrentPosition { get; set; }
        public int TargetPosition
        {
            get
            {
                return _targetPosition;
            }
            set
            {
                _targetPosition = value;

                // 0.01秒毎にモーションコントローラから位置指令がくるはず
                slope = (_targetPosition - CurrentPosition) / 0.01;
                i = 0;
            }
        }
        
        private Timer timer;
        private int _targetPosition;
        private double slope;
        private double subPoint;
    }
}
