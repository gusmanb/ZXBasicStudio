using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using System;

namespace Konamiman.Z80dotNet.Tests
{
    public class Z80RegistersTests
    {
        private Fixture Fixture { get; set; }
        Z80Registers Sut { get; set; }

        [SetUp]
        public void Setup()
        {
            Fixture = new Fixture();
            Sut = new Z80Registers();
        }

        [Test]
        public void Alternate_registers_are_properly_set()
        {
            Assert.IsInstanceOf<MainZ80Registers>(Sut.Alternate);
        }

        [Test]
        public void Can_set_Alternate_to_non_null_value()
        {
            var value = new Mock<IMainZ80Registers>().Object;
            Sut.Alternate = value;
            Assert.AreEqual(value, Sut.Alternate);
        }

        [Test]
        public void Cannot_set_Alternate_to_null()
        {
            Assert.Throws<ArgumentNullException>(() => Sut.Alternate = null);
        }

        [Test]
        public void Gets_IXh_and_IXl_correctly_from_IX()
        {
            var IXh = Fixture.Create<byte>();
            var IXl = Fixture.Create<byte>();
            var IX = NumberUtils.CreateShort(IXl, IXh);

            Sut.IX = IX;

            Assert.AreEqual(IXh, Sut.IXH);
            Assert.AreEqual(IXl, Sut.IXL);
        }

        [Test]
        public void Sets_IX_correctly_from_IXh_and_IXl()
        {
            var IXh = Fixture.Create<byte>();
            var IXl = Fixture.Create<byte>();
            var expected = NumberUtils.CreateShort(IXl, IXh);

            Sut.IXH = IXh;
            Sut.IXL = IXl;

            Assert.AreEqual(expected, Sut.IX);
        }

        [Test]
        public void Gets_IYh_and_IYl_correctly_from_IY()
        {
            var IYh = Fixture.Create<byte>();
            var IYl = Fixture.Create<byte>();
            var IY = NumberUtils.CreateShort(IYl, IYh);

            Sut.IY = IY;

            Assert.AreEqual(IYh, Sut.IYH);
            Assert.AreEqual(IYl, Sut.IYL);
        }

        [Test]
        public void Sets_IY_correctly_from_IYh_and_IYl()
        {
            var IYh = Fixture.Create<byte>();
            var IYl = Fixture.Create<byte>();
            var expected = NumberUtils.CreateShort(IYl, IYh);

            Sut.IYH = IYh;
            Sut.IYL = IYl;

            Assert.AreEqual(expected, Sut.IY);
        }

        [Test]
        public void Gets_I_and_R_correctly_from_IR()
        {
            var I = Fixture.Create<byte>();
            var R = Fixture.Create<byte>();
            var IR = NumberUtils.CreateShort(R, I);

            Sut.IR = IR;

            Assert.AreEqual(I, Sut.I);
            Assert.AreEqual(R, Sut.R);
        }

        [Test]
        public void Sets_IR_correctly_from_I_and_R()
        {
            var I = Fixture.Create<byte>();
            var R = Fixture.Create<byte>();
            var expected = NumberUtils.CreateShort(R, I);

            Sut.I = I;
            Sut.R = R;

            Assert.AreEqual(expected, Sut.IR);
        }
    }
}
