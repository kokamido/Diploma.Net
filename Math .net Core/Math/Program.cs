using System;
using System.IO;
using System.Linq;
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
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static string resFolder;
        private static double totalToProcess;
        private static int totalProcessed;

        private static int DegreeOfParallelism()
        {
            try
            {
                var dop = int.Parse(File.ReadAllText("DOP"));
                log.Info($"DegreeOfParallelism: {dop}");
                return dop;
            }
            catch (Exception e)
            {
                log.Warn("Can't read degree of parallelism from file");
                log.Warn(e);
                return 4;
            }
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException("set mode and config");

            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo(Path.Combine(Directory.GetCurrentDirectory(),"log4net.config")));

            switch (args[0])
            {
                case "auto":
                    AutoMode();
                    break;
                case "interactive":
                    InteractiveMode(args.Skip(1).ToArray());
                    break;
            }
        }

        private static void InteractiveMode(string[] args)
        {
        }

        private static void AutoMode()
        {
            try
            {
                log.Info("Start auto mode");
                resFolder = $"res{DateTime.Now:dd.MM.yyyyThh_mm_ss}";
                if (!Directory.Exists(resFolder))
                    Directory.CreateDirectory(resFolder);
                var confs = Enumerable.Range(1, 20)
                    .Select(n => n / 2.0)
                    .Populate(ConfigHelper.Default, (p, c) => c.InitStateConfig.Picks = p)
                    .SelectMany(c =>
                        new[] {StartProfile.Cos, StartProfile.CosReverse}.Populate(c,
                            (p, conf) => conf.InitStateConfig.ProfileType = p))
                    .SelectMany(c => new[] {0.0, 0.0005, 0.001}.Populate(c, (n, conf) => conf.NoiseAmp = n))
                    .SelectMany(c =>
                        Enumerable.Range(28, 22).Select(n => n / 2.0).Populate(c, (du, conf) => conf.Du = du))
                    .Select(c =>
                    {
                        c.ApplyInitStateConfig();
                        return c;
                    })
                    .SelectMany(c => new[]
                    {
                        c.GetModifiedCopy(cn =>
                        {
                            cn.InitStateConfig.Integrator = IntegratorType.Rk;
                            if (cn.Du > 20)
                            {
                                cn.NoiseAmp /= 4.0;
                                cn.TimeQuant = 0.001 / 4;
                                cn.ItersNum = 2000000 * 4;
                                cn.TimeLineQuant = 400;
                            }
                            else
                            {
                                cn.TimeQuant = 0.001;
                                cn.ItersNum = 2000000;
                                cn.TimeLineQuant = 100;
                            }
                        }),
                        c.GetModifiedCopy(cn =>
                        {
                            cn.InitStateConfig.Integrator = IntegratorType.Net;
                            if (cn.Du > 20)
                            {
                                cn.NoiseAmp /= 20.0;
                                cn.TimeQuant = 0.001 / 20;
                                cn.ItersNum = 2000000 * 20;
                                cn.TimeLineQuant = 100 * 20;
                            }
                            else
                            {
                                cn.NoiseAmp /= 5.0;
                                cn.TimeQuant = 0.001 / 5;
                                cn.ItersNum = 2000000 * 5;
                                cn.TimeLineQuant = 100 * 5;
                            }
                        })
                    })
                    .Select(c => c.GetModifiedCopy(cn => cn.Id = MathHelper.GetRandomInt()))
                    .OrderBy(c => c.InitStateConfig.Integrator == IntegratorType.Rk ? 0 : 1)
                    .ToArray();
                foreach (var c in confs)
                {
                    log.Info(c.ToString());
                }

                log.Info($"Total tasks {confs.Length}");

                totalToProcess = confs.Length;
                confs.Select(c =>
                    {
                        log.Info(JsonConvert.SerializeObject(c.InitStateConfig));
                        return Start(c);
                    })
                    .AsParallel()
                    .AsOrdered()
                    .WithDegreeOfParallelism(6)
                    .ForAll(t => t.RunSynchronously());
            }
            catch (Exception e)
            {
                log.Fatal(e);
            }
        }

        public static Task Start(Config config)
        {
            Integrator integrator;
            switch (config.InitStateConfig.Integrator)
            {
                case IntegratorType.Rk:
                    integrator = new RkMethod();
                    break;
                case IntegratorType.Net:
                    integrator = new NetMethod();
                    break;
                default:
                    integrator = new RkMethod();
                    break;
            }

            return new Task(() =>
                {
                    try
                    {
                        log.Info($"Start {config}");
                        var i = Interlocked.Increment(ref totalProcessed);
                        log.Info(i / totalToProcess);
                        double[] resU;
                        double[] resV;
                        var res = integrator.EvaluateAuto(config, out resU, out resV);
                        var kek = new Result
                        {
                            Config = config,
                            FinalU = resU,
                            FinalV = resV,
                            TimeLine = res
                        };
                        kek.Serialize(resFolder);
                    }
                    catch (Exception e)
                    {
                        log.Fatal(e);
                    }
                }
            );
        }
    }
}