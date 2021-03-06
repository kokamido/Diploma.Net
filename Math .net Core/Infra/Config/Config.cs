using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Math_.net_Core.Math
{
    public class Result
    {
        public Config Config;
        public double[] FinalU;
        public double[] FinalV;
        public double[][] TimeLine;
        private string Filename => Config.Filename;

        public void Serialize(string folder)
        {
            using (var writer = new StreamWriter(File.Open(Path.Combine(folder, Filename + "_meta"), FileMode.Create)))
            {
                if (FinalU == null || FinalV == null || TimeLine == null)
                {
                    writer.Write("Обосрамс");
                    return;
                }

                writer.Write(JsonConvert.SerializeObject(Config));
                writer.Write("\n");
                writer.Write(JsonConvert.SerializeObject(FinalU));
                writer.Write("\n");
                writer.Write(JsonConvert.SerializeObject(FinalV));
                writer.Write("\n");
            }

            using (var writer = new StreamWriter(File.Open(Path.Combine(folder, Filename + "_data"), FileMode.Create)))
            {
                if (FinalU == null || FinalV == null || TimeLine == null)
                {
                    writer.Write("Обосрамс");
                    return;
                }

                foreach (var slice in TimeLine)
                {
                    if (slice != null)
                    {
                        writer.Write(string.Join(";", slice));
                        writer.Write("\n");
                    }
                }
            }
        }
    }

    public enum StartProfile
    {
        None = 0,
        Homo,
        Cos,
        CosReverse,
        Sin,
        SinReverse,
        Rnd
    }

    public class InitStateConfig
    {
        public StartProfile ProfileType;
        public double AvgU;
        public double AvgV;
        public double Amp;
        public double Picks;
        public IntegratorType Integrator;
    }

    public class InteractiveSessionConfig
    {
        public Config InitConfig;
        public string OutFile;
    }

    public class Config
    {
        public int Id;
        public double[] InitStateU;
        public double[] InitStateV;
        public Dictionary<string, double> Parameters;
        public double SpaceQuant;
        public double SpaceRange;
        public double TimeQuant;
        public double NoiseAmp;
        public int ItersNum;
        public InitStateConfig InitStateConfig;
        public int TimeLineQuant = 10;

        public override string ToString()
        {
            var a = GetModifiedCopy();
            a.InitStateU = null;
            a.InitStateV = null;
            return JsonConvert.SerializeObject(a);
        }

        public Config GetModifiedCopy(Action<Config> a = null)
        {
            var copy = JsonConvert.DeserializeObject<Config>(JsonConvert.SerializeObject(this));
            a?.Invoke(copy);
            return copy;
        }

        public static Config FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Config>(json);
        }

        public Config ApplyInitStateConfig()
        {
            Id = MathHelper.GetRandomInt();
            if (InitStateU == null || InitStateU.Length==0)
            {
                InitStateU = MathHelper.GetInitState(InitStateConfig.ProfileType, InitStateConfig.Picks,
                    InitStateConfig.Amp, InitStateConfig.AvgU, SpaceRange, SpaceQuant);
            }
            if (InitStateV == null || InitStateV.Length==0)
            {
                InitStateV = MathHelper.GetInitState(InitStateConfig.ProfileType, InitStateConfig.Picks,
                    InitStateConfig.Amp, InitStateConfig.AvgV, SpaceRange, SpaceQuant);
            }
            return this;
        }

        [JsonIgnore] public string Filename => $"{Id}; {DateTime.Now:yyyy-MM-ddThh_mm_ss_fff}";

        private static double[] GetDataFromFile(string path)
        {
            Console.WriteLine(path);
            var text = File.ReadAllText(path);
            Console.WriteLine(text);
            return text.Split(' ').Where(s => !string.IsNullOrEmpty(s)).Select(double.Parse).ToArray();
        }
    }
}