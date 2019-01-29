using System;

namespace LottieUWP
{
    public struct Point
    {
        public Point(double x, double y)
        {
            _x = x;
            _y = y;
        }

        public double X
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
            }
        }
		
        public double Y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
            }
        }
        internal double _x;
        internal double _y;
    }

    public struct Rect
    {
        static public Rect CreateEmptyRect()
        {
            Rect rect = new Rect();
            // We can't set these via the property setters because negatives widths
            // are rejected in those APIs.
            rect._x = 0.0;
            rect._y = 0.0;
            rect._width = 0.0;
            rect._height = 0.0;
            return rect;
        }

        static public Rect InitRectforBoundBox()
        {
            Rect rect = new Rect();
            rect._x = int.MaxValue;
            rect._y = int.MaxValue;
            rect._width = int.MinValue;
            rect._height = int.MinValue;
            return rect;
        }

        internal double _x;
        internal double _y;
        internal double _width;
        internal double _height;
        private readonly static Rect s_empty = CreateEmptyRect();

        public static bool operator ==(Rect rect1, Rect rect2)
        {
            return rect1.X == rect2.X &&
                   rect1.Y == rect2.Y &&
                   rect1.Width == rect2.Width &&
                   rect1.Height == rect2.Height;
        }
		
        public static bool operator !=(Rect rect1, Rect rect2)
        {
            return !(rect1 == rect2);
        }

        public Rect(Point point1,
                    Point point2)
        {
            _x = Math.Min(point1._x, point2._x);
            _y = Math.Min(point1._y, point2._y);

            //  Max with 0 to prevent double weirdness from causing us to be (-epsilon..0)
            _width = Math.Max(Math.Max(point1._x, point2._x) - _x, 0);
            _height = Math.Max(Math.Max(point1._y, point2._y) - _y, 0);
        }

        public Rect(double x, double y, double width, double height)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        public static Rect Empty
        {
            get
            {
                return s_empty;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (_width < 0 || _height < 0);
            }
        }

        public bool IntersectsWith(Rect rect)
        {
            if (IsEmpty || rect.IsEmpty)
            {
                return false;
            }

            return (rect.Left <= Right) &&
                   (rect.Right >= Left) &&
                   (rect.Top <= Bottom) &&
                   (rect.Bottom >= Top);
        }

        public void Intersect(Rect rect)
        {
            if (!this.IntersectsWith(rect))
            {
                this = Empty;
            }
            else
            {
                double left = Math.Max(Left, rect.Left);
                double top = Math.Max(Top, rect.Top);

                //  Max with 0 to prevent double weirdness from causing us to be (-epsilon..0)
                _width = Math.Max(Math.Min(Right, rect.Right) - left, 0);
                _height = Math.Max(Math.Min(Bottom, rect.Bottom) - top, 0);

                _x = left;
                _y = top;
            }
        }

        public void Union(Rect rect)
        {
            if (IsEmpty)
            {
                this = rect;
            }
            else if (!rect.IsEmpty)
            {
                double left = Math.Min(Left, rect.Left);
                double top = Math.Min(Top, rect.Top);

                // We need this check so that the math does not result in NaN
                if ((rect.Width == Double.PositiveInfinity) || (Width == Double.PositiveInfinity))
                {
                    _width = Double.PositiveInfinity;
                }
                else
                {
                    //  Max with 0 to prevent double weirdness from causing us to be (-epsilon..0)                    
                    double maxRight = Math.Max(Right, rect.Right);
                    _width = Math.Max(maxRight - left, 0);
                }

                // We need this check so that the math does not result in NaN
                if ((rect.Height == Double.PositiveInfinity) || (Height == Double.PositiveInfinity))
                {
                    _height = Double.PositiveInfinity;
                }
                else
                {
                    //  Max with 0 to prevent double weirdness from causing us to be (-epsilon..0)
                    double maxBottom = Math.Max(Bottom, rect.Bottom);
                    _height = Math.Max(maxBottom - top, 0);
                }

                _x = left;
                _y = top;
            }
        }

        public double X
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
            }
        }

        public double Y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
            }
        }

        public double Width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
            }
        }

        public double Height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
            }
        }

        public double Left
        {
            get
            {
                return _x;
            }
        }

        public double Top
        {
            get
            {
                return _y;
            }
        }

        public double Bottom
        {
            get
            {
                if (IsEmpty)
                {
                    return Double.NegativeInfinity;
                }

                return _y + _height;
            }
        }

        public double Right
        {
            get
            {
                if (IsEmpty)
                {
                    return Double.NegativeInfinity;
                }

                return _x + _width;
            }
        }
    }
}