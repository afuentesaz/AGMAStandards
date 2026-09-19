namespace ISO23509Calc.Calc;

/// <summary>Trigonometric helpers in degrees, plus a bracketing bisection root finder used by
/// the Method 1 and Method 2 pitch-cone iterations (ISO 23509:2016, 6.2.2 and 6.2.3), whose
/// convergence rule ("change eta ... until ...", "increase delta2imp if Rm1 &lt; Rmcheck") is a
/// monotonic root-search but does not name a specific numerical procedure.</summary>
public static class MathUtil
{
    public static double ToRad(double deg) => deg * Math.PI / 180.0;
    public static double ToDeg(double rad) => rad * 180.0 / Math.PI;

    public static double Sin(double deg) => Math.Sin(ToRad(deg));
    public static double Cos(double deg) => Math.Cos(ToRad(deg));
    public static double Tan(double deg) => Math.Tan(ToRad(deg));

    public static double Asin(double x) => ToDeg(Math.Asin(Clamp(x)));
    public static double Acos(double x) => ToDeg(Math.Acos(Clamp(x)));
    public static double Atan(double x) => ToDeg(Math.Atan(x));
    public static double Atan2(double y, double x) => ToDeg(Math.Atan2(y, x));

    private static double Clamp(double x) => Math.Max(-1.0, Math.Min(1.0, x));

    /// <summary>
    /// Finds x such that f(x) = 0, given that f is monotonic, by bisection. Starts from a
    /// bracket centred on <paramref name="seed"/> and widens it (doubling the half-width, up to
    /// <paramref name="maxExpand"/> times) until the endpoints have opposite signs, then
    /// bisects down to <paramref name="tol"/>. Used because the standard's own iteration
    /// recipes ("increase the variable if A &lt; B, decrease otherwise") describe a monotonic
    /// search without specifying step sizes.
    /// </summary>
    public static double Bisect(Func<double, double> f, double seed, double initialHalfWidth, double tol = 1e-7, int maxExpand = 40, int maxBisect = 200)
    {
        double lo = seed - initialHalfWidth, hi = seed + initialHalfWidth;
        double flo = f(lo), fhi = f(hi);
        int expand = 0;
        while (Math.Sign(flo) == Math.Sign(fhi) && expand < maxExpand)
        {
            initialHalfWidth *= 1.6;
            lo = seed - initialHalfWidth;
            hi = seed + initialHalfWidth;
            flo = f(lo);
            fhi = f(hi);
            expand++;
        }
        if (Math.Sign(flo) == Math.Sign(fhi))
        {
            // Could not bracket a root; fall back to whichever endpoint is closer to zero.
            return Math.Abs(flo) < Math.Abs(fhi) ? lo : hi;
        }

        for (int i = 0; i < maxBisect && hi - lo > tol; i++)
        {
            double mid = 0.5 * (lo + hi);
            double fmid = f(mid);
            if (fmid == 0.0) return mid;
            if (Math.Sign(fmid) == Math.Sign(flo)) { lo = mid; flo = fmid; }
            else { hi = mid; fhi = fmid; }
        }
        return 0.5 * (lo + hi);
    }
}
