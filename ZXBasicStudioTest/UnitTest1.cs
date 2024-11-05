namespace ZXBasicStudioTest
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            int a = 10;
            int b = 20;

            Assert.NotEqual(a, b);
        }
    }
}