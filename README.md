# LottieUWP
Lottie is a mobile library for Android and iOS that parses [Adobe After Effects](http://www.adobe.com/products/aftereffects.html) animations exported as json with [Bodymovin](https://github.com/bodymovin/bodymovin) and renders them natively on mobile!

**This library is a port from the Java code to support the Universal Windows Platform (UWP).**

## Download

UWP: [![NuGet Badge](https://chohfi.visualstudio.com/_apis/public/build/definitions/be40ef7f-40f7-491c-8bca-b3c52c625319/14/badge)](https://www.nuget.org/packages/LottieUWP/)


For the first time, designers can create **and ship** beautiful animations without an engineer painstakingly recreating it by hand. They say a picture is worth 1,000 words so here are 13,000:

![Example1](https://raw.githubusercontent.com/airbnb/lottie-android/master/gifs/Example1.gif)


![Example2](https://raw.githubusercontent.com/airbnb/lottie-android/master/gifs/Example2.gif)


![Example3](https://raw.githubusercontent.com/airbnb/lottie-android/master/gifs/Example3.gif)


![Community](https://raw.githubusercontent.com/airbnb/lottie-android/master/gifs/Community%202_3.gif)


![Example4](https://raw.githubusercontent.com/airbnb/lottie-android/master/gifs/Example4.gif)

All of these animations were created in After Effects, exported with Bodymovin, and rendered natively with no additional engineering effort.

[Bodymovin](https://github.com/bodymovin/bodymovin) is an After Effects plugin created by Hernan Torrisi that exports After effects files as json and includes a javascript web player. We've built on top of his great work to extend its usage to Android, iOS, and React Native.

Read more about it on our [blog post](http://airbnb.design/introducing-lottie/)
Or get in touch on Twitter ([gpeal8](https://twitter.com/gpeal8)) or via lottie@airbnb.com

## Sample App

The sample app includes some built in animations.

## Using Lottie for UWP
LottieUWP supports Windows Build 10586+ (November Update) and above.
The simplest way to use it is with LottieAnimationView:

```xaml
<Page 
    ...
    xmlns:lottieUwp="using:LottieUWP"
    ...
    />
    <lottieUwp:LottieAnimationView 
        FileName="Assets/Gears.json"
        Loop="True"
        AutoPlay="True"
        VerticalAlignment="Center"
        HorizontalAlignment="Center"/>
</Page>
```

Or you can load it programatically in multiple ways.
From a json asset in app/src/main/assets:
```c#
await animationView.SetAnimationAsync("Assets/hello-world.json");
animationView.Loop = true;
```
This method will load the file and parse the animation in the background and asynchronously start rendering once completed.

If you want to reuse an animation such as in each item of a list or load it from a network request JsonObject:
```c#
LottieAnimationView.Composition = await LottieComposition.Factory.FromJsonAsync(jsonObject);
LottieAnimationView.PlayAnimation();
```

You can then control the animation or add event handlers:
```c#
animationView.AnimatorUpdate += (object sender, ValueAnimator.ValueAnimatorUpdateEventArgs e) => { ... };
animationView.PlayAnimation();
...
if (animationView.IsAnimating) 
{
    // Do something.
}
...
animationView.Progress = 0.5f;
...
// Custom animation speed or duration.
ValueAnimator animator = ValueAnimator.OfFloat(0f, 1f).SetDuration(500);
animator.Update += (sender, e) => animationView.Progress = (float)e.Animation.AnimatedValue;
animator.Start();
...
animationView.CancelAnimation();
```


Under the hood, `LottieAnimationView` uses `LottieDrawable` to render its animations. If you need to, you can use the the drawable form directly:
```c#
LottieDrawable drawable = new LottieDrawable();
var composition = await LottieComposition.Factory.FromAssetFileNameAsync("Assets/hello-world.json");
drawable.SetComposition(composition);
```

If your animation will be frequently reused, `LottieAnimationView` has an optional caching strategy built in. Use `LottieAnimationView.SetAnimationAsync(string, CacheStrategy)`. `CacheStrategy` can be `Strong`, `Weak`, or `None` to have `LottieAnimationView` hold a strong or weak reference to the loaded and parsed animation. 

You can also use the awaitable version of LottieComposition's asynchronous methods:
```c#
var composition = await LottieComposition.Factory.FromAssetFileNameAsync(assetName);
..
var composition = await LottieComposition.Factory.FromJsonAsync(jsonObject);
...
var composition = await LottieComposition.Factory.FromInputStreamAsync(stream);
```

### Image Support
You can animate images if your animation is loaded from assets and your image file is in a 
subdirectory of assets. Just set `ImageAssetsFolder` on `LottieAnimationView` or 
`LottieDrawable` with the relative folder inside of assets and make sure that the images that 
bodymovin export are in that folder with their names unchanged (should be img_#).
If you use `LottieDrawable` directly, you must call `RecycleBitmaps` when you are done with it.

If you need to provide your own bitmaps if you downloaded them from the network or something, you
 can provide a delegate to do that:
 ```c#
animationView.ImageAssetDelegate = new ImageAssetDelegate();
...
class ImageAssetDelegate : IImageAssetDelegate
{
    public BitmapSource FetchBitmap(LottieImageAsset asset)
    {
        return GetBitmap(asset);
    }
}
```

## Supported After Effects Features

### Keyframe Interpolation

---

* Linear Interpolation

* Bezier Interpolation

* Hold Interpolation

* Rove Across Time

* Spatial Bezier

### Solids

---

* Transform Anchor Point

* Transform Position

* Transform Scale

* Transform Rotation

* Transform Opacity

### Masks

---

* Path

* Opacity

* Multiple Masks (additive)

### Track Mattes

---

* Alpha Matte

### Parenting

---

* Multiple Parenting

* Nulls

### Shape Layers

---

* Rectangle (All properties)

* Ellipse (All properties)

* Polystar (All properties)

* Polygon (All properties. Integer point values only.)

* Path (All properties)

* Anchor Point

* Position

* Scale

* Rotation

* Opacity

* Group Transforms (Anchor point, position, scale etc)

* Multiple paths in one group

#### Stroke (shape layer)

---

* Stroke Color

* Stroke Opacity

* Stroke Width

* Line Cap

* Dashes

#### Fill (shape layer)

---

* Fill Color

* Fill Opacity

#### Trim Paths (shape layer)

---

* Trim Paths Start

* Trim Paths End

* Trim Paths Offset

## Performance and Memory
1. If the composition has no masks or mattes then the performance and memory overhead should be quite good. No bitmaps are created and most operations are simple canvas draw operations.
2. If the composition has mattes, 2-3 bitmaps will be created at the composition size. The bitmaps are created automatically by lottie when the animation view is added to the window and recycled when it is removed from the window. For this reason, it is not recommended to use animations with masks or mattes in a RecyclerView because it will cause significant bitmap churn. In addition to memory churn, additional bitmap.eraseColor() and canvas.drawBitmap() calls are necessary for masks and mattes which will slow down the performance of the animation. For small animations, the performance hit should not be large enough to be obvious when actually used.
4. If you are using your animation in a list, it is recommended to use a CacheStrategy in LottieAnimationView.setAnimation(String, CacheStrategy) so the animations do not have to be deserialized every time.

## Try it out
Clone this repository and run the LottieUWP.Sample module to see a bunch of sample animations. The JSON files for them are located in [LottieUWP.Sample/Assets](https://github.com/azchohfi/LottieUWP/tree/master/LottieUWP.Sample/assets) and the orignal After Effects files are located in [/After Effects Samples](https://github.com/airbnb/lottie-android/tree/master/After%20Effects%20Samples)

## Community Contributions
 * [Xamarin bindings](https://github.com/martijn00/LottieXamarin)
 * [NativeScript bindings](https://github.com/bradmartin/nativescript-lottie)
 * [Appcelerator Titanium bindings](https://github.com/m1ga/ti.animation)
 
## Community Contributors
 * [modplug](https://github.com/modplug)
 * [fabionuno](https://github.com/fabionuno)
 * [matthewrdev](https://github.com/matthewrdev)
 * [alexsorokoletov](https://github.com/alexsorokoletov)
 * [jzeferino](https://github.com/jzeferino)
 
## Alternatives
1. Build animations by hand. Building animations by hand is a huge time commitment for design and engineering across Android and iOS. It's often hard or even impossible to justify spending so much time to get an animation right.
2. [Facebook Keyframes](https://github.com/facebookincubator/Keyframes). Keyframes is a wonderful new library from Facebook that they built for reactions. However, Keyframes doesn't support some of Lottie's features such as masks, mattes, trim paths, dash patterns, and more.
2. Gifs. Gifs are more than double the size of a bodymovin JSON and are rendered at a fixed size that can't be scaled up to match large and high density screens.
3. Png sequences. Png sequences are even worse than gifs in that their file sizes are often 30-50x the size of the bodymovin json and also can't be scaled up.

## Why is it called Lottie?
Lottie is named after a German film director and the foremost pioneer of silhouette animation. Her best known films are The Adventures of Prince Achmed (1926) – the oldest surviving feature-length animated film, preceding Walt Disney's feature-length Snow White and the Seven Dwarfs (1937) by over ten years
[The art of Lotte Reineger](https://www.youtube.com/watch?v=LvU55CUw5Ck&feature=youtu.be)

## Contributing
Contributors are more than welcome. Just upload a PR with a description of your changes.

## Classes to improve
* Animator.cs
* BitmapCanvas.cs (fully change to DD?)
* ColorFilter.cs
* DashPathEffect.cs
* Gradient.cs
* ImageAssetBitmapManager.cs
* LinearGradient.cs
* LottieAnimationView.cs
* LottieDrawable.cs
* Paint.cs
* Path.cs
* PathEffect.cs
* PathMeasure.cs
* PorterDuff.cs
* PorterDuffXfermode.cs
* RadialGradient.cs
* Shader.cs
* PorterDuffColorFilter.cs
* ValueAnimator.cs

Other classes may also need changes, but these are the ones that are known to have actionable TODOs.

## Issues or feature requests?
File github issues for anything that is unexpectedly broken. If an After Effects file is not working, please attach it to your issue. Debugging without the original file is much more difficult.