using System;

namespace LottieUWP
{
    public struct PathInterpolator : IInterpolator
    {
        private readonly float _controlX1;
        private readonly float _controlY1;
        private readonly float _controlX2;
        private readonly float _controlY2;

        public PathInterpolator(float controlX1, float controlY1, float controlX2, float controlY2)
        {
            _controlX1 = controlX1;
            _controlY1 = controlY1;
            _controlX2 = controlX2;
            _controlY2 = controlY2;
        }

        public float GetInterpolation(float x)
        {
            // Determine t
            double t;
            if (x == 0)
            {
                // Handle corner cases explicitly to prevent rounding errors
                t = 0;
            }
            else if (x == 1)
            {
                t = 1;
            }
            else
            {
                // Calculate t
                double a = 3.0 * _controlX1 - 3.0 * _controlX2 + 1.0;
                double b = -6.0 * _controlX1 + 3.0 * _controlX2;
                double c = 3.0 * _controlX1;
                double d = x;
                double? tTemp = SolveCubic(a, b, c, d);
                if (tTemp == null)
                    return x;
                t = tTemp.Value;
            }

            // Calculate y from t
            return (float)(Cubed(1 - t) * 0
                   + 3 * t * Squared(1 - t) * _controlY1
                   + 3 * Squared(t) * (1 - t) * _controlY2
                   + Cubed(t) * 1);
        }

        private static double? SolveCubic(double a, double b, double c, double d)
        {
            if (a == 0) return SolveQuadratic(b, c, d);
            if (d == 0) return 0;

            b /= a;
            c /= a;
            d /= a;
            double q = (3.0 * c - Squared(b)) / 9.0;
            double r = (-27.0 * d + b * (9.0 * c - 2.0 * Squared(b))) / 54.0;
            double disc = Cubed(q) + Squared(r);
            double term1 = b / 3.0;

            if (disc > 0)
            {
                double s = r + Math.Sqrt(disc);
                s = s < 0 ? -CubicRoot(-s) : CubicRoot(s);
                double t = r - Math.Sqrt(disc);
                t = t < 0 ? -CubicRoot(-t) : CubicRoot(t);

                double result = -term1 + s + t;
                if (result >= 0 && result <= 1) return result;
            }
            else if (disc == 0)
            {
                double r13 = r < 0 ? -CubicRoot(-r) : CubicRoot(r);

                double result = -term1 + 2.0 * r13;
                if (result >= 0 && result <= 1) return result;

                result = -(r13 + term1);
                if (result >= 0 && result <= 1) return result;
            }
            else
            {
                q = -q;
                double dum1 = q * q * q;
                dum1 = Math.Acos(r / Math.Sqrt(dum1));
                double r13 = 2.0 * Math.Sqrt(q);

                double result = -term1 + r13 * Math.Cos(dum1 / 3.0);
                if (result >= 0 && result <= 1) return result;

                result = -term1 + r13 * Math.Cos((dum1 + 2.0 * Math.PI) / 3.0);
                if (result >= 0 && result <= 1) return result;

                result = -term1 + r13 * Math.Cos((dum1 + 4.0 * Math.PI) / 3.0);
                if (result >= 0 && result <= 1) return result;
            }

            return null;
        }

        private static double? SolveQuadratic(double a, double b, double c)
        {
            double result = (-b + Math.Sqrt(Squared(b) - 4 * a * c)) / (2 * a);
            if (result >= 0 && result <= 1) return result;

            result = (-b - Math.Sqrt(Squared(b) - 4 * a * c)) / (2 * a);
            if (result >= 0 && result <= 1) return result;

            return null;
        }

        private static double Squared(double f) { return f * f; }

        private static double Cubed(double f) { return f * f * f; }

        private static double CubicRoot(double f) { return Math.Pow(f, 1.0 / 3.0); }
    }
}