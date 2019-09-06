using System;
using System.Runtime.CompilerServices;

namespace Math_.net_Core.Math
{
    public class AzisIтtegrator : Integrator
    {
        public override string GetName() => "AzisRk";

        private double Du;
        private double Dv;
        private double a;
        private double b;
        private double c;
        private double delta;

        public override double[][] EvaluateAuto(Config config, out double[] resultU, out double[] resultV)
        {
            try
            {
                int length = config.InitStateU.Length;
                var U0 = config.InitStateU;
                var V0 = config.InitStateV;
                var U1 = new double[length];
                var V1 = new double[length];
                Du = config.Parameters["Du"];
                Dv = config.Parameters["Dv"];
                a = config.Parameters["a"];
                b = config.Parameters["b"];
                c = config.Parameters["c"];
                delta = config.Parameters["delta"];

                double[][] evaluation = new double[config.ItersNum / config.TimeLineQuant + 1][];
                var kok = config.ItersNum / 10;
                for (int i = 0; i < config.ItersNum; ++i)
                {
                    if (i % kok == 0 && i > 0)
                        Log.Info($"Calculating with Azis, {(double) i / config.ItersNum:P}");


                    for (int h = 1; h < length - 1; h++)
                    {
                        U1[h] = NewU(U0[h], V0[h], U0[h - 1], U0[h + 1], Du, config.SpaceQuant, config.TimeQuant,
                            MathHelper.GaussRnd() * config.NoiseAmp);
                        V1[h] = NewV(U0[h], V0[h], V0[h - 1], V0[h + 1], Dv, config.SpaceQuant, config.TimeQuant, MathHelper.GaussRnd() * config.NoiseAmp);
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
                        evaluation[i / config.TimeLineQuant] = U0;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double F(double u, double v, double uM, double uP, double dU, double h)
        {
            return u * (1 - u) - a * u * v / (u + b) + dU * (uM - 2 * u + uP) / h / h;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double K2(double u, double v, double uM, double uP, double dU, double h, double t)
        {
            var k1 = F(u, v, uM, uP, dU, h);
            return F(u + t / 2, v + t / 2 * k1,uM, uP, dU, h);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double K3(double u, double v, double uM, double uP, double dU, double h, double t)
        {
            var k2 = K2(u, v, uM, uP, dU, h, t);
            return F(u + t / 2, v + t / 2 * k2,uM, uP, dU, h);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double K4(double u, double v, double uM, double uP, double dU, double h, double t)
        {
            var k3 = K3(u, v, uM, uP, dU, h, t);
            return F(u + t, v + t * k3,uM, uP, dU, h);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double G(double u, double v, double vM, double vP, double dV, double h)
        {
            return delta * v * (1 - v / (u + c)) + dV * (vM - 2 * v + vP) / h / h;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double L2(double u, double v, double vM, double vP, double dV, double h, double t)
        {
            var l1 = G(u, v, vM, vP, dV, h);
            return G(u + t / 2, v + t / 2 * l1,vM, vP, dV, h);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double L3(double u, double v, double vM, double vP, double dV, double h, double t)
        {
            var l2 = L2(u, v, vM, vP, dV, h,t);
            return G(u + t / 2, v + t / 2 * l2,vM, vP, dV, h);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double L4(double u, double v, double vM, double vP, double dV, double h, double t)
        {
            var l3 = L3(u, v, vM, vP, dV, h, t);
            return G(u + t / 2, v + t / 2 * l3,vM, vP, dV, h);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double NewU(double u, double v, double uM, double uP, double dU, double h, double t, double noise = 0)
        {
            return u + t / 6 * (F(u, v, uM, uP, dU, h) + 2 * K2(u, v, uM, uP, dU, h, t) +
                                2 * K3(u, v, uM, uP, dU, h, t) + K4(u, v, uM, uP, dU, h, t)) + noise;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double NewV(double u, double v, double vM, double vP, double dV, double h, double t,
            double noise = 0)
        {
            return v + t / 6 * (G(u, v, vM, vP, dV, h) + 2 * L2(u, v, vM, vP, dV, h, t) +
                                2 * L3(u, v, vM, vP, dV, h, t) + 2 * L4(u, v, vM, vP, dV, h, t)) + noise;
        }
    }
}