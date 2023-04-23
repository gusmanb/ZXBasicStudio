using NUnit.Framework;

namespace Konamiman.Z80dotNet.Tests
{
    public class HelloWorld
    {
        [Test]
        public void HelloWorldTest()
        {
            var Sut = new Z80Processor();
            Sut.AutoStopOnRetWithStackEmpty = true;

            var program = new byte[]
            {
                0x3E, 0x07, //LD A,7
                0xC6, 0x04, //ADD A,4
                0x3C,       //INC A
                0xC9        //RET
            };
            Sut.Memory.SetContents(0, program);
            //hola

            Sut.Start();

            Assert.AreEqual(12, Sut.Registers.A);
            Assert.AreEqual(28, Sut.TStatesElapsedSinceStart);
        }
    }
}
