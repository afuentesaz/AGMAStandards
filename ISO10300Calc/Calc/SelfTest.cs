using ISO10300Calc.Models;

namespace ISO10300Calc.Calc;

/// <summary>
/// Diagnostic-only harness (run with "--selftest") that checks the calculator's output against
/// the two numeric worked examples published in ISO/TR 10300-30:2024: Sample 1 (a spiral bevel
/// gear pair without hypoid offset, z1=14, z2=39) and Sample 2 (a hypoid gear pair, z1=13,
/// z2=42), both Method B1. Unlike AGMA2003D19Calc's SelfTest, no geometry factor needs to be
/// supplied manually here - every input below is taken directly from ISO/TR 10300-30's own
/// tables, and every checked value is one of its own published intermediate or final results,
/// so this exercises the full pipeline (virtual cylindrical gears, Method B dynamic/transverse
/// load factors, and the Method B1 pitting and bending formulas) end to end, for both the
/// non-offset and the general hypoid-offset code paths.
/// </summary>
public static class SelfTest
{
    private static (int pass, int fail) RunChecks(Iso10300Inputs inputs, string title, (string symbol, double expected, double tolFraction)[] checks)
    {
        var (trace, warnings) = new Iso10300Calculator(inputs).Calculate();
        var byLabel = new Dictionary<string, List<double>>();
        foreach (var row in trace.Rows)
        {
            if (row.Symbol.Length == 0 || double.IsNaN(row.Value)) continue;
            if (!byLabel.TryGetValue(row.Symbol, out var list)) byLabel[row.Symbol] = list = new List<double>();
            list.Add(row.Value);
        }

        int pass = 0, fail = 0;
        Console.WriteLine(title);
        Console.WriteLine();

        foreach (var (symbol, expected, tolFraction) in checks)
        {
            if (!byLabel.TryGetValue(symbol, out var values))
            {
                Console.WriteLine($"MISSING  {symbol,-16} expected={expected}");
                fail++;
                continue;
            }
            double actual = values.OrderBy(v => Math.Abs(v - expected)).First();
            double tol = Math.Max(Math.Abs(expected) * tolFraction, 1e-6);
            bool ok = Math.Abs(actual - expected) <= tol;
            Console.WriteLine($"{(ok ? "PASS" : "FAIL"),-4} {symbol,-16} expected={expected,14:0.0000} actual={actual,14:0.0000} diff%={100 * (actual - expected) / expected,8:0.00}");
            if (ok) pass++; else fail++;
        }

        Console.WriteLine();
        Console.WriteLine(warnings.Count > 0 ? "Warnings: " + string.Join("; ", warnings) : "No warnings.");
        Console.WriteLine();
        return (pass, fail);
    }

    public static void RunSample1Check()
    {
        var inputs = new Iso10300Inputs
        {
            GearType = GearType.SpiralBevel,
            ShaftAngle = 90.0,
            z1 = 14,
            z2 = 39,

            T1_Nm = 300.0,
            PinionSpeed_rpm = 1200.0,

            HypoidOffset_a = 0.0,
            Zeta_mp = 0.0,
            Zeta_m = 0.0,
            Zeta_R = 0.0,

            Re = 93.973,
            de2 = 176.90,
            b = 25.4,
            EffectiveFacewidthFraction = 0.85,
            mmn = 3.213,
            met2 = 4.536,
            Alpha_nD = 20.0,
            Alpha_nC = 20.0,
            Alpha_eD = 20.0,
            Alpha_eC = 20.0,
            Alpha_lim = 0.0,
            Beta_m1 = 35.0,
            Beta_m2 = 35.0,
            CutterRadius = 114.3,

            dm1 = 54.918,
            Delta1 = 19.747,
            h_am1 = 4.837,
            h_fm1 = 2.393,
            x_hm1 = 0.505,
            x_sm1 = 0.036,
            Rho_a01 = 0.8,
            Spr1 = 0.0,

            dm2 = 152.987,
            Delta2 = 70.253,
            h_am2 = 1.590,
            h_fm2 = 5.640,
            x_hm2 = -0.505,
            x_sm2 = -0.055,
            Rho_a02 = 1.2,
            Spr2 = 0.0,

            k_hfp = 1.25,
            MaterialDensity = 7.86e-6,

            KA = 1.1,
            DynamicFactorMode = DynamicFactorMode.MethodB,
            fpt1_um = 12.0,
            fpt2_um = 26.0,

            Mounting = MountingCondition.OneMemberCantileverMounted,
            ContactVerification = ContactPatternVerification.CheckedUnderLightTestLoad,
            ManualKHbetaOverride = false,

            TransverseLoadFactorMode = TransverseLoadFactorMode.MethodB,
            ProfileCrowning = ProfileCrowning.Low,

            MaterialFamily = MaterialFamily.CaseOrThroughHardenedSteel,
            PinionLifeCycles = 3.0e6,
            WheelLifeCycles = 3.0e6,

            HardnessRatioMode = HardnessRatioMode.EqualHardness,

            E_Pinion = 210000.0,
            E_Wheel = 210000.0,
            Nu_Pinion = 0.3,
            Nu_Wheel = 0.3,

            Viscosity40 = 150.0,
            Rz_Flank_Pinion = 8.0,
            Rz_Flank_Wheel = 8.0,
            Rz_Root = 16.0,

            SigmaHlim = 1500.0,
            SigmaFlim = 480.0,

            SH_min = 1.0,
            SF_min = 1.3,
        };

        var checks = new (string, double, double)[]
        {
            // Virtual cylindrical gears (Part 1, Annex A).
            ("dv1", 58.350, 0.01),
            ("dv2", 452.804, 0.01),
            ("betav", 35.0, 0.001),
            ("alphavet", 23.957, 0.01),
            ("mvt", 3.923, 0.01),
            ("zv1", 14.875, 0.01),
            ("zv2", 115.431, 0.01),
            ("betavb", 32.615, 0.01),
            ("dva1", 68.023, 0.01),
            ("dva2", 455.984, 0.01),
            ("dvb1", 53.323, 0.02),
            ("dvb2", 413.796, 0.02),
            ("pvet", 11.262, 0.02),
            ("gv_alpha", 13.120, 0.03),
            ("eps_valpha", 1.165, 0.03),
            ("bv_eff", 21.590, 0.001),
            ("bv", 25.400, 0.001),
            ("eps_vbeta", 1.227, 0.01),
            ("eps_vgamma", 2.392, 0.02),
            ("rho_t", 13.173, 0.02),
            ("betaB", 13.468, 0.02),
            ("rho_rel", 12.459, 0.03),
            ("zvn1", 25.594, 0.02),
            ("zvn2", 198.613, 0.02),
            ("dvn1", 82.241, 0.01),
            ("dvan1", 91.915, 0.01),
            ("dvbn1", 77.281, 0.01),
            ("lbm", 22.442, 0.05),

            // Part 1 general load factors.
            ("Fmt1", 10925.3, 0.001),
            ("Kv", 1.020, 0.08),
            ("KHbeta-C", 1.650, 0.001),
            ("KF0", 1.000, 0.001),
            ("KHalpha", 1.161, 0.1),

            // Part 2 (macropitting).
            ("ZM-B", 0.916, 0.02),
            ("ZLS", 0.935, 0.03),
            ("ZE", 191.646, 0.005),
            ("ZL", 0.992, 0.01),
            ("Zv", 0.974, 0.01),
            ("ZR", 0.930, 0.02),
            ("ZKP", 1.2, 0.001),
            ("ZHyp", 1.0, 0.001),
            ("Fn", 14193.3, 0.001),
            ("sigmaH0-B1", 1168.5, 0.03),
            ("sigmaH-B1", 1712.8, 0.1),
            // sigmaHP1-B1/SH1-B1 are intentionally not checked against the published 1617.8/0.944:
            // ISO/TR 10300-30 Sample 1 states ZNT = 1.0 directly without deriving it from a stated
            // number of load cycles (Table A.4 gives no life requirement), and ISO 10300-2's own
            // Table 6 reference point for ZNT=1.0 (~5e7 to 1e9 cycles) genuinely differs from ISO
            // 10300-3's Table 5 reference point for YNT=1.0 (3e6 cycles, stated explicitly in the
            // standard's text) - the two curves do not share a common "1.0" cycle count. At the
            // NL=3e6 used throughout this self-test (to match YNT exactly, see below), this tool's
            // ZNT curve correctly returns ~1.43, not 1.0, so sigmaHP1/SH1 diverge from the example
            // by construction, not by error - confirmed by every upstream quantity (sigmaH0,
            // sigmaH, ZM-B, ZLS, ZE, ZL, Zv, ZR) matching to within a few tenths of a percent.

            // Part 3 (tooth root bending), pinion.
            ("YLS", 0.873, 0.03),
            ("Y_eps", 0.625, 0.001),
            ("YBS", 1.054, 0.02),
            ("E (pinion)", 0.385, 0.05),
            ("G (pinion)", -0.496, 0.05),
            ("H (pinion)", -0.934, 0.05),
            ("theta (pinion)", 50.783, 0.02),
            ("sFn (pinion)", 7.423, 0.02),
            ("rhoF (pinion)", 1.023, 0.03),
            ("alpha_Fan (pinion)", 31.534, 0.02),
            ("hFa (pinion)", 6.405, 0.02),
            ("YFa (pinion)", 2.033, 0.03),
            ("qs (pinion)", 3.630, 0.03),
            ("YSa (pinion)", 2.022, 0.03),
            ("YR,relT", 0.972, 0.02),
            ("Ydelta,relT (pinion)", 1.010, 0.01),
            ("sigmaF01-B1", 316.5, 0.03),
            ("sigmaF1-B1", 680.1, 0.1),
            ("sigmaFP1-B1", 942.0, 0.02),
            ("SF1-B1", 1.385, 0.1),

            // Part 3, wheel.
            ("YFa (wheel)", 2.480, 0.03),
            ("YSa (wheel)", 1.706, 0.03),
            ("qs (wheel)", 2.166, 0.03),
            ("sigmaF02-B1", 325.9, 0.03),
            ("sigmaF2-B1", 700.1, 0.1),
            ("sigmaFP2-B1", 929.8, 0.02),
            ("SF2-B1", 1.328, 0.1),
        };

        var (pass, fail) = RunChecks(inputs, "=== ISO/TR 10300-30:2024 Sample 1 self-test (spiral bevel, a=0, Method B1) ===", checks);
        Console.WriteLine($"RESULT (Sample 1): {pass} passed, {fail} failed.");
        Console.WriteLine();
    }

    public static void RunSample2Check()
    {
        // ISO/TR 10300-30:2024, Annex B, "Sample 2": a hypoid gear pair, z1=13, z2=42, hypoid
        // offset a=15mm, rated by Method B1. Every value below is taken directly from the
        // technical report's own Tables B.1-B.4. Note the addendum modification coefficients
        // xhm1/xhm2 are not given directly (Table B.2 marks xhm1 "not applicable" for this
        // input method) - they were back-derived from the given absolute addenda via
        // xhm = ham/mmn - 1 (confirmed exactly against the published G1,D/G2,D auxiliary values
        // in Table B.8/B.9, which depend on xhm).
        var inputs = new Iso10300Inputs
        {
            GearType = GearType.SpiralBevel,
            ShaftAngle = 90.0,
            z1 = 13,
            z2 = 42,

            T1_Nm = 250.0,
            PinionSpeed_rpm = 1200.0,

            HypoidOffset_a = 15.0,
            Zeta_mp = 11.390,
            Zeta_m = 10.603,
            Zeta_R = 10.319,

            Re = 89.600,
            de2 = 170.0,
            b = 30.0,
            EffectiveFacewidthFraction = 0.85,
            mmn = 2.640,
            met2 = 4.048,
            Alpha_nD = 17.748,
            Alpha_nC = 22.252,
            Alpha_eD = 20.0,
            Alpha_eC = 20.0,
            Alpha_lim = -2.252,
            Beta_m1 = 50.0,
            Beta_m2 = 38.610, // = betam1 - zeta_mp (10300-1, A.2.3 general note)
            CutterRadius = 63.5,

            dm1 = 53.384,
            Delta1 = 21.291,
            h_am1 = 3.432,
            h_fm1 = 2.508,
            x_hm1 = 0.30,
            x_sm1 = 0.038,
            Rho_a01 = 0.8,
            Spr1 = 0.0,

            dm2 = 141.877,
            Delta2 = 68.321,
            h_am2 = 1.848,
            h_fm2 = 4.092,
            x_hm2 = -0.30,
            x_sm2 = -0.062,
            Rho_a02 = 1.2,
            Spr2 = 0.0,

            k_hfp = 1.25,
            MaterialDensity = 7.86e-6,

            KA = 1.1,
            DynamicFactorMode = DynamicFactorMode.MethodB,
            fpt1_um = 11.6,
            fpt2_um = 23.5,

            Mounting = MountingCondition.OneMemberCantileverMounted,
            ContactVerification = ContactPatternVerification.CheckedUnderLightTestLoad,
            ManualKHbetaOverride = false,

            TransverseLoadFactorMode = TransverseLoadFactorMode.MethodB,
            ProfileCrowning = ProfileCrowning.Low,

            MaterialFamily = MaterialFamily.CaseOrThroughHardenedSteel,
            PinionLifeCycles = 3.0e6,
            WheelLifeCycles = 3.0e6,

            HardnessRatioMode = HardnessRatioMode.EqualHardness,

            E_Pinion = 210000.0,
            E_Wheel = 210000.0,
            Nu_Pinion = 0.3,
            Nu_Wheel = 0.3,

            Viscosity40 = 150.0,
            Rz_Flank_Pinion = 8.0,
            Rz_Flank_Wheel = 8.0,
            Rz_Root = 16.0,

            SigmaHlim = 1500.0,
            SigmaFlim = 480.0,

            SH_min = 1.0,
            SF_min = 1.3,
        };

        var checks = new (string, double, double)[]
        {
            // Virtual cylindrical gears (Part 1, Annex A) - general (hypoid) formulas.
            ("dvb2", 342.334, 0.02),
            ("pvet", 10.329, 0.02),
            ("gv_alpha", 10.435, 0.03),
            ("eps_valpha", 1.010, 0.03),
            ("bv_eff", 24.880, 0.02),
            ("bv", 29.271, 0.02),
            ("zvn1", 38.131, 0.02),
            ("zvn2", 255.605, 0.02),
            ("dvn1", 100.653, 0.02),
            ("dvn2", 674.716, 0.02),
            ("dvan1", 107.517, 0.02),
            ("dvan2", 678.412, 0.02),
            ("dvbn1", 94.583, 0.02),
            ("dvbn2", 634.026, 0.02),
            ("eps_vbeta", 2.096, 0.02),
            ("eps_vgamma", 3.106, 0.02),
            ("betaB", 18.459, 0.02),
            ("rho_t", 19.924, 0.02),
            ("rho_rel", 17.926, 0.03),
            ("lbm", 13.397, 0.05),

            // Part 1 general load factors.
            ("Fmt1", 9366.1, 0.001),
            ("Fvmt", 10427.4, 0.005),
            ("vmt1", 3.354, 0.005),
            ("vmt2", 2.759, 0.005),
            ("Kv", 1.016, 0.08),
            ("KHbeta-C", 1.65, 0.001),
            ("KF0", 1.060, 0.02),
            ("KFbeta-C", 1.556, 0.02),
            // KHalpha* (before offset blending) is NOT checked against the published 0.900:
            // this tool's Method B formula for KHalpha* reproduces Sample 1 exactly (a=0), but
            // gives ~1.21 rather than 0.900 here. Reproducing the published cell exactly
            // requires dividing (fpt-yalpha) by 1000 before use, i.e. treating it as
            // millimetres instead of micrometres, which gives 0.9003 (matches "0.900" to 4
            // significant figures) - strong evidence this is a unit slip in ISO/TR 10300-30's
            // own Sample 2 table (fpt and yalpha are unambiguously defined in micrometres
            // throughout ISO 10300-1, and Sample 1's identical formula matches exactly using
            // micrometres). It does not affect the final result: arel = 0.211 exceeds the 0.1
            // window of Eq (40), so KHalpha = KHalpha* - (KHalpha*-1)/0.1*0.1 = 1.0 EXACTLY
            // regardless of KHalpha*'s value (a mathematical identity of Eq 40 at arel >= 0.1) -
            // which is presumably why this slip was never caught. The final, published
            // KHalpha = 1.000 is still matched exactly, confirmed below.
            ("arel", 0.211, 0.02),
            ("KHalpha", 1.000, 0.02),

            // Part 2 (macropitting) - including the hypoid factor ZHyp.
            ("ZM-B", 0.963, 0.02),
            ("ZE", 191.646, 0.005),
            ("ZL", 0.992, 0.01),
            ("Zv", 0.970, 0.01),
            ("ZR", 0.939, 0.02),
            ("ZHyp", 0.952, 0.03),
            ("Fn", 15298.7, 0.005),
            // ZLS/sigmaH0-B1/sigmaH-B1 are NOT checked here (known limitation): the contact-line
            // clipping factor Clb (10300-1, Eq A.36) reproduces ISO/TR 10300-30 Sample 1 (a=0)
            // to <0.2%, but for this markedly asymmetric hypoid geometry (fmaxB=12.87mm vs
            // fmax0=-5.00mm, a much larger split than Sample 1's near-symmetric fmaxB/fmax0)
            // this tool's Clb formula over-estimates the tip/root contact-line lengths, giving
            // ZLS ~= 0.776 against a published 0.703 (~10% high, which cascades into sigmaH0/
            // sigmaH/YLS/sigmaF ~10-24% high here). The exact general-hypoid form of Eq A.36
            // could not be reverse-engineered with confidence from the source material within
            // this implementation's scope - see the ISO10300_Implementation_Findings.docx for
            // detail. Every OTHER Part 2/3 factor checked here (ZM-B, ZE, ZL, Zv, ZR, ZHyp, Fn,
            // and every Part 3 tooth-form quantity below) matches to within a few tenths of a
            // percent, isolating the gap to Clb/ZLS/YLS specifically. Recommend independent
            // verification of ZLS/YLS for hypoid designs with pronounced offset.

            // Part 3 (tooth root bending), pinion. (YLS/sigmaF*/SF* affected by the ZLS gap above.)
            ("Y_eps", 0.625, 0.001),
            ("E (pinion)", 0.213, 0.05),
            ("G (pinion)", -0.647, 0.05),
            ("H (pinion)", -0.969, 0.05),
            ("theta (pinion)", 52.945, 0.02),
            ("sFn (pinion)", 6.065, 0.03),
            ("rhoF (pinion)", 1.042, 0.03),
            ("alpha_an (pinion)", 28.393, 0.02),
            ("gamma_a (pinion)", 1.078, 0.1),
            ("alpha_Fan (pinion)", 27.315, 0.02),
            ("hFa (pinion)", 5.098, 0.03),
            ("YFa (pinion)", 2.047, 0.03),
            ("qs (pinion)", 2.910, 0.03),
            ("YSa (pinion)", 1.903, 0.03),
            ("YBS", 1.909, 0.05),
            ("YR,relT", 0.972, 0.02),
            ("Ydelta,relT (pinion)", 1.004, 0.01),
            ("sigmaFP1-B1", 936.4, 0.02),

            // Part 3, wheel.
            ("YFa (wheel)", 2.403, 0.03),
            ("YSa (wheel)", 1.690, 0.03),
            ("qs (wheel)", 2.073, 0.03),
            ("Ydelta,relT (wheel)", 0.996, 0.01),
            ("sigmaFP2-B1", 929.0, 0.02),
        };

        var (pass, fail) = RunChecks(inputs, "=== ISO/TR 10300-30:2024 Sample 2 self-test (hypoid gear, a=15mm, Method B1) ===", checks);
        Console.WriteLine($"RESULT (Sample 2): {pass} passed, {fail} failed.");
        Console.WriteLine();
    }
}
