using System.Collections.Generic;

namespace LottieUWP
{
    internal class ShapeKeyframeAnimation : BaseKeyframeAnimation<ShapeData, Path>
    {
        private readonly ShapeData _tempShapeData = new ShapeData();
        private readonly Path _tempPath = new Path();

        internal ShapeKeyframeAnimation(IList<IKeyframe<ShapeData>> keyframes) : base(keyframes)
        {
        }

        public override Path GetValue(IKeyframe<ShapeData> keyframe, float keyframeProgress)
        {
            ShapeData startShapeData = keyframe.StartValue;
            ShapeData endShapeData = keyframe.EndValue;

            _tempShapeData.InterpolateBetween(startShapeData, endShapeData, keyframeProgress);
            MiscUtils.GetPathFromData(_tempShapeData, _tempPath);
            return _tempPath;
        }
    }
}