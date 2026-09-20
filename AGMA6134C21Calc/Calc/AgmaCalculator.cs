using AGMA6134C21Calc.Models;
using static AGMA6134C21Calc.Calc.MathUtil;

namespace AGMA6134C21Calc.Calc;

/// <summary>
/// Implements ANSI/AGMA 6134-C21, clauses 4 (preliminary calculations), 5 (rating factors),
/// 6 (wear and bending rating), 7 (overhung shaft loads), 10.1 (equivalent input power) and
/// 12 (self-locking). Equation and table numbers in comments refer to the published standard.
/// </summary>
public class AgmaCalculator
{
    private readonly AgmaInputs In;
    private readonly CalcTrace T = new();

    public AgmaCalculator(AgmaInputs inputs) => In = inputs;

    public (CalcTrace trace, List<string> warnings) Calculate()
    {
        var warnings = new List<string>();

        // ============================================================= Clause 4: Preliminary calculations
        T.Section("Clause 4: Preliminary calculations");

        double gamma1 = In.LeadAngleWorm;
        double beta1 = T.R("beta1", "Eq(4)", "Helix angle of the worm (complement of the lead angle)", 90.0 - gamma1, "deg");
        double beta2 = In.HelixAngleGear;

        double d1 = T.R("d1", "Eq(6)", "Pitch diameter of the worm", In.NormalModule * In.z1 / Cos(beta1), "mm");
        double d2 = T.R("d2", "Eq(6)", "Pitch diameter of the wormgear", In.NormalModule * In.z2 / Cos(beta2), "mm");

        double px1 = T.R("px1", "Eq(7)", "Axial pitch of the worm", Math.PI * In.NormalModule / Sin(beta1), "mm");

        double a = T.R("a", "Eq(1)", "Mesh center distance", (d1 + d2) / 2.0, "mm");

        // Scope checks, clause 1.2
        double aLow = 0.5 * Math.Pow(a, 0.875);
        double aHigh = 0.935 * Math.Pow(a, 0.875);
        T.Note($"Scope check 1.2 g) (Eq 2): {aLow:0.00} <= d1 ({d1:0.0000}) <= {aHigh:0.00} -> " + (d1 >= aLow && d1 <= aHigh ? "OK" : "OUT OF SCOPE"));
        if (d1 < aLow || d1 > aHigh)
            warnings.Add($"Worm pitch diameter d1={d1:0.0000} mm is outside the scope range [{aLow:0.00}, {aHigh:0.00}] mm of clause 1.2 g) (Eq 2) for a={a:0.00} mm.");

        bool deflectionOk = In.WormMaxRadialDeflection <= 0.025 * Math.Sqrt(px1);
        T.Note($"Scope check 1.2 h) (Eq 3): dy1max ({In.WormMaxRadialDeflection:0.000}) <= 0.025*sqrt(px1) ({0.025 * Math.Sqrt(px1):0.000}) -> " + (deflectionOk ? "OK" : "OUT OF SCOPE"));
        if (!deflectionOk)
            warnings.Add($"Worm radial deflection dy1max={In.WormMaxRadialDeflection:0.000} mm exceeds the clause 1.2 h) (Eq 3) limit of {0.025 * Math.Sqrt(px1):0.000} mm.");

        double u = T.R("u", "Eq(8)", "Gear ratio", (double)In.z2 / In.z1, "");
        if (u < 3.0)
            warnings.Add($"Gear ratio u={u:0.00} is below the clause 1.2 b) minimum of 3.0 - this standard does not apply.");

        double v1 = T.R("v1", "Eq(9)", "Sliding velocity of the worm", Math.PI * In.WormSpeed * d1 / (60000.0 * Cos(gamma1)), "m/s");
        if (v1 > 30.0)
            warnings.Add($"Sliding velocity v1={v1:0.00} m/s exceeds the 30 m/s limit stated in clause 1.1.");

        if (In.WormSpeed > 3600.0)
            warnings.Add($"Worm speed n1={In.WormSpeed:0} rpm exceeds the 3600 rpm limit stated in clause 1.1.");

        double n2 = T.R("n2", "Eq(10)", "Rotational speed of the wormgear", In.WormSpeed / u, "rpm");
        if (n2 >= 3600.0)
            warnings.Add($"Wormgear speed n2={n2:0.0} rpm is at or above the upper bound of the Table 6 gear speed factor.");

        double be2 = T.R("be2", "Eq(11)", "Effective face width of the wormgear", Math.Min(In.ActualFaceWidthGear, 2.0 * d1 / 3.0), "mm");

        double twoAminusDf2 = 2.0 * a - In.RootDiameterGearThroat;
        double lf2 = T.R("lf2", "Eq(12)", "Length of the wormgear tooth root", twoAminusDf2 * AsinRad(be2 / twoAminusDf2), "mm");

        double hardMin = In.WormHeatTreatment == WormHeatTreat.Carburized ? 58.0 : 50.0;
        double hardMax = In.WormHeatTreatment == WormHeatTreat.Carburized ? 62.0 : 62.0;
        bool hardOk = In.WormHardnessHRC >= hardMin && In.WormHardnessHRC <= hardMax;
        T.Note($"Scope check 1.2 d) (Table 1): worm hardness {In.WormHardnessHRC:0} HRC, {In.WormHeatTreatment} range [{hardMin:0},{hardMax:0}] HRC -> " + (hardOk ? "OK" : "OUT OF SCOPE"));
        if (!hardOk)
            warnings.Add($"Worm surface hardness {In.WormHardnessHRC:0} HRC is outside the Table 1 range [{hardMin:0},{hardMax:0}] HRC for {In.WormHeatTreatment}.");

        // ============================================================= Clause 5: Rating factors
        T.Section("Clause 5: Rating factors");

        double Kd = GearMaterialsFactorKd(In.GearCasting, d2);
        T.R("Kd", "Table 3", $"Materials factor based only on casting method ({In.GearCasting}) and pitch diameter", Kd, "");

        double Ks = T.R("Ks", "Eq(13)", "Materials factor", Math.Min(Kd, 720.0 + Math.Pow(a, 3) / 1580.24), "");

        double Km = RatioCorrectionFactorKm(u);
        T.R("Km", "Table 4", "Ratio correction factor", Km, "");

        double Kv = SlidingVelocityFactorKv(v1);
        T.R("Kv", "Table 5", "Sliding velocity factor", Kv, "");

        double Kn = GearSpeedFactorKn(n2);
        T.R("Kn", "Table 6", "Gear speed factor", Kn, "");

        var (zOilWear, zOilMu) = OilTypeFactors(In.Lubricant);
        T.R("Z_oil-wear", "Table 7", $"Oil type factor for wear ({In.Lubricant})", zOilWear, "");
        T.R("Z_oil-mu", "Table 7", $"Oil type factor for dynamic coefficient of friction ({In.Lubricant})", zOilMu, "");

        double mu = T.R("mu", "Eq(14)", "Dynamic coefficient of friction", zOilMu * (0.01087 + 0.13528 * Math.Exp(-1.43941 * Math.Pow(v1, 0.3475))), "");

        // ============================================================= Clause 6.1: Wear related ratings
        T.Section("Clause 6.1: Wear related ratings");

        double FTw2 = T.R("FTw2", "Eq(15)", "Allowable tangential load on the gear tooth due to wear",
            Ks * Km * Kv * be2 * zOilWear * Math.Pow(d2, 0.8) / 75.948, "N");

        double FFw2 = T.R("FFw2", "Eq(16)", "Friction force based on the allowable tangential load due to wear",
            mu * FTw2 / (Cos(gamma1) * Cos(In.NormalPressureAngle)), "N");

        double POw = T.R("POw", "Eq(17)", "Allowable output power based on tooth wear",
            Math.PI * FTw2 * d2 * n2 / 6.0e7, "kW");

        double PFw = T.R("PFw", "Eq(18)", "Power loss in the mesh due to frictional tooth wear",
            v1 * FFw2 / 1000.0, "kW");

        double PIw = T.R("PIw", "Eq(19)", "Input power rating based on wear", POw + PFw, "kW");

        double Tw2 = T.R("Tw2", "Eq(20)", "Allowable torque on the gear based on wear", FTw2 * d2 / 2000.0, "Nm");

        // ============================================================= Clause 6.2: Efficiency
        T.Section("Clause 6.2: Efficiency");

        double etaZ12 = T.R("eta_z1-2", "Eq(21)", "Forward drive efficiency with the worm driving",
            (Cos(In.NormalPressureAngle) - mu * Tan(gamma1)) / (Cos(In.NormalPressureAngle) + mu * Cot(gamma1)), "");

        double etaZ21 = T.R("eta_z2-1", "Eq(22)", "Back drive efficiency with the gear driving",
            (Cos(In.NormalPressureAngle) - mu * Cot(gamma1)) / (Cos(In.NormalPressureAngle) + mu * Tan(gamma1)), "");

        // ============================================================= Clause 6.3: Bending strength related ratings
        T.Section("Clause 6.3: Bending strength related ratings");

        double sigmaAB = T.R("sigma_AB", "Eq(23)", "Allowable wormgear bending stress (bronze)", 69.0, "N/mm2");

        double FTB2 = T.R("FTB2", "Eq(24)", "Allowable tangential load on the gear tooth due to bending",
            3.6 * Kn * px1 * lf2 * sigmaAB * Cos(gamma1) / Math.PI, "N");

        double TB2 = T.R("TB2", "Eq(25)", "Allowable torque on the gear tooth based on bending", FTB2 * d2 / 2000.0, "Nm");

        double PIB = T.R("PIB", "Eq(26)", "Input power rating for bending", TB2 * n2 * Math.PI / (30000.0 * etaZ12), "kW");

        // ============================================================= Clause 6: Overall rating
        T.Section("Clause 6: Overall rating (informative summary)");
        double ratedInputPower = Math.Min(PIw, PIB);
        T.R("P_I (rated)", "min(Eq19,Eq26)", PIw <= PIB ? "Governing rating: wear (Eq 19)" : "Governing rating: bending (Eq 26)", ratedInputPower, "kW");

        // ============================================================= Clause 7: Overhung load (optional)
        if (In.CheckOverhungLoad)
        {
            T.Section("Clause 7.1.4: Overhung shaft loads (optional, Eq 27)");

            double T1 = T.R("T1", "T=30000P/(pi n)", "Torque on the worm (input shaft), from the applied input power",
                In.AppliedInputPower_kW * 30000.0 / (Math.PI * In.WormSpeed), "Nm");
            double outputPower = In.AppliedInputPower_kW * etaZ12;
            double T2 = T.R("T2", "T=30000P/(pi n)", "Torque on the wormgear (output shaft), P1*eta_z1-2 through the mesh",
                outputPower * 30000.0 / (Math.PI * n2), "Nm");

            double Ko1 = In.WormShaftDrive == OverhungDriveType.OtherManualKo ? In.ManualKoWorm : OverhungLoadFactorKo(In.WormShaftDrive);
            T.R("KO1", "Table 8", $"Overhung load factor, worm shaft ({In.WormShaftDrive})", Ko1, "");
            double Ko2 = In.GearShaftDrive == OverhungDriveType.OtherManualKo ? In.ManualKoGear : OverhungLoadFactorKo(In.GearShaftDrive);
            T.R("KO2", "Table 8", $"Overhung load factor, wormgear shaft ({In.GearShaftDrive})", Ko2, "");

            if (In.SprocketDiameterWorm > 0)
            {
                T.R("FO1", "Eq(27)", "Overhung load of the worm shaft", 2000.0 * T1 * Ko1 / In.SprocketDiameterWorm, "N");
            }
            else
            {
                T.Note("Worm shaft overhung load skipped (pitch diameter of mounted element D1 = 0).");
            }

            if (In.SprocketDiameterGear > 0)
            {
                T.R("FO2", "Eq(27)", "Overhung load of the wormgear shaft", 2000.0 * T2 * Ko2 / In.SprocketDiameterGear, "N");
            }
            else
            {
                T.Note("Wormgear shaft overhung load skipped (pitch diameter of mounted element D2 = 0).");
            }
        }

        // ============================================================= Clause 10.1: Equivalent input power
        T.Section("Clause 10.1: Equivalent input power and service factor");

        double serviceFactor = In.ManualServiceFactorOverride
            ? In.ManualServiceFactor
            : ServiceFactorTableA1(In.Duration, In.ShockLoad, warnings);
        T.R("SF", In.ManualServiceFactorOverride ? "manual" : "Table A.1", "Service factor", serviceFactor, "");

        double equivalentInputPower = T.R("P_equiv", "Clause 10.1", "Equivalent input power (specified power x SF)",
            In.SpecifiedInputPower_kW * serviceFactor, "kW");

        if (equivalentInputPower > ratedInputPower)
            warnings.Add($"Equivalent input power ({equivalentInputPower:0.000} kW) exceeds the rated input power ({ratedInputPower:0.000} kW) - this reducer selection does not satisfy clause 10.1.");
        else
            T.Note($"Equivalent input power ({equivalentInputPower:0.000} kW) <= rated input power ({ratedInputPower:0.000} kW) -> selection satisfies clause 10.1.");

        // ============================================================= Clause 12: Self-locking
        T.Section("Clause 12: Self-locking");
        bool selfLocking = etaZ21 <= 0.0;
        T.Note(selfLocking
            ? $"Back drive efficiency eta_z2-1 = {etaZ21 * 100:0.0}% <= 0 -> the mesh may be self-locking under a static load (clause 12.2). Vibration can still unlock the mesh (clause 12.2/12.3); do not rely on this alone."
            : $"Back drive efficiency eta_z2-1 = {etaZ21 * 100:0.0}% > 0 -> the mesh is back-drivable and is NOT self-locking.");

        // ============================================================= Table 9: Lubricant viscosity (informative)
        T.Section("Clause 11.1: Recommended lubricant viscosity (Table 9, informative)");
        string isoVg = LubricantViscosityTable9(a, In.WormSpeed, In.AmbientBand);
        T.Note($"For a={a:0.0} mm, n1={In.WormSpeed:0} rpm, ambient {In.AmbientBand}: recommended lubricant viscosity = ISO VG {isoVg}.");

        return (T, warnings);
    }

    // ----------------------------------------------------------------- Table 3
    private static double GearMaterialsFactorKd(GearCastingMethod method, double d2) => method switch
    {
        GearCastingMethod.Sand => d2 <= 63.5 ? 1000.0 : 1859.1035 - 476.5454 * Math.Log10(d2),
        GearCastingMethod.StaticChillOrForged => d2 <= 203.0 ? 1000.0 : 2052.0114 - 455.8259 * Math.Log10(d2),
        GearCastingMethod.Centrifugal => d2 <= 635.0 ? 1000.0 : 1503.8103 - 179.7503 * Math.Log10(d2),
        _ => throw new ArgumentOutOfRangeException(nameof(method))
    };

    // ----------------------------------------------------------------- Table 4
    private static double RatioCorrectionFactorKm(double u)
    {
        if (u <= 20.0)
            return 0.303 - 33.0 * Math.Pow(u, -6) - 0.0112 * u + 0.4975 * Math.Log(Math.Pow(u, 0.4975));
        if (u <= 76.0)
            return 0.0107 * Math.Sqrt(-(u * u) + 56.0 * u + 5145.0);
        return 1.1483 - 0.00658 * u;
    }

    // ----------------------------------------------------------------- Table 5
    private static double SlidingVelocityFactorKv(double v1)
    {
        if (v1 <= 3.556)
            return 0.659 * Math.Exp(-0.21654 * v1);
        if (v1 <= 15.24)
            return 0.651968 * Math.Pow(v1, -0.571);
        return 1.098278 * Math.Pow(v1, -0.774);
    }

    // ----------------------------------------------------------------- Table 6
    private static double GearSpeedFactorKn(double n2)
    {
        if (n2 < 1.0)
            return 0.6260 + 0.2863 * Math.Exp(-2.4726 * n2);
        if (n2 < 200.0)
            return 1.0 / (1.3776 + 0.163 * Math.Pow(n2, 0.4003));
        return 1.0 / (1.9236 + 0.05128 * Math.Pow(n2, 0.5123));
    }

    // ----------------------------------------------------------------- Table 7
    private static (double zOilWear, double zOilMu) OilTypeFactors(OilType oil) => oil switch
    {
        OilType.Mineral => (1.0, 1.0),
        OilType.PAO => (1.1, 0.85),
        OilType.PAG => (1.21, 0.65),
        _ => throw new ArgumentOutOfRangeException(nameof(oil))
    };

    // ----------------------------------------------------------------- Table 8
    private static double OverhungLoadFactorKo(OverhungDriveType drive) => drive switch
    {
        OverhungDriveType.SingleOrMultipleChain => 1.00,
        OverhungDriveType.TimingBelt => 1.00,
        OverhungDriveType.CutPinionRunWithCutGear => 1.25,
        OverhungDriveType.SingleOrMultipleVeeBelt => 1.50,
        OverhungDriveType.FlatBelt => 2.50,
        OverhungDriveType.VariablePitchPulley => 3.50,
        _ => 1.00
    };

    // ----------------------------------------------------------------- Annex A, Table A.1
    private static double ServiceFactorTableA1(ServiceDuration duration, ShockLoadClass shock, List<string> warnings)
    {
        // rows: duration, columns: shock. "--" cells (unspecified) use 1.00 per the table's own note.
        double[,] table =
        {
            // Uniform, Moderate, Heavy, Extreme
            { 1.00, 1.00, 1.00, 1.25 }, // Occasional 1/2 hour ("--","--" -> 1.00)
            { 1.00, 1.00, 1.25, 2.50 }, // Less than 3 hours
            { 1.00, 1.25, 1.50, 1.70 }, // 3-10 hours
            { 1.25, 1.50, 1.75, 2.00 }, // Over 10 hours
        };
        int row = (int)duration;
        int col = (int)shock;

        if ((duration == ServiceDuration.Occasional0p5Hr) && (shock == ShockLoadClass.UniformLoad || shock == ShockLoadClass.ModerateShock))
            warnings.Add("Annex A Table A.1 leaves uniform/moderate-shock, occasional (<=1/2 hr/day) service unspecified; using SF=1.00 per the table's own note - agree a value with the manufacturer if this matters.");

        return table[row, col];
    }

    // ----------------------------------------------------------------- Table 9
    private static string LubricantViscosityTable9(double a_mm, double n1_rpm, AmbientTemperatureBand band)
    {
        if (band == AmbientTemperatureBand.Minus10CTo10C)
            return "460";

        if (a_mm <= 152.0) return "680";
        if (a_mm <= 305.0) return n1_rpm <= 450.0 ? "680" : "460";
        if (a_mm <= 457.0) return n1_rpm <= 300.0 ? "680" : "460";
        if (a_mm <= 610.0) return n1_rpm <= 250.0 ? "680" : "460";
        return n1_rpm <= 200.0 ? "680" : "460";
    }
}
