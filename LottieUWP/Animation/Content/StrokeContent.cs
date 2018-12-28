using Windows.UI;
using LottieUWP.Animation.Keyframe;
using LottieUWP.Model;
using LottieUWP.Model.Content;
using LottieUWP.Model.Layer;
using LottieUWP.Value;

namespace LottieUWP.Animation.Content
{
    internal class StrokeContent : BaseStrokeContent
    {
        private readonly BaseLayer _layer;
        private readonly bool _hidden;
        private readonly IBaseKeyframeAnimation<Color?, Color?> _colorAnimation;
        private IBaseKeyframeAnimation<ColorFilter, ColorFilter> _colorFilterAnimation;

        internal StrokeContent(ILottieDrawable lottieDrawable, BaseLayer layer, ShapeStroke stroke)
            : base(lottieDrawable, layer, ShapeStroke.LineCapTypeToPaintCap(stroke.CapType), ShapeStroke.LineJoinTypeToPaintLineJoin(stroke.JoinType), stroke.MiterLimit, stroke.Opacity, stroke.Width, stroke.LineDashPattern, stroke.DashOffset)
        {
            _layer = layer;
            Name = stroke.Name;
            _hidden = stroke.IsHidden;
            _colorAnimation = stroke.Color.CreateAnimation();
            _colorAnimation.ValueChanged += OnValueChanged;
            layer.AddAnimation(_colorAnimation);
        }

        public override void Draw(BitmapCanvas canvas, Matrix3X3 parentMatrix, byte parentAlpha)
        {
            if (_hidden)
            {
                return;
            }
            Paint.Color = _colorAnimation.Value ?? Colors.White;
            if (_colorFilterAnimation != null)
            {
                Paint.ColorFilter = _colorFilterAnimation.Value;
            }
            base.Draw(canvas, parentMatrix, parentAlpha);
        }

        public override string Name { get; }

        public override void AddValueCallback<T>(LottieProperty property, ILottieValueCallback<T> callback)
        {
            base.AddValueCallback(property, callback);
            if (property == LottieProperty.StrokeColor)
            {
                _colorAnimation.SetValueCallback((ILottieValueCallback<Color?>)callback);
            }
            else if (property == LottieProperty.ColorFilter)
            {
                if (callback == null)
                {
                    _colorFilterAnimation = null;
                }
                else
                {
                    _colorFilterAnimation = new ValueCallbackKeyframeAnimation<ColorFilter, ColorFilter>((ILottieValueCallback<ColorFilter>)callback);
                    _colorFilterAnimation.ValueChanged += OnValueChanged;
                    _layer.AddAnimation(_colorAnimation);
                }
            }
        }
    }
}