using System;
using System.Runtime.CompilerServices;
using log4net;

namespace Math_.net_Core.Math
{
    public abstract class Integrator
    {
        protected readonly ILog Log;

        protected Integrator()
        {
            Log = LogManager.GetLogger(GetType());
        }

        public abstract double[][] EvaluateAuto(Config config, out double[] resultU, out double[] resultV);
        public abstract string GetName();
    }

    public enum IntegratorType
    {
        None,
        Rk,
        Net,
        Azis
    }

    public class NetMethod : Integrator
    {
        public override double[][] EvaluateAuto(Config config, out double[] resultU, out double[] resultV)
        {
            try
            {
                int length = config.InitStateU.Length;
                var U0 = config.InitStateU;
                var V0 = config.InitStateV;
                var U1 = new double[length];
                var V1 = new double[length];
                double[][] evaluation = new double[config.ItersNum / config.TimeLineQuant + 1][];
                double sq2 = config.SpaceQuant * config.SpaceQuant;
                double Du = config.Parameters["Du"];
                double Dv = config.Parameters["Dv"];
                double p = config.Parameters["p"];;
                double q = config.Parameters["q"];
                double qPlus1 = q + 1;
                double tq = config.TimeQuant;
                double nAmp = config.NoiseAmp;
                for (int i = 0; i < config.ItersNum; ++i)
                {
                    if (i % 1000000 == 0 && i > 0)
                        Log.Info($"Calculating with NetMethod, {(double) i / config.ItersNum:P}");

                    for (int h = 1; h < length - 1; h++)
                    {
                        U1[h] = (1 - U0[h] * V0[h] + Du * (U0[h - 1] - 2 * U0[h] + U0[h + 1]) / sq2)
                                * tq + U0[h] + MathHelper.GaussRnd() * nAmp;
                        V1[h] = (p * V0[h] * (U0[h] - qPlus1 / (q + V0[h])) + Dv *
                                 (V0[h - 1] - 2 * V0[h] + V0[h + 1]) / sq2) * tq
                                + V0[h] + MathHelper.GaussRnd() * nAmp;
                    }

                    if (i % 10000 == 0)
                    {
                        foreach (var t in U1)
                        {
                            if (double.IsNaN(t) || double.IsInfinity(t))
                            {
                                Log.Error($"{GetName()} messed up {config}");
                                resultU = null;
                                resultV = null;
                                return null;
                            }
                        }
                    }

                    U1[0] = U1[1];
                    U1[length - 1] = U1[length - 2];
                    V1[0] = V1[1];
                    V1[length - 1] = V1[length - 2];
                    Array.Copy(U1, U0, U0.Length);
                    Array.Copy(V1, V0, V0.Length);
                    if (i % config.TimeLineQuant == 0)
                        evaluation[i/config.TimeLineQuant]=U0;
                }

                Log.Info($"End calculating {GetName()} {config}");
                resultU = U0;
                resultV = V0;
                return evaluation;
            }
            catch (Exception e)
            {
                resultU = null;
                resultV = null;
                Log.Fatal(e);
                return null;
            }
        }

        public override string GetName()
        {
            return "Net";
        }
    }

    public class RkMethod : Integrator
    {
        public override double[][] EvaluateAuto(Config config, out double[] resultU, out double[] resultV)
        {
            try
            {
                int length = config.InitStateU.Length;
                var U0 = config.InitStateU;
                var V0 = config.InitStateV;
                var U1 = new double[length];
                var V1 = new double[length];
                double Du = config.Parameters["Du"];
                double Dv = config.Parameters["Dv"];
                double p = config.Parameters["p"];
                double q = config.Parameters["q"];
                double[][] evaluation = new double[config.ItersNum / config.TimeLineQuant + 1][];
                var kok = config.ItersNum / 10;
                for (int i = 0; i < config.ItersNum; ++i)
                {
                    if (i % kok == 0 && i > 0)
                        Log.Info($"Calculating with RKMehod, {(double) i / config.ItersNum:P}");


                    for (int h = 1; h < length - 1; h++)
                    {
                        U1[h] = NewU(U0[h], V0[h], U0[h - 1], U0[h + 1], Du, config.SpaceQuant, config.TimeQuant,
                            MathHelper.GaussRnd() * config.NoiseAmp*U0[h]);
                        V1[h] = NewV(U0[h], V0[h], V0[h - 1], V0[h + 1], Dv, p, q,
                            config.SpaceQuant, config.TimeQuant, MathHelper.GaussRnd() * config.NoiseAmp*V0[h]);
                    }

                    if (i % 5000 == 0)
                    {
                        for (int kek = 0; kek < U1.Length; kek++)
                        {
                            if (double.IsNaN(U1[kek]) || double.IsInfinity(U1[kek]))
                            {
                                Log.Error($"{GetName()} messed up {config}");
                                resultU = null;
                                resultV = null;
                                return null;
                            }
                        }
                    }

                    U1[0] = U1[1];
                    U1[length - 1] = U1[length - 2];
                    V1[0] = V1[1];
                    V1[length - 1] = V1[length - 2];
                    U0 = U1;
                    U1 = new double[length];
                    V0 = V1;
                    V1 = new double[length];
                    if (i % config.TimeLineQuant == 0)
                        evaluation[i/config.TimeLineQuant] = U0;
                }

                Log.Info($"End calculating {GetName()} {config}");
                resultU = U0;
                resultV = V0;
                return evaluation;
            }
            catch (Exception e)
            {
                resultU = null;
                resultV = null;
                Log.Fatal(e);
                return null;
            }
        }

        public override string GetName() => "RK4";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double K1(double u, double v, double uM, double uP, double dU, double h)
        {
            return 1 - u * v + dU * (uM - 2 * u + uP) / h / h;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double K2(double u, double v, double uM, double uP, double dU, double h, double t)
        {
            return 1 - (u + t / 2 * K1(u, v, uM, uP, dU, h)) * (v + t / 2) +
                   dU * (uM - 2 * u + uP) / h / h;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double K3(double u, double v, double uM, double uP, double dU, double h, double t)
        {
            return 1 - (u + t / 2 * K2(u, v, uM, uP, dU, h, t)) * (v + t / 2) +
                   dU * (uM - 2 * u + uP) / h / h;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double K4(double u, double v, double uM, double uP, double dU, double h, double t)
        {
            return 1 - (u + t * K3(u, v, uM, uP, dU, h, t)) * (v + t) +
                   dU * (uM - 2 * u + uP) / h / h;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double L1(double u, double v, double vM, double vP, double dV, double p, double q, double h)
        {
            return p * v * (u - (1 + q) / (q + v)) + dV * (vM - 2 * v + vP) / h / h;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double L2(double u, double v, double vM, double vP, double dV, double p, double q, double h, double t)
        {
            return p * (v + t / 2 * L1(u, v, vM, vP, dV, p, q, h)) *
                   ((u + t / 2) - (1 + q) / (q + (v + t / 2 * L1(u, v, vM, vP, dV, p, q, h)))) +
                   dV * (vM - 2 * v + vP) / h / h;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double L3(double u, double v, double vM, double vP, double dV, double p, double q, double h, double t)
        {
            return p * (v + t / 2 * L2(u, v, vM, vP, dV, p, q, h, t)) *
                   ((u + t / 2) - (1 + q) / (q + (v + t / 2 * L2(u, v, vM, vP, dV, p, q, h, t)))) +
                   dV * (vM - 2 * v + vP) / h / h;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double L4(double u, double v, double vM, double vP, double dV, double p, double q, double h, double t)
        {
            return p * (v + t * L3(u, v, vM, vP, dV, p, q, h, t)) *
                   ((u + t) - (1 + q) / (q + (v + t * L3(u, v, vM, vP, dV, p, q, h, t)))) +
                   dV * (vM - 2 * v + vP) / h / h;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double NewU(double u, double v, double uM, double uP, double dU, double h, double t, double noise = 0)
        {
            return u + t / 6 * (K1(u, v, uM, uP, dU, h) + 2 * K2(u, v, uM, uP, dU, h, t) +
                                2 * K3(u, v, uM, uP, dU, h, t) + K4(u, v, uM, uP, dU, h, t)) + noise;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double NewV(double u, double v, double vM, double vP, double dV, double p, double q, double h, double t,
            double noise = 0)
        {
            return v + t / 6 * (L1(u, v, vM, vP, dV, p, q, h) + 2 * L2(u, v, vM, vP, dV, p, q, h, t) +
                                2 * L3(u, v, vM, vP, dV, p, q, h, t) + 2 * L4(u, v, vM, vP, dV, p, q, h, t)) + noise;
        }
    }
}