namespace LottieUWP.Animation.Content
{
    public abstract class Shader
    {
        public Matrix3X3 LocalMatrix { get; set; } = Matrix3X3.CreateIdentity();
    }
}