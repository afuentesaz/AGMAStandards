using ISO23509Calc.Models;

namespace ISO23509Calc.Calc;

/// <summary>
/// Top-level orchestrator for ISO 23509:2016: runs Clause 6 (pitch cone parameters, whichever
/// method is selected), then Clause 7 (gear dimensions, common to all methods), then optionally
/// Clause 8 (undercut check).
/// </summary>
public class Iso23509Calculator
{
    private readonly Iso23509Inputs In;
    private readonly CalcTrace T = new();

    public Iso23509Calculator(Iso23509Inputs inputs) => In = inputs;

    public (CalcTrace trace, PitchConeResult pitchCone, GearDimensionsResult gearDims, List<string> warnings) Calculate()
    {
        var warnings = new List<string>();

        bool isHypoid = Math.Abs(In.a) > 1e-9;
        if (!isHypoid && In.Method != PitchConeMethod.Method0)
            warnings.Add("Hypoid offset a = 0 with a hypoid method (1/2/3) selected - this reduces correctly to the bevel-gear case, but Method 0 is simpler and sufficient whenever a = 0.");
        if (isHypoid && In.Method == PitchConeMethod.Method0)
            warnings.Add("Method 0 is defined only for bevel gears without hypoid offset (a = 0); the supplied hypoid offset will be ignored by the pitch cone calculation.");

        var pc = PitchConeParameters.Calculate(In, T, warnings);
        var gd = GearDimensions.Calculate(In, pc, T, warnings);

        // A tiny negative value (a few thousandths of a degree) is unremarkable rounding noise -
        // ISO 23509:2016's own Annex F.4/F.5 worked examples land on theta_f1 = -0.002 deg and
        // 0.000 deg respectively for a geometry deliberately designed with theta_a2 = theta_f2 = 0
        // (uniform depth). Only flag a value negative enough to represent genuine pointing risk.
        const double thetaWarnThreshold = -0.05;
        if (gd.theta_a1 < thetaWarnThreshold || gd.theta_f1 < thetaWarnThreshold)
            warnings.Add("A computed pinion addendum or dedendum angle is meaningfully negative - check the depthwise taper / dedendum angle modification inputs for consistency.");
        if (pc.c_be2 <= 0 || pc.c_be2 >= 1)
            warnings.Add($"Face width factor c_be2 = {pc.c_be2:0.###} is outside the usual (0, 1) range - the calculation point may lie outside the wheel face width.");

        T.Section("SUMMARY");
        T.R("u (gear ratio)", "", "Gear ratio, z2/z1", pc.u, "");
        T.R("dm1", "", "Pinion mean pitch diameter", gd.dm1, "mm");
        T.R("dm2", "", "Wheel mean pitch diameter", gd.dm2, "mm");
        T.R("delta1", "", "Pinion pitch angle", pc.delta1, "deg");
        T.R("delta2", "", "Wheel pitch angle", pc.delta2, "deg");
        T.R("beta_m1", "", "Pinion mean spiral angle", pc.beta_m1, "deg");
        T.R("beta_m2", "", "Wheel mean spiral angle", pc.beta_m2, "deg");
        T.R("b1", "", "Pinion face width", gd.b1, "mm");
        T.R("b2", "", "Wheel face width", In.b2, "mm");
        T.R("m_mn", "", "Mean normal module", gd.m_mn, "mm");
        T.R("hm", "", "Mean whole depth", gd.hm, "mm");

        if (In.CheckUndercut)
        {
            var (pinionPts, wheelPts) = UndercutCheck.Run(In, pc, gd, T);
            foreach (var p in pinionPts.Where(p => !p.Ok))
                warnings.Add($"Undercut check: pinion undercut predicted at {p.Label} (xhm1 = {gd.x_hm1:0.000} <= limit {p.Limit:0.000}).");
            foreach (var p in wheelPts.Where(p => !p.Ok))
                warnings.Add($"Undercut check: wheel undercut predicted at {p.Label} (xhm1 = {gd.x_hm1:0.000} <= limit {p.Limit:0.000}).");
        }

        return (T, pc, gd, warnings);
    }
}
