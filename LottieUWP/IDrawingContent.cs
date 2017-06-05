using Windows.Foundation;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    internal interface IDrawingContent : IContent
    {
        void Draw(BitmapCanvas canvas, DenseMatrix parentMatrix, int alpha);
        void GetBounds(out Rect outBounds, DenseMatrix parentMatrix);

        /// <summary>
        /// Add a color filter to specific content on a specific layer.
        /// </summary>
        /// <param name="layerName"> name of the layer where the supplied content name lives, null if color
        ///                  filter is to be applied to all layers </param>
        /// <param name="contentName"> name of the specific content that the color filter is to be applied, null
        ///                   is color filter is to be applied to all content that matches the layer param </param>
        /// <param name="colorFilter"> the color filter, null to clear the color filter </param>
        void AddColorFilter(string layerName, string contentName, ColorFilter colorFilter);
    }
}