using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using log4net.Config;
using Math_.net_Core.Math;
using Newtonsoft.Json;

namespace Analyse
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo(Path.Combine(Directory.GetCurrentDirectory(),"log4net.config")));
            var inputFolder = args[0];
            var outputFolder = args[1];
            var files = Directory.GetFiles(inputFolder).ToArray();
            Console.WriteLine($"{inputFolder} | {outputFolder}");
            foreach (var id in new HashSet<string>(files.Select(f => f.Split(';')[0])))
            {
                var metaAndData = files.Where(f => f.StartsWith(id)).OrderBy(x => x).ToArray();
                Result.Calc(metaAndData.First(s => s.EndsWith("_meta")),
                    metaAndData.First(s => s.EndsWith("_data")), 
                    outputFolder, 
                    config => config.InitStateConfig.Integrator == IntegratorType.Rk,
                    true);
            }
        }
    }
}