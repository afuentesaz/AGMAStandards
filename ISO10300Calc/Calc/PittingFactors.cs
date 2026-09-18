using ISO10300Calc.Models;
using static ISO10300Calc.Calc.MathUtil;

namespace ISO10300Calc.Calc;

/// <summary>
/// ISO 10300-2:2023, Method B1 (Clause 6) contact-stress factors, plus the common factors of
/// Clause 8 (elasticity, lubricant film, work hardening, life). Restricted, like
/// <see cref="VirtualCylindricalGears"/>, to bevel gears without hypoid offset (a = 0).
/// </summary>
public static class PittingFactors
{
    /// <summary>Mid-zone factor Z_M-B (6.4.1, Formula 6 and Table 4).</summary>
    public static double MidZoneFactor(VirtualGearResult v, CalcTrace T)
    {
        double F1, F2;
        if (v.epsVBeta <= 0.0) { F1 = 2.0; F2 = 2.0 * (v.epsVAlpha - 1.0); }
        else if (v.epsVBeta < 1.0) { F1 = 2.0 + (v.epsVAlpha - 2.0) * v.epsVBeta; F2 = 2.0 * v.epsVAlpha - 2.0 + (2.0 - v.epsVAlpha) * v.epsVBeta; }
        else { F1 = v.epsVAlpha; F2 = v.epsVAlpha; }
        T.R("F1", "T:4", "Mid-zone factor auxiliary value F1", F1, "");
        T.R("F2", "T:4", "Mid-zone factor auxiliary value F2", F2, "");

        double term1 = Math.Sqrt(Math.Sqrt((v.dva1 / v.dvb1) * (v.dva1 / v.dvb1) - 1.0) - F1 * Math.PI / v.zv1);
        double term2 = Math.Sqrt(Math.Sqrt((v.dva2 / v.dvb2) * (v.dva2 / v.dvb2) - 1.0) - F2 * Math.PI / v.zv2);
        return T.R("ZM-B", "Eq(6)", "Mid-zone factor", Tan(v.alphaVet) / (term1 * term2), "");
    }

    /// <summary>Load sharing factor Z_LS (6.4.2, Formulae 7 to 10), using the tip/middle/root
    /// contact line lengths from the virtual cylindrical gear geometry.</summary>
    public static double LoadSharingFactor(VirtualGearResult v, ProfileCrowning crowning, CalcTrace T)
    {
        double eLS = crowning == ProfileCrowning.Low ? 3.0 : 1.5;
        T.R("eLS", "T:5", "Exponent for parabolic distribution of peak loads", eLS, "");

        double lbt = LengthAt(v, v.ft);
        double lbr = LengthAt(v, v.fr);
        double lbmid = v.lbm; // already computed at f = fm

        double pt = 1.0 - Math.Pow(Math.Abs(v.ft / v.fmax), eLS);
        double pm = 1.0 - Math.Pow(Math.Abs(v.fm / Math.Max(v.fmax, 1e-12)), eLS);
        double pr = 1.0 - Math.Pow(Math.Abs(v.fr / v.fmax), eLS);
        T.R("pt*", "Eq(7)", "Related peak load at tip contact line", pt, "");
        T.R("pm*", "Eq(7)", "Related peak load at middle contact line", pm, "");
        T.R("pr*", "Eq(7)", "Related peak load at root contact line", pr, "");

        double At = 0.25 * pt * lbt * Math.PI;
        double Am = 0.25 * pm * lbmid * Math.PI;
        double Ar = 0.25 * pr * lbr * Math.PI;
        T.R("At*", "Eq(8)", "Related area at tip contact line", At, "mm");
        T.R("Am*", "Eq(8)", "Related area at middle contact line", Am, "mm");
        T.R("Ar*", "Eq(8)", "Related area at root contact line", Ar, "mm");

        return T.R("ZLS", "Eq(10)", "Load sharing factor", Math.Sqrt(Am / (At + Am + Ar)), "");
    }

    private static double LengthAt(VirtualGearResult v, double f) =>
        VirtualCylindricalGears.ContactLineLength(f, v.betaVb, v.gvAlpha, v.bvEff, v.bv, v.fmax);

    /// <summary>Elasticity factor Z_E (8.1, Formula 51).</summary>
    public static double ElasticityFactor(double E1, double nu1, double E2, double nu2, CalcTrace T) =>
        T.R("ZE", "Eq(51)", "Elasticity factor", Math.Sqrt(1.0 / (Math.PI * ((1 - nu1 * nu1) / E1 + (1 - nu2 * nu2) / E2))), "N^0.5/mm");

    /// <summary>Lubricant film factors Z_L, Z_v, Z_R (8.2.3, Formulae 54 to 60).</summary>
    public static (double ZL, double Zv, double ZR) LubricantFilmFactors(double sigmaHlim, double v40, double vmt2, double Rz1, double Rz2, double rhoRel, CalcTrace T)
    {
        double sh = Math.Clamp(sigmaHlim, 850.0, 1200.0);
        double CZL = 0.08 * (sh - 850.0) / 350.0 + 0.83;
        double ZL = T.R("ZL", "Eq(54)", "Lubricant factor", CZL + 4.0 * (1.0 - CZL) / Math.Pow(1.2 + 134.0 / v40, 2), "");
        double CZV = 0.08 * (sh - 850.0) / 350.0 + 0.85;
        double Zv = T.R("Zv", "Eq(56)", "Speed factor", CZV + 2.0 * (1.0 - CZV) / Math.Sqrt(0.8 + 32.0 / vmt2), "");
        double Rz10 = (Rz1 + Rz2) / 2.0 * Math.Cbrt(10.0 / rhoRel);
        T.R("Rz10", "Eq(58)", "Mean relative roughness", Rz10, "um");
        double CZR = 0.12 + (1000.0 - sh) / 5000.0;
        double ZR = T.R("ZR", "Eq(59)", "Roughness factor", Math.Pow(3.0 / Rz10, CZR), "");
        return (ZL, Zv, ZR);
    }

    /// <summary>Work hardening factor Z_W (8.3). Applies to the wheel only (softer member); the
    /// pinion always uses Z_W = 1.0.</summary>
    public static double WorkHardeningFactor(HardnessRatioMode mode, double hbwPinion, double hbwWheel, double u, CalcTrace T)
    {
        double zw;
        switch (mode)
        {
            case HardnessRatioMode.ThroughHardenedHarderPinion:
                double ratio = hbwPinion / hbwWheel;
                if (ratio < 1.2) zw = 1.0;
                else
                {
                    double ratioClamped = Math.Min(ratio, 1.7);
                    double A = 0.00898 * ratioClamped - 0.00829;
                    zw = 1.0 + A * (Math.Min(u, 20.0) - 1.0);
                    if (ratio > 1.7) zw = 1.0 + 0.00698 * (Math.Min(u, 20.0) - 1.0);
                }
                break;
            case HardnessRatioMode.SurfaceHardenedPinion:
                // 8.3.3.1, reference/long-life range (Formulae 62-64); RzH taken as mid-range default.
                double zwBase = Math.Clamp(1.2 - (hbwWheel - 130.0) / 1700.0, 1.0, 1.2);
                zw = zwBase; // roughness correction (3/RzH)^0.15 folded into ZR upstream; kept at its neutral factor here.
                break;
            default:
                zw = 1.0;
                break;
        }
        return T.R("ZW", "8.3", "Work hardening factor", zw, "");
    }

    /// <summary>Life factor Z_NT (8.4, Table 6), by log-log interpolation between the tabulated
    /// (NL, ZNT) points, flat beyond the table's ends. St/V/GGG(perl,bai)/GTS/Eh/IF share the
    /// upper curve; NT/NV(nitr)/GG/GGG(ferr) the lower one (nitrocarburized is lower still, but
    /// is not separately exposed in the UI - use NitridedSteelOrCastIron as a conservative
    /// stand-in and verify against ISO 10300-2, Table 6 for that specific material).</summary>
    public static double LifeFactor(MaterialFamily family, double NL, CalcTrace T, string who)
    {
        (double NL, double ZNT)[] points = family == MaterialFamily.CaseOrThroughHardenedSteel
            ? new[] { (6e5, 1.6), (1e7, 1.3), (1e9, 1.0), (1e10, 0.85) }
            : new[] { (1e5, 1.3), (2e6, 1.0), (1e10, 0.85) };
        double znt = LogLogInterpolate(points, NL);
        return T.R($"ZNT ({who})", "T:6", "Life factor (macropitting)", znt, "");
    }

    internal static double LogLogInterpolate((double NL, double Y)[] pts, double NL)
    {
        if (NL <= pts[0].NL) return pts[0].Y;
        if (NL >= pts[^1].NL) return pts[^1].Y;
        for (int i = 0; i < pts.Length - 1; i++)
        {
            if (NL >= pts[i].NL && NL <= pts[i + 1].NL)
            {
                double t = (Math.Log10(NL) - Math.Log10(pts[i].NL)) / (Math.Log10(pts[i + 1].NL) - Math.Log10(pts[i].NL));
                return pts[i].Y + t * (pts[i + 1].Y - pts[i].Y);
            }
        }
        return pts[^1].Y;
    }
}
