using Windows.Data.Json;
using LottieUWP.Animation.Keyframe;

namespace LottieUWP.Model.Animatable
{
    internal interface IAnimatableValue<out TK, TA>
    {
        IBaseKeyframeAnimation<TK, TA> CreateAnimation();
    }

    internal interface IAnimatableValueFactory<out TV>
    {
        TV ValueFromObject(IJsonValue @object, float scale);
    }
}