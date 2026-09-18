namespace ISO10300Calc.Calc;

/// <summary>
/// Trigonometric helpers in degrees, the involute function, and the fixed-point iteration
/// described in ISO 10300-3 (6.4.1.2.2, Formula 10) for the root-fillet angle vartheta.
/// </summary>
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

    /// <summary>Involute function of an angle given in degrees: inv(a) = tan(a) - a(rad).</summary>
    public static double Inv(double angleDeg)
    {
        double rad = ToRad(angleDeg);
        return Math.Tan(rad) - rad;
    }

    /// <summary>
    /// Solves the transcendent root-fillet angle equation of ISO 10300-3, Formula (10):
    /// vartheta = (2G/zvn)*tan(vartheta) - H, by the exact fixed-point recipe the standard
    /// describes: start at vartheta = pi/6 (30 deg) and iterate until successive values agree
    /// to within the given tolerance (the standard suggests 0.000001 rad).
    /// </summary>
    public static double SolveRootFilletAngle(double G, double H, double zvn, double toleranceRad = 1e-6, int maxIter = 200)
    {
        double thetaDeg = 30.0;
        for (int i = 0; i < maxIter; i++)
        {
            double thetaRad = ToRad(thetaDeg);
            double newThetaRad = (2.0 * G / zvn) * Math.Tan(thetaRad) - H;
            double newThetaDeg = ToDeg(newThetaRad);
            if (Math.Abs(ToRad(newThetaDeg) - thetaRad) <= toleranceRad)
                return newThetaDeg;
            thetaDeg = newThetaDeg;
        }
        return thetaDeg;
    }
}
