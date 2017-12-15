namespace LottieUWP.Model
{
    /// <summary>
    /// Property values are the same type as the generic type of their corresponding
    /// <see cref="Value.ILottieValueCallback{T}"/>. With this, we can use generics to maintain type safety
    /// of the callbacks.
    /// 
    /// Supported properties:
    /// Transform:
    ///    <see cref="TransformAnchorPoint"/>
    ///    <see cref="TransformPosition"/>
    ///    <see cref="TransformOpacity"/>
    ///    <see cref="TransformScale"/>
    ///    <see cref="TransformRotation"/>
    ///
    /// Fill:
    ///    <see cref="Color"/> (non-gradient)
    ///    <see cref="Opacity"/>
    ///    <see cref="LottieUWP.ColorFilter"/>
    ///
    /// Stroke:
    ///    <see cref="Color"/> (non-gradient)
    ///    <see cref="StrokeWidth"/>
    ///    <see cref="Opacity"/>
    ///    <see cref="LottieUWP.ColorFilter"/>
    ///
    /// Ellipse:
    ///    <see cref="Position"/>
    ///    <see cref="EllipseSize"/>
    ///
    /// Polystar:
    ///    <see cref="PolystarPoints"/>
    ///    <see cref="PolystarRotation"/>
    ///    <see cref="Position"/>
    ///    <see cref="PolystarInnerRadius"/> (star)
    ///    <see cref="PolystarOuterRadius"/>
    ///    <see cref="PolystarInnerRoundedness"/> (star)
    ///    <see cref="PolystarOuterRoundedness"/>
    ///
    /// Repeater:
    ///    All transform properties
    ///    <see cref="RepeaterCopies"/>
    ///    <see cref="RepeaterOffset"/>
    ///    <see cref="TransformRotation"/>
    ///    <see cref="TransformStartOpacity"/>
    ///    <see cref="TransformEndOpacity"/>
    ///
    /// Layers:
    ///    All transform properties
    ///    <see cref="TimeRemap"/> (composition layers only)
    /// </summary>
    public enum LottieProperty
    {
        /// ColorInt
        Color,
        StrokeColor,
        /// Opacity value are 0-100 to match after effects
        TransformOpacity,
        /// [0,100]
        Opacity,

        /// In Px
        TransformAnchorPoint,
        /// In Px
        TransformPosition,
        /// In Px
        EllipseSize,
        /// In Px
        Position,

        TransformScale,

        /// In degrees
        TransformRotation,
        /// In Px
        StrokeWidth,
        TextTracking,
        RepeaterCopies,
        RepeaterOffset,
        PolystarPoints,
        /// In degrees
        PolystarRotation,
        /// In Px
        PolystarInnerRadius,
        /// In Px
        PolystarOuterRadius,
        /// [0,100]
        PolystarInnerRoundedness,
        /// [0,100]
        PolystarOuterRoundedness,
        /// [0,100]
        TransformStartOpacity,
        /// [0,100]
        TransformEndOpacity,
        /// The time value in seconds
        TimeRemap,

        ColorFilter
    }
}