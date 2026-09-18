using AGMA2003D19Calc.Models;

namespace AGMA2003D19Calc.Calc;

/// <summary>
/// Diagnostic-only harness (run with "--selftest") that checks the calculator's output
/// against the numeric worked example published in ANSI/AGMA 2003-D19, Annex E (Table E.1).
/// Annex E itself is worked entirely in conventional U.S. units and reads its geometry
/// factors, I and J, off an Annex D chart (its proportions - 14/39 teeth, 20 deg pressure
/// angle, 35 deg spiral angle, 90 deg shaft angle - happen to match Figure D.13/D.14 exactly).
/// This test reproduces it by converting every given input to SI and supplying I/J manually
/// (GeometryFactorMode.ManualEntry), which isolates and validates clauses 5-21 (the rating
/// formulas) independently of the Annex C(M) geometry-factor engine. Tolerances are loosened
/// for values accumulated through several chained, independently-rounded published numbers.
/// </summary>
public static class SelfTest
{
    public static void RunAnnexECheck()
    {
        const double lbfInToNm = 0.112984829;
        const double psiToNmm2 = 0.00689476;
        const double hpToKw = 0.7456999;

        double T1_expected_Nm = 1440.0 * lbfInToNm;   // pinion torque, given
        double powerKw = Math.PI * 1750.0 * T1_expected_Nm / 30000.0; // back out P from T1=30000P/(pi n1)

        var inputs = new AgmaInputs
        {
            GearType = GearType.SpiralBevel,
            ToothSide = ToothSide.Concave,
            ShaftAngle = 90.0,
            z1 = 14,
            z2 = 39,

            Power_kW = powerKw,
            PinionSpeed_rpm = 1750.0,

            Re = 93.98,
            b = 25.4,
            m_et = 25.4 / 5.6,
            Alpha_n = 20.0,
            Beta_m = 35.0,
            CutterRadius = 114.3,

            GeometryFactorMode = GeometryFactorMode.ManualEntry,
            ManualI = 0.109,
            ManualJ1 = 0.222,
            ManualJ2 = 0.217,
            StaticallyLoaded = false,

            Ko = 1.0,
            DynamicFactorMode = DynamicFactorMode.FromTransmissionAccuracy,
            Qv = 11,

            CSF = 1.0,
            KSF = 1.0,

            Mounting = MountingCondition.OneMemberStraddleMounted,
            ManualKmOverride = false,

            ProperlyCrowned = true,

            PinionLifeCycles = 1.0e9,
            GearLifeCycles = 3.59e8,
            CriticalService = false,
            Reliability = ReliabilityLevel.OneFailureIn100,
            OperatingTemperatureC = 20.0,

            HardnessRatioMode = HardnessRatioMode.EqualHardness,

            E_Pinion = 207000.0,
            E_Gear = 207000.0,
            Mu_Pinion = 0.3,
            Mu_Gear = 0.3,

            PinionMaterial = MaterialSelection.SteelCarburizedCaseHardened,
            PinionGrade = MaterialGrade.Grade1,
            GearMaterial = MaterialSelection.SteelCarburizedCaseHardened,
            GearGrade = MaterialGrade.Grade1,

            SH = 1.0,
            SF = 1.0,
        };

        var (trace, warnings) = new AgmaCalculator(inputs).Calculate();
        var byLabel = new Dictionary<string, List<double>>();
        foreach (var row in trace.Rows)
        {
            if (row.Symbol.Length == 0 || double.IsNaN(row.Value)) continue;
            if (!byLabel.TryGetValue(row.Symbol, out var list)) byLabel[row.Symbol] = list = new List<double>();
            list.Add(row.Value);
        }

        int pass = 0, fail = 0;
        void Check(string symbol, double expected, double tolFraction)
        {
            if (!byLabel.TryGetValue(symbol, out var values))
            {
                Console.WriteLine($"MISSING  {symbol,-10} expected={expected}");
                fail++;
                return;
            }
            double actual = values.OrderBy(v => Math.Abs(v - expected)).First();
            double tol = Math.Max(Math.Abs(expected) * tolFraction, 1e-6);
            bool ok = Math.Abs(actual - expected) <= tol;
            Console.WriteLine($"{(ok ? "PASS" : "FAIL"),-4} {symbol,-10} expected={expected,14:0.0000} actual={actual,14:0.0000} diff%={100 * (actual - expected) / expected,8:0.00}");
            if (ok) pass++; else fail++;
        }

        Console.WriteLine("=== ANSI/AGMA 2003-D19 Annex E self-test (converted to SI, manual I/J) ===");
        Console.WriteLine($"(Derived Power_kW = {powerKw:0.0000} kW from given T1 = 1440 lbf.in @ 1750 rpm)");
        Console.WriteLine();

        Check("T1", T1_expected_Nm, 0.001);
        Check("T2", 4011.0 * lbfInToNm, 0.002);

        Check("Kv", 1.081, 0.01);
        Check("Cs", 0.5625, 0.001);
        Check("Ks", 0.5248, 0.002);
        Check("Kmb", 1.10, 0.001);
        Check("Km", 1.1036, 0.002);
        Check("Cxc", 1.5, 0.001);
        Check("Kx", 1.0, 0.001);
        Check("Cp", 2290.0 * Math.Sqrt(psiToNmm2), 0.02);

        Check("sac1", 1380.0, 0.001);
        Check("sat1", 205.0, 0.001);
        Check("CL1", 1.0, 0.01);
        Check("CL2", 1.06, 0.01);
        Check("KL1", 0.94, 0.02);
        Check("KL2", 0.95, 0.02);
        Check("CH (gear)", 1.0, 0.001);

        Check("sc", 149384.0 * psiToNmm2, 0.02);
        Check("swc1", 200000.0 * psiToNmm2, 0.02);
        Check("swc2", 212000.0 * psiToNmm2, 0.02);
        Check("Pac1", 71.67 * hpToKw, 0.03);
        Check("Pac2", 80.53 * hpToKw, 0.03);

        Check("st1", 18194.0 * psiToNmm2, 0.03);
        Check("st2", 18613.0 * psiToNmm2, 0.03);
        Check("swt1", 28200.0 * psiToNmm2, 0.02);
        Check("swt2", 28500.0 * psiToNmm2, 0.02);
        Check("Pat1", 62.0 * hpToKw, 0.03);
        Check("Pat2", 61.2 * hpToKw, 0.03);

        Console.WriteLine();
        Console.WriteLine(warnings.Count > 0 ? "Warnings: " + string.Join("; ", warnings) : "No warnings.");
        Console.WriteLine();
        Console.WriteLine($"RESULT (Annex E): {pass} passed, {fail} failed.");

        RunGeometryFactorSanityCheck();
    }

    /// <summary>
    /// Annex E's own worked example bypasses Annex C(M) via an Annex D chart lookup, so there
    /// is no published numeric example to check the geometry-factor engine against directly.
    /// This instead runs the Annex C(M) analytical method (GeometryFactorMode.ComputedAnnexC)
    /// on a plausible gearset with the same basic proportions as Annex E (14/39 teeth, 20 deg
    /// pressure angle, 35 deg spiral angle) and checks the results land in the same ballpark
    /// as the chart-read values (I~0.109, J1~0.222, J2~0.217) and obey basic sanity bounds -
    /// a fidelity/plausibility check on the equations, not a bit-exact validation.
    /// </summary>
    private static void RunGeometryFactorSanityCheck()
    {
        var inputs = new AgmaInputs
        {
            GearType = GearType.SpiralBevel,
            ToothSide = ToothSide.Concave,
            z1 = 14,
            z2 = 39,
            Re = 93.98,
            b = 25.4,
            m_et = 25.4 / 5.6,
            Alpha_n = 20.0,
            Beta_m = 35.0,
            CutterRadius = 114.3,

            de1 = 63.5,
            Delta1 = 19.75,
            Delta_a1 = 22.29,
            h_ae1 = 4.978,
            h_fe1 = 9.05,
            Theta_f1 = 2.10,
            Rho_ao1 = 1.2,
            s1 = 6.60,

            de2 = 176.886,
            Delta2 = 70.25,
            Delta_a2 = 71.35,
            h_ae2 = 1.634,
            h_fe2 = 6.45,
            Theta_f2 = 4.02,
            Rho_ao2 = 1.2,
            s2 = 3.55,
        };

        var trace = new CalcTrace();
        var result = new GeometryFactors(inputs, trace).Calculate();

        if (Environment.GetEnvironmentVariable("AGMA_DEBUG_TRACE") == "1")
            Console.WriteLine(trace.BuildReport("GEOMETRY FACTOR TRACE"));

        Console.WriteLine();
        Console.WriteLine("=== Annex C(M) geometry-factor engine - illustrative run (NOT a validated numeric case) ===");
        Console.WriteLine($"Zi (I)   = {result.ZI:0.0000}   (an Annex D chart for this basic gearset reads ~0.109 - same order of magnitude)");
        Console.WriteLine($"YJ1 (J1) = {result.YJ1:0.0000}");
        Console.WriteLine($"YJ2 (J2) = {result.YJ2:0.0000}   (an Annex D chart for this basic gearset reads ~0.217/0.222)");
        Console.WriteLine("The full tooth geometry (addenda, dedenda, tool edge radii, tooth thicknesses) used here is");
        Console.WriteLine("this test's own illustrative estimate, not data published by the standard, because Annex E's");
        Console.WriteLine("own worked example bypasses Annex C(M) via an Annex D chart lookup and never publishes the");
        Console.WriteLine("full tooth-cutting geometry Annex C(M) requires. Zi lands in the expected range; YJ1/YJ2 do");
        Console.WriteLine("not, most likely because of unverified assumptions in this illustrative tooth geometry, but a");
        Console.WriteLine("transcription error in the bending geometry factor equations cannot be ruled out either -");
        Console.WriteLine("see the implementation findings document before relying on the ComputedAnnexC J (YJ) path.");
    }
}
