using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;

namespace Math_.net_Core.Math
{
    public class Experiment
    {
        public readonly string OutFolder;
        
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private int totalProcessed;
        private int totalToProcess;

        public Experiment(string outFolder)
        {
            if (Directory.Exists(outFolder))
            {
                log.Info($"Out folder '{outFolder}' is already exists and contains {Directory.GetFiles(outFolder).Length} files");
            }
            else
            {
                Directory.CreateDirectory(outFolder);
                log.Info($"Config folder '{outFolder}' has been created");
            }

            OutFolder = outFolder;
        }

        public void Run()
        {
            
        }
        
        public void RunWithConfig(Config[] configs, int degreeOfParalellism)
        {
            foreach (var c in configs)
            {
                c.ApplyInitStateConfig();
                log.Info(c.ToString());
            }
            
            totalToProcess = configs.Length;
            log.Info($"Total tasks {totalToProcess}");
            
            configs.Select(c =>
                {
                    log.Info(JsonConvert.SerializeObject(c.InitStateConfig));
                    return RunWithConfig(c);
                })
                .AsParallel()
                .AsOrdered()
                .WithDegreeOfParallelism(degreeOfParalellism)
                .ForAll(t => t.RunSynchronously());          
        }

        public Task RunWithConfig(Config config)
        {
            config.ApplyInitStateConfig();
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
                        kek.Serialize(OutFolder);
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