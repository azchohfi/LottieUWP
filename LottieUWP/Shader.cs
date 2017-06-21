using Windows.UI.Xaml.Media;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    public abstract class Shader
    {
        public DenseMatrix LocalMatrix { get; set; }

        protected MatrixTransform GetCurrentRenderTransform()
        {
            return new MatrixTransform
            {
                Matrix = new Windows.UI.Xaml.Media.Matrix(LocalMatrix[0, 0], LocalMatrix[1, 0], LocalMatrix[0, 1], LocalMatrix[1, 1], LocalMatrix[0, 2], LocalMatrix[1, 2])
            };
        }
    }
}