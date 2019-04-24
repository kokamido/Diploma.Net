using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using log4net;
using Math_.net_Core.Math;
using Newtonsoft.Json;


namespace Analyse
{
    public class Result
    {
        private static readonly ILog Log;
        private static int koeffsCount = 31;
        private static Config config;
        private static double[,] timeLine;

        private static double[,] fourierCoeffsCos;
        private static double[,] fourierCoeffsSin;
        
        static Result()
        {
            Log = LogManager.GetLogger(typeof(Result));
        }
        public static void Calc(string meta,string data, string outputFolderPath, Func<Config,bool> metaFilter = null,bool skipExist = false)
        {
            var output = Path.Combine(outputFolderPath, data.Split(';')[0].Split('\\').Last() + "_Fourier");
            if (skipExist && File.Exists(output))
            {
                Log.Info($"Skipping {output}");
                return;
            }
            string current = null;
            
            using (var inp = new StreamReader(File.Open(meta, FileMode.Open)))
            {
                try
                {
                    current = inp.ReadLine();
                    config = JsonConvert.DeserializeObject<Config>(current);
                }
                catch(Exception e)
                {
                    Log.Error(e+$"\n{meta}\n'{current}'");
                    return;
                }
            }
            Log.Info($"Processing {config.Id}");
            if (!metaFilter?.Invoke(config) ?? false)
                return;
            using (var inp = new StreamReader(File.Open(data, FileMode.Open)))
            {
                try
                {
                    ReadTimeLine(inp);
                }
                catch(Exception e)
                {
                    Console.WriteLine(config.Id + " "+e);
                    return;
                    
                }
            }
            
            if((int)(config.SpaceRange/config.SpaceQuant) != timeLine.GetLength(1))
                throw new ArgumentException("Init config does not match with end pattern");
            Log.Info($"Writing result to {output}");
            using (var outp = new StreamWriter(File.Open(output, FileMode.OpenOrCreate)))
            {
                var l0 = fourierCoeffsCos.GetLength(0);
                var l1 = fourierCoeffsCos.GetLength(1);
                for (int j = 0; j <= l1; j++)
                {
                    
                    outp.Write("cos");
                    outp.Write(j);
                    outp.Write(" sin");
                    outp.Write(j);
                    outp.Write(" ");
                }
                outp.WriteLine();
                for (int i = 0; i < l0; i++)
                {
                    for (int j = 0; j < l1; j++)
                    {
                        outp.Write(fourierCoeffsCos[i,j]);
                        outp.Write(' ');
                        outp.Write(fourierCoeffsSin[i,j]);
                        outp.Write(' ');
                    }
                    outp.WriteLine();
                }
            }
        }

        private static void ReadTimeLine(StreamReader r)
        {
            Log.Info("reading timeline");
            Console.WriteLine("reading timeline");
            var str = r.ReadLine();
            var buf = new List<string>();
            while (str != null)
            {
                buf.Add(str);
                str = r.ReadLine();
            }
          
            var splitted = buf.First().Split(';');
            var elemsCount = splitted.Length;
            timeLine = new double[buf.Count,elemsCount];
            fourierCoeffsCos = new double[buf.Count,koeffsCount];
            fourierCoeffsSin = new double[buf.Count,koeffsCount];
            for (int i = 0; i < buf.Count; i++)
                {
                if(i%1000 == 0)
                    Console.WriteLine((double)i/buf.Count);
                var nums = buf[i].Split(';').Select(double.Parse).ToArray();
                for (int j = 0; j < elemsCount; j++)
                {
                    timeLine[i, j] = nums[j];
                }

                for (int k = 0; k < koeffsCount; k++)
                {
                    fourierCoeffsCos[i, k] = MathHelper.GetFourierCoeffCos(k, config.SpaceRange, nums);
                    fourierCoeffsSin[i, k] = MathHelper.GetFourierCoeffSin(k, config.SpaceRange, nums);
                }
            }    
        }
    }
}