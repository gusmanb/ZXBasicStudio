using NUnit.Framework;

namespace Konamiman.Z80dotNet.Tests
{
    public class BitTests
    {
        [Test]
        public void Default_constructor_creates_instance_with_value_zero()
        {
            Assert.AreEqual(0, new Bit().Value);
        }

        [Test]
        public void Can_implicitly_convert_to_int()
        {
            int theInt = new Bit(0);
            Assert.AreEqual(0, theInt);

            theInt = new Bit(1);
            Assert.AreEqual(1, theInt);
        }

        [Test]
        public void Can_implicitly_convert_from_zero_int_to_zero_bit()
        {
            Bit theBit = 0;
            Assert.AreEqual(theBit.Value, 0);
        }

        [Test]
        public void Can_implicitly_convert_from_any_nonzero_int_to_one_bit()
        {
            Bit theBit = 34;
            Assert.AreEqual(theBit.Value, 1);
        }
        
        [Test]
        public void Can_implicity_convert_to_and_from_bool()
        {
            Assert.IsTrue(new Bit(1));
            Assert.IsFalse(new Bit(0));

            Bit theTrue = true;
            Bit theFalse = false;
            Assert.AreEqual(1, theTrue.Value);
            Assert.AreEqual(0, theFalse.Value);
        }

        [Test]
        public void Can_create_one_instance_from_another()
        {
            Bit zero = 0;
            Bit one = 1;

            Assert.AreEqual(zero, new Bit(zero));
            Assert.AreEqual(one, new Bit(one));
        }

        [Test]
        public void Can_compare_to_other_bit_with_equals_sign()
        {
            Assert.True(new Bit(0) == new Bit(0));
            Assert.True(new Bit(1) == new Bit(1));
            Assert.False(new Bit(0) == new Bit(1));
        }

        [Test]
        public void Can_compare_to_other_bit_with_equals_method()
        {
            Assert.True(new Bit(0).Equals(new Bit(0)));
            Assert.True(new Bit(1).Equals(new Bit(1)));
            Assert.False(new Bit(0).Equals(new Bit(1)));
        }

        [Test]
        public void Can_compare_for_equality_to_zero_int()
        {
            int zero = 0;
            int nonZero = 34;

            Assert.True(new Bit(0) == zero);
            Assert.True(new Bit(0).Equals(zero));

            Assert.True(zero == new Bit(0));
            Assert.True(zero.Equals(new Bit(0)));

            Assert.False(new Bit(0) == nonZero);
            Assert.False(new Bit(0).Equals(nonZero));

            Assert.False(nonZero == new Bit(0));
            Assert.False(nonZero.Equals(new Bit(0)));
        }

        [Test]
        public void Can_compare_for_equality_to_non_zero_int()
        {
            int zero = 0;
            int nonZero = 34;

            Assert.True(new Bit(1) == nonZero);
            Assert.True(new Bit(1).Equals(nonZero));

            Assert.True(nonZero == new Bit(1));
            //Assert.True(nonZero.Equals(new Bit(1)));

            Assert.False(new Bit(1) == zero);
            Assert.False(new Bit(1).Equals(zero));

            Assert.False(zero == new Bit(1));
            //Assert.False(zero.Equals(new Bit(1)));
        }

        [Test]
        public void Can_compare_for_inequality_to_zero_int()
        {
            int zero = 0;
            int nonZero = 34;

            Assert.True(new Bit(1) != zero);
            Assert.True(zero != new Bit(1));

            Assert.False(new Bit(1) != nonZero);
            Assert.False(nonZero != new Bit(1));
        }

        [Test]
        public void Can_convert_to_bool()
        {
            Assert.False(new Bit(0));
            Assert.True(new Bit(1));
        }

        [Test]
        public void Can_do_arithmetic_OR()
        {
            Bit zero = 0;
            Bit one = 1;

            Assert.False(zero | zero);
            Assert.True(zero | one);
            Assert.True(one | zero);
            Assert.True(one | one);
        }

        [Test]
        public void Can_do_logical_OR()
        {
            Bit zero = 0;
            Bit one = 1;

            Assert.False(zero || zero);
            Assert.True(zero || one);
            Assert.True(one || zero);
            Assert.True(one || one);
        }

        [Test]
        public void Can_do_arithmetic_AND()
        {
            Bit zero = 0;
            Bit one = 1;

            Assert.False(zero & zero);
            Assert.False(zero & one);
            Assert.False(one & zero);
            Assert.True(one & one);
        }

        [Test]
        public void Can_do_logical_AND()
        {
            Bit zero = 0;
            Bit one = 1;

            Assert.False(zero && zero);
            Assert.False(zero && one);
            Assert.False(one && zero);
            Assert.True(one && one);
        }

        [Test]
        public void Can_do_arithmetic_XOR()
        {
            Bit zero = 0;
            Bit one = 1;

            Assert.False(zero ^ zero);
            Assert.True(zero ^ one);
            Assert.True(one ^ zero);
            Assert.False(one ^ one);
        }

        [Test]
        public void Can_do_arithmetic_NOT()
        {
            Bit zero = 0;
            Bit one = 1;

            Assert.AreEqual(zero, ~one);
            Assert.AreEqual(one, ~zero);
        }

        [Test]
        public void Can_do_logical_NOT()
        {
            Bit zero = 0;
            Bit one = 1;

            Assert.AreEqual(zero, !one);
            Assert.AreEqual(one, !zero);
        }
    }
}
