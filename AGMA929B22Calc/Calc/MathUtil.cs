namespace AGMA929B22Calc.Calc;

/// <summary>
/// Trigonometric helpers in degrees, the involute function, and the secant-style
/// iteration described repeatedly in AGMA 929-B22 ("first trial ... second trial,
/// change by 0.001 ... third and subsequent trials interpolate").
/// </summary>
public static class MathUtil
{
    public static double ToRad(double deg) => deg * Math.PI / 180.0;
    public static double ToDeg(double rad) => rad * 180.0 / Math.PI;

    public static double Sin(double deg) => Math.Sin(ToRad(deg));
    public static double Cos(double deg) => Math.Cos(ToRad(deg));
    public static double Tan(double deg) => Math.Tan(ToRad(deg));
    public static double Sec(double deg) => 1.0 / Math.Cos(ToRad(deg));

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
    /// Root finder matching the standard's own iteration recipe: try x0, then x0+step,
    /// then linearly interpolate (secant method) until |f(x)| &lt;= tol.
    /// Used for Eq (41)-(45) [face hobbing mu_x2], Eq (189)-(192) [chamfer mu_c1] and
    /// Eq (203)-(213) [chamfer h_aci1].
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
}
