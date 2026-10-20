using AGMA2101E25Calc.Models;

namespace AGMA2101E25Calc.Calc;

/// <summary>
/// Diagnostic-only harness (run with "--selftest") that checks the calculator's output against
/// the numeric illustrative example of ANSI/AGMA 2101-E25 Annex B (clause B.1 input data and
/// every published value of the service factor and safety factor methods; the Annex C K / U_L
/// and clause 19 yield parts of Annex B are outside this calculator's scope and are skipped),
/// then the AGMA 908-B89 geometry factor tables (SelfTest908), and finally that the two
/// 908-table examples carry the computed Z_I / Y_J through the full rating.
/// </summary>
public static class SelfTest
{
    public static int RunAll()
    {
        int pass = 0, fail = 0;
        var (p1, f1) = RunAnnexBCheck();
        pass += p1; fail += f1;
        Console.WriteLine();
        var (p2, f2) = SelfTest908.Run();
        pass += p2; fail += f2;
        Console.WriteLine();
        var (p3, f3) = RunExampleGeometryChecks();
        pass += p3; fail += f3;
        Console.WriteLine();
        var (p4, f4) = RunRatedPowerChecks();
        pass += p4; fail += f4;
        Console.WriteLine();
        var (p5, f5) = RunAnnexD2Check();
        pass += p5; fail += f5;
        Console.WriteLine();
        Console.WriteLine($"=== TOTAL: {pass} passed, {fail} failed ===");
        return fail;
    }

    public static Dictionary<string, List<double>> Values(CalcTrace trace)
    {
        var byLabel = new Dictionary<string, List<double>>();
        foreach (var row in trace.Rows)
        {
            if (row.Symbol.Length == 0 || double.IsNaN(row.Value)) continue;
            if (!byLabel.TryGetValue(row.Symbol, out var list)) byLabel[row.Symbol] = list = new List<double>();
            list.Add(row.Value);
        }
        return byLabel;
    }

    /// <summary>
    /// Every value published in Annex B for the service factor and safety factor methods, with the
    /// relative tolerance used (an absolute floor of 5e-7, half a unit in the sixth printed decimal,
    /// always applies) and a note where the standard's own rounding needs a wider tolerance.
    /// </summary>
    public static readonly (string Symbol, double Expected, double Tolerance, string Note)[] AnnexBExpected =
    {
        ("beta", 0.191986, 2e-6, ""),
        ("u", 4.2, 1e-6, ""),
        ("n2", 345.238095, 1e-6, ""),
        ("d1", 68.763374, 1e-6, ""),
        ("d2", 288.806171, 1e-6, ""),
        ("dw1", 70.338462, 1e-6, ""),
        ("vt", 5.340224, 1e-6, ""),
        ("Ft", 42480.314682, 1e-6, ""),
        ("P", 226.854406, 1e-6, ""),
        ("B", 0.25, 1e-6, ""),
        ("C", 92.0, 1e-6, ""),
        ("KV", 1.078395, 1e-6, ""),
        ("KHpf", 0.108324, 1e-6, ""),
        ("KHpm", 1.1, 1e-6, ""),
        ("KHma", 0.105069, 1e-6, ""),
        ("KHbeta", 1.17938, 5e-6, "printed to five decimals"),
        ("KH", 1.17938, 5e-6, "printed to five decimals"),
        ("nL1", 8.7e8, 1e-6, ""),
        ("nL2", 2.071e8, 5e-4, "printed to four significant figures"),
        ("ZE", 190.0, 1e-6, "Table 2 value"),
        ("sigma_HP1", 1550.0, 1e-6, ""),
        ("sigma_HP2", 1550.0, 1e-6, ""),
        ("sigma_FP1", 450.0, 1e-6, ""),
        ("sigma_FP2", 450.0, 1e-6, ""),
        ("ZN1", 0.902401, 1e-6, ""),
        ("ZN2", 0.932688, 1e-5, "B.1 evaluates it with n_L2 rounded to 2.071e8"),
        ("YN1", 0.93988, 5e-6, "printed to five decimals"),
        ("YN2", 0.964201, 1e-5, "B.1 evaluates it with n_L2 rounded to 2.071e8"),
        ("Pazu1", 281.736437, 1e-5, "B.1 substitutes the rounded KV, KH"),
        ("Pazu2", 300.965462, 1e-5, "B.1 substitutes the rounded KV, KH, ZN2"),
        ("Pazu", 281.736437, 1e-5, ""),
        ("CSF", 1.241926, 1e-5, ""),
        ("Payu1", 313.274509, 1e-5, "B.1 substitutes the rounded KV, KH"),
        ("Payu2", 331.563391, 1e-5, "B.1 substitutes the rounded KV, KH, YN2"),
        ("Payu", 313.274509, 1e-5, ""),
        ("KSF", 1.38095, 1e-5, ""),
        ("sigma_H", 1255.114556, 1e-5, ""),
        ("SH1", 1.114417, 1e-5, "Eq 13 line of B.1 repeats it as 1.114517 (typo)"),
        ("SH2", 1.15182, 1e-5, ""),
        ("sigma_F1", 306.271849, 1e-5, ""),
        ("sigma_F2", 296.866188, 1e-5, ""),
        ("SF1", 1.38095, 1e-5, ""),
        ("SF2", 1.461569, 1e-5, ""),
        ("P_SF", 281.736437, 1e-5, "SHmin = SFmin = 1, KO = YZ = 1: min(P_azu, P_ayu) of B.1"),
    };

    /// <summary>
    /// The load for the required safety factors (clause 7.2 solved for the load). Annex B gives four reference points:
    /// with SHmin = SFmin = 1 it is min(P_azu, P_ayu) = 281.736437 kW; with SHmin = S_H1 of B.1 (1.114417) the pinion's
    /// macropitting gives back B.1's own power, 226.854406 kW; so does SFmin = S_F1 of B.1 (1.38095) for its bending.
    /// As a pinion torque, SHmin = 1.114417 gives back B.1's own input torque, 1494 Nm.
    /// Then, on every example, the rating at the rated power has its governing safety factor equal to the one required.
    /// </summary>
    public static (int pass, int fail) RunRatedPowerChecks()
    {
        Console.WriteLine("=== Load for the required safety factors (clause 7.2 solved for the load) ===");
        int pass = 0, fail = 0;
        void Rated(string name, double shMin, double sfMin, double expected, double relTol)
        {
            AgmaInputs inputs = Examples.AnnexB();
            inputs.Goal = RatingGoal.RatedLoadForRequiredSafetyFactors;
            inputs.MinSafetyFactorPitting = shMin;
            inputs.MinSafetyFactorBending = sfMin;
            var (trace, warnings) = new AgmaCalculator(inputs).Calculate();
            double p = trace.Last("P_SF"), pLoad = trace.Last("P");
            bool ok = Math.Abs(p - expected) <= expected * relTol && Math.Abs(pLoad - p) <= 1e-9 * p && warnings.Count == 0;
            Console.WriteLine($"{(ok ? "PASS" : "FAIL"),-4} {name}: P_SF = {p:0.000000} kW (expected {expected:0.000000}), rated at P = {pLoad:0.000000} kW, {warnings.Count} warning(s)");
            if (ok) pass++; else fail++;
        }
        Rated("Annex B, SHmin = SFmin = 1", 1.0, 1.0, 281.736437, 1e-5);
        Rated("Annex B, SHmin = 1.114417 (S_H1 of B.1)", 1.114417, 0.5, 226.854406, 3e-6);
        Rated("Annex B, SFmin = 1.38095 (S_F1 of B.1)", 0.5, 1.38095, 226.854406, 1e-5);

        // The same as a pinion torque ('Load as' = PinionTorque): B.1's own input torque, 1494 Nm, at 1450 rpm
        {
            AgmaInputs inputs = Examples.AnnexB();
            inputs.Goal = RatingGoal.RatedLoadForRequiredSafetyFactors;
            inputs.LoadInput = LoadSpecification.PinionTorque;
            inputs.MinSafetyFactorPitting = 1.114417;
            inputs.MinSafetyFactorBending = 0.5;
            var (trace, warnings) = new AgmaCalculator(inputs).Calculate();
            double t = trace.Last("T_SF"), tLoad = trace.Rows.First(r => r.Symbol == "T").Value;
            bool ok = Math.Abs(t - 1494.0) <= 1494.0 * 3e-6 && Math.Abs(tLoad - t) <= 1e-9 * t && warnings.Count == 0;
            Console.WriteLine($"{(ok ? "PASS" : "FAIL"),-4} Annex B as a torque, SHmin = 1.114417: T_SF = {t:0.0000} Nm (expected 1494), rated at T = {tLoad:0.0000} Nm, {warnings.Count} warning(s)");
            if (ok) pass++; else fail++;
        }

        for (int index = 0; index < Examples.Names.Length; index++)
        {
            AgmaInputs inputs = Examples.Create(index);
            inputs.MinSafetyFactorPitting = 1.3;
            inputs.MinSafetyFactorBending = 1.6;
            var (checkTrace, _) = new AgmaCalculator(inputs.Clone()).Calculate();
            inputs.Goal = RatingGoal.RatedLoadForRequiredSafetyFactors;
            var (trace, warnings) = new AgmaCalculator(inputs).Calculate();
            double sh = Math.Min(trace.Last("SH1"), trace.Last("SH2")), sf = Math.Min(trace.Last("SF1"), trace.Last("SF2"));
            bool governingMet = Math.Abs(sh - 1.3) <= 1e-9 * 1.3 && sf >= 1.6 * (1 - 1e-9) || Math.Abs(sf - 1.6) <= 1e-9 * 1.6 && sh >= 1.3 * (1 - 1e-9);
            bool sameAsCheck = Math.Abs(checkTrace.Last("P_SF") - trace.Last("P_SF")) <= 1e-9 * trace.Last("P_SF");
            bool ok = governingMet && sameAsCheck && !warnings.Any(w => w.Contains("safety factor"));
            Console.WriteLine($"{(ok ? "PASS" : "FAIL"),-4} {Examples.Names[index]}, SHmin 1.3, SFmin 1.6: P_SF = {trace.Last("P_SF"):0.0000} kW, SH = {sh:0.000000000}, SF = {sf:0.000000000}");
            if (ok) pass++; else fail++;
        }
        return (pass, fail);
    }

    /// <summary>
    /// Annex D.2, fatigue life by Miner's rule: every value the annex publishes (K_V per bin, the fatigue stress numbers,
    /// stress cycle factors and cycles to failure per bin, the cycles, N, w_b and L of each member and mode), then the
    /// continuity with the safety factor method: one bin at the load for the required safety factors lasts the design
    /// life. The tolerances widen where a rounded stress cycle factor is raised to the power 1/0.023 or 1/0.0178.
    /// (Written together with IGDv4.0's IGDAnalysis.Tests/Rating/Agma2101MinerLifeTests.cs, 2026-10-20.)
    /// </summary>
    public static (int pass, int fail) RunAnnexD2Check()
    {
        Console.WriteLine("=== Annex D.2: fatigue life by Miner's rule ===");
        int pass = 0, fail = 0;
        var (trace, warnings) = new AgmaCalculator(Examples.AnnexD2()).Calculate();
        void Check(string symbol, double expected, double relTol)
        {
            double actual = trace.Last(symbol);
            bool ok = Math.Abs(actual - expected) <= Math.Abs(expected) * relTol;
            Console.WriteLine($"{(ok ? "PASS" : "FAIL"),-4} {symbol,-14} {actual,16:G9}  expected {expected:G9}");
            if (ok) pass++; else fail++;
        }
        double[] kv = { 1.108759, 1.102643, 1.097519, 1.093105, 1.085901, 1.077762 };
        double[] sh = { 871.13754887, 928.71314045, 982.75760442, 1033.83224837, 1128.7684815, 1257.26318171 };
        double[] zn = { 0.562024, 0.59917, 0.634037, 0.666989, 0.728238, 0.811138 };
        double[] nfh = { 7.596093e17, 4.700059e16, 4.018646e15, 4.439981e14, 9.738288e12, 8.969518e10 };
        double[] nh = { 1.8312e8, 3.204e8, 5.6976e8, 1.9224e8, 1.068e8, 4.272e7 };
        double[] sf1 = { 147.541371, 167.688596, 187.773013, 207.797586, 247.71374, 307.321358 };
        double[] yn1 = { 0.32787, 0.372641, 0.417273, 0.461772, 0.550475, 0.682936 };
        double[] nf1 = { 4.310141e34, 3.247255e31, 5.641881e28, 1.901529e26, 9.818025e21, 5.38536e16 };
        double[] sf2 = { 143.01035, 162.538851, 182.006471, 201.416087, 240.106409, 297.883466 };
        double[] yn2 = { 0.317801, 0.361197, 0.404459, 0.447591, 0.53357, 0.661963 };
        double[] nf2 = { 2.486145e35, 1.873078e32, 3.254039e29, 1.096802e27, 5.662998e22, 3.106293e17 };
        for (int i = 0; i < 6; i++)
        {
            string k = " [" + (i + 1) + "]";
            Check("KV" + k, kv[i], 1e-6);
            Check("sigma_H1" + k, sh[i], 1e-6);
            Check("Z_N1" + k, zn[i], 1e-5);
            Check("Nf_H1" + k, nfh[i], 2e-4);
            Check("n_H1" + k, nh[i], 1e-9);
            Check("sigma_F1" + k, sf1[i], 1e-6);
            Check("Y_N1" + k, yn1[i], 2e-5);
            Check("Nf_F1" + k, nf1[i], 2e-3);
            Check("sigma_F2" + k, sf2[i], 1e-6);
            Check("Y_N2" + k, yn2[i], 2e-5);
            Check("Nf_F2" + k, nf2[i], 2e-3);
        }
        Check("Sum n_H1", 1.41504e9, 1e-9);
        Check("N_H1", 2.900687e12, 2e-4);
        Check("wb_H1", 2358.397665, 1e-5);
        Check("L_H1", 2.0499e7, 1e-4);
        Check("Sum n_H2", 3.369142e8, 1e-6);
        Check("N_H2", 2.900687e12, 2e-4);
        Check("wb_H2", 561.523254, 1e-5);
        Check("L_H2", 8.609578e7, 2e-4);
        Check("N_F1", 1.783798e18, 2e-3);
        Check("L_F1", 1.2606e13, 2e-3);
        Check("N_F2", 1.0289e19, 2e-3);
        Check("L_F2", 3.053895e14, 2e-3);
        if (warnings.Count != 0) { Console.WriteLine($"FAIL D.2 has {warnings.Count} warning(s): {string.Join(" | ", warnings)}"); fail++; }

        // Continuity: one bin at the torque for the required safety factors, for the design life, lasts the design life
        foreach (var (shMin, sfMin) in new[] { (1.0, 1.0), (1.3, 1.6) })
        {
            AgmaInputs inputs = Examples.AnnexB();
            inputs.Goal = RatingGoal.RatedLoadForRequiredSafetyFactors;
            inputs.LoadInput = LoadSpecification.PinionTorque;
            inputs.MinSafetyFactorPitting = shMin;
            inputs.MinSafetyFactorBending = sfMin;
            var (rated, _) = new AgmaCalculator(inputs).Calculate();
            inputs.Goal = RatingGoal.FatigueLifeForLoadSpectrum;
            inputs.LoadSpectrum = AgmaCalculator.FormatLoadSpectrum(new[] { new SpectrumBin { Torque = rated.Last("T_SF"), Speed = inputs.PinionSpeed, Hours = inputs.LifeHours } });
            var (life, _) = new AgmaCalculator(inputs).Calculate();
            bool ok = Math.Abs(life.Last("L min") - inputs.LifeHours) <= inputs.LifeHours * 1e-9;
            Console.WriteLine($"{(ok ? "PASS" : "FAIL"),-4} one bin at T_SF (SHmin {shMin}, SFmin {sfMin}) lasts L = {inputs.LifeHours} h: {life.Last("L min"):G12} h");
            if (ok) pass++; else fail++;
        }
        return (pass, fail);
    }

    public static (int pass, int fail) RunAnnexBCheck()
    {
        var (trace, warnings) = new AgmaCalculator(Examples.AnnexB()).Calculate();
        var byLabel = Values(trace);

        int pass = 0, fail = 0;
        void Check(string symbol, double expected, double tolFraction)
        {
            if (!byLabel.TryGetValue(symbol, out var values))
            {
                Console.WriteLine($"MISSING  {symbol,-14} expected={expected}");
                fail++;
                return;
            }
            double actual = values.OrderBy(v => Math.Abs(v - expected)).First();
            double tol = Math.Max(Math.Abs(expected) * tolFraction, 5e-7);   // half a unit in the 6th printed decimal
            bool ok = Math.Abs(actual - expected) <= tol;
            Console.WriteLine($"{(ok ? "PASS" : "FAIL"),-4} {symbol,-12} expected={expected,16:0.000000} actual={actual,16:0.000000} diff%={100 * (actual - expected) / expected,9:0.0000}");
            if (ok) pass++; else fail++;
        }

        Console.WriteLine("=== ANSI/AGMA 2101-E25 Annex B self-test ===");
        Console.WriteLine();
        foreach (var (symbol, expected, tolFraction, _) in AnnexBExpected)
            Check(symbol, expected, tolFraction);

        Console.WriteLine();
        Console.WriteLine($"Annex B: {pass} passed, {fail} failed.");
        foreach (var w in warnings) Console.WriteLine("  warning: " + w);
        return (pass, fail);
    }

    /// <summary>The 908-table examples: the full rating must carry the table I and J values.</summary>
    public static (int pass, int fail) RunExampleGeometryChecks()
    {
        Console.WriteLine("=== Examples with computed geometry factors (AGMA 908 tables) ===");
        int pass = 0, fail = 0;
        foreach (var (index, I, J1, J2) in new[] { (1, 0.174, 0.49, 0.44), (2, 0.091, 0.34, 0.37) })
        {
            var (trace, warnings) = new AgmaCalculator(Examples.Create(index)).Calculate();
            var v = Values(trace);
            double zi = v["ZI"].Last(), yj1 = v["YJ1"].Last(), yj2 = v["YJ2"].Last();
            bool ok = Math.Abs(zi - I) <= 0.0006 && Math.Abs(yj1 - J1) <= 0.006 && Math.Abs(yj2 - J2) <= 0.006;
            Console.WriteLine($"{(ok ? "PASS" : "FAIL"),-4} {Examples.Names[index]}: ZI={zi:0.0000} (table {I:0.000}), YJ1={yj1:0.0000} (table {J1:0.00}), YJ2={yj2:0.0000} (table {J2:0.00}); {warnings.Count} warning(s)");
            foreach (var w in warnings) Console.WriteLine("  warning: " + w);
            if (ok) pass++; else fail++;
        }
        return (pass, fail);
    }
}
