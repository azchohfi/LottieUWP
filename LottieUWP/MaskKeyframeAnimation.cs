using System.Collections.Generic;

namespace LottieUWP
{
    internal class MaskKeyframeAnimation
    {
        private readonly List<IBaseKeyframeAnimation<Path>> _maskAnimations;
        private readonly List<IBaseKeyframeAnimation<int?>> _opacityAnimations;

        internal MaskKeyframeAnimation(List<Mask> masks)
        {
            Masks = masks;
            _maskAnimations = new List<IBaseKeyframeAnimation<Path>>(masks.Count);
            _opacityAnimations = new List<IBaseKeyframeAnimation<int?>>(masks.Count);
            for (var i = 0; i < masks.Count; i++)
            {
                _maskAnimations.Add(masks[i].MaskPath.CreateAnimation());
                var opacity = masks[i].Opacity;
                _opacityAnimations.Add(opacity.CreateAnimation());
            }
        }

        internal virtual List<Mask> Masks { get; }

        internal virtual List<IBaseKeyframeAnimation<Path>> MaskAnimations => _maskAnimations;

        internal virtual List<IBaseKeyframeAnimation<int?>> OpacityAnimations => _opacityAnimations;
    }
}