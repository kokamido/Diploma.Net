using System;
using System.Linq;
using System.Runtime.CompilerServices;
using MathNet.Numerics.Distributions;

namespace Math_.net_Core.Math
{
    public static class MathHelper
    {
        [ThreadStatic] private static Normal randGauss;
        [ThreadStatic] private static DiscreteUniform randUniform;

        public static double[] GetInitState(StartProfile profile, double picks, double amp, double yAvg, double range,
            double step)
        {
            switch (profile)
            {
                case StartProfile.Cos:
                    return GetCos(picks, step, range, yAvg, amp, true);
                case StartProfile.CosReverse:
                    return GetCos(picks, step, range, yAvg, amp, false);
                case StartProfile.Sin:
                    return GetSin(picks, step, range, yAvg, amp, true);
                case StartProfile.SinReverse:
                    return GetSin(picks, step, range, yAvg, amp, false);
                case StartProfile.Rnd:
                    return GetRand(step, range, yAvg, amp);
                case StartProfile.Homo:
                    return Enumerable.Repeat(yAvg, (int) (range / step)).ToArray();
                default:
                    throw new ArgumentException("Incorrect start profile");
            }
        }

        private static double[] GetCos(double picks, double step, double range, double yAvg, double amp, bool reverse)
        {
            var count = (int)(range / step);
            double[] res = new double[count];
            for (int i = 0; i < count; i++)
                res[i] = (reverse ? 1 : -1)*amp*System.Math.Cos(2 * System.Math.PI / (count-1) * i  * picks )+yAvg;
            return res;
        }
        
        private static double[] GetSin(double picks, double step, double range, double yAvg, double amp, bool reverse)
        {
            var count = (int)(range / step);
            double[] res = new double[count];
            for (int i = 0; i < count; i++)
                res[i] = (reverse ? 1 : -1)*amp*System.Math.Sin(2 * System.Math.PI / (count-1) * i  * picks )+yAvg;
            return res;
        }

        private static double[] GetRand(double step, double range, double yAvg, double amp)
        {
            var count = range / step;
            double[] res = new double[(int)(range/step)];
            for (int i = 0; i < count; i++)
                res[i] = amp*GaussRnd()+yAvg;
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetRandomInt()
        {
            if(randUniform == null)
                randUniform = new DiscreteUniform(int.MinValue+1,int.MaxValue-1);
            return randUniform.Sample();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GaussRnd()
        {
            if (randGauss == null)
                randGauss = new Normal(0, stddev: 1);
            return randGauss.Sample();
        }

        public static double IntSimpson(double[] vals, double step)
        {
            double sum = 0;
            for (int i = 0; i < vals.Length-2; i+=2)
                sum += IntSimpsonInternal(vals[i], vals[i+1], vals[i+2], step * 2);
            return sum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double IntSimpsonInternal(double f_x1, double f_x2, double f_x3, double step)
        {
            return step / 6 * (f_x1 + 4 * f_x2 + f_x3);
        }

        public static double GetFourierCoeffCos(int k, double range, double[] func)
        {
            var step = range / func.Length;
            var forInt = GetInitState(StartProfile.Cos, k, 1, 0, range, step)
                .Zip(func,(fst,snd)=>fst*snd).ToArray();
            return IntSimpson(forInt, step)/System.Math.PI;
        }
        public static double GetFourierCoeffSin(int k, double range, double[] func)
        {
            var step = range / func.Length;
            var forInt = GetInitState(StartProfile.Sin, k, 1, 0, range, step)
                .Zip(func,(fst,snd)=>fst*snd).ToArray();
            return IntSimpson(forInt, step)/System.Math.PI;
        }
    }
}