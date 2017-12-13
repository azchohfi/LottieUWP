using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using LottieUWP.Animation.Content;
using LottieUWP.Manager;
using LottieUWP.Model.Layer;
using LottieUWP.Utils;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace LottieUWP
{
    /// <summary>
    /// This can be used to show an lottie animation in any place that would normally take a drawable.
    /// If there are masks or mattes, then you MUST call <seealso cref="RecycleBitmaps()"/> when you are done
    /// or else you will leak bitmaps.
    /// <para>
    /// It is preferable to use <seealso cref="LottieAnimationView"/> when possible because it
    /// handles bitmap recycling and asynchronous loading
    /// of compositions.
    /// </para>
    /// </summary>
    public class LottieDrawable : UserControl
    {
        private Matrix3X3 _matrix = Matrix3X3.CreateIdentity();
        private LottieComposition _composition;
        private readonly LottieValueAnimator _animator = new LottieValueAnimator();
        private float _scale = 1f;

        private readonly HashSet<ColorFilterData> _colorFilterData = new HashSet<ColorFilterData>();
        private readonly List<Action<LottieComposition>> _lazyCompositionTasks = new List<Action<LottieComposition>>();
        private ImageAssetManager _imageAssetManager;
        private IImageAssetDelegate _imageAssetDelegate;
        private FontAssetManager _fontAssetManager;
        private FontAssetDelegate _fontAssetDelegate;
        private TextDelegate _textDelegate;
        private bool _enableMergePaths;
        private CompositionLayer _compositionLayer;
        private byte _alpha = 255;
        private bool _performanceTrackingEnabled;
        private BitmapCanvas _bitmapCanvas;
        private CanvasAnimatedControl _canvasControl;
        private bool _forceSoftwareRenderer;

        /// <summary>
        /// This value used used with the <see cref="RepeatCount"/> property to repeat
        /// the animation indefinitely.
        /// </summary>
        public const int Infinite = -1;

        public LottieDrawable()
        {
            _animator.Update += (sender, e) =>
            {
                //if (_systemAnimationsAreDisabled)
                //{
                //    Progress = 1f;
                //}
                //else
                //{
                //    Progress = e.Animation.AnimatedValue;
                //}
                if (_compositionLayer != null)
                {
                    _compositionLayer.Progress = _animator.Value;
                }
            };
            Loaded += UserControl_Loaded;
            Unloaded += UserControl_Unloaded;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _canvasControl = new CanvasAnimatedControl
            {
                ForceSoftwareRenderer = _forceSoftwareRenderer
            };

            _canvasControl.Draw += CanvasControlOnDraw;
            Content = _canvasControl;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Explicitly remove references to allow the Win2D controls to get garbage collected
            if (_canvasControl != null)
            {
                _canvasControl.RemoveFromVisualTree();
                _canvasControl = null;
            }
        }

        public void ForceSoftwareRenderer(bool force)
        {
            _forceSoftwareRenderer = force;
            if (_canvasControl != null)
            {
                _canvasControl.ForceSoftwareRenderer = force;
            }
        }

        /// <summary>
        /// Returns whether or not any layers in this composition has masks.
        /// </summary>
        public virtual bool HasMasks()
        {
            return _compositionLayer != null && _compositionLayer.HasMasks();
        }

        /// <summary>
        /// Returns whether or not any layers in this composition has a matte layer.
        /// </summary>
        public virtual bool HasMatte()
        {
            return _compositionLayer != null && _compositionLayer.HasMatte();
        }

        internal virtual bool EnableMergePathsForKitKatAndAbove()
        {
            return _enableMergePaths;
        }

        /// <summary>
        /// Enable this to get merge path support for devices running KitKat (19) and above.
        /// 
        /// Merge paths currently don't work if the the operand shape is entirely contained within the
        /// first shape. If you need to cut out one shape from another shape, use an even-odd fill type
        /// instead of using merge paths.
        /// </summary>
        public virtual void EnableMergePathsForKitKatAndAbove(bool enable)
        {
            _enableMergePaths = enable;
            if (_composition != null)
            {
                BuildCompositionLayer();
            }
        }

        /// <summary>
        /// If you use image assets, you must explicitly specify the folder in assets/ in which they are
        /// located because bodymovin uses the name filenames across all compositions (img_#).
        /// Do NOT rename the images themselves.
        /// 
        /// If your images are located in src/main/assets/airbnb_loader/ then call
        /// `setImageAssetsFolder("airbnb_loader/");`.
        /// 
        /// 
        /// If you use LottieDrawable directly, you MUST call <seealso cref="RecycleBitmaps()"/> when you
        /// are done. Calling <seealso cref="RecycleBitmaps()"/> doesn't have to be final and <seealso cref="LottieDrawable"/>
        /// will recreate the bitmaps if needed but they will leak if you don't recycle them.
        /// </summary>
        public virtual string ImageAssetsFolder { get; set; }

        /// <summary>
        /// If you have image assets and use <seealso cref="LottieDrawable"/> directly, you must call this yourself.
        /// 
        /// Calling recycleBitmaps() doesn't have to be final and <seealso cref="LottieDrawable"/>
        /// will recreate the bitmaps if needed but they will leak if you don't recycle them.
        /// 
        /// </summary>
        public virtual void RecycleBitmaps()
        {
            _imageAssetManager?.RecycleBitmaps();
        }

        /// <returns> True if the composition is different from the previously set composition, false otherwise. </returns>
        public virtual bool SetComposition(LottieComposition composition)
        {
            //if (Callback == null) // TODO: needed?
            //{
            //    throw new System.InvalidOperationException("You or your view must set a Drawable.Callback before setting the composition. This " + "gets done automatically when added to an ImageView. " + "Either call ImageView.setImageDrawable() before setComposition() or call " + "setCallback(yourView.getCallback()) first.");
            //}

            if (_composition == composition)
            {
                return false;
            }

            lock (this)
            {
                ClearComposition();
                _composition = composition;
                BuildCompositionLayer();
                _animator.CompositionDuration = composition.Duration;
                Progress = _animator.Value;
                Scale = _scale;
                UpdateBounds();
                ApplyColorFilters();

                // We copy the tasks to a new ArrayList so that if this method is called from multiple threads, 
                // then there won't be two iterators iterating and removing at the same time. 
                foreach (var t in _lazyCompositionTasks.ToList())
                {
                    t.Invoke(composition);
                }
                _lazyCompositionTasks.Clear();
                composition.PerformanceTrackingEnabled = _performanceTrackingEnabled;
            }

            return true;
        }

        public virtual bool PerformanceTrackingEnabled
        {
            set
            {
                _performanceTrackingEnabled = value;
                if (_composition != null)
                {
                    _composition.PerformanceTrackingEnabled = value;
                }
            }
        }

        public virtual PerformanceTracker PerformanceTracker => _composition?.PerformanceTracker;

        private void BuildCompositionLayer()
        {
            _compositionLayer = new CompositionLayer(this, Layer.Factory.NewInstance(_composition), _composition.Layers, _composition);
        }

        private void ApplyColorFilters()
        {
            if (_compositionLayer == null)
            {
                return;
            }

            foreach (var data in _colorFilterData)
            {
                _compositionLayer.AddColorFilter(data.LayerName, data.ContentName, data.ColorFilter);
            }
        }

        public void ClearComposition()
        {
            RecycleBitmaps();
            if (_animator.IsRunning)
            {
                _animator.Cancel();
            }
            _composition = null;
            _compositionLayer = null;
            _imageAssetManager = null;
            InvalidateSelf();
        }

        public void InvalidateSelf()
        {
            _canvasControl?.Invalidate();
        }

        public void SetAlpha(byte alpha)
        {
            _alpha = alpha;
        }

        public int GetAlpha()
        {
            return _alpha;
        }

        public ColorFilter ColorFilter
        {
            set => Debug.WriteLine("Use addColorFilter instead.", LottieLog.Tag);
        }

        /// <summary>
        /// Add a color filter to specific content on a specific layer. </summary>
        /// <param name="layerName"> name of the layer where the supplied content name lives </param>
        /// <param name="contentName"> name of the specific content that the color filter is to be applied </param>
        /// <param name="colorFilter"> the color filter, null to clear the color filter </param>
        public virtual void AddColorFilterToContent(string layerName, string contentName, ColorFilter colorFilter)
        {
            AddColorFilterInternal(layerName, contentName, colorFilter);
        }

        /// <summary>
        /// Add a color filter to a whole layer </summary>
        /// <param name="layerName"> name of the layer that the color filter is to be applied </param>
        /// <param name="colorFilter"> the color filter, null to clear the color filter </param>
        public virtual void AddColorFilterToLayer(string layerName, ColorFilter colorFilter)
        {
            AddColorFilterInternal(layerName, null, colorFilter);
        }

        /// <summary>
        /// Add a color filter to all layers </summary>
        /// <param name="colorFilter"> the color filter, null to clear all color filters </param>
        public virtual void AddColorFilter(ColorFilter colorFilter)
        {
            AddColorFilterInternal(null, null, colorFilter);
        }

        /// <summary>
        /// Clear all color filters on all layers and all content in the layers
        /// </summary>
        public virtual void ClearColorFilters()
        {
            _colorFilterData.Clear();
            AddColorFilterInternal(null, null, null);
        }

        /// <summary>
        /// Private method to capture all color filter additions.
        /// There are 3 different behaviors here.
        /// 1. layerName is null. All layers supporting color filters will apply the passed in color filter
        /// 2. layerName is not null, contentName is null. This will apply the passed in color filter
        ///    to the whole layer
        /// 3. layerName is not null, contentName is not null. This will apply the pass in color filter
        ///    to a specific composition content.
        /// </summary>
        private void AddColorFilterInternal(string layerName, string contentName, ColorFilter colorFilter)
        {
            var data = new ColorFilterData(layerName, contentName, colorFilter);
            if (colorFilter == null && _colorFilterData.Contains(data))
            {
                _colorFilterData.Remove(data);
            }
            else
            {
                _colorFilterData.Add(new ColorFilterData(layerName, contentName, colorFilter));
            }

            _compositionLayer?.AddColorFilter(layerName, contentName, colorFilter);
        }

        //public int Opacity
        //{
        //    get
        //    {
        //        return PixelFormat.TRANSLUCENT;
        //    }
        //}

        private void CanvasControlOnDraw(ICanvasAnimatedControl canvasControl, CanvasAnimatedDrawEventArgs args)
        {
            lock (this)
            {
                using (_bitmapCanvas.CreateSession(canvasControl.Device, canvasControl.Size.Width, canvasControl.Size.Height, args.DrawingSession))
                {
                    _bitmapCanvas.Clear(Colors.Transparent);
                    LottieLog.BeginSection("Drawable.Draw");
                    if (_compositionLayer == null)
                    {
                        return;
                    }

                    var scale = _scale;
                    float extraScale = 1f;

                    float maxScale = GetMaxScale(_bitmapCanvas);
                    if (scale > maxScale)
                    {
                        scale = maxScale;
                        extraScale = _scale / scale;
                    }

                    if (extraScale > 1)
                    {
                        // This is a bit tricky... 
                        // We can't draw on a canvas larger than ViewConfiguration.get(context).getScaledMaximumDrawingCacheSize() 
                        // which works out to be roughly the size of the screen because Android can't generate a 
                        // bitmap large enough to render to. 
                        // As a result, we cap the scale such that it will never be wider/taller than the screen 
                        // and then only render in the top left corner of the canvas. We then use extraScale 
                        // to scale up the rest of the scale. However, since we rendered the animation to the top 
                        // left corner, we need to scale up and translate the canvas to zoom in on the top left 
                        // corner. 
                        _bitmapCanvas.Save();
                        float halfWidth = (float)_composition.Bounds.Width / 2f;
                        float halfHeight = (float)_composition.Bounds.Height / 2f;
                        float scaledHalfWidth = halfWidth * scale;
                        float scaledHalfHeight = halfHeight * scale;
                        _bitmapCanvas.Translate(
                            Scale * halfWidth - scaledHalfWidth,
                            Scale * halfHeight - scaledHalfHeight);
                        _bitmapCanvas.Scale(extraScale, extraScale, scaledHalfWidth, scaledHalfHeight);
                    }

                    _matrix.Reset();
                    _matrix = MatrixExt.PreScale(_matrix, scale, scale);
                    _compositionLayer.Draw(_bitmapCanvas, _matrix, _alpha);
                    LottieLog.EndSection("Drawable.Draw");

                    if (extraScale > 1)
                    {
                        _bitmapCanvas.Restore();
                    }
                }
            }
        }

        /// <summary>
        /// Plays the animation from the beginning. If speed is &lt; 0, it will start at the end
        /// and play towards the beginning
        /// </summary>
        public virtual void PlayAnimation()
        {
            if (_compositionLayer == null)
            {
                _lazyCompositionTasks.Add(composition =>
                {
                    PlayAnimation();
                });
                return;
            }
            _animator.PlayAnimation();
        }

        /// <summary>
        /// Continues playing the animation from its current position. If speed &lt; 0, it will play backwards 
        /// from the current position.
        /// </summary>
        public virtual void ResumeAnimation()
        {
            if (_compositionLayer == null)
            {
                _lazyCompositionTasks.Add(composition =>
                {
                    ResumeAnimation();
                });
                return;
            }
            _animator.ResumeAnimation();
        }

        /// <summary>
        /// Sets the minimum frame that the animation will start from when playing or looping.
        /// </summary>
        public int MinFrame
        {
            set
            {
                if (_composition == null)
                {
                    _lazyCompositionTasks.Add(composition =>
                        {
                            MinFrame = value;
                        });
                    return;
                }
                MinProgress = value / _composition.DurationFrames;
            }
        }

        /// <summary>
        /// Sets the minimum progress that the animation will start from when playing or looping. 
        /// </summary>
        public float MinProgress
        {
            set => _animator.MinValue = value;
        }

        /// <summary>
        /// Sets the maximum frame that the animation will end at when playing or looping.
        /// </summary>
        public int MaxFrame
        {
            set
            {
                if (_composition == null)
                {
                    _lazyCompositionTasks.Add(composition =>
                        {
                            MaxFrame = value;
                        });
                    return;
                }
                MaxProgress = value / _composition.DurationFrames;
            }
        }

        /// <summary>
        /// Sets the maximum progress that the animation will end at when playing or looping.
        /// </summary>
        public float MaxProgress
        {
            set
            {
                if (value < 0)
                    value = 0;
                if (value > 1)
                    value = 1;
                _animator.MaxValue = value;
            }
        }

        /// <summary>
        /// <see cref="MinFrame"/>
        /// <see cref="MaxFrame"/>
        /// </summary>
        /// <param name="minFrame"></param>
        /// <param name="maxFrame"></param>
        public void SetMinAndMaxFrame(int minFrame, int maxFrame)
        {
			if (_composition == null)
			{
				_lazyCompositionTasks.Add(composition =>
				{
					SetMinAndMaxFrame(minFrame, maxFrame);
				});
				return;
			}
			_animator.SetMinAndMaxValues(minFrame / _composition.DurationFrames, maxFrame / _composition.DurationFrames);
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
            _animator.SetMinAndMaxValues(minProgress, maxProgress);
        }

        /// <summary>
        /// Reverses the current animation speed. This does NOT play the animation.
        /// <see cref="Speed"/>
        /// <see cref="PlayAnimation"/>
        /// <see cref="ResumeAnimation"/>
        /// </summary>
        public void ReverseAnimationSpeed()
        {
            _animator.ReverseAnimationSpeed();
        }

        /// <summary>
        /// Sets the playback speed. If speed &lt; 0, the animation will play backwards.
        /// Returns the current playback speed. This will be &lt; 0 if the animation is playing backwards.
        /// </summary>
        public virtual float Speed
        {
            set => _animator.Speed = value;
            get => _animator.Speed;
        }

        public event EventHandler<ValueAnimator.ValueAnimatorUpdateEventArgs> AnimatorUpdate
        {
            add => _animator.Update += value;
            remove => _animator.Update -= value;
        }

        public event EventHandler ValueChanged
        {
            add => _animator.ValueChanged += value;
            remove => _animator.ValueChanged -= value;
        }

        public int Frame
        {
            /**
            * Sets the progress to the specified frame.
            * If the composition isn't set yet, the progress will be set to the frame when
            * it is.
            */
            set
            {
                if (_composition == null)
                {
                    _lazyCompositionTasks.Add(composition =>
                    {
                        Frame = value;
                    });
                    return;
                }

                Progress = value / _composition.DurationFrames;
            }
            /**
            * Get the currently rendered frame.
            */
            get
            {
                if (_composition == null)
                {
                    return 0;
                }

                return (int)(Progress * _composition.DurationFrames);
            }
        }

        public virtual float Progress
        {
            get => _animator.Value;
            set
            {
                _animator.Value = value;
                if (_compositionLayer != null)
                {
                    _compositionLayer.Progress = value;
                }
            }
        }

        /// <summary>
        /// <see cref="RepeatCount"/>
        /// </summary>
        [Obsolete]
        public virtual bool Looping
        {
            get => _animator.RepeatCount == (int)Infinite;
            set => _animator.RepeatCount = value ? (int)Infinite : 0;
        }

        /// <summary>
        /// Defines what this animation should do when it reaches the end. This
        /// setting is applied only when the repeat count is either greater than
        /// 0 or <see cref="RepeatMode.Infinite"/>. Defaults to <see cref="RepeatMode.Restart"/>.
        /// Return either one of <see cref="RepeatMode.Reverse"/> or <see cref="RepeatMode.Restart"/>
        /// </summary>
        /// <param name="mode"><seealso cref="RepeatMode"/></param>
        public RepeatMode RepeatMode
        {
            set => _animator.RepeatMode = value;
            get => _animator.RepeatMode;
        }

        /// <summary>
        /// Sets how many times the animation should be repeated. If the repeat 
        /// count is 0, the animation is never repeated. If the repeat count is 
        /// greater than 0 or <see cref="RepeatMode.Infinite"/>, the repeat mode will be taken 
        /// into account. The repeat count is 0 by default.
        /// 
        /// Count the number of times the animation should be repeated
        /// 
        /// Return the number of times the animation should repeat, or <see cref="RepeatMode.Infinite"/>
        /// </summary>
        public int RepeatCount
        {
            set => _animator.RepeatCount = value;
            get => _animator.RepeatCount;
        }

        public virtual bool IsAnimating => _animator.IsRunning;

        internal virtual void SystemAnimationsAreDisabled()
        {
            _animator.SystemAnimationsAreDisabled();
        }

        /// <summary>
        /// Use this to manually set fonts. 
        /// </summary>
        public virtual FontAssetDelegate FontAssetDelegate
        {
            set
            {
                _fontAssetDelegate = value;
                if (_fontAssetManager != null)
                {
                    _fontAssetManager.Delegate = value;
                }
            }
        }

        public virtual TextDelegate TextDelegate
        {
            set => _textDelegate = value;
            get => _textDelegate;
        }

        internal virtual bool UseTextGlyphs()
        {
            return _textDelegate == null && _composition.Characters.Count > 0;
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
            set
            {
                _scale = value;
                lock (this)
                {
                    UpdateBounds();
                    InvalidateMeasure();
                    InvalidateSelf();
                }
            }
            get => _scale;
        }

        /// <summary>
        /// Use this if you can't bundle images with your app. This may be useful if you download the
        /// animations from the network or have the images saved to an SD Card. In that case, Lottie
        /// will defer the loading of the bitmap to this delegate.
        /// </summary>
        public virtual IImageAssetDelegate ImageAssetDelegate
        {
            set
            {
                _imageAssetDelegate = value;
                if (_imageAssetManager != null)
                {
                    _imageAssetManager.Delegate = value;
                }
            }
        }

        public virtual LottieComposition Composition => _composition;

        private void UpdateBounds()
        {
            if (_composition == null)
            {
                return;
            }
            Width = (int)(_composition.Bounds.Width * _scale);
            Height = (int)(_composition.Bounds.Height * _scale);
            _bitmapCanvas = new BitmapCanvas(Width, Height);
        }

        public virtual void CancelAnimation()
        {
            _lazyCompositionTasks.Clear();
            _animator.Cancel();
        }

        public void PauseAnimation()
        {
            _lazyCompositionTasks.Clear();
            _animator.PauseAnimation();
        }

        public int IntrinsicWidth => _composition == null ? -1 : (int)(_composition.Bounds.Width * _scale);

        public int IntrinsicHeight => _composition == null ? -1 : (int)(_composition.Bounds.Height * _scale);

        /// 
        /// <summary>
        /// Allows you to modify or clear a bitmap that was loaded for an image either automatically
        /// 
        /// through <seealso cref="ImageAssetsFolder"/> or with an <seealso cref="ImageAssetDelegate"/>.
        /// 
        /// 
        /// </summary>
        /// <returns> the previous Bitmap or null.
        ///  </returns>
        public virtual CanvasBitmap UpdateBitmap(string id, CanvasBitmap bitmap)
        {
            var bm = ImageAssetManager;
            if (bm == null)
            {
                Debug.WriteLine("Cannot update bitmap. Most likely the drawable is not added to a View " + "which prevents Lottie from getting a Context.", LottieLog.Tag);
                return null;
            }
            var ret = bm.UpdateBitmap(id, bitmap);
            InvalidateSelf();
            return ret;
        }

        internal virtual CanvasBitmap GetImageAsset(string id)
        {
            return ImageAssetManager?.BitmapForId(_canvasControl.Device, id);
        }

        private ImageAssetManager ImageAssetManager
        {
            get
            {
                if (_imageAssetManager != null && false)//!_imageAssetManager.hasSameContext(Context))
                {
                    _imageAssetManager.RecycleBitmaps();
                    _imageAssetManager = null;
                }

                if (_imageAssetManager == null)
                {
                    _imageAssetManager = new ImageAssetManager(ImageAssetsFolder, _imageAssetDelegate, _composition.Images);
                }

                return _imageAssetManager;
            }
        }

        internal virtual Typeface GetTypeface(string fontFamily, string style)
        {
            var assetManager = FontAssetManager;
            return assetManager?.GetTypeface(fontFamily, style);
        }

        private FontAssetManager FontAssetManager => _fontAssetManager ??
            (_fontAssetManager = new FontAssetManager(_fontAssetDelegate));

        /**
        * If there are masks or mattes, we can't scale the animation larger than the canvas or else 
        * the off screen rendering for masks and mattes after saveLayer calls will get clipped. 
        */
        private float GetMaxScale(BitmapCanvas canvas)
        {
            var maxScaleX = (float)canvas.Width / (float)_composition.Bounds.Width;
            var maxScaleY = (float)canvas.Height / (float)_composition.Bounds.Height;
            return Math.Min(maxScaleX, maxScaleY);
        }

        private class ColorFilterData
        {
            internal readonly string LayerName;
            internal readonly string ContentName;
            internal readonly ColorFilter ColorFilter;

            internal ColorFilterData(string layerName, string contentName, ColorFilter colorFilter)
            {
                LayerName = layerName;
                ContentName = contentName;
                ColorFilter = colorFilter;
            }

            public override int GetHashCode()
            {
                var hashCode = 17;
                if (!string.IsNullOrEmpty(LayerName))
                {
                    hashCode = hashCode * 31 * LayerName.GetHashCode();
                }

                if (!string.IsNullOrEmpty(ContentName))
                {
                    hashCode = hashCode * 31 * ContentName.GetHashCode();
                }
                return hashCode;
            }

            public override bool Equals(object obj)
            {
                if (this == obj)
                {
                    return true;
                }

                if (!(obj is ColorFilterData))
                {
                    return false;
                }

                var other = (ColorFilterData)obj;

                return GetHashCode() == other.GetHashCode() && ColorFilter == other.ColorFilter;
            }
        }
    }
}