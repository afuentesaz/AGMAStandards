using AGMA6134C21Calc.Models;

namespace AGMA6134C21Calc.Calc;

/// <summary>
/// Diagnostic-only harness (run with "--selftest") that checks the calculator's output
/// against the numeric worked example published in ANSI/AGMA 6134-C21, Annex C (Table C.1
/// input data, clauses C.3 through C.5). All inputs and expected results are taken directly
/// from the standard; the worm is 8620 steel at 58 HRC (not modeled - hardness is only used
/// for the clause 1.2 d) scope check) and the wormgear is C90700 bronze, centrifugally cast.
/// </summary>
public static class SelfTest
{
    public static void RunAnnexCCheck()
    {
        var inputs = new AgmaInputs
        {
            NormalModule = 4.0,
            z1 = 1,
            z2 = 30,
            LeadAngleWorm = 6.0,
            HelixAngleGear = 6.0,
            NormalPressureAngle = 20.0,
            WormSpeed = 1750.0,
            ActualFaceWidthGear = 27.94,
            RootDiameterGearThroat = 110.66,
            WormMaxRadialDeflection = 0.076,

            WormHeatTreatment = WormHeatTreat.Carburized,
            WormHardnessHRC = 58.0,
            GearCasting = GearCastingMethod.Centrifugal,

            Lubricant = OilType.PAO,

            CheckOverhungLoad = false,
            ManualServiceFactorOverride = true,
            ManualServiceFactor = 1.0,
            SpecifiedInputPower_kW = 1.0,
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
                Console.WriteLine($"MISSING  {symbol,-14} expected={expected}");
                fail++;
                return;
            }
            double actual = values.OrderBy(v => Math.Abs(v - expected)).First();
            double tol = Math.Max(Math.Abs(expected) * tolFraction, 1e-6);
            bool ok = Math.Abs(actual - expected) <= tol;
            Console.WriteLine($"{(ok ? "PASS" : "FAIL"),-4} {symbol,-14} expected={expected,14:0.00000} actual={actual,14:0.00000} diff%={100 * (actual - expected) / expected,8:0.00}");
            if (ok) pass++; else fail++;
        }

        Console.WriteLine("=== ANSI/AGMA 6134-C21 Annex C self-test ===");
        Console.WriteLine();

        // C.3 Preliminary calculations
        Check("beta1", 84.0, 0.0001);
        Check("d1", 38.2670, 0.001);
        Check("d2", 120.6609, 0.001);
        Check("px1", 12.64, 0.005);
        Check("a", 79.46, 0.001);
        Check("u", 30.0, 0.0001);
        Check("v1", 3.5, 0.01);
        Check("n2", 58.33, 0.001);
        Check("be2", 25.51, 0.001);
        Check("lf2", 26.88, 0.002);

        // C.4 Rating factor calculations
        Check("Ks", 1000.0, 0.001);
        Check("Km", 0.82362, 0.001);
        Check("Kv", 0.30713, 0.002);
        Check("Kn", 0.45298, 0.001);
        Check("mu", 0.02169, 0.02);

        // C.5 Rating calculations
        Check("FTw2", 4324.3, 0.01);
        Check("FFw2", 100.3, 0.01);
        Check("POw", 1.594, 0.01);
        Check("PFw", 0.352, 0.01);
        Check("PIw", 1.946, 0.01);
        Check("Tw2", 260.9, 0.01);
        Check("eta_z1-2", 0.818, 0.005);
        Check("eta_z2-1", 0.778, 0.005);
        Check("sigma_AB", 69.0, 0.0001);
        Check("FTB2", 12097.8, 0.01);
        Check("TB2", 729.9, 0.01);
        Check("PIB", 5.447, 0.01);

        Console.WriteLine();
        Console.WriteLine(warnings.Count > 0 ? "Warnings: " + string.Join("; ", warnings) : "No warnings.");
        Console.WriteLine();
        Console.WriteLine($"RESULT (Annex C): {pass} passed, {fail} failed.");
    }
}
