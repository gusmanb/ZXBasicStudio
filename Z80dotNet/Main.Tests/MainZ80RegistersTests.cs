using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests
{
    public class MainZ80RegistersTests
    {
        private Fixture Fixture { get; set; }
        MainZ80Registers Sut { get; set; }

        [SetUp]
        public void Setup()
        {
            Fixture = new Fixture();
            Sut = new MainZ80Registers();
        }

        [Test]
        public void Gets_A_and_F_correctly_from_AF()
        {
            var A = Fixture.Create<byte>();
            var F = Fixture.Create<byte>();
            var AF = NumberUtils.CreateShort(F, A);

            Sut.AF = AF;

            Assert.AreEqual(A, Sut.A);
            Assert.AreEqual(F, Sut.F);
        }

        [Test]
        public void Sets_AF_correctly_from_A_and_F()
        {
            var A = Fixture.Create<byte>();
            var F = Fixture.Create<byte>();
            var expected = NumberUtils.CreateShort(F, A);

            Sut.A = A;
            Sut.F = F;

            Assert.AreEqual(expected, Sut.AF);
        }

        [Test]
        public void Gets_B_and_C_correctly_from_BC()
        {
            var B = Fixture.Create<byte>();
            var C = Fixture.Create<byte>();
            var BC = NumberUtils.CreateShort(C, B);

            Sut.BC = BC;

            Assert.AreEqual(B, Sut.B);
            Assert.AreEqual(C, Sut.C);
        }

        [Test]
        public void Sets_BC_correctly_from_B_and_C()
        {
            var B = Fixture.Create<byte>();
            var C = Fixture.Create<byte>();
            var expected = NumberUtils.CreateShort(C, B);

            Sut.B = B;
            Sut.C = C;

            Assert.AreEqual(expected, Sut.BC);
        }

        [Test]
        public void Gets_D_and_E_correctly_from_DE()
        {
            var D = Fixture.Create<byte>();
            var E = Fixture.Create<byte>();
            var DE = NumberUtils.CreateShort(E, D);

            Sut.DE = DE;

            Assert.AreEqual(D, Sut.D);
            Assert.AreEqual(E, Sut.E);
        }

        [Test]
        public void Sets_DE_correctly_from_D_and_E()
        {
            var D = Fixture.Create<byte>();
            var E = Fixture.Create<byte>();
            var expected = NumberUtils.CreateShort(E, D);

            Sut.D = D;
            Sut.E = E;

            Assert.AreEqual(expected, Sut.DE);
        }

        [Test]
        public void Gets_H_and_L_correctly_from_HL()
        {
            var H = Fixture.Create<byte>();
            var L = Fixture.Create<byte>();
            var HL = NumberUtils.CreateShort(L, H);

            Sut.HL = HL;

            Assert.AreEqual(H, Sut.H);
            Assert.AreEqual(L, Sut.L);
        }

        [Test]
        public void Sets_HL_correctly_from_H_and_L()
        {
            var H = Fixture.Create<byte>();
            var L = Fixture.Create<byte>();
            var expected = NumberUtils.CreateShort(L, H);

            Sut.H = H;
            Sut.L = L;

            Assert.AreEqual(expected, Sut.HL);
        }

        [Test]
        public void Gets_CF_correctly_from_F()
        {
            Sut.F = 0xFE;
            Assert.AreEqual(0, Sut.CF);

            Sut.F = 0x01;
            Assert.AreEqual(1, Sut.CF);
        }

        [Test]
        public void Sets_F_correctly_from_CF()
        {
            Sut.F = 0xFF;
            Sut.CF = 0;
            Assert.AreEqual(0xFE, Sut.F);

            Sut.F = 0x00;
            Sut.CF = 1;
            Assert.AreEqual(0x01, Sut.F);
        }

        [Test]
        public void Gets_NF_correctly_from_F()
        {
            Sut.F = 0xFD;
            Assert.AreEqual(0, Sut.NF);

            Sut.F = 0x02;
            Assert.AreEqual(1, Sut.NF);
        }

        [Test]
        public void Sets_F_correctly_from_NF()
        {
            Sut.F = 0xFF;
            Sut.NF = 0;
            Assert.AreEqual(0xFD, Sut.F);

            Sut.F = 0x00;
            Sut.NF = 1;
            Assert.AreEqual(0x02, Sut.F);
        }

        [Test]
        public void Gets_PF_correctly_from_F()
        {
            Sut.F = 0xFB;
            Assert.AreEqual(0, Sut.PF);

            Sut.F = 0x04;
            Assert.AreEqual(1, Sut.PF);
        }

        [Test]
        public void Sets_F_correctly_from_PF()
        {
            Sut.F = 0xFF;
            Sut.PF = 0;
            Assert.AreEqual(0xFB, Sut.F);

            Sut.F = 0x00;
            Sut.PF = 1;
            Assert.AreEqual(0x04, Sut.F);
        }

        [Test]
        public void Gets_Flag3_correctly_from_F()
        {
            Sut.F = 0xF7;
            Assert.AreEqual(0, Sut.Flag3);

            Sut.F = 0x08;
            Assert.AreEqual(1, Sut.Flag3);
        }

        [Test]
        public void Sets_F_correctly_from_Flag3()
        {
            Sut.F = 0xFF;
            Sut.Flag3 = 0;
            Assert.AreEqual(0xF7, Sut.F);

            Sut.F = 0x00;
            Sut.Flag3 = 1;
            Assert.AreEqual(0x08, Sut.F);
        }

        [Test]
        public void Gets_HF_correctly_from_F()
        {
            Sut.F = 0xEF;
            Assert.AreEqual(0, Sut.HF);

            Sut.F = 0x10;
            Assert.AreEqual(1, Sut.HF);
        }

        [Test]
        public void Sets_F_correctly_from_HF()
        {
            Sut.F = 0xFF;
            Sut.HF = 0;
            Assert.AreEqual(0xEF, Sut.F);

            Sut.F = 0x00;
            Sut.HF = 1;
            Assert.AreEqual(0x10, Sut.F);
        }

        [Test]
        public void Gets_Flag5_correctly_from_F()
        {
            Sut.F = 0xDF;
            Assert.AreEqual(0, Sut.Flag5);

            Sut.F = 0x20;
            Assert.AreEqual(1, Sut.Flag5);
        }

        [Test]
        public void Sets_F_correctly_from_Flag5()
        {
            Sut.F = 0xFF;
            Sut.Flag5 = 0;
            Assert.AreEqual(0xDF, Sut.F);

            Sut.F = 0x00;
            Sut.Flag5 = 1;
            Assert.AreEqual(0x20, Sut.F);
        }

        [Test]
        public void Gets_ZF_correctly_from_F()
        {
            Sut.F = 0xBF;
            Assert.AreEqual(0, Sut.ZF);

            Sut.F = 0x40;
            Assert.AreEqual(1, Sut.ZF);
        }

        [Test]
        public void Sets_F_correctly_from_ZF()
        {
            Sut.F = 0xFF;
            Sut.ZF = 0;
            Assert.AreEqual(0xBF, Sut.F);

            Sut.F = 0x00;
            Sut.ZF = 1;
            Assert.AreEqual(0x40, Sut.F);
        }

        [Test]
        public void Gets_SF_correctly_from_F()
        {
            Sut.F = 0x7F;
            Assert.AreEqual(0, Sut.SF);

            Sut.F = 0x80;
            Assert.AreEqual(1, Sut.SF);
        }

        [Test]
        public void Sets_F_correctly_from_SF()
        {
            Sut.F = 0xFF;
            Sut.SF = 0;
            Assert.AreEqual(0x7F, Sut.F);

            Sut.F = 0x00;
            Sut.SF = 1;
            Assert.AreEqual(0x80, Sut.F);
        }
    }
}
