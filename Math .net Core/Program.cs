using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using log4net.Config;

namespace Math_.net_Core.Math
{
    public static class Program
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static string resFolder;

        public static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository,
                new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "log4net.config")));
            var exp = new Experiment($"AzisSlowFast_{DateTime.Now:yyyy_MM_ddThh_mm_ss}");
            var config = new Config
            {
                Id=1,
                Parameters = new Dictionary<string, double>
                {
                    ["a"] = 1,
                    ["b"] = 0.08,
                    ["c"] = 0.01,
                    ["delta"] = 0.5,
                    ["Du"] = 0.2,
                    ["Dv"] = 1.0,
                },
                SpaceQuant = 0.05,
                SpaceRange = 50,
                TimeQuant = 0.00025,
                ItersNum = 4000000,
                TimeLineQuant = 400,
                InitStateConfig = new InitStateConfig
                {
                    ProfileType = StartProfile.Cos,
                    AvgU = 0.227581763,
                    AvgV = 0.237581763,
                    Amp = 0.1,
                    Integrator = IntegratorType.Azis
                }
            };
            var configs = new List<Config>{config.GetModifiedCopy(co =>
                {
                    co.Parameters["Du"] = 0.4;
                    co.SpaceQuant = 0.01;
                    co.TimeQuant = 0.00001;
                    co.ItersNum = 40000000;
                    co.TimeLineQuant = 10000;
                })}.Concat(
                config
                .InfinityRepeat()
                .Take(20)
                .Select((c, i) =>
                {
                    var a = c.GetModifiedCopy(co => co.InitStateConfig.Picks = i / 2.0);
                    var e = a.GetModifiedCopy(co => co.Parameters["Du"] = 0.1);
                    return new[]{e,a};
                })
                .SelectMany(x=>x))
                .ToArray();
            exp.RunWithConfig(configs,5);
        }
    }
}