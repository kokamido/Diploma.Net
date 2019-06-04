using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework.Constraints;

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
            TimeQuant = 0.001,
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

        public static IEnumerable<T> InfinityRepeat<T>(this T t)
        {
            while (true)
            {
                yield return t;
            }
        }

        public static Config ApplyInitStateFromMeta(this Config c, string pathToMeta)
        {
            var res = File.ReadAllLines(pathToMeta);
            c.InitStateU = JsonConvert.DeserializeObject<double[]>(res[1]);
            c.InitStateV = JsonConvert.DeserializeObject<double[]>(res[2]);
            return c;
        }
    }
}