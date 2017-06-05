using System.Collections.Generic;

namespace LottieUWP
{
    internal class MaskKeyframeAnimation
    {
        private readonly IList<IBaseKeyframeAnimation<Path>> _maskAnimations;

        internal MaskKeyframeAnimation(IList<Mask> masks)
        {
            Masks = masks;
            _maskAnimations = new List<IBaseKeyframeAnimation<Path>>(masks.Count);
            for (int i = 0; i < masks.Count; i++)
            {
                _maskAnimations.Add(masks[i].MaskPath.CreateAnimation());
            }
        }

        internal virtual IList<Mask> Masks { get; }

        internal virtual IList<IBaseKeyframeAnimation<Path>> MaskAnimations => _maskAnimations;
    }
}