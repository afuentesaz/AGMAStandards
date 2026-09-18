using AGMA2003D19Calc.Models;
using static AGMA2003D19Calc.Calc.MathUtil;

namespace AGMA2003D19Calc.Calc;

/// <summary>
/// Implements every rating calculation in ANSI/AGMA 2003-D19, clauses 5 through 21 (SI/"M"
/// equations), using the pitting/bending geometry factors from <see cref="GeometryFactors"/>
/// (Annex C(M)) or manually supplied values. Equation numbers in comments refer to the
/// published standard.
/// </summary>
public class AgmaCalculator
{
    private readonly AgmaInputs In;
    private readonly CalcTrace T = new();

    public AgmaCalculator(AgmaInputs inputs) => In = inputs;

    public (CalcTrace trace, List<string> warnings) Calculate()
    {
        var warnings = new List<string>();
        bool spiral = In.GearType == GearType.SpiralBevel;

        // ============================================================= Clause 6: Pinion/gear torque
        T.Section("Clause 6: Pinion and gear torque");
        double n2 = T.R("n2", "", "Gear speed (n1 * z1/z2)", In.PinionSpeed_rpm * In.z1 / In.z2, "rpm");
        double T1 = T.R("T1", "Eq(9M)", "Pinion operating torque", 30000.0 * In.Power_kW / (Math.PI * In.PinionSpeed_rpm), "N.m");
        double T2 = T.R("T2", "", "Gear operating torque (= T1 * z2/z1)", T1 * (double)In.z2 / In.z1, "N.m");

        // ============================================================= Clause 10: Dynamic factor
        T.Section("Clause 10: Dynamic factor, Kv");
        double Kv;
        if (In.DynamicFactorMode == DynamicFactorMode.Manual)
        {
            Kv = T.R("Kv", "10.7/10.8", "Manual dynamic factor", In.ManualKv, "");
        }
        else
        {
            double vet = T.R("vet", "Eq(16M)", "Pitchline velocity at outer pitch diameter", 5.236e-5 * In.de1 * In.PinionSpeed_rpm, "m/s");
            double B = T.R("B", "Eq(15)", "Exponent", 0.25 * Math.Pow(12 - In.Qv, 0.667), "");
            double A = T.R("A", "Eq(14)", "Coefficient", 50 + 56 * (1.0 - B), "");
            Kv = T.R("Kv", "Eq(13M)", "Dynamic factor", Math.Pow(A / (A + Math.Sqrt(200 * vet)), -B), "");
            double vetMax = T.R("vet_max", "Eq(17M)", "Maximum recommended pitch line velocity for this Qv", Math.Pow(A + (In.Qv - 3), 2) / 200.0, "m/s");
            if (vet > vetMax)
                warnings.Add($"Pitch line velocity ({vet:0.##} m/s) exceeds the recommended maximum ({vetMax:0.##} m/s) for Qv={In.Qv} - select a higher Qv or verify by dynamic analysis (10.7).");
        }

        // ============================================================= Clause 11: Size factors
        T.Section("Clause 11: Size factors, Cs (Zx) and Ks (Yx)");
        double Cs = In.b < 12.7 ? 0.5 : In.b > 79.8 ? 0.83 : 0.00492 * In.b + 0.4375;
        Cs = T.R("Cs", "Eq(18M)", "Size factor for pitting resistance", Cs, "");
        double Ks = In.m_et < 1.6 ? 0.5 : 0.4867 + 0.008399 * In.m_et;
        Ks = T.R("Ks", "Eq(19M)", "Size factor for bending strength", Ks, "");

        // ============================================================= Clause 12: Load distribution factor
        T.Section("Clause 12: Load distribution factor, Km (KHβ)");
        double Km;
        if (In.ManualKmOverride)
        {
            Km = T.R("Km", "12 (manual)", "Manually supplied load distribution factor", In.ManualKm, "");
        }
        else
        {
            double Kmb = In.Mounting switch
            {
                MountingCondition.BothMembersStraddleMounted => 1.00,
                MountingCondition.OneMemberStraddleMounted => 1.10,
                _ => 1.25
            };
            T.R("Kmb", "Figure 4", "Load distribution modifier", Kmb, "");
            if (In.b > 356.0)
                warnings.Add($"Net face width ({In.b:0.#} mm) exceeds the 356 mm (14 in) validity limit of Eq 20M / Figure 4.");
            Km = T.R("Km", "Eq(20M)", "Load distribution factor", Kmb + 5.6e-6 * In.b * In.b, "");
        }

        // ============================================================= Clause 13: Crowning factor
        T.Section("Clause 13: Crowning factor, Cxc (Zxc)");
        double Cxc = In.ProperlyCrowned ? 1.5 : In.CxcManual;
        Cxc = T.R("Cxc", "Clause 13", In.ProperlyCrowned ? "Crowning factor (properly crowned teeth)" : "Crowning factor (non-crowned teeth)", Cxc, "");
        if (!In.ProperlyCrowned && In.CxcManual < 2.0)
            warnings.Add("Non-crowned teeth should use Cxc >= 2.0 per clause 13.");

        // ============================================================= Clause 14: Lengthwise curvature factor
        T.Section("Clause 14: Lengthwise curvature factor, Kx (Yβ)");
        double Rm = In.Re - 0.5 * In.b;
        double Kx;
        if (!spiral)
        {
            Kx = T.R("Kx", "Eq(22)", "Lengthwise curvature factor (straight/Zerol bevel)", 1.0, "");
        }
        else
        {
            double q = T.R("q", "Eq(23M)", "Exponent", 0.279 / Math.Log10(Sin(In.Beta_m)), "");
            double kxRaw = 0.211 * Math.Pow(In.CutterRadius / Rm, q) + 0.789;
            T.R("Kx (raw)", "Eq(21M)", "Lengthwise curvature factor, before clamping", kxRaw, "");
            Kx = Math.Clamp(kxRaw, 1.0, 1.15);
            T.R("Kx", "Eq(21M)", "Lengthwise curvature factor (clamped to [1.0, 1.15] per the note under Eq 22)", Kx, "");
        }

        // ============================================================= Clause 15: Geometry factors
        T.Section("Clause 15: Geometry factors I (Zi) and J (YJ)");
        double I, J1, J2;
        if (In.GeometryFactorMode == GeometryFactorMode.ManualEntry)
        {
            I = T.R("I", "Clause 15 (manual)", "Pitting resistance geometry factor", In.ManualI, "");
            J1 = T.R("J1", "Clause 15 (manual)", "Pinion bending strength geometry factor", In.ManualJ1, "");
            J2 = T.R("J2", "Clause 15 (manual)", "Gear bending strength geometry factor", In.ManualJ2, "");
        }
        else
        {
            var gf = new GeometryFactors(In, T).Calculate();
            I = gf.ZI; J1 = gf.YJ1; J2 = gf.YJ2;
            warnings.Add("Geometry factors computed via Annex C(M): Zi (I) has been validated to be in the right " +
                "range against an Annex D chart cross-check, but YJ1/YJ2 (J) could not be validated against any " +
                "published numeric example (Annex E's own worked example reads J off an Annex D chart instead of " +
                "using Annex C(M)). Cross-check J against an Annex D chart or independent calculation before " +
                "relying on it, or switch to Geometry factor source = ManualEntry and supply a chart-read value.");
        }

        // ============================================================= Clause 16: Stress cycle factors
        T.Section("Clause 16: Stress cycle factors, CL (ZNT) and KL (YNT)");
        double CL1 = T.R("CL1", "Fig 5", "Pitting stress cycle factor, pinion", PittingLifeFactor(In.PinionLifeCycles, warnings, "pinion"), "");
        double CL2 = T.R("CL2", "Fig 5", "Pitting stress cycle factor, gear", PittingLifeFactor(In.GearLifeCycles, warnings, "gear"), "");
        double KL1 = T.R("KL1", "Fig 6", "Bending stress cycle factor, pinion", BendingLifeFactor(In.PinionLifeCycles, In.CriticalService), "");
        double KL2 = T.R("KL2", "Fig 6", "Bending stress cycle factor, gear", BendingLifeFactor(In.GearLifeCycles, In.CriticalService), "");

        // ============================================================= Clause 17: Hardness ratio factor
        T.Section("Clause 17: Hardness ratio factor, CH (Zw)");
        double CH_pinion = T.R("CH (pinion)", "17.1", "Hardness ratio factor - always 1.0 for the pinion", 1.0, "");
        double CH_gear;
        switch (In.HardnessRatioMode)
        {
            case HardnessRatioMode.ThroughHardenedHarderPinion:
                double ratio = In.HB_Pinion / In.HB_Gear;
                if (ratio < 1.2)
                {
                    warnings.Add($"HBP/HBG ratio ({ratio:0.00}) is below 1.2 - use CH = 1.0 per Figure 7's shaded region.");
                    CH_gear = 1.0;
                }
                else
                {
                    double B1 = 0.00898 * Math.Min(ratio, 1.7) - 0.00829;
                    if (ratio > 1.7) warnings.Add($"HBP/HBG ratio ({ratio:0.00}) exceeds the 1.7 validity limit of Eq 27M/28M; clamped to 1.7.");
                    T.R("B1", "Eq(28M)", "Hardness ratio coefficient", B1, "");
                    CH_gear = 1.0 + B1 * ((double)In.z2 / In.z1 - 1.0);
                }
                break;
            case HardnessRatioMode.SurfaceHardenedPinion:
                double B2 = 0.00075 * Math.Exp(-0.52 * In.PinionSurfaceRoughness_um);
                T.R("B2", "Eq(30M)", "Hardness ratio coefficient", B2, "");
                CH_gear = 1.0 + B2 * (450.0 - In.HB_Gear);
                break;
            default:
                CH_gear = 1.0;
                break;
        }
        CH_gear = T.R("CH (gear)", "Eq(27M)/(29M)", "Hardness ratio factor, gear", CH_gear, "");

        // ============================================================= Clause 18: Temperature factor
        T.Section("Clause 18: Temperature factor, KT (Kθ)");
        double KT = In.OperatingTemperatureC <= 120.0
            ? T.R("KT", "Clause 18", "Temperature factor (0-120 C)", 1.0, "")
            : T.R("KT", "Eq(31M)", "Temperature factor (high temperature)", (273.0 + In.OperatingTemperatureC) / 393.0, "");

        // ============================================================= Clause 19: Reliability factors
        T.Section("Clause 19: Reliability factors, CR (Zz) and KR (Yz)");
        var (CR, KR) = In.Reliability switch
        {
            ReliabilityLevel.OneFailureIn10000 => (1.22, 1.50),
            ReliabilityLevel.OneFailureIn1000 => (1.12, 1.25),
            ReliabilityLevel.OneFailureIn100 => (1.00, 1.00),
            ReliabilityLevel.OneFailureIn10 => (0.92, 0.85),
            _ => (0.84, 0.70)
        };
        T.R("CR", "Table 2", "Reliability factor, pitting", CR, "");
        T.R("KR", "Table 2", "Reliability factor, bending", KR, "");

        // ============================================================= Clause 20: Elastic coefficient
        T.Section("Clause 20: Elastic coefficient, Cp (ZE)");
        double Cp = T.R("Cp", "Eq(32M)", "Elastic coefficient",
            Math.Sqrt(1.0 / (Math.PI * ((1 - In.Mu_Pinion * In.Mu_Pinion) / In.E_Pinion + (1 - In.Mu_Gear * In.Mu_Gear) / In.E_Gear))), "sqrt(N/mm^2)");

        // ============================================================= Clause 21: Allowable stress numbers
        T.Section("Clause 21: Allowable stress numbers, sac (σHlim) and sat (σFlim)");
        var (sac1, sat1) = AllowableStress(In.PinionMaterial, In.PinionGrade, In.PinionHB, In.PinionRootHardening, In.PinionSacManual, In.PinionSatManual, warnings, "pinion");
        var (sac2, sat2) = AllowableStress(In.GearMaterial, In.GearGrade, In.GearHB, In.GearRootHardening, In.GearSacManual, In.GearSatManual, warnings, "gear");
        T.R("sac1", "Clause 21", "Allowable contact stress number, pinion", sac1, "N/mm^2");
        T.R("sac2", "Clause 21", "Allowable contact stress number, gear", sac2, "N/mm^2");
        T.R("sat1", "Clause 21", "Bending stress number (allowable), pinion", sat1, "N/mm^2");
        T.R("sat2", "Clause 21", "Bending stress number (allowable), gear", sat2, "N/mm^2");

        // ============================================================= Clause 5.1: Pitting resistance
        T.Section("Clause 5.1: Pitting resistance - calculated contact stress");
        double sc = T.R("sc", "Eq(1M)", "CALCULATED CONTACT STRESS NUMBER",
            Cp * Math.Sqrt(2000.0 * T1 * In.Ko * Kv * Km * Cs * Cxc / (In.b * In.de1 * In.de1 * I)), "N/mm^2");

        T.Section("Clause 5.1.2: Permissible contact stress number");
        double swc1 = T.R("swc1", "Eq(2M)", "Permissible contact stress number, pinion", sac1 * CL1 * CH_pinion / (In.SH * KT * CR), "N/mm^2");
        double swc2 = T.R("swc2", "Eq(2M)", "Permissible contact stress number, gear", sac2 * CL2 * CH_gear / (In.SH * KT * CR), "N/mm^2");

        if (sc > swc1) warnings.Add($"Pinion pitting: calculated stress sc ({sc:0.#} N/mm^2) exceeds permissible swc1 ({swc1:0.#} N/mm^2).");
        if (sc > swc2) warnings.Add($"Gear pitting: calculated stress sc ({sc:0.#} N/mm^2) exceeds permissible swc2 ({swc2:0.#} N/mm^2).");

        T.Section("Clause 5.1.3: Pitting resistance power rating");
        // NOTE: Eq 4M is defined in terms of the pinion's own speed n1 and outer pitch diameter
        // de1 throughout (Table 1 gives no separate "gear-referenced" symbols for these) - the
        // gear's power rating is obtained by keeping n1/de1 fixed and swapping in the GEAR's
        // own sac/CL/CH. Annex E's worked example confirms this numerically: its Pac(gear) is
        // only reproduced using the pinion's n and d together with the gear's sac/CL/CH, not
        // the gear's own speed/diameter.
        double Pac1 = T.R("Pac1", "Eq(4M)", "Allowable transmitted power for pitting resistance, pinion",
            PittingPower(In.PinionSpeed_rpm, In.b, In.de1, I, Kv, Km, In.Ko, Cs, Cxc, sac1, CL1, CH_pinion, In.SH, Cp, KT, CR) / In.CSF, "kW");
        double Pac2 = T.R("Pac2", "Eq(4M)", "Allowable transmitted power for pitting resistance, gear",
            PittingPower(In.PinionSpeed_rpm, In.b, In.de1, I, Kv, Km, In.Ko, Cs, Cxc, sac2, CL2, CH_gear, In.SH, Cp, KT, CR) / In.CSF, "kW");

        // ============================================================= Clause 5.2: Bending strength
        T.Section("Clause 5.2: Bending strength - calculated bending stress");
        double st1 = T.R("st1", "Eq(5M)", "CALCULATED BENDING STRESS NUMBER, pinion",
            2000.0 * T1 * In.Ko * Kv * Ks * Km / (In.b * In.de1 * In.m_et * Kx * J1), "N/mm^2");
        double st2 = T.R("st2", "Eq(5M)", "CALCULATED BENDING STRESS NUMBER, gear",
            2000.0 * T2 * In.Ko * Kv * Ks * Km / (In.b * In.de2 * In.m_et * Kx * J2), "N/mm^2");

        T.Section("Clause 5.2.2: Permissible bending stress number");
        double swt1 = T.R("swt1", "Eq(6M)", "Permissible bending stress number, pinion", sat1 * KL1 / (In.SF * KT * KR), "N/mm^2");
        double swt2 = T.R("swt2", "Eq(6M)", "Permissible bending stress number, gear", sat2 * KL2 / (In.SF * KT * KR), "N/mm^2");

        if (st1 > swt1) warnings.Add($"Pinion bending: calculated stress st1 ({st1:0.#} N/mm^2) exceeds permissible swt1 ({swt1:0.#} N/mm^2).");
        if (st2 > swt2) warnings.Add($"Gear bending: calculated stress st2 ({st2:0.#} N/mm^2) exceeds permissible swt2 ({swt2:0.#} N/mm^2).");

        T.Section("Clause 5.2.3: Bending strength power rating");
        // NOTE: as with Eq 4M above, Eq 8M keeps the pinion's own n1/de1 fixed for both members
        // and swaps in the relevant member's own J/sat/KL (confirmed against Annex E).
        double Pat1 = T.R("Pat1", "Eq(8M)", "Allowable transmitted power for bending strength, pinion",
            BendingPower(In.PinionSpeed_rpm, In.b, In.de1, J1, Kx, Ks, Km, In.Ko, Kv, sat1, In.m_et, KL1, KT, KR, In.SF) / In.KSF, "kW");
        double Pat2 = T.R("Pat2", "Eq(8M)", "Allowable transmitted power for bending strength, gear",
            BendingPower(In.PinionSpeed_rpm, In.b, In.de1, J2, Kx, Ks, Km, In.Ko, Kv, sat2, In.m_et, KL2, KT, KR, In.SF) / In.KSF, "kW");

        // ============================================================= Summary
        T.Section("SUMMARY - Governing rating");
        double PaPitting = Math.Min(Pac1, Pac2);
        double PaBending = Math.Min(Pat1, Pat2);
        double Pa = Math.Min(PaPitting, PaBending);
        T.R("Pa (pitting)", "Eq(4M)", "Allowable transmitted power, pitting-limited (lesser of pinion/gear)", PaPitting, "kW");
        T.R("Pa (bending)", "Eq(8M)", "Allowable transmitted power, bending-limited (lesser of pinion/gear)", PaBending, "kW");
        T.R("Pa (governing)", "Clause 5", "GOVERNING ALLOWABLE TRANSMITTED POWER", Pa, "kW");
        T.Note(Pa < In.Power_kW
            ? $"Design power ({In.Power_kW:0.##} kW) EXCEEDS the governing allowable power ({Pa:0.##} kW) - this design is UNDER-RATED."
            : $"Design power ({In.Power_kW:0.##} kW) is within the governing allowable power ({Pa:0.##} kW).");

        return (T, warnings);
    }

    /// <summary>Figure 5: pitting stress cycle factor, carburized case hardened steel bevel
    /// gears. Single power-law curve, valid for NL in [10^3, 10^10] (clause 16, 16.1); values
    /// below 10^3 use the 10^3 end point per the "does not extend to stress levels above those
    /// permissible for 10^3 cycles" guidance.</summary>
    private static double PittingLifeFactor(double NL, List<string> warnings, string who)
    {
        if (NL < 1000)
        {
            warnings.Add($"{who}: required life ({NL:0.###e0} cycles) is below the 10^3-cycle validity floor of Figure 5 - using the 10^3-cycle value.");
            NL = 1000;
        }
        if (NL > 1e10)
            warnings.Add($"{who}: required life ({NL:0.###e0} cycles) exceeds the 10^10-cycle end point plotted in Figure 5 (clause 3.7: curve-fit equations should not be extrapolated beyond the plotted curve).");
        return 3.4822 * Math.Pow(NL, -0.0602);
    }

    /// <summary>Figure 6: bending stress cycle factor, carburized case hardened steel bevel
    /// gears. Flat at the 10^3-cycle value below 10^3 cycles, a single power-law segment from
    /// 10^3 cycles to the general/critical-service transition (~3x10^6 cycles), then either the
    /// upper (general design) or lower (critical service) edge of the shaded band.</summary>
    private static double BendingLifeFactor(double NL, bool criticalService)
    {
        const double seg2Coeff = 6.1514, seg2Exp = -0.1192;
        double upperCoeff = criticalService ? 1.683 : 1.3558;
        double upperExp = criticalService ? -0.0323 : -0.0178;

        double NTrans = Math.Exp(Math.Log(upperCoeff / seg2Coeff) / (seg2Exp - upperExp));

        if (NL <= 1000) return seg2Coeff * Math.Pow(1000, seg2Exp);
        if (NL <= NTrans) return seg2Coeff * Math.Pow(NL, seg2Exp);
        return upperCoeff * Math.Pow(NL, upperExp);
    }

    /// <summary>Eq 4M: allowable transmitted power for pitting resistance, evaluated with the
    /// speed and outer pitch diameter of whichever member (pinion or gear) is being rated.</summary>
    private static double PittingPower(double n, double b, double de, double I, double Kv, double Km, double Ko, double Cs, double Cxc,
        double sac, double CL, double CH, double SH, double Cp, double KT, double CR)
    {
        double bracket = de * sac * CL * CH / (SH * Cp * KT * CR);
        return (Math.PI * n * b / 6.0e7) * (I / (Kv * Km * Ko * Cs * Cxc)) * bracket * bracket;
    }

    /// <summary>Eq 8M: allowable transmitted power for bending strength, evaluated with the
    /// speed and outer pitch diameter of whichever member (pinion or gear) is being rated.</summary>
    private static double BendingPower(double n, double b, double de, double J, double Kx, double Ks, double Km, double Ko, double Kv,
        double sat, double met, double KL, double KT, double KR, double SF)
    {
        return (Math.PI * n * b / 6.0e7) * (J * Kx / (Ks * Km * Ko * Kv)) * (sat * de * met) * (KL / (KT * KR * SF));
    }

    private (double sac, double sat) AllowableStress(MaterialSelection mat, MaterialGrade grade, double hb, RootHardening root,
        double manualSac, double manualSat, List<string> warnings, string who)
    {
        switch (mat)
        {
            case MaterialSelection.SteelThroughHardened:
                {
                    double sac = grade == MaterialGrade.Grade1 ? 2.35 * hb + 162.89 : 2.51 * hb + 203.86;
                    double sat = grade == MaterialGrade.Grade1 ? 0.30 * hb + 14.48 : 0.33 * hb + 41.24;
                    if (grade == MaterialGrade.Grade3) warnings.Add($"{who}: through-hardened steel has no Grade 3 in Table 3/5 - using Grade 2 formulas.");
                    return (sac, sat);
                }
            case MaterialSelection.SteelFlameOrInductionHardened:
                {
                    double sac = grade == MaterialGrade.Grade1 ? 1210.0 : 1310.0;
                    double sat = root == RootHardening.UnhardenedRoots ? (grade == MaterialGrade.Grade1 ? 85.0 : 95.0) : 154.0;
                    return (sac, sat);
                }
            case MaterialSelection.SteelCarburizedCaseHardened:
                {
                    double sac = grade switch { MaterialGrade.Grade1 => 1380.0, MaterialGrade.Grade2 => 1550.0, _ => 1720.0 };
                    double sat = grade switch { MaterialGrade.Grade1 => 205.0, MaterialGrade.Grade2 => 240.0, _ => 275.0 };
                    return (sac, sat);
                }
            case MaterialSelection.SteelNitridedAisi4140:
                if (grade != MaterialGrade.Grade2) warnings.Add($"{who}: AISI 4140 nitrided is only tabulated at Grade 2 (Table 3/5).");
                return (1000.0, 150.0);
            case MaterialSelection.SteelNitridedNitralloy135M:
                if (grade != MaterialGrade.Grade2) warnings.Add($"{who}: Nitralloy 135M nitrided is only tabulated at Grade 2 (Table 3/5).");
                return (1100.0, 165.0);
            case MaterialSelection.CastIronClass30:
                return (345.0, 30.0);
            case MaterialSelection.CastIronClass40:
                return (450.0, 45.0);
            case MaterialSelection.DuctileIronGrade80_55_06:
                return (650.0, 70.0);
            case MaterialSelection.DuctileIronGrade120_90_02:
                return (930.0, 95.0);
            default:
                return (manualSac, manualSat);
        }
    }
}
