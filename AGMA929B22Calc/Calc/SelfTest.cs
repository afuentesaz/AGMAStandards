using AGMA929B22Calc.Models;

namespace AGMA929B22Calc.Calc;

/// <summary>
/// Diagnostic-only harness (run with "--selftest") that checks the calculator's output
/// against the numeric worked example published in AGMA 929-B22, Annex C
/// (spiral bevel, face milling / spread blade, generated wheel, completing).
/// </summary>
public static class SelfTest
{
    public static void RunAnnexCCheck()
    {
        var inputs = new AgmaInputs
        {
            CuttingMethod = CuttingMethod.FaceMilling,
            WheelGeneration = WheelGeneration.Generated,
            PinionProcess = MemberProcess.Completing,
            WheelProcess = MemberProcess.Completing,
            KeMethod = KeMethod.FaceMillingSpreadBlade,

            Sigma = 90.0,
            Offset_a = 0.0,
            z1 = 14,
            z2 = 39,
            m_mn = 3.21331,
            b1 = 25.4,
            b2 = 25.4,
            Re1 = 93.973,
            Re2 = 93.973,
            Rm1 = 81.273,
            Rm2 = 81.273,
            Delta1 = 19.7468,
            Delta2 = 70.2532,
            ThetaF1 = 2.2259,
            ThetaF2 = 6.4935,
            Clearance_c = 0.964,

            Alpha_cv1 = 20.0,
            Alpha_cx1 = -20.0,
            Beta_m2 = 35.0,
            h_am1 = 4.83648,
            h_am2 = 1.58937,
            h_fm1 = 2.55337,
            h_fm2 = 5.80048,
            s_mn1 = 6.2788,
            s_mn2 = 3.67226,

            r_c0 = 114.3,
            S_A1 = 0.0,
            S_A2 = 0.0,
            y_mutilation = 0.05,
            Delta_f = 0.20,
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
        void Check(string symbol, double expected, double tol = 0.001)
        {
            if (!byLabel.TryGetValue(symbol, out var values))
            {
                Console.WriteLine($"MISSING  {symbol,-14} expected={expected}");
                fail++;
                return;
            }
            // If logged more than once (e.g. inner/mean/outer), match against the closest.
            double actual = values.OrderBy(v => Math.Abs(v - expected)).First();
            bool ok = Math.Abs(actual - expected) <= tol;
            Console.WriteLine($"{(ok ? "PASS" : "FAIL"),-4} {symbol,-14} expected={expected,12:0.00000} actual={actual,12:0.00000} diff={actual - expected,10:0.00000}");
            if (ok) pass++; else fail++;
        }

        Console.WriteLine("=== AGMA 929-B22 Annex C self-test ===");
        Check("mu_m1", 0.50000);
        Check("mu_m2", 0.50000);
        Check("d_m1", 54.91847);
        Check("d_m2", 152.98746);
        Check("d_e1", 63.50022);
        Check("d_e2", 176.89381);
        Check("delta_f2", 63.75970);
        Check("Re21", 93.97300);
        Check("Ri21", 68.57300);
        Check("mu_e21", 0.00000);
        Check("mu_i21", 1.00000);
        Check("beta_e21", 36.84579);
        Check("beta_i21", 33.94556);
        Check("beta_e1", 36.84579);
        Check("beta_i1", 33.94556);
        Check("beta_e2", 36.84579);
        Check("beta_i2", 33.94556);
        Check("Rx2", 148.58600);
        Check("beta_x2", 50.28672);
        Check("Sigma_alpha", 40.00000);
        Check("p_n", 10.09492);
        Check("j_en", 0.15270);
        Check("Delta_c1", 0.16887);
        Check("Delta_c2", 0.16887);
        Check("W_m2", 2.20026);
        Check("k_E", 0.00000);
        Check("W_eff", 2.20026);
        Check("xi", 0.00000);
        Check("W_e2", 2.20026);
        Check("W_e1", 1.87263);
        Check("W_m1", 1.95742);
        Check("W_i2", 2.20026);
        Check("W_i1", 1.87892);
        Check("W_min2", 2.20026);
        Check("W_min1", 1.87263);
        Check("W_max2", 2.20026);
        Check("W_max1", 1.95742);
        Check("W_B2", 1.12513);
        Check("W_B1", 0.96132);
        Check("r_a0max1", 1.08727);
        Check("r_a0max2", 1.32122);
        Check("r_i1", 35.76936);
        Check("r_i2", 277.57864);
        Check("h_ai1", 3.39095);
        Check("h_ai2", 1.09573);
        Check("Delta_h_ai1", 0.01928);
        Check("Delta_h_ai2", 0.14959);
        Check("h_aiadj1", 3.41023);
        Check("h_aiadj2", 1.24532);
        Check("r_iadj1", 35.75008);
        Check("r_iadj2", 277.42906);
        Check("r_ai1", 39.16031);
        Check("r_ai2", 278.67438);
        Check("alpha_icv1", 19.91495);
        Check("alpha_icx1", 19.91495);
        Check("rho_icv1", 8.58158);
        Check("rho_icx1", 8.58158);
        Check("rho_icx2", 86.58306);
        Check("rho_icv2", 86.58306);
        Check("h_infcv1", 0.81059);
        Check("h_infcx1", 0.81059);
        Check("h_infcx2", 1.49757);
        Check("h_infcv2", 1.49757);
        Check("r_a0cv11", 1.23193);
        Check("r_a0cx11", 1.23193);
        Check("r_a0cx12", 2.27601);
        Check("r_a0cv12", 2.27601);
        Check("r_a011", 1.23193);
        Check("r_a012", 2.27601);
        Check("r_fincv1", 1.08727);
        Check("r_fincx1", 1.08727);
        Check("r_fincx2", 1.32122);
        Check("r_fincv2", 1.32122);
        Check("r_a021", 1.08727);
        Check("r_a022", 1.32122);
        Check("Delta_Wmin1", 0.91132);
        Check("Delta_Wmin2", 1.07513);
        Check("Omega_cv1", 0.06631);
        Check("Omega_cx1", 0.06631);
        Check("Omega_cx2", 0.07778);
        Check("Omega_cv2", 0.07778);
        Check("r_a0cx31", 1.92869);
        Check("r_a0cv31", 1.92869);
        Check("r_a0cx32", 2.20626);
        Check("r_a0cv32", 2.20626);
        Check("r_a031", 1.92869);
        Check("r_a032", 2.20626);

        // Top land at inner/mean/outer (logged three times each — check as a set).
        CheckSet(byLabel, "r_npt1", new[] { 35.76936, 43.47903, 52.67587 }, ref pass, ref fail);
        CheckSet(byLabel, "r_npt2", new[] { 277.57864, 337.40747, 408.77715 }, ref pass, ref fail);
        CheckSet(byLabel, "r_na1", new[] { 39.16031, 48.31550, 58.95787 }, ref pass, ref fail);
        CheckSet(byLabel, "r_na2", new[] { 278.67438, 338.99684, 410.86015 }, ref pass, ref fail);
        CheckSet(byLabel, "s_n1", new[] { 5.24748, 6.27880, 7.31241 }, ref pass, ref fail);
        CheckSet(byLabel, "s_n2", new[] { 3.25536, 3.67226, 3.92816 }, ref pass, ref fail);

        Console.WriteLine();
        Console.WriteLine("Top land results (the primary deliverable):");
        Check("s_ain1", 2.29128);
        Check("s_amn1", 1.83117);
        Check("s_aen1", 1.36288);
        Check("s_ain2", 2.45566);
        Check("s_amn2", 2.50681);
        Check("s_aen2", 2.39535);

        Console.WriteLine();
        Console.WriteLine(warnings.Count > 0 ? "Warnings: " + string.Join("; ", warnings) : "No warnings.");
        Console.WriteLine();
        Console.WriteLine($"RESULT (Annex C): {pass} passed, {fail} failed.");

        RunAnnexDCheck();
        RunAnnexECheck();
    }

    private static void RunAnnexDCheck()
    {
        // AGMA 929-B22, Annex D — hypoid example (1): face milling / spread blade,
        // NON-generated wheel, completing. Exercises the hypoid-offset and
        // non-generated-wheel code paths that Annex C's spiral-bevel example does not.
        var inputs = new AgmaInputs
        {
            CuttingMethod = CuttingMethod.FaceMilling,
            WheelGeneration = WheelGeneration.NonGenerated,
            PinionProcess = MemberProcess.Completing,
            WheelProcess = MemberProcess.Completing,
            KeMethod = KeMethod.FaceMillingSpreadBlade,

            Sigma = 90.0,
            Offset_a = 38.10000,
            z1 = 11,
            z2 = 45,
            m_mn = 4.49574,
            b1 = 46.71200,
            b2 = 40.64000,
            Re1 = 151.29400,
            Re2 = 143.57000,
            Rm1 = 127.82600,
            Rm2 = 123.10500,
            Delta1 = 16.86930,
            Delta2 = 72.31850,
            ThetaF1 = 1.53490,
            ThetaF2 = 4.62770,
            Clearance_c = 1.77781,

            Alpha_cv1 = 18.14480,
            Alpha_cx1 = -21.85520,
            Beta_m2 = 30.41010,
            h_am1 = 7.27566,
            h_am2 = 1.53072,
            h_fm1 = 3.18216,
            h_fm2 = 9.05347,
            s_mn1 = 9.11957,
            s_mn2 = 4.85460,

            r_c0 = 114.30000,
            Alpha_BX2 = 25.00000,
            S_A1 = 0.0,
            S_A2 = 0.0,
            y_mutilation = 0.05,
            Delta_f = 0.20,
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
        void Check(string symbol, double expected, double tol = 0.001)
        {
            if (!byLabel.TryGetValue(symbol, out var values))
            {
                Console.WriteLine($"MISSING  {symbol,-14} expected={expected}");
                fail++;
                return;
            }
            double actual = values.OrderBy(v => Math.Abs(v - expected)).First();
            bool ok = Math.Abs(actual - expected) <= tol;
            Console.WriteLine($"{(ok ? "PASS" : "FAIL"),-4} {symbol,-14} expected={expected,12:0.00000} actual={actual,12:0.00000} diff={actual - expected,10:0.00000}");
            if (ok) pass++; else fail++;
        }

        Console.WriteLine();
        Console.WriteLine("=== AGMA 929-B22 Annex D (hypoid, non-generated) self-test ===");
        Check("mu_m1", 0.50240);
        Check("mu_m2", 0.50357);
        Check("d_m1", 74.18752);
        Check("d_m2", 234.57894);
        Check("d_e1", 87.80785);
        Check("d_e2", 273.57539);
        Check("delta_f2", 67.69080);
        Check("zeta_m", 17.16989);
        Check("zeta_mp", 17.96767);
        Check("a_p", 37.97546);
        Check("Re21", 145.60856);
        Check("Ri21", 101.24881);
        Check("mu_e21", -0.05016);
        Check("mu_i21", 1.04137);
        Check("beta_e21", 37.56227);
        Check("beta_i21", 23.80328);
        Check("beta_e1", 52.68011);
        Check("beta_i1", 45.83196);
        Check("beta_e2", 36.89265);
        Check("beta_i2", 24.30501);
        Check("Rx2", 164.43491);
        Check("beta_x2", 44.03581);
        Check("Delta_alpha2", 6.85520);
        Check("Sigma_alpha", 40.00000);
        Check("p_n", 14.12377);
        Check("j_en", 0.15204);
        Check("Delta_c1", 0.17667);
        Check("Delta_c2", 0.18168);
        Check("W_m2", 2.71949);
        Check("k_E", 0.00000);
        Check("W_eff", 2.71949);
        Check("xi", 0.00000);
        Check("W_e2", 2.66700);
        Check("W_e1", 2.68428);
        Check("W_m1", 2.68503);
        Check("W_i2", 2.67661);
        Check("W_i1", 2.99880);
        Check("W_min2", 2.66700);
        Check("W_min1", 2.68428);
        Check("W_max2", 2.67661);
        Check("W_max1", 2.99880);
        Check("W_B2", 1.35850);
        Check("W_B1", 1.36714);
        Check("r_a0max1", 1.61071);
        Check("r_a0max2", 1.59878);
        Check("r_i1", 65.32326);
        Check("r_i2", 388.73823);
        Check("h_ai1", 5.17541);
        Check("h_ai2", 0.78152);
        Check("Delta_h_ai1", 0.01308);
        Check("Delta_h_ai2", 0.16395);
        Check("h_aiadj1", 5.18849);
        Check("h_aiadj2", 0.94547);
        Check("r_iadj1", 65.31018);
        Check("r_iadj2", 388.57429);
        Check("r_ai1", 70.49868);
        Check("r_ai2", 389.51975);
        Check("r_bicv1", 62.07490);
        Check("r_bicx1", 60.62832);
        Check("r_bicx2", 369.40726);
        Check("r_bicv2", 360.79869);
        Check("alpha_icv1", 18.10975);
        Check("alpha_icx1", 21.82657);
        Check("rho_icv1", 17.53742);
        Check("rho_icx1", 21.95453);
        Check("rho_icx2", 107.66652);
        Check("rho_icv2", 132.77756);
        Check("h_infcv1", 1.60114);
        Check("h_infcx1", 1.60114);
        Check("h_infcx2", 2.78202);
        Check("h_infcv2", 2.51028);
        Check("r_a0cv11", 2.32528);
        Check("r_a0cx11", 2.55066);
        Check("r_a0cx12", 4.04023);
        Check("r_a0cv12", 3.99893);
        Check("r_a011", 2.32528);
        Check("r_a012", 3.99893);
        Check("r_fincv1", 1.61071);
        Check("r_fincx1", 1.72565);
        Check("r_fincx2", 1.59878);
        Check("r_fincv2", 1.71287);
        Check("r_a021", 1.61071);
        Check("r_a022", 1.59878);
        Check("Delta_Wmin1", 1.31714);
        Check("Delta_Wmin2", 1.30850);
        Check("Omega_cv1", 0.09794);
        Check("Omega_cx1", 0.09158);
        Check("Omega_cx2", 0.09732);
        Check("Omega_cv2", 0.09100);
        Check("r_a0cx31", 2.71829);
        Check("r_a0cv31", 2.50898);
        Check("r_a0cx32", 2.70340);
        Check("r_a0cv32", 2.49514);
        Check("r_a031", 2.50898);
        Check("r_a032", 2.49514);

        CheckSet(byLabel, "r_npt1", new[] { 65.32326, 87.85849, 124.81912 }, ref pass, ref fail);
        CheckSet(byLabel, "r_npt2", new[] { 388.73823, 519.20125, 704.11671 }, ref pass, ref fail);
        CheckSet(byLabel, "r_na1", new[] { 70.28643, 95.13415, 133.17980 }, ref pass, ref fail);
        CheckSet(byLabel, "r_na2", new[] { 383.56282, 511.92559, 695.78654 }, ref pass, ref fail);
        CheckSet(byLabel, "alpha_acx2", new[] { 15.61448, 15.46831, 15.91919 }, ref pass, ref fail);
        CheckSet(byLabel, "alpha_acv2", new[] { 19.83885, 19.72583, 20.07578 }, ref pass, ref fail);
        CheckSet(byLabel, "s_n1", new[] { 7.46147, 9.11941, 9.89392 }, ref pass, ref fail);
        CheckSet(byLabel, "s_n2", new[] { 4.73512, 4.85460, 5.24114 }, ref pass, ref fail);

        Console.WriteLine();
        Console.WriteLine("Top land results (the primary deliverable):");
        Check("s_ain1", 2.93349);
        Check("s_amn1", 2.26074);
        Check("s_aen1", 2.28484);
        Check("s_ain2", 4.13018);
        Check("s_amn2", 3.83110);
        Check("s_aen2", 3.80060);

        Console.WriteLine();
        Console.WriteLine(warnings.Count > 0 ? "Warnings: " + string.Join("; ", warnings) : "No warnings.");
        Console.WriteLine();
        Console.WriteLine($"RESULT (Annex D): {pass} passed, {fail} failed.");
    }

    private static void RunAnnexECheck()
    {
        // AGMA 929-B22, Annex E — hypoid example (2): face hobbing (Gleason), non-generated
        // wheel, completing. This is the only one of the three published examples that also
        // works the pinion tip chamfer (4.8, Eq 189-215) all the way through, so it is the
        // first real validation of that part of the calculator.
        var inputs = new AgmaInputs
        {
            CuttingMethod = CuttingMethod.FaceHobbing,
            WheelGeneration = WheelGeneration.NonGenerated,
            PinionProcess = MemberProcess.Completing,
            WheelProcess = MemberProcess.Completing,
            KeMethod = KeMethod.FaceHobbingGleasonOrKlingelnberg,

            Sigma = 70.00000,
            Offset_a = 25.40000,
            z1 = 13,
            z2 = 44,
            m_mn = 4.04412,
            b1 = 38.10200,
            b2 = 35.00000,
            Re1 = 118.74600,
            Re2 = 142.87900,
            Rm1 = 99.56800,
            Rm2 = 125.25100,
            Delta1 = 18.80470,
            Delta2 = 50.34310,
            ThetaF1 = 0.00000,
            ThetaF2 = 0.00000,
            Clearance_c = 1.26400,

            Alpha_cv1 = 18.98480,
            Alpha_cx1 = -21.01520,
            Beta_m2 = 22.68220,
            h_am1 = 6.71300,
            h_am2 = 1.37500,
            h_fm1 = 2.63900,
            h_fm2 = 7.97700,
            s_mn1 = 8.98499,
            s_mn2 = 3.56128,

            r_c0 = 76.00000,
            z0 = 7,
            Alpha_BX2 = 25.00000,
            S_A1 = 0.0,
            S_A2 = 0.0,
            y_mutilation = 0.05,
            Delta_f = 0.20,

            CalculatePinionChamfer = true,
            DesiredChamferTopLand = 0.80000,
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
        void Check(string symbol, double expected, double tol = 0.001)
        {
            if (!byLabel.TryGetValue(symbol, out var values))
            {
                Console.WriteLine($"MISSING  {symbol,-14} expected={expected}");
                fail++;
                return;
            }
            double actual = values.OrderBy(v => Math.Abs(v - expected)).First();
            bool ok = Math.Abs(actual - expected) <= tol;
            Console.WriteLine($"{(ok ? "PASS" : "FAIL"),-4} {symbol,-14} expected={expected,12:0.00000} actual={actual,12:0.00000} diff={actual - expected,10:0.00000}");
            if (ok) pass++; else fail++;
        }

        Console.WriteLine();
        Console.WriteLine("=== AGMA 929-B22 Annex E (hypoid, face hobbing, non-generated) self-test ===");
        Check("mu_m1", 0.50333);
        Check("mu_m2", 0.50366);
        Check("d_m1", 64.19016);
        Check("d_m2", 192.85644);
        Check("d_e1", 76.55397);
        Check("d_e2", 219.99932);
        Check("delta_f2", 50.34310);
        Check("zeta_m", 12.42339);
        Check("zeta_mp", 12.33101);
        Check("a_p", 26.74849);
        Check("Re21", 144.04480);
        Check("Ri21", 106.84004);
        Check("mu_e21", -0.03331);
        Check("mu_i21", 1.02968);
        Check("nu", 10.73356);
        Check("rho_P0", 132.37165);
        Check("rho_b", 117.92775);
        Check("beta_e21", 35.21205);
        Check("beta_i21", 8.63180);
        Check("beta_e1", 45.91374);
        Check("beta_i1", 23.13063);
        Check("beta_e2", 34.46726);
        Check("beta_i2", 9.49491);
        Check("Rx2", 139.55017);
        Check("beta_x2", 32.32203);
        Check("Delta_alpha2", 6.01520);
        Check("Sigma_alpha", 40.00000);
        Check("p_n", 12.70496);
        Check("j_en", 0.15200);
        Check("Delta_c1", 0.19950);
        Check("Delta_c2", 0.20075);
        Check("W_m2", 3.33553);
        Check("k_E", 1.00000);
        Check("W_eff", -3.01695);
        Check("xi", 0.00000);
        Check("W_e2", 3.45756);
        Check("W_e1", 2.21793);
        Check("W_m1", 1.79825);
        Check("W_i2", 2.83123);
        Check("W_i1", 1.44671);
        Check("W_x2", 3.46477);
        Check("W_x1", 2.09666);
        Check("W_min2", 2.83123);
        Check("W_min1", 1.44671);
        Check("W_max2", 3.46477);
        Check("W_max1", 2.21793);
        Check("W_B2", 1.44061);
        Check("W_B1", 0.74836);
        Check("r_a0max1", 0.76855);
        Check("r_a0max2", 1.73879);
        Check("r_i1", 32.47169);
        Check("r_i2", 133.78035);
        Check("h_ai1", 6.45705);
        Check("h_ai2", 1.37500);
        Check("Delta_h_ai1", 0.03334);
        Check("Delta_h_ai2", 0.16637);
        Check("h_aiadj1", 6.49040);
        Check("h_aiadj2", 1.54137);
        Check("r_iadj1", 32.43834);
        Check("r_iadj2", 133.61399);
        Check("r_ai1", 38.92874);
        Check("r_ai2", 135.15535);
        Check("r_bicv1", 30.70539);
        Check("r_bicx1", 30.31184);
        Check("r_bicx2", 126.50336);
        Check("r_bicv2", 124.88200);
        Check("alpha_icv1", 18.81285);
        Check("alpha_icx1", 20.86135);
        Check("rho_icv1", 5.96809);
        Check("rho_icx1", 7.44639);
        Check("rho_icx2", 29.61862);
        Check("rho_icv2", 34.70633);
        Check("h_infcv1", 1.06450);
        Check("h_infcx1", 1.06450);
        Check("h_infcx2", 3.19835);
        Check("h_infcv2", 2.96244);
        Check("r_a0cv11", 1.57777);
        Check("r_a0cx11", 1.65969);
        Check("r_a0cx12", 4.74052);
        Check("r_a0cv12", 4.61882);
        Check("r_a011", 1.57777);
        Check("r_a012", 4.61882);
        Check("r_fincv1", 0.76855);
        Check("r_fincx1", 0.79809);
        Check("r_fincx2", 1.73879);
        Check("r_fincv2", 1.80562);
        Check("r_a021", 0.76855);
        Check("r_a022", 1.73879);
        Check("Delta_Wmin1", 0.69836, 0.002);
        Check("Delta_Wmin2", 1.39061, 0.002);
        Check("Omega_cv1", 0.05233);
        Check("Omega_cx1", 0.05048);
        Check("Omega_cx2", 0.10172);
        Check("Omega_cv2", 0.09805);
        Check("r_a0cx31", 1.59826);
        Check("r_a0cv31", 1.52635);
        Check("r_a0cx32", 2.79312);
        Check("r_a0cv32", 2.67374);
        Check("r_a031", 1.52635);
        Check("r_a032", 2.67374);

        CheckSet(byLabel, "r_npt1", new[] { 32.47169, 50.54432, 83.53454 }, ref pass, ref fail);
        CheckSet(byLabel, "r_npt2", new[] { 133.78035, 177.48976, 253.57927 }, ref pass, ref fail);
        CheckSet(byLabel, "r_na1", new[] { 38.89874, 57.25732, 89.83559 }, ref pass, ref fail);
        CheckSet(byLabel, "r_na2", new[] { 127.32330, 170.77676, 247.22397 }, ref pass, ref fail);
        CheckSet(byLabel, "alpha_acx2", new[] { 6.50591, 10.64972, 14.09030 }, ref pass, ref fail);
        CheckSet(byLabel, "alpha_acv2", new[] { 11.23805, 14.02754, 16.76773 }, ref pass, ref fail);
        CheckSet(byLabel, "s_n1", new[] { 8.24425, 8.98402, 8.79869 }, ref pass, ref fail);
        CheckSet(byLabel, "s_n2", new[] { 3.24393, 3.56128, 3.94449 }, ref pass, ref fail);

        Console.WriteLine();
        Console.WriteLine("Top land results:");
        Check("s_ain1", 0.13975);
        Check("s_amn1", 1.52334);
        Check("s_aen1", 2.72862);
        Check("s_ain2", 2.24266);
        Check("s_amn2", 2.56000);
        Check("s_aen2", 2.94322);

        Console.WriteLine();
        Console.WriteLine("Pinion tip chamfer (4.8, Eq 189-215):");
        Check("R_c1", 89.59727);
        Check("t_c1", 8.95327);
        Check("r_nipt1", 32.47169);
        Check("r_nipt21", 131.85670);
        Check("r_bicv1(chamfer)", 30.70539);
        Check("r_bicx1(chamfer)", 30.31184);
        Check("r_bicx21", 124.68434);
        Check("r_bicv21", 123.08629);
        Check("s_ni1", 8.24425);
        Check("s_ni21", 3.22338);
        Check("h_aci1", 6.09248, 0.005);
        Check("s_anci1", 0.79970, 0.005);
        Check("h_ac1", 6.62892, 0.005);
        Check("delta_ac1", 22.23354, 0.02);

        Console.WriteLine();
        Console.WriteLine(warnings.Count > 0 ? "Warnings: " + string.Join("; ", warnings) : "No warnings.");
        Console.WriteLine();
        Console.WriteLine($"RESULT (Annex E): {pass} passed, {fail} failed.");
    }

    private static void CheckSet(Dictionary<string, List<double>> byLabel, string symbol, double[] expectedSet, ref int pass, ref int fail)
    {
        if (!byLabel.TryGetValue(symbol, out var values) || values.Count < expectedSet.Length)
        {
            Console.WriteLine($"MISSING  {symbol,-14} (need {expectedSet.Length} occurrences)");
            fail++;
            return;
        }
        var remaining = new List<double>(values);
        foreach (var exp in expectedSet)
        {
            double actual = remaining.OrderBy(v => Math.Abs(v - exp)).First();
            remaining.Remove(actual);
            bool ok = Math.Abs(actual - exp) <= 0.001;
            Console.WriteLine($"{(ok ? "PASS" : "FAIL"),-4} {symbol,-14} expected={exp,12:0.00000} actual={actual,12:0.00000} diff={actual - exp,10:0.00000}");
            if (ok) pass++; else fail++;
        }
    }
}
