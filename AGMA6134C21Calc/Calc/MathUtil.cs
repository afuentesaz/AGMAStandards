namespace AGMA6134C21Calc.Calc;

/// <summary>Trigonometric helpers in degrees, used throughout ANSI/AGMA 6134-C21.</summary>
public static class MathUtil
{
    public static double ToRad(double deg) => deg * Math.PI / 180.0;
    public static double ToDeg(double rad) => rad * 180.0 / Math.PI;

    public static double Sin(double deg) => Math.Sin(ToRad(deg));
    public static double Cos(double deg) => Math.Cos(ToRad(deg));
    public static double Tan(double deg) => Math.Tan(ToRad(deg));
    public static double Cot(double deg) => 1.0 / Math.Tan(ToRad(deg));

    /// <summary>Arcsine of x (dimensionless), returned in RADIANS. Eq 12 note 2 requires the
    /// arcsine argument to be treated in radians directly (not converted to degrees).</summary>
    public static double AsinRad(double x) => Math.Asin(Math.Max(-1.0, Math.Min(1.0, x)));
}
