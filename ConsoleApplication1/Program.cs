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
    static class Program
    {
        const int MaximumAcceleration = 200;
        const int MaximumDeceleration = 300;
        const int MaximumJerk = 20;

        //const int v_precision = 3;

        //static int j = 0;
        //static int j_calc = 0;
        static int CurrentAcceleration = 0;
        static int CurrentVelocity = 0;

        static int TargetVelocity = 0;

        static int v_delta
        {
            get
            {
                return (TargetVelocity - CurrentVelocity);
            }
        }

        static int t = 0;

        //static StreamWriter sw = new StreamWriter(".\\plot.csv", false, Encoding.ASCII);

        /*
        static double RpsToARRValue(double rps)
        {
            // tim2 works with f=500kHz (PSC = 144 - 1)
            // 500000 / 6.4 = 78125
            return (double)((rps > 0) ? (78125 / rps) - 1 : 0);
        }
        */

        static int last_a = 0;

        static void prdouble()
        {
            // t, 0, N/A, a, v, vt, as, vs

            string line =
                t.ToString("D6") + ", " +
                //j.ToString() + ", " +
                //(v_delta != 0 ? ((-1.0) * (a * a) / (2.0 * v_delta)).ToString() : "NaN") + ", " +
                (CurrentAcceleration - last_a).ToString("D4") + ", " +
                "N/A, " +
                CurrentAcceleration.ToString("D4") + ", " +
                //RpsToARRValue(CurrentVelocity).ToString("D7") + ", " +
                //RpsToARRValue(TargetVelocity).ToString("D7") + ", " +
                CurrentVelocity.ToString("D7") + ", " +
                TargetVelocity.ToString("D7") + ", " +
                "AState:" + astate.ToString() + ", " +
                "VState:" + vstate.ToString();

            //sw.WriteLine(line);
            Console.WriteLine(line);

            last_a = CurrentAcceleration;
        }
        
        static AState astate = AState.ZERO;
        static VState vstate = VState.CONST;

        enum AState : int
        {
            ZERO = 0,
            RAMP_UP,
            NON_ZERO,
            RAMP_DOWN
        }

        enum VState : int
        {
            CONST = 0,
            RAMP_UP = 1,
            RAMP_DOWN = -1
        }

#if FALSE

        /// <summary>
        /// 加速度を出来る限り早くゼロにするのに必要なジャーク
        /// これがジャークのリミットを超えると，速度がオーバーシュートする
        /// </summary>
        /// <returns></returns>
        static double calcJerkToBrake()
        {
            return (-1) * (CurrentAcceleration * CurrentAcceleration) / (2 * v_delta);
        }

        static double calcJerktoBrakeOnNextCycle(double candidateJerk)
        {
            double a1 = CurrentAcceleration + candidateJerk;
            return (-1) * (a1 * a1) / (2 * (TargetVelocity - CurrentVelocity - a1));
        }

#endif
        
        /// <summary>
        /// V_RAMPのconvexで，
        /// 最大ジャークで加速度をゼロまで持ってく間の速度変化
        /// </summary>
        /// <returns></returns>
        static double minVelDeltaInConvex()
        {
            return (CurrentAcceleration * CurrentAcceleration) / (2 * MaximumJerk);
        }

        /// <summary>
        /// V_RAMPのconvexにおいて，
        /// 現在の加速度からvelDeltaだけピッタリ変化させるために必要なジャーク
        /// これが最大ジャークを超えると死
        /// </summary>
        /// <returns></returns>
        static double JerkForVelDeltaInConvex(double velocityDelta)
        {
            return -(CurrentAcceleration * CurrentAcceleration) / (2 * velocityDelta);
        }

        /// <summary>
        /// V_RAMPのconvexにおいて，
        /// 現在の加速度からvelDeltaだけピッタリ変化させるために必要なジャーク
        /// これが最大ジャークを超えると死
        /// </summary>
        /// <returns></returns>
        static double NextJerkForVelDeltaInConvex(double velocityDelta)
        {
            var NextAcc = Math.Abs(CurrentAcceleration) - MaximumJerk;
            return -(NextAcc * NextAcc) / (2 * velocityDelta);
        }

        /// <summary>
        /// 加速度はゼロ
        /// </summary>
        static void state0()
        {
            if (v_delta > MaximumJerk)
            {
                //j = j_limit;
                CurrentAcceleration += MaximumJerk;
                astate = AState.RAMP_UP;
                vstate = VState.RAMP_UP;

                return;
            }
            else if(v_delta < -MaximumJerk)
            {
                //j = -j_limit;
                CurrentAcceleration -= MaximumJerk;
                astate = AState.RAMP_UP;
                vstate = VState.RAMP_DOWN;

                return;
            }
            else
            //if (-j_limit <= v_delta && v_delta <= j_limit)
            {
                CurrentVelocity = TargetVelocity;

                astate = AState.ZERO;
                vstate = VState.CONST;

                return;
            }
        }

#if FALSE

        /// <summary>
        /// 加速度がゼロから徐々に大きく/小さくなる．
        /// 状態1からは2または3にのみ移行できる．
        /// </summary>
        static void state1()
        {
            if (vstate == VState.RAMP_UP)
            {
                //if (v_delta == 0 || calcJerkToBrake() <= (1 - j_limit))
                if (v_delta == 0 || calcJerkToBrake() <= -j_limit)
                {
                    j = -j_limit;
                    astate = AState.RAMP_DOWN;

                    return;
                }

                if (a >= maximumAcceleration)
                {
                    j = 0;
                    astate = AState.NON_ZERO;

                    return;
                }

                return;
            }

            if (vstate == VState.RAMP_DOWN)
            {
                //if (v_delta == 0 || calcJerkToBrake() >= j_limit - 1)
                if (v_delta == 0 || calcJerkToBrake() >= j_limit)
                {
                    j = j_limit;
                    astate = AState.RAMP_DOWN;

                    return;
                }

                if (a <= -maximumAcceleration)
                {
                    j = 0;
                    astate = AState.NON_ZERO;

                    return;
                }
            }
        }

        /// <summary>
        /// 加速度はゼロでないが一定
        /// </summary>
        static void state2()
        {
            if (vstate == VState.RAMP_UP)
            {
                if (v_delta <= 0 || calcJerkToBrake() <= -j_limit)
                //if (v_delta <= 0 || calcJerkToBrake() <= (1 - j_limit))
                {
                    j = -j_limit;
                    astate = AState.RAMP_DOWN;

                    return;
                }

                /*
                 * TODO: 加速度のオーバーシュート修正
                 * 
                if (a >= maximumAcceleration)
                {
                    j = 0;

                    return;
                }
                */

                return;
            }

            if (vstate == VState.RAMP_DOWN)
            {
                if (v_delta >= 0 || calcJerkToBrake() >= j_limit)
                //if (v_delta >= 0 || calcJerkToBrake() >= j_limit)
                {
                    j = j_limit;
                    astate = AState.RAMP_DOWN;

                    return;
                }

                /*
                if (a <= -maximumAcceleration)
                {
                    j = 0;
                    astate = AState.NON_ZERO;

                    return;
                }
                */
            }
        }
#endif
#if FALSE
        static void state12()
        {
            if (vstate == VState.RAMP_UP)
            {
                //if (v_delta <= 0 || calcJerkToBrake() <= -MaximumJerk)
                if (v_delta <= 0 || (v_delta - (2 * CurrentAcceleration)) <= minVelDeltaInConvex())
                {
                    // transition from concave or linear to convex

                    //j = -j_limit;
                    CurrentAcceleration -= MaximumJerk;

                    astate = AState.RAMP_DOWN;

                    return;
                }

                if (CurrentAcceleration < MaximumAcceleration - MaximumJerk)
                {
                    // concave

                    //j = j_limit;
                    CurrentAcceleration += MaximumJerk;

                    //astate = AState.RAMP_UP;

                    return;
                }
                //else if (maximumAcceleration - j_limit <= a && a <= maximumAcceleration + j_limit)
                //else if (CurrentAcceleration <= MaximumAcceleration)
                {
                    // linear or transition from concave to linear

                    CurrentAcceleration = MaximumAcceleration;

                    //astate = AState.NON_ZERO;

                    return;
                }
                //else
                {
                    // 

                    //j = -j_limit;
                    //CurrentAcceleration -= MaximumJerk;
                    //astate = AState.NON_ZERO;

                    throw new Exception();

                    return;
                }
            }

            if (vstate == VState.RAMP_DOWN)
            {
                //if (v_delta >= 0 || calcJerkToBrake() >= MaximumJerk)
                if (v_delta >= 0 || ((2 * CurrentAcceleration) - v_delta) <= minVelDeltaInConvex())
                {
                    //j = j_limit;
                    CurrentAcceleration += MaximumJerk;
                    astate = AState.RAMP_DOWN;

                    return;
                }

                if (-MaximumAcceleration + MaximumJerk < CurrentAcceleration)
                {
                    //j = -j_limit;
                    CurrentAcceleration -= MaximumJerk;
                    //astate = AState.RAMP_UP;
                    return;
                }
                //else if (maximumAcceleration - j_limit <= a && a <= maximumAcceleration + j_limit)
                //else if (-MaximumAcceleration <= CurrentAcceleration)
                {
                    CurrentAcceleration = -MaximumAcceleration;
                    //j = 0;
                    //astate = AState.NON_ZERO;

                    return;
                }
                //else
                {
                    //j = j_limit;
                    CurrentAcceleration += MaximumJerk;
                    //astate = AState.NON_ZERO;

                    return;
                }
            }
        }
        

        /// <summary>
        /// convex period
        /// </summary>
        static void state3()
        {
            if (-MaximumJerk <= CurrentAcceleration && CurrentAcceleration <= MaximumJerk)
            {
                // rest

                CurrentAcceleration = 0;
                //j = 0;
                astate = AState.ZERO;

                return;
            }

            if (vstate == VState.RAMP_UP)
            {
                //if (v_delta > 0 && calcJerkToBrake() > -MaximumJerk)
                if (v_delta > 0 && (v_delta - (2 * CurrentAcceleration)) > minVelDeltaInConvex())
                {
                    // concave or linear

                    //j = j_limit;
                    CurrentAcceleration += MaximumJerk;
                    astate = AState.RAMP_UP;

                    return;
                }

                // convex

                //j = -j_limit;
                CurrentAcceleration -= MaximumJerk;
                return;
            }

            if (vstate == VState.RAMP_DOWN)
            {
                //if (v_delta < 0 && calcJerkToBrake() < MaximumJerk)
                if (v_delta < 0 && ((2 * CurrentAcceleration) - v_delta) > minVelDeltaInConvex())
                {
                    // concave or linear

                    //j = -j_limit;
                    CurrentAcceleration -= MaximumJerk;
                    astate = AState.RAMP_UP;

                    return;
                }

                // convex

                //j = j_limit;
                CurrentAcceleration += MaximumJerk;
                return;
            }
        }
#endif

        static void state123_old()
        {
            if (vstate == VState.RAMP_UP)
            {
                // 速度が上昇する状態で：

                //if (v_delta <= 0 || (v_delta - minVelDeltaInConvex() <= 2 * CurrentAcceleration))
                if (v_delta <= 0 || JerkForVelDeltaInConvex(v_delta) >= MaximumJerk)
                {
                    // これ以上の加速は必要ない　または　加速度を落とさないとオーバーシュートしてしまう
                    // ので，加速度を落とす操作：

                    if (-MaximumJerk <= CurrentAcceleration && CurrentAcceleration <= MaximumJerk)
                    {
                        // 現在の加速度が十分小さい（最大躍度より小さい）とき
                        // 加速度をゼロにしてよい；速度一定となる

                        // rest
                        CurrentAcceleration = 0;
                        astate = AState.ZERO;
                        return;
                    }
                    else
                    {
                        // ゼロ時間で加速度をゼロにはできないので，最大躍度で加速度を落とす

                        // convex
                        CurrentAcceleration -= MaximumJerk;
                        return;
                    }
                }

                int _maxAccelDecel = (CurrentVelocity >= 0) ? MaximumAcceleration : MaximumDeceleration;

                if (CurrentAcceleration < _maxAccelDecel - MaximumJerk)
                {
                    // 加速度を落とす必要はない
                    // ので，攻める．

                    // concave
                    CurrentAcceleration += MaximumJerk;
                    return;
                }
                else if (CurrentAcceleration <= _maxAccelDecel + MaximumJerk)
                {
                    // ゼロ時間で最大加速にする

                    // linear
                    CurrentAcceleration = _maxAccelDecel;
                    return;
                }
                else
                {
                    // 加速度大きすぎる
                    // きっと最大ジャークの範囲内で加速度を落とす操作をすべきなのだろう

                    // linear
                    CurrentAcceleration = _maxAccelDecel;
                    return;
                }
            }

            if (vstate == VState.RAMP_DOWN)
            {
                int _v_delta;
                if (CurrentVelocity * TargetVelocity < 0)
                {
                    _v_delta = -CurrentVelocity;
                }
                else
                {
                    _v_delta = v_delta;
                }

                //if (_v_delta >= 0 || ((2 * CurrentAcceleration) - _v_delta) <= minVelDeltaInConvex())
                if (_v_delta >= 0 || JerkForVelDeltaInConvex(_v_delta) < -MaximumJerk)
                {
                    // rest or convex
                    if (-MaximumJerk <= CurrentAcceleration && CurrentAcceleration <= MaximumJerk)
                    {
                        // rest
                        CurrentAcceleration = 0;
                        astate = AState.ZERO;
                        return;
                    }
                    else
                    {
                        // concave
                        CurrentAcceleration += MaximumJerk;
                        return;
                    }
                }

                int _maxAccelDecel = (CurrentVelocity <= 0) ? MaximumAcceleration : MaximumDeceleration;

                if (-_maxAccelDecel + MaximumJerk < CurrentAcceleration)
                {
                    CurrentAcceleration -= MaximumJerk;
                    return;
                }
                else if (-_maxAccelDecel - MaximumJerk > CurrentAcceleration)
                {
                    CurrentAcceleration += MaximumJerk;
                    return;
                }
                else
                {
                    CurrentAcceleration = -_maxAccelDecel;
                    return;
                }
            }
        }

        static void state123()
        {
            int delta_v;
            if (CurrentVelocity * TargetVelocity < 0)
            {
                delta_v = -CurrentVelocity;
            }
            else
            {
                delta_v = TargetVelocity - CurrentVelocity;
            }

            if(TargetVelocity - CurrentVelocity >= 0)
            {
                vstate = VState.RAMP_UP;
            }
            else
            {
                vstate = VState.RAMP_DOWN;
            }

            if (vstate == VState.RAMP_UP)
            {
                // 速度が上昇する状態で：
                
                if ((int)(JerkForVelDeltaInConvex(delta_v)) <= -MaximumJerk)
                {
                    // これ以上の加速は必要ない　または　加速度を落とさないとオーバーシュートしてしまう
                    // （というか，オーバーシュートは確実かも）
                    // ので，加速度を落とす操作：

                    if (-MaximumJerk <= CurrentAcceleration && CurrentAcceleration <= MaximumJerk)
                    {
                        // 現在の加速度が十分小さい（最大躍度より小さい）とき
                        // 加速度をゼロにしてよい；速度一定となる

                        // rest
                        CurrentAcceleration = 0;
                        astate = AState.ZERO;
                        return;
                    }
                    else
                    {
                        // ゼロ時間で加速度をゼロにはできないので，最大躍度で加速度を落とす

                        // convex
                        CurrentAcceleration -= MaximumJerk;
                        return;
                    }
                }

                int _maxAccelDecel = (CurrentVelocity >= 0) ? MaximumAcceleration : MaximumDeceleration;

                if (CurrentAcceleration < _maxAccelDecel - MaximumJerk)
                {
                    // 加速度を落とす必要はない
                    // ので，攻める．

                    // concave
                    CurrentAcceleration += MaximumJerk;
                    return;
                }
                else if (CurrentAcceleration > _maxAccelDecel + MaximumJerk)
                {
                    // linear
                    CurrentAcceleration -= MaximumJerk;
                    return;
                }
                else
                {
                    // 加速度大きすぎる
                    // きっと最大ジャークの範囲内で加速度を落とす操作をすべきなのだろう

                    // linear
                    CurrentAcceleration = _maxAccelDecel;
                    return;
                }
            }

            if (vstate == VState.RAMP_DOWN)
            {
                if ((int)(JerkForVelDeltaInConvex(delta_v)) >= MaximumJerk)
                {
                    // rest or convex
                    if (-MaximumJerk <= CurrentAcceleration && CurrentAcceleration <= MaximumJerk)
                    {
                        // rest
                        CurrentAcceleration = 0;
                        astate = AState.ZERO;
                        return;
                    }
                    else
                    {
                        // concave
                        CurrentAcceleration += MaximumJerk;
                        return;
                    }
                }
                else if (JerkForVelDeltaInConvex(delta_v) - 1 < -MaximumJerk)
                {
                    CurrentAcceleration += -(int)JerkForVelDeltaInConvex(delta_v);
                }

                int _maxAccelDecel = (CurrentVelocity <= 0) ? MaximumAcceleration : MaximumDeceleration;

                if (-_maxAccelDecel + MaximumJerk < CurrentAcceleration)
                {
                    //CurrentAcceleration += (int)Math.Round(JerkForVelDeltaInConvex(delta_v));
                    CurrentAcceleration -= MaximumJerk;
                    return;
                }
                //else if (JerkForVelDeltaInConvex(delta_v) > -MaximumJerk)
                //{
                //    CurrentAcceleration += MaximumJerk;
                //    return;
                //}
                else
                {
                    CurrentAcceleration = -_maxAccelDecel;
                    return;
                }
            }
        }

        static object locker = new object();
        
        /// <summary>
        /// タイマ割り込みをシミュレートする．
        /// 頑張って計算する．
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        static async Task t2(CancellationToken token)
        {
            await Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    switch (astate)
                    {
                        case AState.ZERO: state0();
                            break;
                        case AState.RAMP_UP: state123();
                            break;
                        //case AState.NON_ZERO: state12();
                        //    break;
                        //case AState.RAMP_DOWN:state3();
                        //    break;
                    }

                    //a += j;
                    CurrentVelocity += CurrentAcceleration;
                    t++;

                    prdouble();

                    //Thread.Sleep(1);
                }
            });
        }

        static void wait_t(double _t)
        {
            double _t0 = t;
            while (t < _t0 + _t) ;
            return;
        }

        static void Main(string[] args)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            //Task task1 = t1(tokenSource.Token);
            //Task tasko = t_out(tokenSource.Token);
            Stopwatch swatch = new Stopwatch();
            /*

            //System.Timers.Timer tim2 = new System.Timers.Timer(1);
            //tim2.Elapsed += t2_worker;
            swatch.Start();
            //tim2.Start();

            Task task2 = t2(tokenSource.Token);

            wait_t(100);

            TargetVelocity = 15000;
            //while (CurrentVelocity != TargetVelocity) ;
            wait_t(250);

            TargetVelocity = 10000;
            //while (CurrentVelocity != TargetVelocity) ;
            wait_t(150);

            TargetVelocity = -10000;
            //while (CurrentVelocity != TargetVelocity) ;
            wait_t(300);

            TargetVelocity = 0;
            //while (CurrentVelocity != TargetVelocity) ;
            wait_t(150);

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
            */

            FifthOrderMotionController fo = new FifthOrderMotionController();
            fo.SetTarget(new WayPoint() { Position = 1000, Velocity = 0, Acceleration = 0 });
            //var traj = fo.GetTrajectory().ToList();
            /*
            double _t = 0;
            const double dt = 0.01;
            double last_pos = 0.0;
            double last_vel = 0.0;
            double last_acc = 0.0;
            double last_jerk = 0.0;

            foreach (var p in traj)
            {
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
            }
            */

            //sw.Flush();
            //sw.Close();
            //sw.Dispose();

            Console.WriteLine("total: " + swatch.ElapsedMilliseconds + " [ms]; step: " + ((double)swatch.ElapsedMilliseconds / t).ToString() + " [ms/step]");

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
