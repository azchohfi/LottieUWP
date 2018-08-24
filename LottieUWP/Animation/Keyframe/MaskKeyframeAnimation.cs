using System.Collections.Generic;
using LottieUWP.Model.Content;

namespace LottieUWP.Animation.Keyframe
{
    internal class MaskKeyframeAnimation
    {
        private readonly List<IBaseKeyframeAnimation<ShapeData, Path>> _maskAnimations;
        private readonly List<IBaseKeyframeAnimation<int?, int?>> _opacityAnimations;

        internal MaskKeyframeAnimation(List<Mask> masks)
        {
            Masks = masks;
            _maskAnimations = new List<IBaseKeyframeAnimation<ShapeData, Path>>(masks.Count);
            _opacityAnimations = new List<IBaseKeyframeAnimation<int?, int?>>(masks.Count);
            for (var i = 0; i < masks.Count; i++)
            {
                _maskAnimations.Add(masks[i].MaskPath.CreateAnimation());
                var opacity = masks[i].Opacity;
                _opacityAnimations.Add(opacity.CreateAnimation());
            }
        }

        internal List<Mask> Masks { get; }

        internal List<IBaseKeyframeAnimation<ShapeData, Path>> MaskAnimations => _maskAnimations;

        internal List<IBaseKeyframeAnimation<int?, int?>> OpacityAnimations => _opacityAnimations;
    }
}