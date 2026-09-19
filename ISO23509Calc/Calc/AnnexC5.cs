using ISO23509Calc.Models;
using static ISO23509Calc.Calc.MathUtil;

namespace ISO23509Calc.Calc;

/// <summary>
/// Optional convenience calculator implementing ISO 23509:2016, Annex C.5 ("Addendum angle and
/// dedendum angle of wheel", Tables C.4/C.5): derives the wheel addendum/dedendum angles
/// theta_a2/theta_f2 from the chosen depthwise taper type, for designers who have not already
/// fixed them by other means (e.g. matching an existing cutter/blank). Every Clause 7 formula
/// that needs theta_a2/theta_f2 (from Formula (140) onward) takes them as plain Table 3 inputs;
/// this annex is simply one way to obtain reasonable values for them, so it must run after 7.2
/// (needs alpha_n, m_et2, beta_m2) and 7.3 (needs hfm1, hfm2, ham2, hmw) but before 7.4.
/// </summary>
public static class AnnexC5
{
    /// <param name="Re2">Outer pitch cone distance, wheel (7.2, Formula 122) - Formula (C.5) uses
    /// this, not Rm2, in the constant-slot-width denominator.</param>
    public static (double theta_a2, double theta_f2) DeriveThetaA2ThetaF2(
        DepthwiseTaper taper, double hfm1, double hfm2, double ham2, double hmw,
        double Rm2, double Re2, double met2, double alpha_n, double beta_m2, double rc0, CalcTrace T)
    {
        T.Section("Annex C.5: Addendum angle and dedendum angle of wheel (auto, from taper type)");

        double sumThetaFS = T.R("SumThetaF_S", "Eq(C.3)", "Sum of dedendum angles, standard taper", Atan(hfm1 / Rm2) + Atan(hfm2 / Rm2), "deg");
        double sumThetaFC = T.R("SumThetaF_C", "Eq(C.5)", "Sum of dedendum angles, constant slot width",
            (90.0 * met2 / (Re2 * Tan(alpha_n) * Cos(beta_m2))) * (1.0 - Rm2 * Sin(beta_m2) / rc0), "deg");
        double sumThetaFM = T.R("SumThetaF_M", "Eq(C.6)", "Sum of dedendum angles, modified slot width (smaller of C and 1.3*S)",
            Math.Min(sumThetaFC, 1.3 * sumThetaFS), "deg");

        double thetaA2, thetaF2;
        switch (taper)
        {
            case DepthwiseTaper.StandardDepth:
                thetaA2 = T.R("theta_a2", "Eq(C.6)", "Addendum angle, wheel (standard taper)", Atan(hfm1 / Rm2), "deg");
                thetaF2 = T.R("theta_f2", "Eq(C.7)", "Dedendum angle, wheel (standard taper)", sumThetaFS - thetaA2, "deg");
                break;
            case DepthwiseTaper.UniformDepth:
                thetaA2 = T.R("theta_a2", "", "Addendum angle, wheel (uniform depth)", 0.0, "deg");
                thetaF2 = T.R("theta_f2", "", "Dedendum angle, wheel (uniform depth)", 0.0, "deg");
                break;
            case DepthwiseTaper.ConstantSlotWidth:
                thetaA2 = T.R("theta_a2", "Eq(C.8)", "Addendum angle, wheel (constant slot width)", sumThetaFC * (ham2 / hmw), "deg");
                thetaF2 = T.R("theta_f2", "Eq(C.9)", "Dedendum angle, wheel (constant slot width)", sumThetaFC - thetaA2, "deg");
                break;
            case DepthwiseTaper.ModifiedSlotWidth:
            default:
                thetaA2 = T.R("theta_a2", "Eq(C.10)", "Addendum angle, wheel (modified slot width)", sumThetaFM * (ham2 / hmw), "deg");
                thetaF2 = T.R("theta_f2", "Eq(C.11)", "Dedendum angle, wheel (modified slot width)", sumThetaFM - thetaA2, "deg");
                break;
        }
        return (thetaA2, thetaF2);
    }
}
