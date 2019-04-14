using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace Test_.net
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Vector<double>.Count);
            var arr = Enumerable.Range(0, 100).Select(x => (double)x).ToArray();
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 100000; i++)
            {
                for (var j = 1; j < arr.Length - 1; j++)
                {
                    arr[j] = Math.Pow(arr[j - 1] * arr[j] - arr[j + 1], 2);
                }
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            sw.Reset();
            
            Vector<double> vec = new Vector<double>(new Span<double>(arr));
            var len = Vector<double>.Count;
            sw.Start();
            for (int i = 0; i < 100000; i++)
            {
                for (var j = 1; j < arr.Length - len; j+=len)
                {
                    Math.Pow(vec[j - 1] * vec[j] - vec[j + 1], 2);
                }
            }
            
        }
        public int Vectors() {
            int vectorSize = Vector<int>.Count;
            var accVector = Vector<int>.Zero;
            int i;
            var array = Enumerable.Range(0, 100).ToArray();;
            for (i = 0; i < array.Length - vectorSize; i += vectorSize) {
                var v = new Vector<int>(array, i);
                accVector = Vector.Add(accVector, v);
            }
            int result = Vector.Dot(accVector, Vector<int>.One);
            for (; i < array.Length; i++) {
                result += array[i];
            }
            return result;
        }
    }
}