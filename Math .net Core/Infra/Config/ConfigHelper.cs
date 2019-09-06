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