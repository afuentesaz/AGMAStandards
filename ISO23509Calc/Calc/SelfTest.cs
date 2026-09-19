using ISO23509Calc.Models;

namespace ISO23509Calc.Calc;

/// <summary>
/// Diagnostic-only harness (run with "--selftest") that checks the calculator's output against
/// the four numeric worked examples published in ISO 23509:2016 itself, Annex F: Sample 1 (F.2,
/// a spiral bevel gear pair without hypoid offset, Method 0, face milling, z1=14/z2=39), Sample 2
/// (F.3, a hypoid gear pair, Method 1/Gleason, face milling, z1=13/z2=42, a=15mm), Sample 3 (F.4,
/// a hypoid gear pair, Method 2/Oerlikon, face hobbing, z1=9/z2=34, a=31.75mm) and Sample 4 (F.5,
/// a hypoid gear pair, Method 3/Klingelnberg, face hobbing, z1=12/z2=49, a=40mm). Every input
/// below is taken directly from the standard's own Tables F.1-F.12, and every checked value is
/// one of its own published intermediate or final results - between them, the four samples
/// exercise all four pitch cone methods, both manufacturing processes, both input data types
/// (I and II), all four backlash options, and both the Sigma != 90 deg auxiliary-angle branch
/// (not exercised - all four samples use Sigma = 90 deg) and its Sigma = 90 deg counterpart.
/// (These four gear pairs are also the ones behind ISO/TR 10300-30:2024's own Samples 1-4,
/// which ISO10300Calc's own SelfTest validates against independently.)
/// </summary>
public static class SelfTest
{
    private static (int pass, int fail) RunChecks(Iso23509Inputs inputs, string title, (string symbol, double expected, double tolFraction)[] checks)
    {
        var (trace, _, _, warnings) = new Iso23509Calculator(inputs).Calculate();
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
                Console.WriteLine($"MISSING  {symbol,-20} expected={expected}");
                fail++;
                continue;
            }
            double actual = values.OrderBy(v => Math.Abs(v - expected)).First();
            double tol = Math.Max(Math.Abs(expected) * tolFraction, 1e-6);
            bool ok = Math.Abs(actual - expected) <= tol;
            Console.WriteLine($"{(ok ? "PASS" : "FAIL"),-4} {symbol,-20} expected={expected,14:0.0000} actual={actual,14:0.0000} diff%={100 * (actual - expected) / expected,8:0.00}");
            if (ok) pass++; else fail++;
        }

        Console.WriteLine();
        Console.WriteLine(warnings.Count > 0 ? "Warnings: " + string.Join("; ", warnings) : "No warnings.");
        Console.WriteLine();
        return (pass, fail);
    }

    public static void RunSample1Check()
    {
        // ISO 23509:2016, Annex F.2: spiral bevel gear pair without hypoid offset (z1=14,
        // z2=39), Method 0, face milling. Data type II. Every value below is taken directly
        // from Tables F.1-F.3 and Formulae (F.1)-(F.108).
        var inputs = new Iso23509Inputs
        {
            Method = PitchConeMethod.Method0,
            Process = ManufacturingProcess.FaceMilling,
            Sigma = 90.0,
            a = 0.0,
            z1 = 14,
            z2 = 39,
            de2 = 176.893,
            b2 = 25.4,
            beta_m2 = 35.0,
            rc0 = 114.3,

            DataType = InputDataType.TypeII,
            alpha_dD = 20.0,
            alpha_dC = 20.0,
            f_alim = 0.0,
            c_ham = 0.24737,
            k_d = 2.000,
            k_c = 0.125,
            k_t = 0.0915,

            Backlash = BacklashMode.OuterNormal_jen,
            BacklashValue = 0.127,

            ThetaSource = ThetaA2Source.Manual,
            theta_a2 = 2.1342,
            theta_f2 = 6.4934,

            CheckUndercut = false,
        };

        var checks = new (string, double, double)[]
        {
            ("u", 2.786, 0.002), ("delta1", 19.747, 0.002), ("delta2", 70.253, 0.002),
            ("Rm", 81.273, 0.002), ("beta_m1=beta_m2", 35.000, 0.002), ("c_be2", 0.5, 0.002),
            ("dm1", 54.918, 0.002), ("dm2", 152.987, 0.002),
            ("zeta_m", 0.0, 0.01), ("zeta_mp", 0.0, 0.01), ("ap", 0.0, 0.01), ("m_mn", 3.213, 0.002),
            ("alpha_lim", 0.0, 0.01), ("alpha_nD", 20.0, 0.002), ("alpha_nC", 20.0, 0.002),
            ("alpha_eD", 20.0, 0.002), ("alpha_eC", 20.0, 0.002),
            ("Re2", 93.973, 0.002), ("Ri2", 68.573, 0.002), ("de2", 176.893, 0.002), ("di2", 129.080, 0.002),
            ("met2", 4.536, 0.002), ("be2", 12.700, 0.002), ("bi2", 12.700, 0.002),
            ("tzm2", 27.459, 0.003), ("tzm1", 76.493, 0.003), ("tz1", 0.0, 0.01), ("tz2", 0.0, 0.01),
            ("hmw", 6.427, 0.002), ("ham2", 1.591, 0.003), ("hfm2", 5.639, 0.002), ("ham1", 4.836, 0.002),
            ("hfm1", 2.394, 0.003), ("c", 0.803, 0.003), ("hm", 7.230, 0.002),
            ("delta_a2", 72.387, 0.002), ("delta_f2", 63.760, 0.002),
            ("phi_R", 0.0, 0.01), ("phi_o", 0.0, 0.01), ("zeta_R", 0.0, 0.01), ("zeta_o", 0.0, 0.01),
            ("delta_a1", 26.240, 0.002), ("delta_f1", 17.613, 0.002),
            ("theta_a1", 6.493, 0.003), ("theta_f1", 2.134, 0.003),
            ("tzF2", -1.508, 0.02), ("tzR2", 3.999, 0.02), ("tzF1", -9.931, 0.02), ("tzR1", 2.094, 0.02),
            ("bp1", 25.400, 0.002), ("b1A", 12.700, 0.002), ("b1", 25.400, 0.002),
            ("be1", 12.700, 0.002), ("bi1", 12.700, 0.002),
            ("Re21", 93.973, 0.002), ("Ri21", 68.573, 0.002),
            ("beta_e21", 36.846, 0.003), ("beta_i21", 33.946, 0.003),
            ("zeta_ep21", 0.0, 0.01), ("zeta_ip21", 0.0, 0.01),
            ("beta_e1", 36.846, 0.003), ("beta_i1", 33.946, 0.003),
            ("beta_e2", 36.846, 0.003), ("beta_i2", 33.946, 0.003),
            ("hae1", 6.281, 0.003), ("hae2", 2.064, 0.003), ("hfe1", 2.867, 0.003), ("hfe2", 7.085, 0.003),
            ("he1", 9.149, 0.003), ("he2", 9.149, 0.003),
            ("hai1", 3.391, 0.003), ("hai2", 1.117, 0.003), ("hfi1", 1.921, 0.003), ("hfi2", 4.194, 0.003),
            ("hi1", 5.311, 0.003), ("hi2", 5.311, 0.003),
            ("alpha_n", 20.0, 0.002),
            ("xsm1", 0.037, 0.05), ("smn1", 6.465, 0.003), ("xsm2", -0.055, 0.05), ("smn2", 3.511, 0.003),
            ("smt1", 7.892, 0.003), ("smt2", 4.286, 0.003),
            ("dmn1", 82.241, 0.003), ("dmn2", 638.207, 0.003),
            ("smnc1", 6.458, 0.003), ("smnc2", 3.511, 0.003),
            ("hamc1", 4.955, 0.003), ("hamc2", 1.592, 0.005),
            ("Re1", 93.973, 0.002), ("Ri1", 68.573, 0.002), ("de1", 63.500, 0.002), ("di1", 46.337, 0.002),
            ("dae1", 75.324, 0.002), ("dae2", 178.288, 0.002), ("dfe1", 58.102, 0.002), ("dfe2", 172.106, 0.002),
            ("dai1", 52.719, 0.002), ("dai2", 129.835, 0.002), ("dfi1", 42.721, 0.002), ("dfi2", 126.246, 0.002),
            ("txo1", 86.324, 0.003), ("txo2", 29.808, 0.003), ("txi1", 63.395, 0.003), ("txi2", 22.117, 0.003),
            ("ht1", 9.137, 0.003),
        };

        var (pass, fail) = RunChecks(inputs, "=== ISO 23509:2016 Annex F.2 self-test (spiral bevel, Method 0, face milling) ===", checks);
        Console.WriteLine($"RESULT (Sample 1): {pass} passed, {fail} failed.");
        Console.WriteLine();
    }

    public static void RunSample2Check()
    {
        // ISO 23509:2016, Annex F.3: hypoid gear pair (z1=13, z2=42, a=15mm), Method 1
        // (Gleason), face milling. Data type II. Every value below is taken directly from
        // Tables F.4-F.6 and Formulae (F.109)-(F.267).
        var inputs = new Iso23509Inputs
        {
            Method = PitchConeMethod.Method1,
            Process = ManufacturingProcess.FaceMilling,
            Sigma = 90.0,
            a = 15.0,
            z1 = 13,
            z2 = 42,
            de2 = 170.0,
            b2 = 30.0,
            beta_m1_desired = 50.0,
            rc0 = 63.5,

            DataType = InputDataType.TypeII,
            alpha_dD = 20.0,
            alpha_dC = 20.0,
            f_alim = 1.0,
            c_ham = 0.35,
            k_d = 2.000,
            k_c = 0.125,
            k_t = 0.1,

            Backlash = BacklashMode.OuterTransverse_jet2,
            BacklashValue = 0.2,

            ThetaSource = ThetaA2Source.Manual,
            theta_a2 = 1.0,
            theta_f2 = 4.0,

            CheckUndercut = false,
        };

        var checks = new (string, double, double)[]
        {
            ("u", 3.231, 0.002),
            ("Rm1", 73.519, 0.02), ("Rm2", 76.337, 0.02),
            ("beta_m1", 49.998, 0.01), ("beta_m2", 38.609, 0.01), ("c_be2", 0.504, 0.02),
            ("dm1", 53.383, 0.02), ("dm2", 141.877, 0.02),
            ("zeta_m", 10.603, 0.02), ("zeta_mp", 11.390, 0.02), ("ap", 15.075, 0.02), ("m_mn", 2.640, 0.02),
            ("alpha_lim", -2.253, 0.02), ("alpha_nD", 17.747, 0.01), ("alpha_nC", 22.253, 0.01),
            ("alpha_eD", 20.0, 0.01), ("alpha_eC", 20.0, 0.01),
            ("Re2", 91.468, 0.02), ("Ri2", 61.468, 0.02), ("de2", 170.0, 0.005), ("di2", 114.243, 0.02),
            ("met2", 4.047, 0.02), ("be2", 15.1314, 0.02), ("bi2", 14.869, 0.02),
            ("tzm2", 26.621, 0.02), ("tzm1", 69.727, 0.02), ("tz1", -1.225, 0.1), ("tz2", 1.576, 0.1),
            ("hmw", 5.280, 0.02), ("ham2", 1.848, 0.02), ("hfm2", 4.092, 0.02), ("ham1", 3.432, 0.02),
            ("hfm1", 2.508, 0.02), ("c", 0.660, 0.02), ("hm", 5.939, 0.02),
            ("delta_a2", 69.323, 0.01), ("delta_f2", 64.324, 0.01),
            ("zeta_R", 10.319, 0.03), ("zeta_o", 10.674, 0.03),
            ("delta_a1", 25.232, 0.02), ("delta_f1", 20.303, 0.02),
            ("theta_a1", 3.943, 0.03), ("theta_f1", 0.985, 0.05),
            ("bp1", 30.626, 0.02), ("b1A", 15.243, 0.02),
            ("lambda'", 1.239, 0.05), ("b_reri1", 30.470, 0.03),
            ("be1", 16.089, 0.03), ("bi1", 15.822, 0.03), ("b1", 31.910, 0.03),
            ("Re21", 92.163, 0.02), ("Ri21", 60.907, 0.02),
            ("beta_e21", 48.130, 0.02), ("beta_i21", 30.549, 0.02),
            ("beta_e1", 57.544, 0.02), ("beta_i1", 44.879, 0.02),
            ("beta_e2", 47.674, 0.02), ("beta_i2", 30.826, 0.02),
            ("hae1", 4.541, 0.03), ("hae2", 2.112, 0.03), ("hfe1", 2.784, 0.03), ("hfe2", 5.150, 0.03),
            ("he1", 7.325, 0.03), ("he2", 7.262, 0.03),
            ("hai1", 2.341, 0.03), ("hai2", 1.588, 0.03), ("hfi1", 2.236, 0.03), ("hfi2", 3.052, 0.03),
            ("hi1", 4.577, 0.03), ("hi2", 4.640, 0.03),
            ("alpha_n", 20.0, 0.002),
            ("xsm1", 0.038, 0.1), ("smn1", 4.922, 0.02), ("xsm2", -0.062, 0.1), ("smn2", 3.241, 0.02),
            ("smt1", 7.656, 0.02), ("smt2", 4.147, 0.02),
            ("dmn1", 118.898, 0.02), ("dmn2", 585.370, 0.02),
            ("smnc1", 4.917, 0.02), ("smnc2", 3.241, 0.02),
            ("hamc1", 3.432, 0.02), ("hamc2", 1.849, 0.02),
            ("Re1", 89.608, 0.02), ("Ri1", 57.697, 0.02), ("de1", 65.065, 0.02), ("di1", 41.895, 0.02),
            ("dae1", 73.528, 0.02), ("dae2", 171.560, 0.01), ("dfe1", 59.877, 0.02), ("dfe2", 166.196, 0.01),
            ("dai1", 46.258, 0.02), ("dai2", 115.416, 0.02), ("dfi1", 37.730, 0.03), ("dfi2", 111.989, 0.02),
            ("txo1", 83.070, 0.02), ("txo2", 30.247, 0.03), ("txi1", 54.135, 0.03), ("txi2", 19.653, 0.03),
            ("ht1", 7.320, 0.03),
        };

        var (pass, fail) = RunChecks(inputs, "=== ISO 23509:2016 Annex F.3 self-test (hypoid, Method 1/Gleason, face milling) ===", checks);
        Console.WriteLine($"RESULT (Sample 2): {pass} passed, {fail} failed.");
        Console.WriteLine();
    }

    public static void RunSample3Check()
    {
        // ISO 23509:2016, Annex F.4: hypoid gear pair (z1=9, z2=34, a=31.75mm), Method 2
        // (Oerlikon), face hobbing. Data type II. Every value below is taken directly from
        // Tables F.7-F.9 and Formulae (F.268)-(F.418).
        var inputs = new Iso23509Inputs
        {
            Method = PitchConeMethod.Method2,
            Process = ManufacturingProcess.FaceHobbing,
            Sigma = 90.0,
            a = 31.75,
            z1 = 9,
            z2 = 34,
            dm2 = 146.7,
            b2 = 26.0,
            beta_m2 = 21.009,
            rc0 = 76.0,
            z0 = 13,

            DataType = InputDataType.TypeII,
            alpha_dD = 20.0,
            alpha_dC = 20.0,
            f_alim = 1.0,
            c_ham = 0.275,
            k_d = 2.000,
            k_c = 0.125,
            k_t = 0.1,

            Backlash = BacklashMode.OuterTransverse_jet2,
            BacklashValue = 0.2,

            ThetaSource = ThetaA2Source.Manual,
            theta_a2 = 0.0,
            theta_f2 = 0.0,

            CheckUndercut = false,
        };

        var checks = new (string, double, double)[]
        {
            ("nu", 20.151, 0.01), ("lambda", 89.142, 0.01), ("u", 3.778, 0.002),
            ("Rm1", 61.186, 0.02), ("Rm2", 82.168, 0.02),
            ("beta_m1", 44.990, 0.02), ("beta_m2", 21.009, 0.002), ("c_be2", 0.5, 0.002),
            ("dm1", 51.258, 0.02), ("dm2", 146.7, 0.002),
            ("zeta_m", 21.647, 0.02), ("zeta_mp", 23.969, 0.02), ("ap", 33.380, 0.02), ("m_mn", 4.028, 0.02),
            ("alpha_lim", -4.132, 0.02), ("alpha_nD", 15.868, 0.01), ("alpha_nC", 24.132, 0.01),
            ("alpha_eD", 20.0, 0.01), ("alpha_eC", 20.0, 0.01),
            ("Re2", 95.168, 0.02), ("Ri2", 69.168, 0.02), ("de2", 169.910, 0.02), ("di2", 123.490, 0.02),
            ("met2", 4.997, 0.02), ("be2", 13.000, 0.02), ("bi2", 13.000, 0.02),
            ("tzm2", 25.195, 0.03), ("tzm1", 68.178, 0.03), ("tz1", -12.618, 0.1), ("tz2", 11.836, 0.1),
            ("hmw", 8.056, 0.02), ("ham2", 2.215, 0.02), ("hfm2", 6.847, 0.02), ("ham1", 5.840, 0.02),
            ("hfm1", 3.222, 0.02), ("c", 1.007, 0.02), ("hm", 9.063, 0.02),
            ("delta_a2", 63.212, 0.01), ("delta_f2", 63.212, 0.01),
            ("zeta_R", 21.647, 0.03), ("zeta_o", 21.647, 0.03),
            ("delta_a1", 24.765, 0.02), ("delta_f1", 24.765, 0.02),
            ("bp1", 28.542, 0.02), ("b1A", 14.502, 0.02),
            ("b1", 31.139, 0.02), ("be1", 15.569, 0.02), ("bi1", 15.569, 0.02),
            ("Re21", 96.602, 0.02), ("Ri21", 68.235, 0.02),
            ("rho_b", 82.820, 0.02),
            ("beta_e21", 32.362, 0.03), ("beta_i21", 7.101, 0.1),
            ("beta_e1", 52.576, 0.03), ("beta_i1", 36.388, 0.05),
            ("beta_e2", 31.334, 0.03), ("beta_i2", 8.159, 0.1),
            ("hae1", 5.841, 0.02), ("hae2", 2.215, 0.02), ("hfe1", 3.222, 0.02), ("hfe2", 6.847, 0.02),
            ("he1", 9.063, 0.02), ("he2", 9.063, 0.02),
            ("hai1", 5.840, 0.02), ("hai2", 2.215, 0.02), ("hfi1", 3.223, 0.02), ("hfi2", 6.847, 0.02),
            ("hi1", 9.063, 0.02), ("hi2", 9.063, 0.02),
            ("alpha_n", 20.0, 0.002),
            ("xsm1", 0.040, 0.1), ("smn1", 7.969, 0.02), ("xsm2", -0.060, 0.1), ("smn2", 4.524, 0.02),
            ("smt1", 11.267, 0.02), ("smt2", 4.846, 0.02),
            ("dmn1", 101.047, 0.02), ("dmn2", 367.179, 0.02),
            ("smnc1", 7.961, 0.02), ("smnc2", 4.524, 0.02),
            ("hamc1", 5.982, 0.02), ("hamc2", 2.221, 0.02),
            ("Re1", 76.755, 0.02), ("Ri1", 45.617, 0.02), ("de1", 64.301, 0.02), ("di1", 38.215, 0.02),
            ("dae1", 74.909, 0.02), ("dae2", 171.907, 0.01), ("dfe1", 58.450, 0.02), ("dfe2", 163.738, 0.01),
            ("dai1", 48.821, 0.02), ("dai2", 125.487, 0.02), ("dfi1", 32.362, 0.03), ("dfi2", 117.318, 0.02),
            ("txo1", 79.869, 0.03), ("txo2", 29.077, 0.03), ("txi1", 51.594, 0.03), ("txi2", 17.359, 0.03),
            ("ht1", 9.063, 0.02),
        };

        var (pass, fail) = RunChecks(inputs, "=== ISO 23509:2016 Annex F.4 self-test (hypoid, Method 2/Oerlikon, face hobbing) ===", checks);
        Console.WriteLine($"RESULT (Sample 3): {pass} passed, {fail} failed.");
        Console.WriteLine();
    }

    public static void RunSample4Check()
    {
        // ISO 23509:2016, Annex F.5: hypoid gear pair (z1=12, z2=49, a=40mm), Method 3
        // (Klingelnberg), face hobbing. Data type I (given directly, unlike Samples 1-3).
        // Every value below is taken directly from Tables F.10-F.12 and Formulae
        // (F.419)-(F.575).
        var inputs = new Iso23509Inputs
        {
            Method = PitchConeMethod.Method3,
            Process = ManufacturingProcess.FaceHobbing,
            Sigma = 90.0,
            a = 40.0,
            z1 = 12,
            z2 = 49,
            de2 = 400.0,
            b2 = 60.0,
            beta_m2 = 30.0,
            rc0 = 135.0,
            z0 = 5,

            DataType = InputDataType.TypeI,
            alpha_dD = 19.0,
            alpha_dC = 21.0,
            f_alim = 0.0,
            x_hm1 = 0.2,
            k_hap = 1.0,
            k_hfp = 1.25,
            x_smn = 0.031,

            Backlash = BacklashMode.OuterTransverse_jet2,
            BacklashValue = 0.0,

            ThetaSource = ThetaA2Source.Manual,
            theta_a2 = 0.0,
            theta_f2 = 0.0,

            CheckUndercut = false,
        };

        var checks = new (string, double, double)[]
        {
            ("u", 4.083, 0.002),
            ("Rm1", 159.10, 0.03), ("Rm2", 181.074, 0.02),
            ("beta_m1", 42.922, 0.02), ("beta_m2", 30.0, 0.002), ("c_be2", 0.5, 0.002),
            ("dm1", 99.38, 0.02), ("dm2", 343.151, 0.02),
            ("zeta_m", 12.265, 0.02), ("zeta_mp", 12.922, 0.02), ("ap", 40.492, 0.02), ("m_mn", 6.065, 0.02),
            ("alpha_lim", -1.731, 0.03), ("alpha_nD", 19.0, 0.002), ("alpha_nC", 21.0, 0.002),
            ("alpha_eD", 20.731, 0.01), ("alpha_eC", 19.269, 0.01),
            ("Re2", 211.072, 0.02), ("Ri2", 151.072, 0.02), ("de2", 400.0, 0.002), ("di2", 286.295, 0.02),
            ("met2", 8.163, 0.02), ("be2", 29.998, 0.02), ("bi2", 30.002, 0.02),
            ("tzm2", 49.561, 0.03), ("tzm1", 167.660, 0.03), ("tz1", -16.530, 0.1), ("tz2", 8.315, 0.1),
            ("hmw", 12.130, 0.02), ("ham2", 4.852, 0.02), ("hfm2", 8.794, 0.02), ("ham1", 7.278, 0.02),
            ("hfm1", 6.368, 0.02), ("c", 1.516, 0.02), ("hm", 13.646, 0.02),
            ("delta_a2", 71.360, 0.01), ("delta_f2", 71.360, 0.01),
            ("zeta_R", 12.265, 0.03), ("zeta_o", 12.265, 0.03),
            ("delta_a1", 18.200, 0.02), ("delta_f1", 18.200, 0.02),
            ("bp1", 61.607, 0.02), ("b1A", 30.944, 0.02),
            ("b1", 66.000, 0.02), ("bx", 2.196, 0.1), ("bi1", 33.140, 0.03), ("be1", 32.860, 0.03),
            ("Re21", 213.228, 0.02), ("Ri21", 148.957, 0.02),
            ("rho_b", 161.778, 0.02),
            ("beta_e21", 40.675, 0.03), ("beta_i21", 18.639, 0.05),
            ("beta_e1", 51.622, 0.03), ("beta_i1", 34.412, 0.03),
            ("beta_e2", 39.966, 0.03), ("beta_i2", 19.426, 0.05),
            ("hae1", 7.278, 0.02), ("hae2", 4.852, 0.02), ("hfe1", 6.368, 0.02), ("hfe2", 8.794, 0.02),
            ("he1", 13.646, 0.02), ("he2", 13.646, 0.02),
            ("hai1", 7.278, 0.02), ("hai2", 4.852, 0.02), ("hfi1", 6.368, 0.02), ("hfi2", 8.794, 0.02),
            ("hi1", 13.646, 0.02), ("hi2", 13.646, 0.02),
            ("alpha_n", 20.0, 0.002),
            ("xsm1", 0.031, 0.1), ("smn1", 10.786, 0.02), ("xsm2", -0.031, 0.1), ("smn2", 8.268, 0.02),
            ("smt1", 14.729, 0.02), ("smt2", 9.547, 0.02),
            ("dmn1", 177.158, 0.02), ("dmn2", 1377.740, 0.02),
            ("smnc1", 10.779, 0.02), ("smnc2", 8.268, 0.02),
            ("hamc1", 7.434, 0.02), ("hamc2", 4.855, 0.02),
            ("Re1", 191.947, 0.02), ("Ri1", 125.947, 0.02), ("de1", 119.903, 0.02), ("di1", 78.675, 0.02),
            ("dae1", 133.730, 0.02), ("dae2", 403.102, 0.01), ("dfe1", 107.804, 0.02), ("dfe2", 394.378, 0.01),
            ("dai1", 92.502, 0.02), ("dai2", 289.396, 0.02), ("dfi1", 66.576, 0.03), ("dfi2", 280.673, 0.02),
            ("txo1", 196.602, 0.03), ("txo2", 54.552, 0.03), ("txi1", 133.904, 0.03), ("txi2", 35.374, 0.03),
            ("ht1", 13.646, 0.02),
        };

        var (pass, fail) = RunChecks(inputs, "=== ISO 23509:2016 Annex F.5 self-test (hypoid, Method 3/Klingelnberg, face hobbing) ===", checks);
        Console.WriteLine($"RESULT (Sample 4): {pass} passed, {fail} failed.");
        Console.WriteLine();
    }
}
