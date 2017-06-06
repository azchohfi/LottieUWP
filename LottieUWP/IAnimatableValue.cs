using Windows.Data.Json;

namespace LottieUWP
{
    internal interface IAnimatableValue<out TO>
    {
        IBaseKeyframeAnimation<TO> CreateAnimation();
    }

    internal interface IAnimatableValueFactory<out TV>
    {
        TV ValueFromObject(IJsonValue @object, float scale);
    }
}