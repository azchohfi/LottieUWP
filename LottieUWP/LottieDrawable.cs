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
        private float _speed = 1f;
        private float _scale = 1f;

        private readonly HashSet<ColorFilterData> _colorFilterData = new HashSet<ColorFilterData>();
        private readonly List<Action<LottieComposition>> _lazyCompositionTasks = new List<Action<LottieComposition>>();
        private ImageAssetManager _imageAssetManager;
        private IImageAssetDelegate _imageAssetDelegate;
        private FontAssetManager _fontAssetManager;
        private FontAssetDelegate _fontAssetDelegate;
        private TextDelegate _textDelegate;
        private bool _systemAnimationsAreDisabled;
        private bool _enableMergePaths;
        private CompositionLayer _compositionLayer;
        private byte _alpha = 255;
        private bool _performanceTrackingEnabled;
        private BitmapCanvas _bitmapCanvas;
        private CanvasAnimatedControl _canvasControl;
        private bool _forceSoftwareRenderer;

        public LottieDrawable()
        {
            _animator.Loop = false;
            _animator.Interpolator = new LinearInterpolator();
            _animator.Update += (sender, e) =>
            {
                if (_systemAnimationsAreDisabled)
                {
                    Progress = 1f;
                }
                else
                {
                    Progress = e.Animation.AnimatedValue;
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
                Speed = _speed;
                Scale = 1f;
                UpdateBounds();
                BuildCompositionLayer();
                ApplyColorFilters();

                // We copy the tasks to a new ArrayList so that if this method is called from multiple threads, 
                // then there won't be two iterators iterating and removing at the same time. 
                foreach (var t in _lazyCompositionTasks.ToList())
                {
                    t.Invoke(composition);
                }
                _lazyCompositionTasks.Clear();
                composition.PerformanceTrackingEnabled = _performanceTrackingEnabled;

                _animator.ForceUpdate();
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

        private void ClearComposition()
        {
            RecycleBitmaps();
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
            set => throw new NotSupportedException("Use AddColorFilter instead.");
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
                    var hasExtraScale = false;
                    float maxScale = GetMaxScale(_bitmapCanvas);
                    if (_compositionLayer.HasMatte() || _compositionLayer.HasMasks())
                    {
                        // Since we can only scale up the animation so much before masks and mattes get clipped, we
                        // may have to scale the canvas to fake the rest. This isn't a problem for software rendering
                        // but hardware accelerated scaling is rasterized so it will appear pixelated.
                        extraScale = scale / maxScale;
                        scale = Math.Min(scale, maxScale);
                        // This check fixes some floating point rounding issues.
                        hasExtraScale = extraScale > 1.001f;
                    }

                    if (hasExtraScale)
                    {
                        _bitmapCanvas.Save();
                        // This is extraScale ^2 because what happens is when the scale increases, the intrinsic size 
                        // of the view increases. That causes the drawable to keep growing even though we are only 
                        // rendering to the size of the view in the top left quarter, leaving the rest blank. 
                        // The first scale by extraScale scales up the canvas so that we are back at the original 
                        // size. The second extraScale is what actually has the scaling effect. 
                        float extraScaleSquared = extraScale * extraScale;
                        int px = (int)(_composition.Bounds.Width * scale / 2f);
                        int py = (int)(_composition.Bounds.Height * scale / 2f);
                        _bitmapCanvas.Scale(extraScaleSquared, extraScaleSquared, px, py);
                    }

                    _matrix.Reset();
                    _matrix = MatrixExt.PreScale(_matrix, scale, scale);
                    _compositionLayer.Draw(_bitmapCanvas, _matrix, _alpha);
                    if (hasExtraScale)
                    {
                        _bitmapCanvas.Restore();
                    }
                    LottieLog.EndSection("Drawable.Draw");
                }
            }
        }

        internal virtual void SystemAnimationsAreDisabled()
        {
            _systemAnimationsAreDisabled = true;
            _animator.SystemAnimationsAreDisabled();
        }

        public virtual bool Looping
        {
            get => _animator.Loop;
            set => _animator.Loop = value;
        }

        public virtual bool IsAnimating => _animator.IsRunning;

        public virtual void PlayAnimation()
        {
            PlayAnimation(true);
        }

        public virtual void ResumeAnimation()
        {
            PlayAnimation(_animator.AnimatedFraction == 1);
        }

        private void PlayAnimation(bool resetProgress)
        {
            if (_compositionLayer == null)
            {
                _lazyCompositionTasks.Add(composition =>
                {
                    PlayAnimation(resetProgress);
                });
                return;
            }
            var progress = _animator.Progress;
            _animator.Start();
            if (resetProgress || _animator.AnimatedFraction == 1f)
            {
                _animator.Progress = _animator.MinProgress;
            }
            else
            {
                _animator.Progress = progress;
            }
        }

        public void PlayAnimation(int startFrame, int endFrame)
        {
            if (_composition == null)
            {
                _lazyCompositionTasks.Add(composition =>
                    {
                        PlayAnimation(startFrame, endFrame);
                    });
                return;
            }
            PlayAnimation(startFrame / _composition.DurationFrames, endFrame / _composition.DurationFrames);
        }

        public void PlayAnimation(float startProgress, float endProgress)
        {
            _animator.UpdateValues(startProgress, endProgress);
            _animator.CurrentPlayTime = 0;
            Progress = startProgress;
            PlayAnimation(false);
        }

        public virtual void ResumeReverseAnimation()
        {
            ReverseAnimation(false);
        }

        public virtual void ReverseAnimation()
        {
            float progress = Progress;
            ReverseAnimation(true);
        }

        private void ReverseAnimation(bool resetProgress)
        {
            if (_compositionLayer == null)
            {
                _lazyCompositionTasks.Add(composition =>
                {
                    ReverseAnimation(resetProgress);
                });
                return;
            }
            var progress = _animator.Progress;
            _animator.Reverse();
            if (resetProgress || Progress == 1f)
            {
                _animator.Progress = _animator.MinProgress;
            }
            else
            {
                _animator.Progress = progress;
            }
        }

        public virtual float Speed
        {
            set
            {
                _speed = value;
                _animator.IsReversed = value < 0;

                if (_composition != null)
                {
                    _animator.Duration = (long)(_composition.Duration / Math.Abs(value));
                }
            }
        }

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

        public float MinProgress
        {
            set => _animator.MinProgress = value;
        }

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

        public float MaxProgress
        {
            set => _animator.MaxProgress = value;
        }

        public void SetMinAndMaxFrame(int minFrame, int maxFrame)
        {
            MinFrame = minFrame;
            MaxFrame = maxFrame;
        }

        public void SetMinAndMaxProgress(float minProgress, float maxProgress)
        {
            MinProgress = minProgress;
            MaxProgress = maxProgress;
        }

        public virtual float Progress
        {
            get => _animator.Progress;
            set
            {
                _animator.Progress = value;
                if (_compositionLayer != null)
                {
                    _compositionLayer.Progress = value;
                }
            }
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