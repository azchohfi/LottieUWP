using System;
using System.Collections.Generic;
using System.IO;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using LottieUWP.Model;
using Xunit;

namespace LottieUWP.Tests
{
    public class KeyPathTest : IDisposable
    {
        private static readonly string[] V =
        {
            "Shape Layer 1",
            "Group 1",
            "Rectangle",
            "Stroke"
        };

        private static readonly string I = "INVALID";
        private static readonly string W = "*";
        private static readonly string G = "**";

        private LottieDrawable _lottieDrawable;

        public KeyPathTest()
        {
            var task = CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                _lottieDrawable = new LottieDrawable();

                try
                {
                    LottieComposition composition = LottieComposition.Factory.FromJsonSync(new JsonReader(new StringReader(Fixtures.Squares)));
                    _lottieDrawable.SetComposition(composition);
                }
                catch (IOException e)
                {
                    throw new ArgumentException(e.Message, e);
                }
            }).AsTask();

            task.Wait();
        }

        public void Dispose()
        {
            var task = CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                _lottieDrawable.ClearComposition();
                _lottieDrawable = null;
            }).AsTask();

            task.Wait();
        }

        #region Basic Tests
        [Fact]
        public void TestV()
        {
            AssertSize(1, V[0]);
        }

        [Fact]
        public void TestI()
        {
            AssertSize(0, I);
        }

        [Fact]
        public void TestIi()
        {
            AssertSize(0, I, I);
        }

        [Fact]
        public void TestIv()
        {
            AssertSize(0, I, V[1]);
        }

        [Fact]
        public void TestVi()
        {
            AssertSize(0, V[0], I);
        }

        [Fact]
        public void TestVv()
        {
            AssertSize(1, V[0], V[1]);
        }

        [Fact]
        public void TestIii()
        {
            AssertSize(0, I, I, I);
        }

        [Fact]
        public void TestIiv()
        {
            AssertSize(0, I, I, V[3]);
        }

        [Fact]
        public void TestIvi()
        {
            AssertSize(0, I, V[2], I);
        }

        [Fact]
        public void TestIvv()
        {
            AssertSize(0, I, V[2], V[3]);
        }

        [Fact]
        public void TestVii()
        {
            AssertSize(0, V[0], I, I);
        }

        [Fact]
        public void TestViv()
        {
            AssertSize(0, V[0], I, V[2]);
        }

        [Fact]
        public void TestVvi()
        {
            AssertSize(0, V[0], V[1], I);
        }

        [Fact]
        public void TestVvv()
        {
            AssertSize(1, V[0], V[1], V[2]);
        }

        [Fact]
        public void TestIiii()
        {
            AssertSize(0, I, I, I, I);
        }

        [Fact]
        public void TestIiiv()
        {
            AssertSize(0, I, I, I, V[3]);
        }

        [Fact]
        public void TestIivi()
        {
            AssertSize(0, I, I, V[2], I);
        }

        [Fact]
        public void TestIivv()
        {
            AssertSize(0, I, I, V[2], V[3]);
        }

        [Fact]
        public void TestIvii()
        {
            AssertSize(0, I, V[1], I, I);
        }

        [Fact]
        public void TestIviv()
        {
            AssertSize(0, I, V[1], I, V[3]);
        }

        [Fact]
        public void TestIvvi()
        {
            AssertSize(0, I, V[1], V[2], V[3]);
        }

        [Fact]
        public void TestIvvv()
        {
            AssertSize(0, I, V[1], V[2], V[3]);
        }

        [Fact]
        public void TestViii()
        {
            AssertSize(0, V[0], I, I, I);
        }

        [Fact]
        public void TestViiv()
        {
            AssertSize(0, V[0], I, I, V[3]);
        }

        [Fact]
        public void TestVivi()
        {
            AssertSize(0, V[0], I, V[2], I);
        }

        [Fact]
        public void TestVivv()
        {
            AssertSize(0, V[0], I, V[2], V[3]);
        }

        [Fact]
        public void TestVvii()
        {
            AssertSize(0, V[0], V[1], I, I);
        }

        [Fact]
        public void TestVviv()
        {
            AssertSize(0, V[0], V[1], I, V[3]);
        }

        [Fact]
        public void TestVvvi()
        {
            AssertSize(0, V[0], V[1], V[2], I);
        }

        [Fact]
        public void TestVvvv()
        {
            AssertSize(1, V[0], V[1], V[2], V[3]);
        }
        #endregion

        #region One Wildcard
        [Fact]
        public void TestWvvv()
        {
            AssertSize(2, W, V[1], V[2], V[3]);
        }

        [Fact]
        public void TestVwvv()
        {
            AssertSize(2, V[0], W, V[2], V[3]);
        }

        [Fact]
        public void TestVvwv()
        {
            AssertSize(1, V[0], V[1], W, V[3]);
        }

        [Fact]
        public void TestVvvw()
        {
            AssertSize(2, V[0], V[1], V[2], W);
        }
        #endregion

        #region Two Wildcards
        [Fact]
        public void TestWwvv()
        {
            AssertSize(4, W, W, V[2], V[3]);
        }

        [Fact]
        public void TestWvwv()
        {
            AssertSize(2, W, V[1], W, V[3]);
        }

        [Fact]
        public void TestWvvw()
        {
            AssertSize(4, W, V[1], V[2], W);
        }

        [Fact]
        public void TestWwiv()
        {
            AssertSize(0, W, W, I, V[3]);
        }

        [Fact]
        public void TestWwvi()
        {
            AssertSize(0, W, W, V[2], I);
        }

        [Fact]
        public void TestWvw()
        {
            AssertSize(2, W, V[1], W);
        }
        #endregion

        #region Three Wildcards
        [Fact]
        public void TestWww()
        {
            AssertSize(4, W, W, W);
        }

        [Fact]
        public void TestWwwv()
        {
            AssertSize(4, W, W, W, V[3]);
        }

        [Fact]
        public void TestWwwi()
        {
            AssertSize(0, W, W, W, I);
        }
        #endregion

        #region Four Wildcards
        [Fact]
        public void TestWwww()
        {
            AssertSize(8, W, W, W, W);
        }
        #endregion

        #region One Globstar
        [Fact]
        public void TestG()
        {
            AssertSize(18, G);
        }

        [Fact]
        public void TestGi()
        {
            AssertSize(0, G, I);
        }

        [Fact]
        public void TestGv0()
        {
            AssertSize(1, G, V[0]);
        }

        [Fact]
        public void TestGv0V0()
        {
            AssertSize(0, G, V[0], V[0]);
        }

        [Fact]
        public void TestGv1()
        {
            AssertSize(2, G, V[1]);
        }

        [Fact]
        public void TestGv2()
        {
            AssertSize(4, G, V[2]);
        }

        [Fact]
        public void TestGv3()
        {
            AssertSize(4, G, V[3]);
        }
        #endregion

        #region Two Globstars
        [Fact]
        public void TestGv0G()
        {
            AssertSize(9, G, V[0], G);
        }

        [Fact]
        public void TestGv1G()
        {
            AssertSize(8, G, V[1], G);
        }

        [Fact]
        public void TestGv2G()
        {
            AssertSize(12, G, V[2], G);
        }

        [Fact]
        public void TestGig()
        {
            AssertSize(0, G, I, G);
        }
        #endregion

        #region Wildcard and Globstar
        [Fact]
        public void TestWg()
        {
            AssertSize(18, W, G);
        }

        [Fact]
        public void TestGv0W()
        {
            AssertSize(2, G, V[0], W);
        }

        [Fact]
        public void TestWv0I()
        {
            AssertSize(0, W, V[0], I);
        }

        [Fact]
        public void TestGv1W()
        {
            AssertSize(2, G, V[1], W);
        }

        [Fact]
        public void TestWv1I()
        {
            AssertSize(0, W, V[1], I);
        }

        [Fact]
        public void TestGv2W()
        {
            AssertSize(8, G, V[2], W);
        }

        [Fact]
        public void TestWv2I()
        {
            AssertSize(0, W, V[2], I);
        }
        #endregion

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private void AssertSize(int size, params string[] keys)
        {
            KeyPath keyPath = new KeyPath(keys);
            List<KeyPath> resolvedKeyPaths = _lottieDrawable.ResolveKeyPath(keyPath);
            Assert.Equal(size, resolvedKeyPaths.Count);
        }
    }
}