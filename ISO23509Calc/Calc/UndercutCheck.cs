using ISO23509Calc.Models;
using static ISO23509Calc.Calc.MathUtil;

namespace ISO23509Calc.Calc;

/// <summary>One checkpoint's result from Clause 8: the allowable profile shift coefficient limit
/// at that point, and whether the actual xhm1 avoids undercut there.</summary>
public readonly record struct UndercutPoint(string Label, double R, double Limit, bool Ok);

/// <summary>
/// Implements ISO 23509:2016, Clause 8 ("Undercut check"): 8.1 checks the pinion (by picking a
/// point along the pinion face width and mapping it to the corresponding wheel-side cone
/// distance through the crown-gear/cutter geometry), 8.2 checks the wheel directly (since xhm2 =
/// -xhm1, the check is still expressed as a limit on xhm1). Both are evaluated at the toe (Ri)
/// and heel (Re) of the relevant member, since undercut is most likely at one of the two
/// extremes of the face width.
/// </summary>
public static class UndercutCheck
{
    public static (List<UndercutPoint> pinion, List<UndercutPoint> wheel) Run(Iso23509Inputs In, PitchConeResult pc, GearDimensionsResult gd, CalcTrace T)
    {
        T.Section("Clause 8: Undercut check");
        var pinionPoints = new List<UndercutPoint>
        {
            CheckPinion(In, pc, gd, gd.Ri1, "toe (Rx1 = Ri1)", T),
            CheckPinion(In, pc, gd, gd.Re1, "heel (Rx1 = Re1)", T),
        };
        var wheelPoints = new List<UndercutPoint>
        {
            CheckWheel(In, pc, gd, gd.Ri2, "toe (Rx2 = Ri2)", T),
            CheckWheel(In, pc, gd, gd.Re2, "heel (Rx2 = Re2)", T),
        };
        return (pinionPoints, wheelPoints);
    }

    // ================================================================= 8.1 Pinion (Formulae 227-247)
    private static UndercutPoint CheckPinion(Iso23509Inputs In, PitchConeResult pc, GearDimensionsResult gd, double Rx1, string label, CalcTrace T)
    {
        T.Note($"8.1 Pinion, {label}:");
        double Rx2 = T.R($"Rx2 [{label}]", "Eq(228)", "Wheel cone distance of the corresponding pinion boundary point",
            Math.Sqrt(pc.Rm2 * pc.Rm2 + (pc.Rm1 - Rx1) * (pc.Rm1 - Rx1) - 2.0 * pc.Rm2 * (pc.Rm1 - Rx1) * Cos(gd.zeta_mp)), "mm");

        double betaX2;
        if (In.Process == ManufacturingProcess.FaceHobbing)
        {
            double phiX2 = T.R($"phi_x2 [{label}]", "Eq(229)", "Auxiliary angle", Acos((Rx2 * Rx2 + gd.rho_P0 * gd.rho_P0 - In.rc0 * In.rc0) / (2.0 * Rx2 * gd.rho_P0)), "deg");
            betaX2 = T.R($"beta_x2 [{label}]", "Eq(230)", "Wheel spiral angle at checkpoint", Atan((Rx2 - gd.rho_b * Cos(phiX2)) / (gd.rho_b * Sin(phiX2))), "deg");
        }
        else
        {
            betaX2 = T.R($"beta_x2 [{label}]", "Eq(231)", "Wheel spiral angle at checkpoint",
                Asin((2.0 * pc.Rm2 * In.rc0 * Sin(pc.beta_m2) - pc.Rm2 * pc.Rm2 + Rx2 * Rx2) / (2.0 * Rx2 * In.rc0)), "deg");
        }
        double zetaXp2 = T.R($"zeta_xp2 [{label}]", "Eq(232)", "Pinion offset angle in pitch plane at checkpoint", Asin(gd.ap / Rx2), "deg");
        double betaX1 = T.R($"beta_x1 [{label}]", "Eq(233)", "Pinion spiral angle at checkpoint", betaX2 + zetaXp2, "deg");
        double dx1 = T.R($"dx1 [{label}]", "Eq(234)", "Pinion pitch diameter at checkpoint", 2.0 * Rx1 * Sin(pc.delta1), "mm");
        double dx2 = T.R($"dx2 [{label}]", "Eq(235)", "Wheel pitch diameter at checkpoint", 2.0 * Rx2 * Sin(pc.delta2), "mm");
        double mXn = T.R($"m_xn [{label}]", "Eq(236)", "Normal module at checkpoint", (dx2 / In.z2) * Cos(betaX2), "mm");
        double dEx1 = T.R($"dEx1 [{label}]", "Eq(237)", "Effective diameter at checkpoint, pinion", dx2 * (In.z1 * Cos(betaX2)) / (In.z2 * Cos(betaX1)), "mm");
        double REx1 = T.R($"REx1 [{label}]", "Eq(238)", "Appropriate cone distance", dEx1 / (2.0 * Sin(pc.delta1)), "mm");
        double znx1 = T.R($"znx1 [{label}]", "Eq(239)", "Intermediate value",
            In.z1 / ((1.0 - Sin(betaX1) * Sin(betaX1) * Cos(gd.alpha_n) * Cos(gd.alpha_n)) * Cos(betaX1) * Cos(pc.delta1)), "");
        double alphaLimx = T.R($"alpha_limx [{label}]", "Eq(240)", "Limit pressure angle at checkpoint",
            -Atan(Tan(pc.delta1) * Tan(pc.delta2) / Cos(gd.zeta_mp) *
                ((REx1 * Sin(betaX1) - Rx2 * Sin(betaX2)) / (REx1 * Tan(pc.delta1) + Rx2 * Tan(pc.delta2)))), "deg");
        double alphaEDx = T.R($"alpha_eDx [{label}]", "Eq(241)", "Effective pressure angle at checkpoint, drive side", gd.alpha_nD - alphaLimx, "deg");
        double alphaECx = T.R($"alpha_eCx [{label}]", "Eq(242)", "Effective pressure angle at checkpoint, coast side", gd.alpha_nC + alphaLimx, "deg");
        double alphaEminx = T.R($"alpha_eminx [{label}]", "Eq(243/244)", "Smaller effective pressure angle at checkpoint", Math.Min(alphaECx, alphaEDx), "deg");

        double kHapx = T.R($"khapx [{label}]", "Eq(245)", "Working tool addendum at checkpoint", gd.k_hap + (Rx2 - pc.Rm2) * Tan(gd.theta_a2) / gd.m_mn, "");
        double xHx1 = T.R($"xhx1 [{label}]", "Eq(246)", "Minimum profile shift coefficient at checkpoint", 1.1 * kHapx - znx1 * mXn * Sin(alphaEminx) * Sin(alphaEminx) / (2.0 * gd.m_mn), "");
        double xHmMinX1 = T.R($"xhm,min,x1 [{label}]", "Eq(247)", "Minimum profile shift coefficient at calculation point", xHx1 + (dEx1 - dx1) * Cos(pc.delta1) / (2.0 * gd.m_mn), "");

        bool ok = gd.x_hm1 > xHmMinX1;
        T.Note(ok
            ? $"  xhm1 = {gd.x_hm1:0.000} > xhm,min,x1 = {xHmMinX1:0.000}: pinion undercut avoided at {label}."
            : $"  xhm1 = {gd.x_hm1:0.000} <= xhm,min,x1 = {xHmMinX1:0.000}: PINION UNDERCUT at {label}.");
        return new UndercutPoint(label, Rx1, xHmMinX1, ok);
    }

    // ================================================================= 8.2 Wheel (Formulae 248-258)
    private static UndercutPoint CheckWheel(Iso23509Inputs In, PitchConeResult pc, GearDimensionsResult gd, double Rx2, string label, CalcTrace T)
    {
        T.Note($"8.2 Wheel, {label}:");
        double betaX2;
        if (In.Process == ManufacturingProcess.FaceHobbing)
        {
            double phiX2 = T.R($"phi_x2 [{label}]", "Eq(249)", "Auxiliary angle", Acos((Rx2 * Rx2 + gd.rho_P0 * gd.rho_P0 - In.rc0 * In.rc0) / (2.0 * Rx2 * gd.rho_P0)), "deg");
            betaX2 = T.R($"beta_x2 [{label}]", "Eq(250)", "Wheel spiral angle at checkpoint", Atan((Rx2 - gd.rho_b * Cos(phiX2)) / (gd.rho_b * Sin(phiX2))), "deg");
        }
        else
        {
            betaX2 = T.R($"beta_x2 [{label}]", "Eq(251)", "Wheel spiral angle at checkpoint",
                Asin((2.0 * pc.Rm2 * In.rc0 * Sin(pc.beta_m2) - pc.Rm2 * pc.Rm2 + Rx2 * Rx2) / (2.0 * Rx2 * In.rc0)), "deg");
        }
        double dx2 = T.R($"dx2 [{label}]", "Eq(252)", "Wheel pitch diameter at checkpoint", 2.0 * Rx2 * Sin(pc.delta2), "mm");
        double mXn = T.R($"m_xn [{label}]", "Eq(253)", "Normal module at checkpoint", (dx2 / In.z2) * Cos(betaX2), "mm");
        double znx2 = T.R($"znx2 [{label}]", "Eq(254)", "Intermediate value",
            In.z2 / ((1.0 - Sin(betaX2) * Sin(betaX2) * Cos(gd.alpha_n) * Cos(gd.alpha_n)) * Cos(betaX2) * Cos(pc.delta2)), "");
        double alphaEminx = T.R($"alpha_eminx [{label}]", "Eq(255/256)", "Smaller generated pressure angle", Math.Min(gd.alpha_nC, gd.alpha_nD), "deg");

        double kHapx = T.R($"khapx [{label}]", "Eq(257)", "Working tool addendum at checkpoint", gd.k_hap + (Rx2 - pc.Rm2) * Tan(gd.theta_f2) / gd.m_mn, "");
        double xHmMaxX1 = T.R($"xhm,max,x1 [{label}]", "Eq(258)", "Maximum profile shift coefficient at calculation point",
            -(1.1 * kHapx - znx2 * mXn * Sin(alphaEminx) * Sin(alphaEminx) / (2.0 * gd.m_mn)), "");

        bool ok = gd.x_hm1 > xHmMaxX1;
        T.Note(ok
            ? $"  xhm1 = {gd.x_hm1:0.000} > xhm,max,x1 = {xHmMaxX1:0.000}: wheel undercut avoided at {label}."
            : $"  xhm1 = {gd.x_hm1:0.000} <= xhm,max,x1 = {xHmMaxX1:0.000}: WHEEL UNDERCUT at {label}.");
        return new UndercutPoint(label, Rx2, xHmMaxX1, ok);
    }
}
