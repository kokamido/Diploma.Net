using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Newtonsoft.Json;

namespace Math_.net_Core.Math
{
    public class Program
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static string resFolder;

        public static void Main(string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException("set mode and config");

            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository,
                new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "log4net.config")));

            switch (args[0])
            {
                case "auto":
                    AutoMode(args);
                    break;
                case "onerun":
                    OneRunMode(args);
                    break;
                case "multiplerun":
                    MultipleRunMode(args);
                    break;
            }
        }

        private static void MultipleRunMode(string[] args)
        {
            log.Info("Start multiple run mode");
            resFolder = $"res{DateTime.Now:dd.MM.yyyyThh_mm_ss}";
            if (!Directory.Exists(resFolder))
                Directory.CreateDirectory(resFolder);
            var config = Config.FromJson(File.ReadAllText(args[1]));
            var configs = Enumerable.Range(0, int.Parse(args[2]))
                .Select(c => config.GetModifiedCopy())
                .Select(c => c.ApplyInitStateConfig())
                .ToArray();
            new Experiment(args[4]).RunWithConfig(configs, int.Parse(args[3]));
        }

        private static void OneRunMode(string[] args)
        {
            log.Info("Start one run mode");
            resFolder = $"res{DateTime.Now:dd.MM.yyyyThh_mm_ss}";
            if (!Directory.Exists(resFolder))
                Directory.CreateDirectory(resFolder);
            new Experiment(args[2]).RunWithConfig(Config.FromJson(File.ReadAllText(args[1])));
        }

        private static void AutoMode(string[] args)
        {
            try
            {
                log.Info("Start auto mode");
                resFolder = $"res{DateTime.Now:dd.MM.yyyyThh_mm_ss}";
                if (!Directory.Exists(resFolder))
                    Directory.CreateDirectory(resFolder);
                Config[] confs = new[] {1, 2.5}
                    .Populate(ConfigHelper.Default, (p, c) =>
                    {
                        c.Du = 16.5;
                        c.InitStateConfig.Integrator = IntegratorType.Rk;
                        c.InitStateConfig.Picks = p;
                    })
                    .ToArray();

               
                new Experiment(resFolder).RunWithConfig(confs, int.Parse(args[1]));
            }
            catch (Exception e)
            {
                log.Fatal(e);
            }
        }
    }
}