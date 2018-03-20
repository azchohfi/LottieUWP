using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas;
using LottieUWP.Model;
using LottieUWP.Value;

namespace LottieUWP
{
    /// <summary>
    /// This view will load, deserialize, and display an After Effects animation exported with
    /// bodymovin (https://github.com/bodymovin/bodymovin).
    /// <para>
    /// You may set the animation in one of two ways:
    /// 1) Attrs: <seealso cref="LottieAnimationView.FileNameProperty"/>
    /// 2) Programatically: <seealso cref="SetAnimationAsync(string)"/>, <seealso cref="Composition"/>,
    /// or <seealso cref="SetAnimationAsync(JsonReader)"/>.
    /// </para>
    /// <para>
    /// You can set a default cache strategy with <seealso cref="CacheStrategy.None"/>.
    /// </para>
    /// <para>
    /// You can manually set the progress of the animation with <seealso cref="Progress"/> or
    /// <seealso cref="LottieAnimationView.Progress"/>
    /// </para>
    /// </summary>
    public class LottieAnimationView : UserControl, IDisposable
    {
        private new static readonly string Tag = typeof(LottieAnimationView).Name;

        public static CacheStrategy GlobalDefaultCacheStrategy = CacheStrategy.Weak;

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

        private static readonly Dictionary<string, LottieComposition> AssetStrongRefCache = new Dictionary<string, LottieComposition>();
        private static readonly Dictionary<string, WeakReference<LottieComposition>> AssetWeakRefCache = new Dictionary<string, WeakReference<LottieComposition>>();

        private readonly LottieDrawable _lottieDrawable;

        public CacheStrategy DefaultCacheStrategy
        {
            get => (CacheStrategy)GetValue(DefaultCacheStrategyProperty);
            set => SetValue(DefaultCacheStrategyProperty, value);
        }

        // Using a DependencyProperty as the backing store for DefaultCacheStrategy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultCacheStrategyProperty =
            DependencyProperty.Register("DefaultCacheStrategy", typeof(CacheStrategy), typeof(LottieAnimationView), PropertyMetadata.Create(() => GlobalDefaultCacheStrategy));

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
            if (dependencyObject is LottieAnimationView lottieAnimationView)
            {
                await lottieAnimationView.SetAnimationAsync((string)e.NewValue);
            }
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

        /// <summary>
        /// If you use image assets, you must explicitly specify the folder in assets/ in which they are
        /// located because bodymovin uses the name filenames across all compositions (img_#).
        /// Do NOT rename the images themselves.
        /// 
        /// If your images are located in src/main/assets/airbnb_loader/ then call
        /// `setImageAssetsFolder("airbnb_loader/");`.
        /// 
        /// Be wary if you are using many images, however. Lottie is designed to work with vector shapes
        /// from After Effects.If your images look like they could be represented with vector shapes,
        /// see if it is possible to convert them to shape layers and re-export your animation.Check
        /// the documentation at http://airbnb.io/lottie for more information about importing shapes from
        /// Sketch or Illustrator to avoid this.
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
        /// Takes a <see cref="KeyPath"/>, potentially with wildcards or globstars and resolve it to a list of
        /// zero or more actual <see cref="KeyPath"/>s that exist in the current animation.
        /// 
        /// If you want to set value callbacks for any of these values, it is recommend to use the 
        /// returned <see cref="KeyPath"/> objects because they will be internally resolved to their content
        /// and won't trigger a tree walk of the animation contents when applied. 
        /// </summary>
        /// <param name="keyPath"></param>
        /// <returns></returns>
        public List<KeyPath> ResolveKeyPath(KeyPath keyPath)
        {
            return _lottieDrawable.ResolveKeyPath(keyPath);
        }

        /// Add an property callback for the specified <see cref="KeyPath"/>. This <see cref="KeyPath"/> can resolve 
        /// to multiple contents. In that case, the callbacks's value will apply to all of them. 
        /// 
        /// Internally, this will check if the <see cref="KeyPath"/> has already been resolved with 
        /// <see cref="ResolveKeyPath"/> and will resolve it if it hasn't. 
        public void AddValueCallback<T>(KeyPath keyPath, LottieProperty property, ILottieValueCallback<T> callback)
        {
            _lottieDrawable.AddValueCallback(keyPath, property, callback);
        }

        /// <summary>
        /// Overload of <see cref="AddValueCallback{T}(KeyPath, LottieProperty, ILottieValueCallback{T})"/> that takes an interface. This allows you to use a single abstract 
        /// method code block in Kotlin such as: 
        /// animationView.AddValueCallback(yourKeyPath, LottieProperty.Color) { yourColor }
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyPath"></param>
        /// <param name="property"></param>
        /// <param name="callback"></param>
        public void AddValueCallback<T>(KeyPath keyPath, LottieProperty property, SimpleLottieValueCallback<T> callback)
        {
            _lottieDrawable.AddValueCallback(keyPath, property, new SimpleImplLottieValueCallback<T>(callback));
        }

        /// <summary>
        /// Set the scale on the current composition. The only cost of this function is re-rendering the
        /// current frame so you may call it frequent to scale something up or down.
        /// 
        /// The smaller the animation is, the better the performance will be. You may find that scaling an
        /// animation down then rendering it in a larger ImageView and letting ImageView scale it back up
        /// with a scaleType such as centerInside will yield better performance with little perceivable
        /// quality loss.
        /// 
        /// You can also use a fixed view width/height in conjunction with the normal ImageView 
        /// scaleTypes centerCrop and centerInside.
        /// </summary>
        public virtual double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        // Using a DependencyProperty as the backing store for Scale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), typeof(LottieAnimationView), new PropertyMetadata(1.0, ScalePropertyChangedCallback));

        private static void ScalePropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is LottieAnimationView lottieAnimationView)
                lottieAnimationView._lottieDrawable.Scale = (float)Convert.ToDouble(e.NewValue);
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
            _lottieDrawable.RepeatCount = RepeatCount;

            EnableMergePathsForKitKatAndAbove(false);

            SimpleColorFilter filter = new SimpleColorFilter(ColorFilter);
            KeyPath keyPath = new KeyPath("**");
            var callback = new LottieValueCallback<ColorFilter>(filter);
            AddValueCallback(keyPath, LottieProperty.ColorFilter, callback);

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
        //    ss.imageAssetsFolder = lottieDrawable.ImageAssetsFolder;
        //    ss.RepeatMode = lottieDrawable.RepeatMode; 
        //    ss.RepeatCount = lottieDrawable.RepeatCount; 
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
        //    if (ss.isAnimating)
        //    {
        //        playAnimation();
        //    }
        //    lottieDrawable.ImagesAssetsFolder = ss.imageAssetsFolder;
        //    RepeatMode = ss.RepeatMode; 
        //    RepeatCount = ss.RepeatCount;
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
        public void EnableMergePathsForKitKatAndAbove(bool enable)
        {
            _lottieDrawable.EnableMergePathsForKitKatAndAbove(enable);
        }

        /// <summary>
        /// Returns whether merge paths are enabled for KitKat and above.
        /// </summary>
        /// <returns></returns>
        public bool IsMergePathsEnabledForKitKatAndAbove()
        {
            return _lottieDrawable.IsMergePathsEnabledForKitKatAndAbove();
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

        public bool GetUseHardwareAcceleration()
        {
            return _useHardwareLayer;
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
            if (AssetWeakRefCache.ContainsKey(animationName))
            {
                var compRef = AssetWeakRefCache[animationName];
                if (compRef.TryGetTarget(out LottieComposition lottieComposition))
                {
                    Composition = lottieComposition;
                    return;
                }
            }
            else if (AssetStrongRefCache.ContainsKey(animationName))
            {
                Composition = AssetStrongRefCache[animationName];
                return;
            }

            ClearComposition();
            CancelLoaderTask();

            var cancellationTokenSource = new CancellationTokenSource();

            _compositionLoader = cancellationTokenSource;

            var composition = await LottieComposition.Factory.FromAssetFileNameAsync(animationName, cancellationTokenSource.Token);

            if (cacheStrategy == CacheStrategy.Strong)
            {
                AssetStrongRefCache[animationName] = composition;
            }
            else if (cacheStrategy == CacheStrategy.Weak)
            {
                AssetWeakRefCache[animationName] = new WeakReference<LottieComposition>(composition);
            }

            Composition = composition;
        }

        /// <summary>
        /// <see cref="SetAnimationAsync(JsonReader)"/> which is more efficient than using a JSONObject.
        /// For animations loaded from the network, use <see cref="SetAnimationFromJsonAsync(string)"/>
        /// 
        /// If you must use a JsonObject, you can convert it to a StreamReader with:
        /// <code>new JsonReader(new StringReader(json.ToString()));</code>
        /// </summary>
        /// <param name="json"></param>
        [Obsolete]
        public async Task SetAnimationAsync(JsonObject json)
        {
            await SetAnimationAsync(new JsonReader(new StringReader(json.ToString())));
        }

        /// <summary>
        /// Sets the animation from json string. This is the ideal API to use when loading an animation 
        /// over the network because you can use the raw response body here and a converstion to a
        /// JsonObject never has to be done.
        /// </summary>
        /// <param name="jsonString"></param>
        public async Task SetAnimationFromJsonAsync(string jsonString)
        {
            await SetAnimationAsync(new JsonReader(new StringReader(jsonString)));
        }

        /// <summary>
        /// Sets the animation from a JSONReader.
        /// This will load and deserialize the file asynchronously.
        /// <para>
        /// This is particularly useful for animations loaded from the network. You can fetch the
        /// bodymovin json from the network and pass it directly here.
        /// </para>
        /// </summary>
        public virtual async Task SetAnimationAsync(JsonReader reader)
        {
            ClearComposition();
            CancelLoaderTask();
            var cancellationTokenSource = new CancellationTokenSource();

            _compositionLoader = cancellationTokenSource;

            var composition = await LottieComposition.Factory.FromJsonReaderAsync(reader, cancellationTokenSource.Token);

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
            get
            {
                return _composition;
            }
            set
            {
                Debug.WriteLine("Set Composition \n" + value, Tag);
                //lottieDrawable.Callback = this;

                _composition = value;
                var isNewComposition = _lottieDrawable.SetComposition(value);
                EnableOrDisableHardwareLayer();
                if (_viewbox?.Child == _lottieDrawable && !isNewComposition)
                {
                    // We can avoid re-setting the drawable, and invalidating the view, since the value
                    // hasn't changed.
                    return;
                }

                ImageDrawable = _lottieDrawable;

                FrameRate = _composition.FrameRate;

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

        /// <summary>
        /// Plays the animation from the beginning.If speed is &lt; 0, it will start at the end
        /// and play towards the beginning
        /// </summary>
        public virtual void PlayAnimation()
        {
            _lottieDrawable.PlayAnimation();
            EnableOrDisableHardwareLayer();
        }

        /// <summary>
        /// Continues playing the animation from its current position. If speed &lt; 0, it will play backwards
        /// from the current position.
        /// </summary>
        public virtual void ResumeAnimation()
        {
            _lottieDrawable.ResumeAnimation();
            EnableOrDisableHardwareLayer();
        }

        /// <summary>
        /// Gets or sets the minimum frame that the animation will start from when playing or looping.
        /// </summary>
        public float MinFrame
        {
            set => _lottieDrawable.MinFrame = value;
            get => _lottieDrawable.MinFrame;
        }

        /// <summary>
        /// Sets the minimum progress that the animation will start from when playing or looping.
        /// </summary>
        public float MinProgress
        {
            set => _lottieDrawable.MinProgress = value;
        }

        /// <summary>
        /// Gets or sets the maximum frame that the animation will end at when playing or looping.
        /// </summary>
        public float MaxFrame
        {
            set => _lottieDrawable.MaxFrame = value;
            get => _lottieDrawable.MaxFrame;
        }

        /// <summary>
        /// Sets the maximum progress that the animation will end at when playing or looping.
        /// </summary>
        public float MaxProgress
        {
            set => _lottieDrawable.MaxProgress = value;
        }

        /// <summary>
        /// <see cref="MinFrame"/>
        /// <see cref="MaxFrame"/>
        /// </summary>
        /// <param name="minFrame"></param>
        /// <param name="maxFrame"></param>
        public void SetMinAndMaxFrame(float minFrame, float maxFrame)
        {
            _lottieDrawable.SetMinAndMaxFrame(minFrame, maxFrame);
        }

        /// <summary>
        /// <see cref="MinProgress"/>
        /// <see cref="MaxProgress"/>
        /// </summary>
        /// <param name="minProgress"></param>
        /// <param name="maxProgress"></param>
        public void SetMinAndMaxProgress(float minProgress, float maxProgress)
        {
            if (minProgress < 0)
                minProgress = 0;
            if (minProgress > 1)
                minProgress = 1;

            if (maxProgress < 0)
                maxProgress = 0;
            if (maxProgress > 1)
                maxProgress = 1;

            _lottieDrawable.SetMinAndMaxProgress(minProgress, maxProgress);
        }

        /// <summary>
        /// Reverses the current animation speed. This does NOT play the animation. 
        ///<see cref="Speed"/>
        ///<see cref="PlayAnimation"/>
        ///<see cref="ResumeAnimation"/>
        /// </summary>
        public void ReverseAnimationSpeed()
        {
            _lottieDrawable.ReverseAnimationSpeed();
        }

        /// <summary>
        /// Sets the playback speed. If speed &lt; 0, the animation will play backwards. 
        /// Returns the current playback speed. This will be &lt; 0 if the animation is playing backwards. 
        /// </summary>
        public double Speed
        {
            get { return (double)GetValue(SpeedProperty); }
            set { SetValue(SpeedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Speed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpeedProperty =
            DependencyProperty.Register("Speed", typeof(double), typeof(LottieAnimationView), new PropertyMetadata(1.0, SpeedProperyChangedCallback));

        private static void SpeedProperyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is LottieAnimationView lottieAnimationView)
                lottieAnimationView._lottieDrawable.Speed = (float)Convert.ToDouble(e.NewValue);
        }

        public event EventHandler<ValueAnimator.ValueAnimatorUpdateEventArgs> AnimatorUpdate
        {
            add => _lottieDrawable.AnimatorUpdate += value;
            remove => _lottieDrawable.AnimatorUpdate -= value;
        }

        public void RemoveAllUpdateListeners()
        {
            _lottieDrawable.RemoveAllUpdateListeners();
        }

        public event EventHandler ValueChanged
        {
            add => _lottieDrawable.ValueChanged += value;
            remove => _lottieDrawable.ValueChanged -= value;
        }

        public void RemoveAllAnimatorListeners()
        {
            _lottieDrawable.RemoveAllAnimatorListeners();
        }

        /// <summary>
        /// <see cref="RepeatCount"/>
        /// </summary>
        [Obsolete]
        public bool Loop
        {
            get => (bool)GetValue(LoopProperty);
            set => SetValue(LoopProperty, value);
        }

        // Using a DependencyProperty as the backing store for Loop.  This enables animation, styling, binding, etc...
        [Obsolete]
        public static readonly DependencyProperty LoopProperty =
            DependencyProperty.Register("Loop", typeof(bool), typeof(LottieAnimationView), new PropertyMetadata(false, LoopPropertyChangedCallback));

        private static void LoopPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is LottieAnimationView lottieAnimationView && (bool)e.NewValue)
                lottieAnimationView._lottieDrawable.RepeatCount = LottieDrawable.Infinite;
        }

        /// <summary>
        /// Defines what this animation should do when it reaches the end. This 
        /// setting is applied only when the repeat count is either greater than 
        /// 0 or <see cref="LottieUWP.RepeatMode.Infinite"/>. Defaults to <see cref="LottieUWP.RepeatMode.Restart"/>.
        /// Return either one of <see cref="LottieUWP.RepeatMode.Reverse"/> or <see cref="LottieUWP.RepeatMode.Restart"/>
        /// </summary>
        public RepeatMode RepeatMode
        {
            get { return (RepeatMode)GetValue(RepeatModeProperty); }
            set { SetValue(RepeatModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RepeatMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RepeatModeProperty =
            DependencyProperty.Register("RepeatMode", typeof(RepeatMode), typeof(LottieAnimationView), new PropertyMetadata(RepeatMode.Restart, RepeatModePropertyChangedCallback));

        private static void RepeatModePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is LottieAnimationView lottieAnimationView)
                lottieAnimationView._lottieDrawable.RepeatMode = (RepeatMode)e.NewValue;
        }

        /// <summary>
        /// Sets how many times the animation should be repeated. If the repeat 
        /// count is 0, the animation is never repeated. If the repeat count is 
        /// greater than 0 or <see cref="LottieUWP.RepeatMode.Infinite"/>, the repeat mode will be taken 
        /// into account. The repeat count is 0 by default. 
        /// 
        /// Count the number of times the animation should be repeated
        /// 
        /// Return the number of times the animation should repeat, or <see cref="LottieUWP.RepeatMode.Infinite"/>
        /// </summary>
        public int RepeatCount
        {
            get { return (int)GetValue(RepeatCountProperty); }
            set { SetValue(RepeatCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RepeatCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RepeatCountProperty =
            DependencyProperty.Register("RepeatCount", typeof(int), typeof(LottieAnimationView), new PropertyMetadata(LottieDrawable.Infinite, RepeatCountPropertyChangedCallback));

        private static void RepeatCountPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is LottieAnimationView lottieAnimationView)
                lottieAnimationView._lottieDrawable.RepeatCount = (int)e.NewValue;
        }

        public double FrameRate
        {
            get { return (double)GetValue(FrameRateProperty); }
            set { SetValue(FrameRateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RepeatCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FrameRateProperty =
            DependencyProperty.Register("FrameRate", typeof(double), typeof(LottieAnimationView), new PropertyMetadata(60.0, FrameRatePropertyChangedCallback));

        private static void FrameRatePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is LottieAnimationView lottieAnimationView)
                lottieAnimationView._lottieDrawable.FrameRate = (float)Convert.ToDouble(e.NewValue);
        }

        public virtual bool IsAnimating => _lottieDrawable.IsAnimating;

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
        /// 
        /// Be wary if you are using many images, however. Lottie is designed to work with vector shapes
        /// from After Effects. If your images look like they could be represented with vector shapes,
        /// see if it is possible to convert them to shape layers and re-export your animation. Check
        /// the documentation at http://airbnb.io/lottie for more information about importing shapes from
        /// Sketch or Illustrator to avoid this.
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
            _lottieDrawable.PauseAnimation();
            EnableOrDisableHardwareLayer();
        }

        public float Frame
        {
            /** 
            * Sets the progress to the specified frame. 
            * If the composition isn't set yet, the progress will be set to the frame when 
            * it is. 
            */
            set => _lottieDrawable.Frame = value;
            /** 
            * Get the currently rendered frame. 
            */
            get => _lottieDrawable.Frame;
        }

        public virtual float Progress
        {
            get => _lottieDrawable.Progress;
            set => _lottieDrawable.Progress = value;
        }

        public virtual long Duration => _composition != null ? (long)_composition.Duration : 0;

        public virtual bool PerformanceTrackingEnabled
        {
            set => _lottieDrawable.PerformanceTrackingEnabled = value;
        }

        public virtual PerformanceTracker PerformanceTracker => _lottieDrawable.PerformanceTracker;

        private void ClearComposition()
        {
            _composition = null;
            _lottieDrawable.ClearComposition();
        }

        private void EnableOrDisableHardwareLayer()
        {
            var useHardwareLayer = _useHardwareLayer && _lottieDrawable.IsAnimating;
            _lottieDrawable.ForceSoftwareRenderer(!useHardwareLayer);
        }

        private void Dispose(bool disposing)
        {
            _compositionLoader?.Dispose();
            _lottieDrawable.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~LottieAnimationView()
        {
            Dispose(false);
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new FrameworkElementAutomationPeer(this);
        }

        //private class SavedState : BaseSavedState
        //{
        //    internal string animationName;
        //    internal float progress;
        //    internal bool isAnimating;
        //    internal string imageAssetsFolder;
        //    internal int repeatMode; 
        //    internal int repeatCount;

        //    internal SavedState(Parcelable superState) : base(superState)
        //    {
        //    }

        //    internal SavedState(Parcel @in) : base(@in)
        //    {
        //        animationName = @in.readString();
        //        progress = @in.readFloat();
        //        isAnimating = @in.readInt() == 1;
        //        imageAssetsFolder = @in.readString();
        //        repeatMode = in.readInt();
        //        repeatCount = in.readInt();
        //    }

        //    public void writeToParcel(Parcel @out, int flags)
        //    {
        //        base.writeToParcel(@out, flags);
        //        @out.writeString(animationName);
        //        @out.writeFloat(progress);
        //        @out.writeInt(isAnimating ? 1 : 0);
        //        @out.writeString(imageAssetsFolder);
        //        @out.writeInt(repeatMode);
        //        @out.writeInt(repeatCount);
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