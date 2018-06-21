using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class PIDTest
    {
        const int MaximumAcceleration = 100;

        float KP { get; set; }
        float KI { get; set; }
        float KD { get; set; }
       

        int[] error = { 0, 0 };
        int integral = 0;

        int pid_sample(int target, int actual)
        {
            //static int16_t integral;

            error[0] = error[1];
            error[1] = target - actual;

            //int16_t _integral = (diff[1] + diff[0]);// / 2.0 * 26;//DELTA_T;

            integral += error[1];

            int p = (int)(KP * error[1]);
            int i = (int)(KI * integral * 0.026);
            int d = (int)(KD * (error[1] - error[0]) / 0.026);

            return p + i + d;
        }




        //const int v_precision = 3;

        //static int j = 0;
        //static int j_calc = 0;
        static int CurrentAcceleration = 0;
        static int CurrentVelocity = 0;

        static int TargetVelocity = 0;

        static int v_delta { get { return (TargetVelocity - CurrentVelocity); } }

        static int t = 0;

        static StreamWriter sw = new StreamWriter(".\\plot.dat", false, Encoding.ASCII);
        

        static void print()
        {
            string line =
                t.ToString("D6") + ", " +
                //j.ToString() + ", " +
                //(v_delta != 0 ? ((-1.0) * (a * a) / (2.0 * v_delta)).ToString() : "NaN") + ", " +
                "0, " +
                "N/A, " +
                CurrentAcceleration.ToString("D4") + ", " +
                RpsToARRValue(CurrentVelocity).ToString("D7") + ", " +
                //RpsToARRValue(TargetVelocity).ToString("D7") + ", " +
                CurrentVelocity.ToString("D7") + ", " +
                TargetVelocity.ToString("D7") + ", " +
                "AState:" + astate.ToString() + ", " +
                "VState:" + vstate.ToString();

            sw.WriteLine(line);
            Console.WriteLine(line);
        }

        /// <summary>
        /// タイマ割り込みをシミュレートする．
        /// 頑張って計算する．
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        async Task t2(CancellationToken token)
        {
            await Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    CurrentVelocity += pid_sample(TargetVelocity, CurrentVelocity);


                    switch (astate)
                    {
                        case AState.ZERO: state0();
                            break;
                        case AState.RAMP_UP: state12();
                            break;
                        //case AState.NON_ZERO: state12();
                        //    break;
                        case AState.RAMP_DOWN:state3();
                            break;
                    }

                    //a += j;
                    CurrentVelocity += CurrentAcceleration;
                    t++;

                    print();

                    //Thread.Sleep(1);
                }
            });
        }

        static void Main(string[] args)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            //Task task1 = t1(tokenSource.Token);
            //Task tasko = t_out(tokenSource.Token);

            Stopwatch swatch = new Stopwatch();

            //System.Timers.Timer tim2 = new System.Timers.Timer(1);
            //tim2.Elapsed += t2_worker;
            swatch.Start();
            //tim2.Start();

            Task task2 = t2(tokenSource.Token);

            Thread.Sleep(150);

            TargetVelocity = 7000;

            Thread.Sleep(500);

            //TargetVelocity = -200000;
            while (CurrentVelocity != TargetVelocity) ;
            Thread.Sleep(300);

            TargetVelocity = 0;
            while (CurrentVelocity != TargetVelocity) ;

            Thread.Sleep(150);

            //tim2.Stop();
            tokenSource.Cancel();
            //task1.Wait();
            //tasko.Wait();
            task2.Wait();

            swatch.Stop();

            sw.Flush();
            sw.Close();
            sw.Dispose();
            sw = null;

            Console.WriteLine("total: " + swatch.ElapsedMilliseconds + " [ms]; step: " + ((double)swatch.ElapsedMilliseconds / t).ToString() + " [ms]");

            Process extPro = new Process();
            extPro.StartInfo.FileName = @"C:\Program Files\gnuplot\bin\gnuplot.exe";
            //extPro.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            extPro.StartInfo.UseShellExecute = false;
            extPro.StartInfo.RedirectStandardInput = true;
            extPro.Start();

            StreamWriter gnupStWr = extPro.StandardInput;
            gnupStWr.WriteLine("load \"plot.plt\"");
            gnupStWr.Flush();

            extPro.WaitForExit();
        }
    }
}
