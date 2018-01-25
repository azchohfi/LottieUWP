using LottieUWP.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LottieUWP.Tests
{
    [TestClass]
    public class LottieValueAnimatorUnitTest
    {
        private readonly LottieValueAnimator _animator;

        public LottieValueAnimatorUnitTest()
        {
            _animator = new LottieValueAnimator
            {
                CompositionDuration = 1000
            };
        }
        
        [TestMethod]
        public void TestInitialState()
        {
            Assert.AreEqual(0f, _animator.Value);
        }

        [TestMethod]
        public void TestResumingMaintainsValue()
        {
            _animator.Value = 0.5f;
            _animator.ResumeAnimation();
            Assert.AreEqual(0.5f, _animator.Value);
        }

        [TestMethod]
        public void TestPlayingResetsValue()
        {
            _animator.Value = 0.5f;
            _animator.PlayAnimation();
            Assert.AreEqual(0f, _animator.Value);
            Assert.AreEqual(0f, _animator.AnimatedFraction);
        }

        [TestMethod]
        public void TestReversingMaintainsValue()
        {
            _animator.Value = 0.25f;
            _animator.ReverseAnimationSpeed();
            Assert.AreEqual(0.25f, _animator.Value);
            Assert.AreEqual(0.75f, _animator.AnimatedFraction);
        }

        [TestMethod]
        public void TestReversingWithMinValueMaintainsValue()
        {
            _animator.MinValue = 0.1f;
            _animator.Value = 1f;
            _animator.ReverseAnimationSpeed();
            Assert.AreEqual(1f, _animator.Value);
            Assert.AreEqual(0f, _animator.AnimatedFraction);
        }

        [TestMethod]
        public void TestReversingWithMaxValueMaintainsValue()
        {
            _animator.MaxValue = 0.9f;
            _animator.ReverseAnimationSpeed();
            Assert.AreEqual(0f, _animator.Value);
            Assert.AreEqual(1f, _animator.AnimatedFraction);
        }

        [TestMethod]
        public void TestResumeReversingWithMinValueMaintainsValue()
        {
            _animator.MaxValue = 0.9f;
            _animator.ReverseAnimationSpeed();
            _animator.ResumeAnimation();
            Assert.AreEqual(0.9f, _animator.Value);
            Assert.AreEqual(0f, _animator.AnimatedFraction);
        }

        [TestMethod]
        public void TestPlayReversingWithMinValueMaintainsValue()
        {
            _animator.MaxValue = 0.9f;
            _animator.ReverseAnimationSpeed();
            _animator.PlayAnimation();
            Assert.AreEqual(0.9f, _animator.Value);
            Assert.AreEqual(0f, _animator.AnimatedFraction);
        }

        [TestMethod]
        public void TestMinAndMaxBothSet()
        {
            _animator.MinValue = 0.2f;
            _animator.MaxValue = 0.8f;
            _animator.Value = 0.4f;
            Assert.AreEqual(0.33f, _animator.AnimatedFraction, 0.01);
            _animator.ReverseAnimationSpeed();
            Assert.AreEqual(0.4f, _animator.Value, 0.01);
            Assert.AreEqual(0.66f, _animator.AnimatedFraction, 0.01);
            _animator.ResumeAnimation();
            Assert.AreEqual(0.4f, _animator.Value, 0.01);
            Assert.AreEqual(0.66f, _animator.AnimatedFraction, 0.01);
            _animator.PlayAnimation();
            Assert.AreEqual(0.8f, _animator.Value);
            Assert.AreEqual(0f, _animator.AnimatedFraction);
        }
    }
}
