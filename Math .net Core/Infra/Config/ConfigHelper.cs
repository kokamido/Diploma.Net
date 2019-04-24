using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Math_.net_Core.Math
{
    public static class ConfigHelper
    {
        public static Config Default => new Config
        {
            Du = 2,
            Dv = 1,
            p = 2,
            q = 2,
            SpaceRange = 40,
            SpaceQuant = 0.2,
            TimeQuant = 0.01,
            NoiseAmp = 0,
            ItersNum = 2000000,
            TimeLineQuant = 100,
            InitStateConfig = new InitStateConfig
            {
                ProfileType = StartProfile.Cos,
                Avg = 1,
                Amp = 0.5,
                Picks = 2.5,
                Integrator = IntegratorType.Rk
            }
        };

        public static IEnumerable<Config> Populate<T>(this IEnumerable<T> args, Config config, Action<T, Config> func)
        {
            return args.Select(a =>
            {
                var buf = config.GetModifiedCopy();
                func(a, buf);
                return buf;
            });
        }
    }
}