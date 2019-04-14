using System.Linq;
using Math_.net_Core.Math;
using NUnit.Framework;

namespace Math_.net_Core.Test
{
    [TestFixture]
    public class MathTest
    {
        [Test]
        public static void TestSimpson()
        {
            var points = MathHelper.GetInitState(StartProfile.Cos, 1, 1, 0, 2 * System.Math.PI, 0.001);
            Assert.AreEqual(0, MathHelper.IntSimpson(points, 0.001), 0.001);
        }
        
        [Test]
        public static void TestSimpson1()
        {
            var points = Enumerable.Range(0, 200).Select(x => System.Math.Pow(x / 200.0, 2)).ToArray();
            Assert.AreEqual(0.3333333, MathHelper.IntSimpson(points, 1/200.0), 0.01);
        }

        [Test]
        public static void Fourier0()
        {
            var cosX = MathHelper.GetInitState(StartProfile.Cos, 1, 1, 0, 2 * System.Math.PI, 0.001);
            var cos2X = MathHelper.GetInitState(StartProfile.Cos, 2, 1, 0, 2 * System.Math.PI, 0.001);
            var func = cosX.Zip(cos2X, (x, y) => x + y).ToArray();
            Assert.AreEqual(1,MathHelper.GetFourierCoeffCos(1,2 * System.Math.PI,func),0.001);
            Assert.AreEqual(1,MathHelper.GetFourierCoeffCos(2,2 * System.Math.PI,func),0.001);
            Assert.AreEqual(0.0,MathHelper.GetFourierCoeffCos(3,2 * System.Math.PI,func),0.001);
            Assert.AreEqual(0.0,MathHelper.GetFourierCoeffCos(4,2 * System.Math.PI,func),0.001);
            Assert.AreEqual(0.0,MathHelper.GetFourierCoeffCos(5,2 * System.Math.PI,func),0.001);
        }
        
        [Test]
        public static void Fourier1()
        {            
            var cos2X = MathHelper.GetInitState(StartProfile.Cos, 2, 1, 0, 2 * System.Math.PI, 0.001);
            Assert.AreEqual(0,MathHelper.GetFourierCoeffCos(1,2 * System.Math.PI,cos2X),0.001);
            Assert.AreEqual(1,MathHelper.GetFourierCoeffCos(2,2 * System.Math.PI,cos2X),0.001);
            Assert.AreEqual(0.0,MathHelper.GetFourierCoeffCos(3,2 * System.Math.PI,cos2X),0.001);
            Assert.AreEqual(0.0,MathHelper.GetFourierCoeffCos(4,2 * System.Math.PI,cos2X),0.001);
            Assert.AreEqual(0.0,MathHelper.GetFourierCoeffCos(5,2 * System.Math.PI,cos2X),0.001);
        }
        
        [Test]
        public static void Fourier2()
        {
            var sinX = Enumerable.Range(0, 1000).Select(x => System.Math.Sin(x / 1000.0*2*System.Math.PI)).ToArray();
            Assert.AreEqual(0,MathHelper.GetFourierCoeffCos(1,2 * System.Math.PI,sinX),0.01);
            Assert.AreEqual(0,MathHelper.GetFourierCoeffCos(2,2 * System.Math.PI,sinX),0.01);
            Assert.AreEqual(0.0,MathHelper.GetFourierCoeffCos(3,2 * System.Math.PI,sinX),0.01);
            Assert.AreEqual(0.0,MathHelper.GetFourierCoeffCos(4,2 * System.Math.PI,sinX),0.01);
            Assert.AreEqual(0.0,MathHelper.GetFourierCoeffCos(5,2 * System.Math.PI,sinX),0.01);
        }
    }
}