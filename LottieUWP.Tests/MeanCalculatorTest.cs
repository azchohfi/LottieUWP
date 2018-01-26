using LottieUWP.Utils;
using Xunit;

namespace LottieUWP.Tests
{
    public class MeanCalculatorTest
    {
        private readonly MeanCalculator _meanCalculator;

        public MeanCalculatorTest()
        {
            _meanCalculator = new MeanCalculator();
        }

        [Fact]
        public void TestMeanWithNoNumbers()
        {
            Assert.Equal(0f, _meanCalculator.Mean);
        }

        [Fact]
        public void TestMeanWithOneNumber()
        {
            _meanCalculator.Add(2);
            Assert.Equal(2f, _meanCalculator.Mean);
        }

        [Fact]
        public void TestMeanWithTwoNumbers()
        {
            _meanCalculator.Add(2);
            _meanCalculator.Add(4);
            Assert.Equal(3f, _meanCalculator.Mean);
        }

        [Fact]
        public void TestMeanWithTwentyNumbers()
        {
            for (int i = 1; i <= 20; i++)
            {
                _meanCalculator.Add(i);
            }
            Assert.Equal(10.5f, _meanCalculator.Mean);
        }

        [Fact]
        public void TestMeanWithHugeNumber()
        {
            _meanCalculator.Add(int.MaxValue - 1);
            _meanCalculator.Add(int.MaxValue - 1);
            Assert.Equal(int.MaxValue - 1, _meanCalculator.Mean);
        }

        [Fact]
        public void TestMeanWithHugeNumberAndNegativeHugeNumber()
        {
            _meanCalculator.Add(int.MaxValue - 1);
            _meanCalculator.Add(int.MaxValue - 1);
            _meanCalculator.Add(-int.MaxValue + 1);
            _meanCalculator.Add(-int.MaxValue + 1);
            Assert.Equal(0f, _meanCalculator.Mean);
        }
    }
}
