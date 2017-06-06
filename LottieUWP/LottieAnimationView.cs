using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Windows.Data.Json;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace LottieUWP
{
    /// <summary>
    /// This view will load, deserialize, and display an After Effects animation exported with
    /// bodymovin (https://github.com/bodymovin/bodymovin).
    /// <para>
    /// You may set the animation in one of two ways:
    /// 1) Attrs: <seealso cref="R.styleable#LottieAnimationView_lottie_fileName"/>
    /// 2) Programatically: <seealso cref="#setAnimation(String)"/>, <seealso cref="#setComposition(LottieComposition)"/>,
    /// or <seealso cref="#setAnimation(JSONObject)"/>.
    /// </para>
    /// <para>
    /// You can set a default cache strategy with <seealso cref="R.attr#lottie_cacheStrategy"/>.
    /// </para>
    /// <para>
    /// You can manually set the progress of the animation with <seealso cref="#setProgress(float)"/> or
    /// <seealso cref="R.attr#lottie_progress"/>
    /// </para>
    /// </summary>
    public class LottieAnimationView : UserControl
    {
        private new static readonly string Tag = typeof(LottieAnimationView).Name;

        /// <summary>
        /// Caching strategy for compositions that will be reused frequently.
        /// Weak or Strong indicates the GC reference strength of the composition in the cache.
        /// </summary>
        public enum CacheStrategy
        {
            None,
            Weak,
            Strong
        }

        private static readonly IDictionary<string, LottieComposition> StrongRefCache = new Dictionary<string, LottieComposition>();
        private static readonly IDictionary<string, WeakReference<LottieComposition>> WeakRefCache = new Dictionary<string, WeakReference<LottieComposition>>();

        private readonly IOnCompositionLoadedListener _loadedListener;

        private class OnCompositionLoadedListenerAnonymousInnerClass : IOnCompositionLoadedListener
        {
            private readonly LottieAnimationView _lottieAnimationView;

            public OnCompositionLoadedListenerAnonymousInnerClass(LottieAnimationView lottieAnimationView)
            {
                _lottieAnimationView = lottieAnimationView;
            }

            public void OnCompositionLoaded(LottieComposition composition)
            {
                if (composition != null)
                {
                    _lottieAnimationView.Composition = composition;
                }
                _lottieAnimationView._compositionLoader = null;
            }
        }

        private readonly LottieDrawable _lottieDrawable;

        public CacheStrategy DefaultCacheStrategy
        {
            get => (CacheStrategy)GetValue(DefaultCacheStrategyProperty);
            set => SetValue(DefaultCacheStrategyProperty, value);
        }

        // Using a DependencyProperty as the backing store for DefaultCacheStrategy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultCacheStrategyProperty =
            DependencyProperty.Register("DefaultCacheStrategy", typeof(CacheStrategy), typeof(LottieAnimationView), new PropertyMetadata(CacheStrategy.None));

        private string _animationName;
        //private bool wasAnimatingWhenDetached = false;
        private bool _useHardwareLayer;

        private CancellationTokenSource _compositionLoader;
        /// <summary>
        /// Can be null because it is created async
        /// </summary>
        private LottieComposition _composition;

        public string FileName
        {
            get => (string)GetValue(FileNameProperty);
            set => SetValue(FileNameProperty, value);
        }

        // Using a DependencyProperty as the backing store for FileName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register("FileName", typeof(string), typeof(LottieAnimationView), new PropertyMetadata(null, FileNamePropertyChangedCallback));

        private static void FileNamePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var lottieAnimationView = dependencyObject as LottieAnimationView;
            lottieAnimationView?.SetAnimation((string)e.NewValue);
        }

        public bool AutoPlay
        {
            get => (bool)GetValue(AutoPlayProperty);
            set => SetValue(AutoPlayProperty, value);
        }

        // Using a DependencyProperty as the backing store for AutoPlay.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoPlayProperty =
            DependencyProperty.Register("AutoPlay", typeof(bool), typeof(LottieAnimationView), new PropertyMetadata(false, AutoPlayPropertyChangedCallback));

        private static void AutoPlayPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var lottieAnimationView = dependencyObject as LottieAnimationView;
            if ((bool)e.NewValue)
                lottieAnimationView?._lottieDrawable.PlayAnimation();
        }

        public bool Loop
        {
            get => (bool)GetValue(LoopProperty);
            set => SetValue(LoopProperty, value);
        }

        // Using a DependencyProperty as the backing store for Loop.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LoopProperty =
            DependencyProperty.Register("Loop", typeof(bool), typeof(LottieAnimationView), new PropertyMetadata(false, LoopPropertyChangedCallback));

        private static void LoopPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is LottieAnimationView lottieAnimationView)
                lottieAnimationView._lottieDrawable.Looping = (bool)e.NewValue;
        }

        /// <summary>
        /// If you use image assets, you must explicitly specify the folder in assets/ in which they are
        /// located because bodymovin uses the name filenames across all compositions (img_#).
        /// Do NOT rename the images themselves.
        /// 
        /// If your images are located in src/main/assets/airbnb_loader/ then call
        /// `setImageAssetsFolder("airbnb_loader/");`.
        /// </summary>
        public string ImageAssetsFolder
        {
            get => (string)GetValue(ImageAssetsFolderProperty);
            set => SetValue(ImageAssetsFolderProperty, value);
        }

        // Using a DependencyProperty as the backing store for ImageAssetsFolder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageAssetsFolderProperty =
            DependencyProperty.Register("ImageAssetsFolder", typeof(string), typeof(LottieAnimationView), new PropertyMetadata(null, ImageAssetsFolderPropertyChangedCallback));

        private static void ImageAssetsFolderPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is LottieAnimationView lottieAnimationView)
                lottieAnimationView._lottieDrawable.ImagesAssetsFolder = (string)e.NewValue;
        }

        public Color ColorFilter
        {
            get => (Color)GetValue(ColorFilterProperty);
            set => SetValue(ColorFilterProperty, value);
        }

        // Using a DependencyProperty as the backing store for ColorFilter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorFilterProperty =
            DependencyProperty.Register("ColorFilter", typeof(Color), typeof(LottieAnimationView), new PropertyMetadata(Colors.Transparent));

        /// <summary>
        /// Set the scale on the current composition. The only cost of this function is re-rendering the
        /// current frame so you may call it frequent to scale something up or down.
        /// 
        /// The smaller the animation is, the better the performance will be. You may find that scaling an
        /// animation down then rendering it in a larger ImageView and letting ImageView scale it back up
        /// with a scaleType such as centerInside will yield better performance with little perceivable
        /// quality loss.
        /// </summary>
        public virtual float Scale
        {
            get => (float)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        // Using a DependencyProperty as the backing store for Scale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(float), typeof(LottieAnimationView), new PropertyMetadata(1, ScalePropertyChangedCallback));

        internal BitmapCanvas Canvas;

        private static void ScalePropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is LottieAnimationView lottieAnimationView)
            {
                lottieAnimationView._lottieDrawable.Scale = (float)e.NewValue;
                //if (Drawable == lottieDrawable)
                //{
                //    ImageDrawable = null;
                //    ImageDrawable = lottieDrawable;
                //}
            }
        }

        public LottieAnimationView()
        {
            _lottieDrawable = new LottieDrawable(this);
            _loadedListener = new OnCompositionLoadedListenerAnonymousInnerClass(this);

            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled && !string.IsNullOrEmpty(FileName))
            {
                SetAnimation(FileName);
            }
            if (AutoPlay)
            {
                _lottieDrawable.PlayAnimation();
            }
            _lottieDrawable.Looping = Loop;

            EnableMergePathsForKitKatAndAbove();
            AddColorFilter(new SimpleColorFilter(ColorFilter));

            // TODO: Detect battery saver mode or UWP system animations disabled
            //float systemAnimationScale = Settings.Global.getFloat(Context.ContentResolver, Settings.Global.ANIMATOR_DURATION_SCALE, 1.0f);
            float systemAnimationScale = 1;
            if (systemAnimationScale == 0f)
            {
                _lottieDrawable.SystemAnimationsAreDisabled();
            }

            EnableOrDisableHardwareLayer();
        }

        //public int ImageResource
        //{
        //    set
        //    {
        //        base.ImageResource = value;
        //        recycleBitmaps();
        //    }
        //}

        public LottieDrawable ImageDrawable
        {
            set
            {
                if (value != _lottieDrawable)
                {
                    RecycleBitmaps();
                }

                // TODO: verify
                //base.ImageDrawable = value;
            }
        }

        /// <summary>
        /// Add a color filter to specific content on a specific layer. </summary>
        /// <param name="layerName"> name of the layer where the supplied content name lives </param>
        /// <param name="contentName"> name of the specific content that the color filter is to be applied </param>
        /// <param name="colorFilter"> the color filter, null to clear the color filter </param>
        public virtual void AddColorFilterToContent(string layerName, string contentName, ColorFilter colorFilter)
        {
            _lottieDrawable.AddColorFilterToContent(layerName, contentName, colorFilter);
        }

        /// <summary>
        /// Add a color filter to a whole layer </summary>
        /// <param name="layerName"> name of the layer that the color filter is to be applied </param>
        /// <param name="colorFilter"> the color filter, null to clear the color filter </param>
        public virtual void AddColorFilterToLayer(string layerName, ColorFilter colorFilter)
        {
            _lottieDrawable.AddColorFilterToLayer(layerName, colorFilter);
        }

        /// <summary>
        /// Add a color filter to all layers </summary>
        /// <param name="colorFilter"> the color filter, null to clear all color filters </param>
        public virtual void AddColorFilter(ColorFilter colorFilter)
        {
            _lottieDrawable.AddColorFilter(colorFilter);
        }

        /// <summary>
        /// Clear all color filters on all layers and all content in the layers
        /// </summary>
        public virtual void ClearColorFilters()
        {
            _lottieDrawable.ClearColorFilters();
        }

        //public void invalidateDrawable(Drawable dr)
        //{
        //    if (Drawable == lottieDrawable)
        //    {
        //        // We always want to invalidate the root drawable so it redraws the whole drawable.
        //        // Eventually it would be great to be able to invalidate just the changed region.
        //        base.invalidateDrawable(lottieDrawable);
        //    }
        //    else
        //    {
        //        // Otherwise work as regular ImageView
        //        base.invalidateDrawable(dr);
        //    }
        //}
        //
        //protected internal Parcelable onSaveInstanceState()
        //{
        //    Parcelable superState = base.onSaveInstanceState();
        //    SavedState ss = new SavedState(superState);
        //    ss.animationName = animationName;
        //    ss.progress = lottieDrawable.Progress;
        //    ss.isAnimating = lottieDrawable.Animating;
        //    ss.isLooping = lottieDrawable.Looping;
        //    ss.imageAssetsFolder = lottieDrawable.ImageAssetsFolder;
        //    return ss;
        //}
        //
        //protected internal void onRestoreInstanceState(Parcelable state)
        //{
        //    if (!(state is SavedState))
        //    {
        //        base.onRestoreInstanceState(state);
        //        return;
        //    }
        //
        //    SavedState ss = (SavedState)state;
        //    base.onRestoreInstanceState(ss.SuperState);
        //    this.animationName = ss.animationName;
        //    if (!TextUtils.isEmpty(animationName))
        //    {
        //        Animation = animationName;
        //    }
        //    Progress = ss.progress;
        //    loop(ss.isLooping);
        //    if (ss.isAnimating)
        //    {
        //        playAnimation();
        //    }
        //    lottieDrawable.ImagesAssetsFolder = ss.imageAssetsFolder;
        //}
        //
        //protected internal void onAttachedToWindow()
        //{
        //    base.onAttachedToWindow();
        //    if (autoPlay && wasAnimatingWhenDetached)
        //    {
        //        playAnimation();
        //    }
        //}
        //
        //protected internal void onDetachedFromWindow()
        //{
        //    if (Animating)
        //    {
        //        cancelAnimation();
        //        wasAnimatingWhenDetached = true;
        //    }
        //    recycleBitmaps();
        //    base.onDetachedFromWindow();
        //}

        internal virtual void RecycleBitmaps()
        {
            // AppCompatImageView constructor will set the image when set from xml
            // before LottieDrawable has been initialized
            _lottieDrawable?.RecycleBitmaps();
        }

        /// <summary>
        /// Enable this to get merge path support for devices running KitKat (19) and above.
        /// 
        /// Merge paths currently don't work if the the operand shape is entirely contained within the
        /// first shape. If you need to cut out one shape from another shape, use an even-odd fill type
        /// instead of using merge paths.
        /// </summary>
        public virtual void EnableMergePathsForKitKatAndAbove()
        {
            _lottieDrawable.EnableMergePathsForKitKatAndAbove();
        }

        public virtual void UseExperimentalHardwareAcceleration()
        {
            UseExperimentalHardwareAcceleration(true);
        }

        /// <summary>
        /// Enable hardware acceleration for this view.
        /// READ THIS BEFORE ENABLING HARDWARE ACCELERATION:
        /// 1) Test your animation on the minimum API level you support. Some drawing features such as
        ///    dashes and stroke caps have min api levels
        ///    (https://developer.android.com/guide/topics/graphics/hardware-accel.html#unsupported)
        /// 2) Enabling hardware acceleration is not always more performant. Check it with your specific
        ///    animation only if you are having performance issues with software rendering.
        /// 3) Software rendering is safer and will be consistent across devices. Manufacturers can
        ///    potentially break hardware rendering with bugs in their SKIA engine. Lottie cannot do
        ///    anything about that.
        /// </summary>
        public virtual void UseExperimentalHardwareAcceleration(bool use)
        {
            _useHardwareLayer = use;
            EnableOrDisableHardwareLayer();
        }

        /// <summary>
        /// Sets the animation from a file in the assets directory.
        /// This will load and deserialize the file asynchronously.
        /// <para>
        /// Will not cache the composition once loaded.
        /// </para>
        /// </summary>
        public virtual void SetAnimation(string animationName)
        {
            SetAnimation(animationName, DefaultCacheStrategy);
        }

        /// <summary>
        /// Sets the animation from a file in the assets directory.
        /// This will load and deserialize the file asynchronously.
        /// <para>
        /// You may also specify a cache strategy. Specifying <seealso cref="CacheStrategy#Strong"/> will hold a
        /// strong reference to the composition once it is loaded
        /// and deserialized. <seealso cref="CacheStrategy#Weak"/> will hold a weak reference to said composition.
        /// </para>
        /// </summary>
        public virtual void SetAnimation(string animationName, CacheStrategy cacheStrategy)
        {
            _animationName = animationName;
            if (WeakRefCache.ContainsKey(animationName))
            {
                var compRef = WeakRefCache[animationName];
                LottieComposition lottieComposition;
                if (compRef.TryGetTarget(out lottieComposition))
                {
                    Composition = lottieComposition;
                    return;
                }
            }
            else if (StrongRefCache.ContainsKey(animationName))
            {
                Composition = StrongRefCache[animationName];
                return;
            }

            _animationName = animationName;
            _lottieDrawable.CancelAnimation();
            CancelLoaderTask();

            var cancellationTokenSource = new CancellationTokenSource();
            LottieComposition.Factory.FromAssetFileNameAsync(animationName, new OnCompositionLoadedListenerAnonymousInnerClass2(this, animationName, cacheStrategy), cancellationTokenSource.Token);
            _compositionLoader = cancellationTokenSource;
        }

        private class OnCompositionLoadedListenerAnonymousInnerClass2 : IOnCompositionLoadedListener
        {
            private readonly LottieAnimationView _outerInstance;

            private readonly string _animationName;
            private readonly CacheStrategy _cacheStrategy;

            public OnCompositionLoadedListenerAnonymousInnerClass2(LottieAnimationView outerInstance, string animationName, CacheStrategy cacheStrategy)
            {
                _outerInstance = outerInstance;
                _animationName = animationName;
                _cacheStrategy = cacheStrategy;
            }

            public void OnCompositionLoaded(LottieComposition composition)
            {
                if (_cacheStrategy == CacheStrategy.Strong)
                {
                    StrongRefCache[_animationName] = composition;
                }
                else if (_cacheStrategy == CacheStrategy.Weak)
                {
                    WeakRefCache[_animationName] = new WeakReference<LottieComposition>(composition);
                }

                _outerInstance.Composition = composition;
            }
        }

        /// <summary>
        /// Sets the animation from a JSONObject.
        /// This will load and deserialize the file asynchronously.
        /// <para>
        /// This is particularly useful for animations loaded from the network. You can fetch the
        /// bodymovin json from the network and pass it directly here.
        /// </para>
        /// </summary>
        public virtual JsonObject Animation
        {
            set
            {
                CancelLoaderTask();
                var cancellationTokenSource = new CancellationTokenSource();
                LottieComposition.Factory.FromJsonAsync(value, _loadedListener, cancellationTokenSource.Token);
                _compositionLoader = cancellationTokenSource;
            }
        }

        private void CancelLoaderTask()
        {
            if (_compositionLoader != null)
            {
                _compositionLoader.Cancel();
                _compositionLoader = null;
            }
        }

        /// <summary>
        /// Sets a composition.
        /// You can set a default cache strategy if this view was inflated with xml by
        /// using <seealso cref="R.attr#lottie_cacheStrategy"/>.
        /// </summary>
        public virtual LottieComposition Composition
        {
            set
            {
                Debug.WriteLine("Set Composition \n" + value, Tag);
                //lottieDrawable.Callback = this;

                var isNewComposition = _lottieDrawable.SetComposition(value);
                if (!isNewComposition)
                {
                    // We can avoid re-setting the drawable, and invalidating the view, since the value
                    // hasn't changed.
                    return;
                }

                var screenWidth = Utils.GetScreenWidth();
                var screenHeight = Utils.GetScreenHeight();
                var compWidth = (int)value.Bounds.Width;
                var compHeight = (int)value.Bounds.Height;
                if (compWidth > screenWidth || compHeight > screenHeight)
                {
                    var xScale = screenWidth / (float)compWidth;
                    var yScale = screenHeight / (float)compHeight;
                    var maxScaleForScreen = Math.Min(xScale, yScale);
                    Scale = Math.Min(maxScaleForScreen, _lottieDrawable.Scale);
                    Debug.WriteLine($"Composition larger than the screen {compWidth:D}x{compHeight:D} vs {screenWidth:D}x{screenHeight:D}. Scaling down.", "LOTTIE");
                }

                // If you set a different value on the view, the bounds will not update unless
                // the drawable is different than the original.
                ImageDrawable = null;
                ImageDrawable = _lottieDrawable;

                Progress = 0.5f;
                Canvas = CanvasPool.Instance.Acquire(_lottieDrawable.Width, _lottieDrawable.Height);
                _lottieDrawable.Draw(Canvas);

                Content = new Image
                {
                    Stretch = Stretch.None,
                    Source = Canvas.Bitmap
                };

                _composition = value;

                UpdateLayout(); // TODO: Is this equivalent?

                //requestLayout();
            }
        }

        /// <summary>
        /// Returns whether or not any layers in this composition has masks.
        /// </summary>
        public virtual bool HasMasks()
        {
            return _lottieDrawable.HasMasks();
        }

        /// <summary>
        /// Returns whether or not any layers in this composition has a matte layer.
        /// </summary>
        public virtual bool HasMatte()
        {
            return _lottieDrawable.HasMatte();
        }

        public virtual void AddAnimatorUpdateListener(LottieDrawable.IValueAnimatorAnimatorUpdateListener updateListener)
        {
            _lottieDrawable.AddAnimatorUpdateListener(updateListener);
        }

        public virtual void RemoveUpdateListener(LottieDrawable.IValueAnimatorAnimatorUpdateListener updateListener)
        {
            _lottieDrawable.RemoveAnimatorUpdateListener(updateListener);
        }

        public virtual void AddAnimatorListener(Animator.IAnimatorListener listener)
        {
            _lottieDrawable.AddAnimatorListener(listener);
        }

        public virtual void RemoveAnimatorListener(Animator.IAnimatorListener listener)
        {
            _lottieDrawable.RemoveAnimatorListener(listener);
        }

        public virtual bool Animating => _lottieDrawable.Animating;

        public virtual void PlayAnimation()
        {
            _lottieDrawable.PlayAnimation();
            EnableOrDisableHardwareLayer();
        }

        public virtual void ResumeAnimation()
        {
            _lottieDrawable.ResumeAnimation();
            EnableOrDisableHardwareLayer();
        }

        public virtual void ReverseAnimation()
        {
            _lottieDrawable.ReverseAnimation();
            EnableOrDisableHardwareLayer();
        }

        public virtual void ResumeReverseAnimation()
        {
            _lottieDrawable.ResumeReverseAnimation();
            EnableOrDisableHardwareLayer();
        }

        public virtual float Speed
        {
            set => _lottieDrawable.Speed = value;
        }


        /// <summary>
        /// Allows you to modify or clear a bitmap that was loaded for an image either automatically 
        /// through {@link #setImageAssetsFolder(String)} or with an {@link ImageAssetDelegate}. 
        /// Return the previous Bitmap or null. 
        public WriteableBitmap UpdateBitmap(string id, WriteableBitmap bitmap)
        {
            return _lottieDrawable.UpdateBitmap(id, bitmap);
        }

        /// <summary>
        /// Use this if you can't bundle images with your app. This may be useful if you download the
        /// animations from the network or have the images saved to an SD Card. In that case, Lottie
        /// will defer the loading of the bitmap to this delegate.
        /// </summary>
        public virtual IImageAssetDelegate ImageAssetDelegate
        {
            set => _lottieDrawable.ImageAssetDelegate = value;
        }

        public virtual void CancelAnimation()
        {
            _lottieDrawable.CancelAnimation();
            EnableOrDisableHardwareLayer();
        }

        public virtual void PauseAnimation()
        {
            var progress = Progress;
            _lottieDrawable.CancelAnimation();
            Progress = progress;
            EnableOrDisableHardwareLayer();
        }

        public virtual float Progress
        {
            set => _lottieDrawable.Progress = value;
            get => _lottieDrawable.Progress;
        }

        public virtual long Duration => _composition?.Duration ?? 0;

        private void EnableOrDisableHardwareLayer()
        {
            var useHardwareLayer = _useHardwareLayer && _lottieDrawable.Animating;
            //setLayerType(useHardwareLayer ? LAYER_TYPE_HARDWARE : LAYER_TYPE_SOFTWARE, null);
            // TODO: FIX
        }

        //private class SavedState : BaseSavedState
        //{
        //    internal string animationName;
        //    internal float progress;
        //    internal bool isAnimating;
        //    internal bool isLooping;
        //    internal string imageAssetsFolder;

        //    internal SavedState(Parcelable superState) : base(superState)
        //    {
        //    }

        //    internal SavedState(Parcel @in) : base(@in)
        //    {
        //        animationName = @in.readString();
        //        progress = @in.readFloat();
        //        isAnimating = @in.readInt() == 1;
        //        isLooping = @in.readInt() == 1;
        //        imageAssetsFolder = @in.readString();
        //    }

        //    public void writeToParcel(Parcel @out, int flags)
        //    {
        //        base.writeToParcel(@out, flags);
        //        @out.writeString(animationName);
        //        @out.writeFloat(progress);
        //        @out.writeInt(isAnimating ? 1 : 0);
        //        @out.writeInt(isLooping ? 1 : 0);
        //        @out.writeString(imageAssetsFolder);
        //    }

        //    public static readonly Parcelable.Creator<SavedState> CREATOR = new CreatorAnonymousInnerClass();

        //    private class CreatorAnonymousInnerClass : Parcelable.Creator<SavedState>
        //    {
        //        public CreatorAnonymousInnerClass()
        //        {
        //        }

        //        public virtual SavedState createFromParcel(Parcel @in)
        //        {
        //            return new SavedState(@in);
        //        }

        //        public virtual SavedState[] newArray(int size)
        //        {
        //            return new SavedState[size];
        //        }
        //    }
        //}
    }
}