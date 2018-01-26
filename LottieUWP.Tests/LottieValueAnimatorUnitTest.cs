using System;
using LottieUWP.Utils;
using Xunit;

namespace LottieUWP.Tests
{
    public class LottieValueAnimatorUnitTest : IDisposable
    {
        private LottieValueAnimator _animator;

        public LottieValueAnimatorUnitTest()
        {
            _animator = new LottieValueAnimator
            {
                CompositionDuration = 1000
            };
        }

        public void Dispose()
        {
            _animator.Cancel();
            _animator = null;
        }

        [Fact]
        public void TestInitialState()
        {
            Assert.Equal(0f, _animator.Value);
        }

        [Fact]
        public void TestResumingMaintainsValue()
        {
            _animator.Value = 0.5f;
            _animator.ResumeAnimation();
            Assert.Equal(0.5f, _animator.Value);
        }

        [Fact]
        public void TestPlayingResetsValue()
        {
            _animator.Value = 0.5f;
            _animator.PlayAnimation();
            Assert.Equal(0f, _animator.Value);
            Assert.Equal(0f, _animator.AnimatedFraction);
        }

        [Fact]
        public void TestReversingMaintainsValue()
        {
            _animator.Value = 0.25f;
            _animator.ReverseAnimationSpeed();
            Assert.Equal(0.25f, _animator.Value);
            Assert.Equal(0.75f, _animator.AnimatedFraction);
        }

        [Fact]
        public void TestReversingWithMinValueMaintainsValue()
        {
            _animator.MinValue = 0.1f;
            _animator.Value = 1f;
            _animator.ReverseAnimationSpeed();
            Assert.Equal(1f, _animator.Value);
            Assert.Equal(0f, _animator.AnimatedFraction);
        }

        [Fact]
        public void TestReversingWithMaxValueMaintainsValue()
        {
            _animator.MaxValue = 0.9f;
            _animator.ReverseAnimationSpeed();
            Assert.Equal(0f, _animator.Value);
            Assert.Equal(1f, _animator.AnimatedFraction);
        }

        [Fact]
        public void TestResumeReversingWithMinValueMaintainsValue()
        {
            _animator.MaxValue = 0.9f;
            _animator.ReverseAnimationSpeed();
            _animator.ResumeAnimation();
            Assert.Equal(0.9f, _animator.Value);
            Assert.Equal(0f, _animator.AnimatedFraction);
        }

        [Fact]
        public void TestPlayReversingWithMinValueMaintainsValue()
        {
            _animator.MaxValue = 0.9f;
            _animator.ReverseAnimationSpeed();
            _animator.PlayAnimation();
            Assert.Equal(0.9f, _animator.Value);
            Assert.Equal(0f, _animator.AnimatedFraction);
        }

        [Fact]
        public void TestMinAndMaxBothSet()
        {
            _animator.MinValue = 0.2f;
            _animator.MaxValue = 0.8f;
            _animator.Value = 0.4f;
            Assert.Equal(0.33f, _animator.AnimatedFraction, 2);
            _animator.ReverseAnimationSpeed();
            Assert.Equal(0.4f, _animator.Value, 2);
            Assert.Equal(0.666f, _animator.AnimatedFraction, 2);
            _animator.ResumeAnimation();
            Assert.Equal(0.4f, _animator.Value, 2);
            Assert.Equal(0.666f, _animator.AnimatedFraction, 2);
            _animator.PlayAnimation();
            Assert.Equal(0.8f, _animator.Value);
            Assert.Equal(0f, _animator.AnimatedFraction, 2);
        }
    }
}
