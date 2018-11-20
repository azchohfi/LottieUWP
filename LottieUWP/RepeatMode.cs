namespace LottieUWP
{
    public enum RepeatMode
    {
        /// <summary>
        /// When the animation reaches the end and <see cref="LottieDrawable.RepeatCount"/> is <see cref="Utils.LottieValueAnimator.Infinite"/>
        /// or a positive value, the animation restarts from the beginning. 
        /// </summary>
        Restart = 1,
        /// <summary>
        /// When the animation reaches the end and <see cref="LottieDrawable.RepeatCount"/> is <see cref="Utils.LottieValueAnimator.Infinite"/>
        /// or a positive value, the animation reverses direction on every iteration. 
        /// </summary>
        Reverse = 2
    }
}
