namespace LottieUWP.Utils
{
    /// <summary>
    /// Class to calculate the average in a stream of numbers on a continuous basis.
    /// </summary>
    public class MeanCalculator
    {
        private float _sum;
        private int _n;

        public virtual void Add(float number)
        {
            _sum += number;
            _n++;
            if (_n == int.MaxValue)
            {
                _sum /= 2f;
                _n /= 2;
            }
        }

        public virtual float Mean
        {
            get
            {
                if (_n == 0)
                {
                    return 0;
                }
                return _sum / _n;
            }
        }
    }
}
