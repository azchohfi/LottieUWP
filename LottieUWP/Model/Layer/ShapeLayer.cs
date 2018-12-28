using System.Collections.Generic;
using Windows.Foundation;
using LottieUWP.Animation.Content;
using LottieUWP.Model.Content;

namespace LottieUWP.Model.Layer
{
    internal class ShapeLayer : BaseLayer
    {
        private readonly ContentGroup _contentGroup;

        internal ShapeLayer(ILottieDrawable lottieDrawable, Layer layerModel) : base(lottieDrawable, layerModel)
        {
            // Naming this __container allows it to be ignored in KeyPath matching. 
            ShapeGroup shapeGroup = new ShapeGroup("__container", layerModel.Shapes, false);
            _contentGroup = new ContentGroup(lottieDrawable, this, shapeGroup);
            _contentGroup.SetContents(new List<IContent>(), new List<IContent>());
        }

        public override void DrawLayer(BitmapCanvas canvas, Matrix3X3 parentMatrix, byte parentAlpha)
        {
            _contentGroup.Draw(canvas, parentMatrix, parentAlpha);
        }

        public override void GetBounds(out Rect outBounds, Matrix3X3 parentMatrix)
        {
            base.GetBounds(out outBounds, parentMatrix);
            _contentGroup.GetBounds(out outBounds, BoundsMatrix);
        }

        internal override void ResolveChildKeyPath(KeyPath keyPath, int depth, List<KeyPath> accumulator, KeyPath currentPartialKeyPath)
        {
            _contentGroup.ResolveKeyPath(keyPath, depth, accumulator, currentPartialKeyPath);
        }
    }
}