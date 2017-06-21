using Windows.Data.Json;

namespace LottieUWP
{
    internal interface IAnimatableValue<out TO>
    {
        IBaseKeyframeAnimation<TO> CreateAnimation();
        bool HasAnimation();
    }

    internal interface IAnimatableValueFactory<out TV>
    {
        TV ValueFromObject(IJsonValue @object, float scale);
    }
}