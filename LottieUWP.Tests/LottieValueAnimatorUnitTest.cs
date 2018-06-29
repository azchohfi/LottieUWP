using System;
using System.Collections.Generic;
using LottieUWP.Model;
using LottieUWP.Model.Layer;
using LottieUWP.Utils;
using Moq;
using Windows.Foundation;
using Xunit;

namespace LottieUWP.Tests
{
    public class LottieValueAnimatorUnitTest : IDisposable
    {
        private LottieComposition _composition;
        private TestLottieValueAnimator _animator;
        private readonly Mock<TestLottieValueAnimator> _mockAnimator;
        private volatile bool isDone;

        public LottieValueAnimatorUnitTest()
        {
            _composition = new LottieComposition();
            _composition.Init(new Rect(), 0, 1000, 1000, new List<Layer>(),
                new Dictionary<long, Layer>(0), new Dictionary<string, List<Layer>>(0),
                new Dictionary<string, LottieImageAsset>(0), new Dictionary<int, FontCharacter>(0),
                new Dictionary<string, Font>(0));
            _mockAnimator = new Mock<TestLottieValueAnimator>
            {
                CallBase = true,
            };
            _animator = _mockAnimator.Object;
            _animator.Composition = _composition;

            isDone = false;
        }

        public class TestLottieValueAnimator : LottieValueAnimator
        {
            public int OnValueChangedCount = 0;

            public void OnValueChanged2()
            {
                OnValueChangedCount++;
            }

            protected override void OnValueChanged()
            {
                base.OnValueChanged();
                OnValueChanged2();
            }

            protected override void PostFrameCallback()
            {
                _isRunning = true;
            }

            protected override void RemoveFrameCallback()
            {
                _isRunning = false;
            }
        }

        public void Dispose()
        {
            _animator.Cancel();
            _animator = null;
        }

        [Fact]
        public void TestInitialState()
        {
            Assert.Equal(0f, _animator.Frame);
        }

        [Fact]
        public void TestResumingMaintainsValue()
        {
            _animator.Frame = 500;
            _animator.ResumeAnimation();
            Assert.Equal(500f, _animator.Frame);
        }

        [Fact]
        public void TestFrameConvertsToAnimatedFraction()
        {
            _animator.Frame = 500;
            _animator.ResumeAnimation();
            Assert.Equal(0.5f, _animator.AnimatedFraction);
            Assert.Equal(0.5f, _animator.AnimatedValueAbsolute);
        }

        [Fact]
        public void TestPlayingResetsValue()
        {
            _animator.Frame = 500;
            _animator.PlayAnimation();
            Assert.Equal(0f, _animator.Frame);
            Assert.Equal(0f, _animator.AnimatedFraction);
        }

        [Fact]
        public void TestReversingMaintainsValue()
        {
            _animator.Frame = 250;
            _animator.ReverseAnimationSpeed();
            Assert.Equal(250, _animator.Frame);
            Assert.Equal(0.75f, _animator.AnimatedFraction);
            Assert.Equal(0.25f, _animator.AnimatedValueAbsolute);
        }

        [Fact]
        public void TestReversingWithMinValueMaintainsValue()
        {
            _animator.MinFrame = 100;
            _animator.Frame = 1000;
            _animator.ReverseAnimationSpeed();
            Assert.Equal(1000f, _animator.Frame);
            Assert.Equal(0f, _animator.AnimatedFraction);
            Assert.Equal(1f, _animator.AnimatedValueAbsolute);
        }

        [Fact]
        public void TestReversingWithMaxValueMaintainsValue()
        {
            _animator.MaxFrame = 900;
            _animator.ReverseAnimationSpeed();
            Assert.Equal(0f, _animator.Frame);
            Assert.Equal(1f, _animator.AnimatedFraction);
            Assert.Equal(0f, _animator.AnimatedValueAbsolute);
        }

        [Fact]
        public void TestResumeReversingWithMinValueMaintainsValue()
        {
            _animator.MaxFrame = 900;
            _animator.ReverseAnimationSpeed();
            _animator.ResumeAnimation();
            Assert.Equal(900f, _animator.Frame);
            Assert.Equal(0f, _animator.AnimatedFraction);
            Assert.Equal(0.9f, _animator.AnimatedValueAbsolute);
        }

        [Fact]
        public void TestPlayReversingWithMinValueMaintainsValue()
        {
            _animator.MaxFrame = 900;
            _animator.ReverseAnimationSpeed();
            _animator.PlayAnimation();
            Assert.Equal(900f, _animator.Frame);
            Assert.Equal(0f, _animator.AnimatedFraction);
            Assert.Equal(0.9f, _animator.AnimatedValueAbsolute);
        }

        [Fact]
        public void TestMinAndMaxBothSet()
        {
            _animator.MinFrame = 200;
            _animator.MaxFrame = 800;
            _animator.Frame = 400;
            Assert.Equal(0.33f, _animator.AnimatedFraction, 2);
            Assert.Equal(0.4f, _animator.AnimatedValueAbsolute);
            _animator.ReverseAnimationSpeed();
            Assert.Equal(400f, _animator.Frame);
            Assert.Equal(0.666f, _animator.AnimatedFraction, 2);
            Assert.Equal(0.4f, _animator.AnimatedValueAbsolute);
            _animator.ResumeAnimation();
            Assert.Equal(400f, _animator.Frame);
            Assert.Equal(0.666f, _animator.AnimatedFraction, 2);
            Assert.Equal(0.4f, _animator.AnimatedValueAbsolute);
            _animator.PlayAnimation();
            Assert.Equal(800f, _animator.Frame);
            Assert.Equal(0f, _animator.AnimatedFraction, 2);
            Assert.Equal(0.8f, _animator.AnimatedValueAbsolute);
        }

        [Fact]
        public void TestDefaultAnimator()
        {
            int state = 0;

            _mockAnimator.Setup(l => l.OnAnimationStart(false)).Callback(() => { if (state == 0) state = 1; }).Verifiable();
            _mockAnimator.Setup(l => l.OnAnimationEnd(false)).Callback(() =>
            {
                if (state == 1)
                    state = 2;
                
                _mockAnimator.Verify();
                Assert.Equal(2, state);
                _mockAnimator.Verify(l => l.OnAnimationCancel(), Times.Never);
                _mockAnimator.Verify(l => l.OnAnimationRepeat(), Times.Never);

                isDone = true;
            }).Verifiable();
                        
            TestAnimator(null);
        }

        [Fact]
        public void TestReverseAnimator()
        {
            _animator.ReverseAnimationSpeed();

            int state = 0;

            _mockAnimator.Setup(l => l.OnAnimationStart(true)).Callback(() => { if (state == 0) state = 1; }).Verifiable();
            _mockAnimator.Setup(l => l.OnAnimationEnd(true)).Callback(() =>
            {
                if (state == 1)
                    state = 2;

                _mockAnimator.Verify();
                Assert.Equal(2, state);
                _mockAnimator.Verify(l => l.OnAnimationCancel(), Times.Never);
                _mockAnimator.Verify(l => l.OnAnimationRepeat(), Times.Never);

                isDone = true;
            }).Verifiable();

            TestAnimator(null);
        }

        [Fact]
        public void TestLoopingAnimatorOnce()
        {
            _animator.RepeatCount = 1;
            TestAnimator(() =>
            {
                _mockAnimator.Verify(l => l.OnAnimationStart(false), Times.Once);
                _mockAnimator.Verify(l => l.OnAnimationRepeat(), Times.Once);
                _mockAnimator.Verify(l => l.OnAnimationEnd(false), Times.Once);
                _mockAnimator.Verify(l => l.OnAnimationCancel(), Times.Never);
            });
        }

        [Fact]
        public void TestLoopingAnimatorZeroTimes()
        {
            _animator.RepeatCount = 0;
            TestAnimator(() =>
            {
                _mockAnimator.Verify(l => l.OnAnimationStart(false), Times.Once);
                _mockAnimator.Verify(l => l.OnAnimationRepeat(), Times.Never);
                _mockAnimator.Verify(l => l.OnAnimationEnd(false), Times.Once);
                _mockAnimator.Verify(l => l.OnAnimationCancel(), Times.Never);
            });
        }

        [Fact]
        public void TestLoopingAnimatorTwice()
        {
            _animator.RepeatCount = 2;
            TestAnimator(() =>
            {
                _mockAnimator.Verify(l => l.OnAnimationStart(false), Times.Once);
                _mockAnimator.Verify(l => l.OnAnimationRepeat(), Times.Exactly(2));
                _mockAnimator.Verify(l => l.OnAnimationEnd(false), Times.Once);
                _mockAnimator.Verify(l => l.OnAnimationCancel(), Times.Never);
            });
        }

        [Fact]
        public void TestLoopingAnimatorOnceReverse()
        {
            _animator.Frame = 1000;
            _animator.RepeatCount = 1;
            _animator.ReverseAnimationSpeed();
            TestAnimator(() =>
            {
                _mockAnimator.Verify(l => l.OnAnimationStart(true), Times.Once);
                _mockAnimator.Verify(l => l.OnAnimationRepeat(), Times.Once);
                _mockAnimator.Verify(l => l.OnAnimationEnd(true), Times.Once);
                _mockAnimator.Verify(l => l.OnAnimationCancel(), Times.Never);
            });
        }

        [Fact]
        public void SetMinFrameSmallerThanComposition()
        {
            _animator.MinFrame = -9000;
            Assert.Equal(_animator.MinFrame, _composition.StartFrame);
        }

        [Fact]
        public void SetMaxFrameLargerThanComposition()
        {
            _animator.MaxFrame = 9000;
            Assert.Equal(_animator.MaxFrame, _composition.EndFrame);
        }

        private void TestAnimator(Action verifyListener)
        {
            _animator.AnimationEnd += (s, e) =>
            {
                verifyListener?.Invoke();
                isDone = true;
            };

            _animator.PlayAnimation();
            while (!isDone)
            {
                _animator.DoFrame();
            }
        }
    }
}
