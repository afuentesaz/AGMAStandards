namespace AGMA2003D19Calc.Calc;

/// <summary>
/// Trigonometric helpers in degrees, the involute function, and the iterative solvers
/// described repeatedly in ANSI/AGMA 2003-D19 Annex C(M) ("for the second trial ... change
/// by 0.005 m_et ... for the third and subsequent trials, interpolate").
/// </summary>
public static class MathUtil
{
    public static double ToRad(double deg) => deg * Math.PI / 180.0;
    public static double ToDeg(double rad) => rad * 180.0 / Math.PI;

    public static double Sin(double deg) => Math.Sin(ToRad(deg));
    public static double Cos(double deg) => Math.Cos(ToRad(deg));
    public static double Tan(double deg) => Math.Tan(ToRad(deg));
    public static double Sec(double deg) => 1.0 / Math.Cos(ToRad(deg));
    public static double Cot(double deg) => 1.0 / Math.Tan(ToRad(deg));

    public static double Asin(double x) => ToDeg(Math.Asin(Clamp(x)));
    public static double Acos(double x) => ToDeg(Math.Acos(Clamp(x)));
    public static double Atan(double x) => ToDeg(Math.Atan(x));
    public static double Atan2(double y, double x) => ToDeg(Math.Atan2(y, x));

    private static double Clamp(double x) => Math.Max(-1.0, Math.Min(1.0, x));

    /// <summary>Involute function of an angle given in degrees: inv(a) = tan(a) - a(rad).</summary>
    public static double Inv(double angleDeg)
    {
        double rad = ToRad(angleDeg);
        return Math.Tan(rad) - rad;
    }

    /// <summary>
    /// Root finder matching the standard's own iteration recipe (Annex C(M), Eq C.80M/C.93M
    /// and analogues): try x0, then x0+step, then linearly interpolate (secant method) until
    /// |f(x)| &lt;= tol.
    /// </summary>
    public static double SecantSolve(Func<double, double> f, double x0, double step, double tol = 0.001, int maxIter = 200)
    {
        double x1 = x0;
        double f1 = f(x1);
        if (Math.Abs(f1) <= tol) return x1;

        double x2 = x1 + step;
        double f2 = f(x2);

        for (int i = 0; i < maxIter; i++)
        {
            if (Math.Abs(f2) <= tol) return x2;

            double denom = f2 - f1;
            double x3 = Math.Abs(denom) < 1e-14 ? x2 + step : x2 - f2 * (x2 - x1) / denom;

            x1 = x2; f1 = f2;
            x2 = x3; f2 = f(x2);
        }
        return x2;
    }

    /// <summary>
    /// Minimizes f(y) using the exact step-halving alternating-sign search described in Annex
    /// C(M).1.3.3 for locating the critical point y_I (and analogously y_J): start at y0, step
    /// by +/-step0 while f keeps decreasing, reverse sign and halve the step once f increases,
    /// and stop once the step magnitude has been halved down to stepFloor and the last two
    /// values of f agree to within tol.
    /// </summary>
    public static double MinimizeStepHalving(Func<double, double> f, double y0, double step0, double stepFloor, double tol = 0.001)
    {
        double y = y0;
        double fy = f(y);
        double step = step0;

        while (true)
        {
            double yNext = y + step;
            double fNext = f(yNext);

            if (fNext < fy)
            {
                // Still improving in this direction — keep going the same way.
                y = yNext;
                fy = fNext;
                continue;
            }

            // f increased (or is no better): reverse direction and halve the step.
            step = -step / 2.0;

            if (Math.Abs(step) < stepFloor)
            {
                double yFinal = y + step;
                double fFinal = f(yFinal);
                if (Math.Abs(fFinal - fy) <= tol) return fFinal <= fy ? yFinal : y;
                y = fFinal <= fy ? yFinal : y;
                return y;
            }
        }
    }
}
