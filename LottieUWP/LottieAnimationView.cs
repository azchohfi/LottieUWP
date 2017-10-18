using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas;

namespace LottieUWP
{
    /// <summary>
    /// This view will load, deserialize, and display an After Effects animation exported with
    /// bodymovin (https://github.com/bodymovin/bodymovin).
    /// <para>
    /// You may set the animation in one of two ways:
    /// 1) Attrs: <seealso cref="LottieAnimationView.FileNameProperty"/>
    /// 2) Programatically: <seealso cref="SetAnimationAsync(string)"/>, <seealso cref="Composition"/>,
    /// or <seealso cref="SetAnimationAsync(JsonObject)"/>.
    /// </para>
    /// <para>
    /// You can set a default cache strategy with <seealso cref="CacheStrategy.None"/>.
    /// </para>
    /// <para>
    /// You can manually set the progress of the animation with <seealso cref="Progress"/> or
    /// <seealso cref="LottieAnimationView.Progress"/>
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

        private static readonly Dictionary<string, LottieComposition> StrongRefCache = new Dictionary<string, LottieComposition>();
        private static readonly Dictionary<string, WeakReference<LottieComposition>> WeakRefCache = new Dictionary<string, WeakReference<LottieComposition>>();

        private readonly LottieDrawable _lottieDrawable;

        public CacheStrategy DefaultCacheStrategy
        {
            get => (CacheStrategy)GetValue(DefaultCacheStrategyProperty);
            set => SetValue(DefaultCacheStrategyProperty, value);
        }

        // Using a DependencyProperty as the backing store for DefaultCacheStrategy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultCacheStrategyProperty =
            DependencyProperty.Register("DefaultCacheStrategy", typeof(CacheStrategy), typeof(LottieAnimationView), new PropertyMetadata(CacheStrategy.Weak));

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

        private static async void FileNamePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var lottieAnimationView = dependencyObject as LottieAnimationView;
            await lottieAnimationView?.SetAnimationAsync((string)e.NewValue);
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
                lottieAnimationView._lottieDrawable.ImageAssetsFolder = (string)e.NewValue;
        }

        public Color ColorFilter
        {
            get => (Color)GetValue(ColorFilterProperty);
            set => SetValue(ColorFilterProperty, value);
        }

        // Using a DependencyProperty as the backing store for ColorFilter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorFilterProperty =
            DependencyProperty.Register("ColorFilter", typeof(Color), typeof(LottieAnimationView), new PropertyMetadata(Colors.Transparent));

        /** 
        * Use this to manually set fonts. 
        */
        public FontAssetDelegate FontAssetDelegate
        {
            set => _lottieDrawable.FontAssetDelegate = value;
        }

        /** 
         * Set this to replace animation text with custom text at runtime 
         */
        public TextDelegate TextDelegate
        {
            set => _lottieDrawable.TextDelegate = value;
        }

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

        private static void ScalePropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is LottieAnimationView lottieAnimationView)
                lottieAnimationView._lottieDrawable.Scale = (float)e.NewValue;
        }

        public LottieAnimationView()
        {
            _lottieDrawable = new LottieDrawable();

            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled && !string.IsNullOrEmpty(FileName))
            {
                SetAnimationAsync(FileName).RunSynchronously();
            }
            if (AutoPlay)
            {
                _lottieDrawable.PlayAnimation();
            }
            _lottieDrawable.Looping = Loop;

            EnableMergePathsForKitKatAndAbove();
            AddColorFilter(new SimpleColorFilter(ColorFilter));

            if (Utils.Utils.GetAnimationScale() == 0f)
            {
                _lottieDrawable.SystemAnimationsAreDisabled();
            }

            EnableOrDisableHardwareLayer();
        }

        //public int ImageResource
        //{
        //    set
        //    {
        //        RecycleBitmaps();
        //        CancelLoaderTask();
        //        base.ImageResource = value;
        //    }
        //}

        private Viewbox _viewbox;

        public LottieDrawable ImageDrawable
        {
            set
            {
                if (_viewbox?.Child == value)
                    return;

                if (value != _lottieDrawable)
                {
                    RecycleBitmaps();
                }
                CancelLoaderTask();

                if (_viewbox == null)
                {
                    _viewbox = new Viewbox
                    {
                        Stretch = Stretch.Uniform,
                        StretchDirection = StretchDirection.DownOnly
                    };
                    Content = _viewbox;
                }
                _viewbox.Child = value;
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

        public virtual void UseExperimentalHardwareAcceleration(bool use = true)
        {
            UseHardwareAcceleration(use);
        }

        /** 
        * @see #useHardwareAcceleration(boolean) 
        */

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
        public virtual void UseHardwareAcceleration(bool use = true)
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
        public virtual async Task SetAnimationAsync(string animationName)
        {
            await SetAnimationAsync(animationName, DefaultCacheStrategy);
        }

        /// <summary>
        /// Sets the animation from a file in the assets directory.
        /// This will load and deserialize the file asynchronously.
        /// <para>
        /// You may also specify a cache strategy. Specifying <seealso cref="CacheStrategy.Strong"/> will hold a
        /// strong reference to the composition once it is loaded
        /// and deserialized. <seealso cref="CacheStrategy.Weak"/> will hold a weak reference to said composition.
        /// </para>
        /// </summary>
        public virtual async Task SetAnimationAsync(string animationName, CacheStrategy cacheStrategy)
        {
            _animationName = animationName;
            if (WeakRefCache.ContainsKey(animationName))
            {
                var compRef = WeakRefCache[animationName];
                if (compRef.TryGetTarget(out LottieComposition lottieComposition))
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

            _lottieDrawable.CancelAnimation();
            CancelLoaderTask();

            var cancellationTokenSource = new CancellationTokenSource();

            _compositionLoader = cancellationTokenSource;

            var composition = await LottieComposition.Factory.FromAssetFileNameAsync(animationName, cancellationTokenSource.Token);

            if (cacheStrategy == CacheStrategy.Strong)
            {
                StrongRefCache[animationName] = composition;
            }
            else if (cacheStrategy == CacheStrategy.Weak)
            {
                WeakRefCache[animationName] = new WeakReference<LottieComposition>(composition);
            }

            Composition = composition;
        }

        /// <summary>
        /// Sets the animation from a JSONObject.
        /// This will load and deserialize the file asynchronously.
        /// <para>
        /// This is particularly useful for animations loaded from the network. You can fetch the
        /// bodymovin json from the network and pass it directly here.
        /// </para>
        /// </summary>
        public virtual async Task SetAnimationAsync(JsonObject value)
        {
            CancelLoaderTask();
            var cancellationTokenSource = new CancellationTokenSource();

            _compositionLoader = cancellationTokenSource;

            var composition = await LottieComposition.Factory.FromJsonAsync(value, cancellationTokenSource.Token);

            if (composition != null)
            {
                Composition = composition;
            }
            _compositionLoader = null;
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
        /// using <seealso cref="LottieAnimationView.CacheStrategy"/>.
        /// </summary>
        public virtual LottieComposition Composition
        {
            set
            {
                Debug.WriteLine("Set Composition \n" + value, Tag);
                //lottieDrawable.Callback = this;

                var isNewComposition = _lottieDrawable.SetComposition(value);
                EnableOrDisableHardwareLayer();
                if (!isNewComposition)
                {
                    // We can avoid re-setting the drawable, and invalidating the view, since the value
                    // hasn't changed.
                    return;
                }

                ImageDrawable = _lottieDrawable;

                _composition = value;

                InvalidateArrange();
                InvalidateMeasure();
                _lottieDrawable.InvalidateSelf();
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

        public event EventHandler<ValueAnimator.ValueAnimatorUpdateEventArgs> AnimatorUpdate
        {
            add => _lottieDrawable.AnimatorUpdate += value;
            remove => _lottieDrawable.AnimatorUpdate -= value;
        }

        public event EventHandler ValueChanged
        {
            add => _lottieDrawable.ValueChanged += value;
            remove => _lottieDrawable.ValueChanged -= value;
        }

        public virtual bool IsAnimating => _lottieDrawable.IsAnimating;

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

        public void PlayAnimation(int startFrame, int endFrame)
        {
            _lottieDrawable.PlayAnimation(startFrame, endFrame);
        }

        public void PlayAnimation(float startProgress, float endProgress)
        {
            _lottieDrawable.PlayAnimation(startProgress, endProgress);
        }

        public virtual void ReverseAnimation()
        {
            _lottieDrawable.ReverseAnimation();
            EnableOrDisableHardwareLayer();
        }

        public int MinFrame
        {
            set => _lottieDrawable.MinFrame = value;
        }

        public float MinProgress
        {
            set => _lottieDrawable.MinProgress = value;
        }

        public int MaxFrame
        {
            set => _lottieDrawable.MaxFrame = value;
        }

        public float MaxProgress
        {
            set => _lottieDrawable.MaxProgress = value;
        }

        public void SetMinAndMaxFrame(int minFrame, int maxFrame)
        {
            _lottieDrawable.SetMinAndMaxFrame(minFrame, maxFrame);
        }

        public void SetMinAndMaxProgress(float minProgress, float maxProgress)
        {
            _lottieDrawable.SetMinAndMaxProgress(minProgress, maxProgress);
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
        /// through ImageAssetsFolder or with an ImageAssetDelegate.
        /// Return the previous Bitmap or null.
        /// </summary>
        public CanvasBitmap UpdateBitmap(string id, CanvasBitmap bitmap)
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
            get => _lottieDrawable.Progress;
            set => _lottieDrawable.Progress = value;
        }

        public virtual long Duration => _composition?.Duration ?? 0;

        public virtual bool PerformanceTrackingEnabled
        {
            set => _lottieDrawable.PerformanceTrackingEnabled = value;
        }

        public virtual PerformanceTracker PerformanceTracker => _lottieDrawable.PerformanceTracker;

        private void EnableOrDisableHardwareLayer()
        {
            var useHardwareLayer = _useHardwareLayer && _lottieDrawable.IsAnimating;
            _lottieDrawable.ForceSoftwareRenderer(!useHardwareLayer);
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