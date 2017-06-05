using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    public class Shader
    {
        public enum TileMode
        {
            Clamp
        }

        public DenseMatrix LocalMatrix { get; set; }
    }
}