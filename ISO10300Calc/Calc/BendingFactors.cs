using ISO10300Calc.Models;
using static ISO10300Calc.Calc.MathUtil;

namespace ISO10300Calc.Calc;

/// <summary>Per-member (pinion or wheel) tooth-form results from ISO 10300-3:2023, 6.4.1.2
/// (generated gears, drive flank only) and 6.4.2 (stress correction factor).</summary>
public record ToothFormResult(double sFn, double rhoF, double hFa, double YFa, double YSa, double qs);

/// <summary>
/// ISO 10300-3:2023, Method B1 (Clause 6) tooth-root-stress factors, restricted to generated
/// bevel gears without hypoid offset, drive flank only (matching ISO/TR 10300-30:2024, Sample 1).
/// </summary>
public static class BendingFactors
{
    /// <summary>Tooth form factor Y_Fa and stress correction factor Y_Sa for one member
    /// (6.4.1.2, Formulae 6-17, and 6.4.2, Formulae 24-26), including the fixed-point solve for
    /// the root-fillet angle vartheta (Formula 10).</summary>
    public static ToothFormResult ToothForm(double mmn, double ha0, double xhm, double xsm, double rhoA0, double spr,
        double alphaE, double alphaN, double zvn, double dvan, double dvbn, CalcTrace T, string who)
    {
        double E = (Math.PI / 4.0 - xsm) * mmn - ha0 * Tan(alphaE) - rhoA0 * (1.0 - Sin(alphaE)) / Cos(alphaE) - spr;
        double G = rhoA0 / mmn - ha0 / mmn + xhm;
        double H = (2.0 / zvn) * (Math.PI / 2.0 - E / mmn) - Math.PI / 3.0;
        T.R($"E ({who})", "Eq(7)", "Tooth-form auxiliary quantity E", E, "mm");
        T.R($"G ({who})", "Eq(8)", "Tooth-form auxiliary quantity G", G, "");
        T.R($"H ({who})", "Eq(9)", "Tooth-form auxiliary quantity H", H, "");

        double thetaDeg = SolveRootFilletAngle(G, H, zvn);
        T.R($"theta ({who})", "Eq(10)", "Root fillet angle (converged)", thetaDeg, "deg");

        const double sixtyDeg = 60.0; // pi/3 rad
        double sFn = mmn * zvn * Sin(sixtyDeg - thetaDeg) + Math.Sqrt(3.0) * mmn * (G / Cos(thetaDeg) - rhoA0 / mmn);
        double rhoF = rhoA0 + (2.0 * G * G * mmn) / (Cos(thetaDeg) * (zvn * Cos(thetaDeg) * Cos(thetaDeg) - 2.0 * G));
        T.R($"sFn ({who})", "Eq(11)", "Tooth root chordal thickness", sFn, "mm");
        T.R($"rhoF ({who})", "Eq(13)", "Fillet radius at contact point of 30deg tangent", rhoF, "mm");

        double alphaAn = Acos(dvbn / dvan);
        // Inv() already returns the involute value on the natural (radian) scale, so the whole
        // bracket here - including the pi/2 term - is accumulated in radians before conversion.
        double gammaARad = (1.0 / zvn) * (Math.PI / 2.0 + 2.0 * (xhm * Tan(alphaE) + xsm)) + Inv(alphaE) - Inv(alphaAn);
        double gammaA = ToDeg(gammaARad);
        double alphaFan = alphaAn - gammaA;
        T.R($"alpha_an ({who})", "Eq(16)", "Normal pressure angle at tooth tip", alphaAn, "deg");
        T.R($"gamma_a ({who})", "Eq(17)", "Auxiliary angle for tooth form/correction factor", gammaA, "deg");
        T.R($"alpha_Fan ({who})", "Eq(15)", "Load application angle at tooth tip", alphaFan, "deg");

        double hFa = (mmn / 2.0) * ((Cos(gammaA) - Sin(gammaA) * Tan(alphaFan)) * (dvan / mmn) - zvn * Cos(sixtyDeg - thetaDeg) - G / Cos(thetaDeg) + rhoA0 / mmn);
        T.R($"hFa ({who})", "Eq(14)", "Bending moment arm", hFa, "mm");

        double YFa = (6.0 * (hFa / mmn) * Cos(alphaFan)) / (Math.Pow(sFn / mmn, 2) * Cos(alphaN));
        T.R($"YFa ({who})", "Eq(6)", "Tooth form factor", YFa, "");

        double La = sFn / hFa;
        double qs = sFn / (2.0 * rhoF);
        double YSa = (1.2 + 0.13 * La) * Math.Pow(qs, 1.0 / (1.21 + 2.3 / La));
        T.R($"La ({who})", "Eq(25)", "Stress-correction auxiliary ratio", La, "");
        T.R($"qs ({who})", "Eq(26)", "Notch parameter", qs, "");
        T.R($"YSa ({who})", "Eq(24)", "Stress correction factor", YSa, "");

        return new ToothFormResult(sFn, rhoF, hFa, YFa, YSa, qs);
    }

    /// <summary>Contact ratio factor Y_epsilon (6.4.3, Formulae 27-29): the eps_vbeta = 0 and
    /// eps_vbeta >= 1 endpoints are given directly by the standard; the 0 &lt; eps_vbeta &lt; 1
    /// case is a linear interpolation between them (consistent with both endpoint formulae and
    /// with ISO 6336-3's equivalent helical-gear factor).</summary>
    public static double ContactRatioFactor(double epsVAlpha, double epsVBeta, CalcTrace T)
    {
        double yEpsAlpha = 0.25 + 0.75 / epsVAlpha;
        double yEps = epsVBeta <= 0.0 ? yEpsAlpha
                    : epsVBeta >= 1.0 ? 0.625
                    : yEpsAlpha - epsVBeta * (yEpsAlpha - 0.625);
        return T.R("Y_eps", "Eq(27-29)", "Contact ratio factor", yEps, "");
    }

    /// <summary>Bevel spiral angle factor Y_BS (6.4.4, Formulae 30-35). Not validated by the
    /// standard for betam = 0 (straight/Zerol bevel); returns 1.0 in that case per 6.4.4's own
    /// guidance.</summary>
    public static double BevelSpiralAngleFactor(GearType gearType, double betav, double betaVb, double lbm, double bv, double hm1, double hm2, CalcTrace T)
    {
        if (gearType != GearType.SpiralBevel || betav <= 1e-9)
            return T.R("YBS", "6.4.4", "Bevel spiral angle factor (straight/Zerol bevel: not validated, use 1.0)", 1.0, "");

        double ba = bv / Cos(betav);
        double lbb = lbm * Cos(betaVb) / Cos(betav);
        double x = 2.0 * ba / (hm1 + hm2);
        double aBS = -0.0182 * x * x + 0.4736 * x - 0.32;
        double bBS = -0.0032 * x * x + 0.0526 * x + 0.712;
        double cBS = -0.0050 * x * x + 0.0850 * x + 0.54;
        T.R("ba", "Eq(34)", "Developed length of one tooth (facewidth of calculation model)", ba, "mm");
        T.R("lbb", "Eq(35)", "Part of model facewidth covered by the contact line", lbb, "mm");
        double ybs = (aBS / cBS) * Math.Pow(lbb / ba - 1.05 * bBS, 2) + 1.0;
        return T.R("YBS", "Eq(30)", "Bevel spiral angle factor", ybs, "");
    }

    /// <summary>Relative surface condition factor Y_R,relT (6.5.1, Formulae 37-42), range
    /// 1 um &lt;= Rz &lt;= 40 um (the sub-1um branch is not implemented - root roughness this low
    /// is unusual for bevel gears and the standard gives a flat constant there instead).</summary>
    public static double SurfaceConditionFactor(MaterialFamily family, double Rz, CalcTrace T)
    {
        double y = family switch
        {
            MaterialFamily.CaseOrThroughHardenedSteel => 1.674 - 0.529 * Math.Pow(Rz + 1.0, 1.0 / 10.0),
            MaterialFamily.NonHardenedStructuralSteel => 5.306 - 4.203 * Math.Pow(Rz + 1.0, 1.0 / 100.0),
            _ => 4.299 - 3.259 * Math.Pow(Rz + 1.0, 1.0 / 200.0),
        };
        return T.R("YR,relT", "Eq(40-42)", "Relative surface condition factor", y, "");
    }

    /// <summary>Relative notch sensitivity factor Y_delta,relT for reference stress (6.5.2.2,
    /// Formulae 43-45), using a single representative slip-layer thickness rho' per material
    /// family (ISO 10300-3, Table 4 - not modelled per individual strength grade).</summary>
    public static double NotchSensitivityFactor(MaterialFamily family, double qs, CalcTrace T, string who)
    {
        double rhoPrime = family switch
        {
            MaterialFamily.CaseOrThroughHardenedSteel => 0.0030,
            MaterialFamily.NonHardenedStructuralSteel => 0.0833,
            _ => 0.1005,
        };
        double chi = 0.2 * (1.0 + 2.0 * qs);
        const double chiT = 1.2;
        double y = (1.0 + Math.Sqrt(rhoPrime * chi)) / (1.0 + Math.Sqrt(rhoPrime * chiT));
        return T.R($"Ydelta,relT ({who})", "Eq(43)", "Relative notch sensitivity factor", y, "");
    }

    /// <summary>Size factor Y_X (8.1, Formulae 208-210).</summary>
    public static double SizeFactor(MaterialFamily family, double mmn, CalcTrace T)
    {
        double y = family switch
        {
            MaterialFamily.CaseOrThroughHardenedSteel => Math.Clamp(1.05 - 0.01 * mmn, 0.80, 1.0),
            MaterialFamily.NonHardenedStructuralSteel => Math.Clamp(1.03 - 0.006 * mmn, 0.85, 1.0),
            _ => Math.Clamp(1.075 - 0.015 * mmn, 0.70, 1.0),
        };
        return T.R("YX", "Eq(208-210)", "Size factor", y, "");
    }

    /// <summary>Life factor Y_NT (8.2, Table 5). St/V/GGG(perl,bai)/GTS and Eh/IF share the upper
    /// curve family in this simplified 2-curve model; NT/NV(nitr)/GG/GGG(ferr) the lower one.</summary>
    public static double LifeFactor(MaterialFamily family, double NL, CalcTrace T, string who)
    {
        (double NL, double YNT)[] points = family == MaterialFamily.CaseOrThroughHardenedSteel
            ? new[] { (1e4, 2.5), (3e6, 1.0), (1e10, 0.85) }
            : new[] { (1e3, 1.6), (3e6, 1.0), (1e10, 0.85) };
        double y = PittingFactors.LogLogInterpolate(points, NL);
        return T.R($"YNT ({who})", "T:5", "Life factor (bending)", y, "");
    }
}
