using ISO10300Calc.Models;
using static ISO10300Calc.Calc.MathUtil;

namespace ISO10300Calc.Calc;

/// <summary>
/// Implements ISO 10300:2023 parts 1-3, Method B1, for bevel gears without hypoid offset
/// (a = 0, Sigma = 90 deg), drive flank only - the same scope restriction AGMA2003D19Calc
/// applies to ANSI/AGMA 2003-D19. Equation numbers in comments refer to the published parts:
/// [1] = ISO 10300-1:2023, [2] = ISO 10300-2:2023, [3] = ISO 10300-3:2023.
/// </summary>
public class Iso10300Calculator
{
    private readonly Iso10300Inputs In;
    private readonly CalcTrace T = new();

    public Iso10300Calculator(Iso10300Inputs inputs) => In = inputs;

    public (CalcTrace trace, List<string> warnings) Calculate()
    {
        var warnings = new List<string>();
        double u = (double)In.z2 / In.z1;

        if (Math.Abs(In.Alpha_nD - In.Alpha_nC) > 1e-9 || Math.Abs(In.Alpha_eD - In.Alpha_eC) > 1e-9)
            warnings.Add("Drive- and coast-side pressure angles differ, but this tool only rates the drive flank (matching ISO/TR 10300-30 Sample 1) - the coast flank is not separately evaluated.");

        // ============================================================= Clause 6 [1]: Torque, forces, speeds
        T.Section("ISO 10300-1, Clause 6: Torque, forces and speeds");
        double n2 = T.R("n2", "", "Wheel speed (n1*z1/z2)", In.PinionSpeed_rpm * In.z1 / In.z2, "rpm");
        double T2 = T.R("T2", "", "Wheel torque (T1*z2/z1)", In.T1_Nm * u, "Nm");
        double vmt1 = T.R("vmt1", "Eq(5)", "Tangential speed at mean point, pinion", In.dm1 * In.PinionSpeed_rpm / 19098.0, "m/s");
        double vmt2 = T.R("vmt2", "Eq(5)", "Tangential speed at mean point, wheel", In.dm2 * n2 / 19098.0, "m/s");

        // ============================================================= Annex A [1]: Virtual cylindrical gears
        T.Section("ISO 10300-1, Annex A: Virtual cylindrical gears (a=0, drive flank)");
        var v = VirtualCylindricalGears.Calculate(In, T, warnings);

        // ============================================================= Clause 6 [1] (cont.): forces
        T.Section("ISO 10300-1, Clause 6 (cont.): nominal tangential forces");
        double Fmt1 = T.R("Fmt1", "Eq(1)", "Nominal tangential force, pinion", 2000.0 * In.T1_Nm / In.dm1, "N");
        double Fmt2 = T.R("Fmt2", "Eq(1)", "Nominal tangential force, wheel", 2000.0 * T2 / In.dm2, "N");
        double Fvmt = T.R("Fvmt", "Eq(2)", "Nominal tangential force of virtual cylindrical gears", Fmt1 * Cos(v.betav) / Cos(In.Beta_m1), "N");

        double Rm1 = In.dm1 / (2.0 * Sin(In.Delta1));
        double Rm2 = In.dm2 / (2.0 * Sin(In.Delta2));
        double de2 = In.dm2 * In.Re / Rm2;

        // ============================================================= Clause 7 [1]: Dynamic factor
        T.Section("ISO 10300-1, Clause 7: Dynamic factor, Kv");
        double Kv;
        double fptWheel = In.fpt2_um; // 9.3.1: use the wheel's tolerance for design calculations.
        double yAlpha = T.R("y_alpha", "Eq(51)", "Running-in allowance (case hardened/nitrided formula)", Math.Min(0.075 * fptWheel, 3.0), "um");
        if (In.DynamicFactorMode == DynamicFactorMode.Manual)
        {
            Kv = T.R("Kv", "Manual", "Manually supplied dynamic factor", In.ManualKv, "");
        }
        else if (In.DynamicFactorMode == DynamicFactorMode.MethodC)
        {
            double vet2 = T.R("vet2", "Eq(23)", "Tangential speed at outer pitch diameter, wheel", vmt2 * de2 / In.dm2, "m/s");
            double X = T.R("X", "Eq(25)", "Exponent", 0.25 * Math.Pow(In.AccuracyGradeB - 4.0, 0.667), "");
            double A = T.R("A", "Eq(24)", "Coefficient", 50.0 + 56.0 * (1.0 - X), "");
            Kv = T.R("Kv", "Eq(22)", "Dynamic factor (Method C)", Math.Pow(A / (A + Math.Sqrt(200.0 * vet2)), -X), "");
            double vet2Max = T.R("vet2,max", "Eq(27)", "Maximum recommended wheel pitch line velocity for this grade", Math.Pow(A + (13.0 - In.AccuracyGradeB), 2) / 200.0, "m/s");
            if (vet2 > vet2Max)
                warnings.Add($"Wheel pitch line velocity ({vet2:0.##} m/s) exceeds the recommended maximum ({vet2Max:0.##} m/s) for accuracy grade B={In.AccuracyGradeB} (Kv-C, 7.7.4.3).");
            if (In.AccuracyGradeB < 5)
                warnings.Add("Accuracy grade B < 5 is 'very accurate gearing' (7.7.4.2) - the Kv-C empirical curve is not validated below B=5; consider Kv between 1.0 and 1.1 directly.");
        }
        else
        {
            double CF = (Fvmt * In.KA / v.bvEff) >= 100.0 ? 1.0 : (Fvmt * In.KA / v.bvEff) / 100.0;
            T.R("CF", "Eq(12/13)", "Correction factor for non-average conditions", CF, "");
            double cGamma = T.R("c_gamma", "Eq(11)", "Mean mesh stiffness per unit facewidth", 20.0 * CF, "N/(mm.um)");
            double m1s = 0.125 * In.MaterialDensity * Math.PI * (In.dm1 * In.dm1) / Math.Pow(Cos((In.Alpha_nD + In.Alpha_nC) / 2.0), 2);
            double m2s = 0.125 * In.MaterialDensity * Math.PI * (In.dm2 * In.dm2) / Math.Pow(Cos((In.Alpha_nD + In.Alpha_nC) / 2.0), 2);
            T.R("m1*", "Eq(14)", "Relative pinion mass per unit facewidth", m1s, "kg/mm");
            T.R("m2*", "Eq(14)", "Relative wheel mass per unit facewidth", m2s, "kg/mm");
            double mred = T.R("m_red", "Eq(10)", "Reduced mass", m1s * m2s / (m1s + m2s), "kg/mm");
            double nE1 = T.R("nE1", "Eq(9)", "Resonance speed of pinion", (30000.0 / (Math.PI * In.z1)) * Math.Sqrt(cGamma / mred), "rpm");
            double N = T.R("N", "Eq(8)", "Dimensionless reference speed", In.PinionSpeed_rpm / nE1, "");

            double fpEff = T.R("fp,eff", "Eq(17)", "Effective pitch deviation", fptWheel - yAlpha, "um");
            (double cv12, double cv3, double cv4, double cv56, double cv7) = DynamicFactorCoefficients(v.epsVGamma);

            double cPrime = T.R("c'", "Eq(18)", "Single stiffness", 14.0 * CF, "N/(mm.um)");
            double kFactorSub = (v.bvEff * fpEff * cPrime / (Fvmt * In.KA)) * cv12 + cv3;
            double kFactorMain = (v.bvEff * fpEff * cPrime / (Fvmt * In.KA)) * cv12 + cv4;
            double kFactorSuper = (v.bvEff * fpEff * cPrime / (Fvmt * In.KA)) * cv56 + cv7;

            if (N <= 0.75)
            {
                T.R("K", "Eq(16)", "Dynamic-factor constant (subcritical)", kFactorSub, "");
                Kv = T.R("Kv", "Eq(15)", "Dynamic factor (Method B, subcritical)", N * kFactorSub + 1.0, "");
            }
            else if (N <= 1.25)
            {
                T.R("K", "Eq(19)", "Dynamic-factor constant (main resonance)", kFactorMain, "");
                Kv = T.R("Kv", "Eq(19)", "Dynamic factor (Method B, main resonance)", kFactorMain + 1.0, "");
                warnings.Add("Operation is in the main resonance sector (0.75 < N <= 1.25, 7.7.3.5) - this should be avoided; a refined Method A analysis is recommended.");
            }
            else if (N < 1.5)
            {
                double kv125 = kFactorMain + 1.0; // Kv-B at N=1.25, main-resonance formula (Eq 19).
                double kv15 = kFactorSuper + 1.0; // Kv-B at N=1.5, supercritical formula (Eq 21).
                Kv = T.R("Kv", "Eq(20)", "Dynamic factor (Method B, interpolated intermediate sector)", kv15 + (kv125 - kv15) * (1.5 - N) / 0.25, "");
            }
            else
            {
                T.R("K", "Eq(21)", "Dynamic-factor constant (supercritical)", kFactorSuper, "");
                Kv = T.R("Kv", "Eq(21)", "Dynamic factor (Method B, supercritical)", kFactorSuper + 1.0, "");
            }
        }

        // ============================================================= Clause 8 [1]: Face load factor
        T.Section("ISO 10300-1, Clause 8: Face load factor, KHbeta");
        double KHbeta, KFbeta;
        if (In.ManualKHbetaOverride)
        {
            KHbeta = T.R("KHbeta", "Manual", "Manually supplied face load factor", In.ManualKHbeta, "");
            KFbeta = KHbeta;
        }
        else
        {
            double KHbetaBe = MountingFactorTable(In.Mounting, In.ContactVerification);
            T.R("KHbeta-be", "T:4", "Mounting factor", KHbetaBe, "");
            KHbeta = T.R("KHbeta-C", "Eq(28)", "Face load factor for contact stress", 1.5 * KHbetaBe, "");

            double KF0;
            if (In.GearType != GearType.SpiralBevel || In.CutterRadius > Rm2)
            {
                KF0 = T.R("KF0", "Eq(33)", "Lengthwise curvature factor for bending (large cutter radius / straight bevel)", 1.0, "");
            }
            else
            {
                double q = T.R("q", "Eq(35)", "Exponent", 0.279 / Math.Log10(Sin(In.Beta_m2)), "");
                double kf0raw = 0.211 * Math.Pow(In.CutterRadius / Rm2, q) + 0.789;
                KF0 = T.R("KF0", "Eq(34)", "Lengthwise curvature factor for bending (raw)", Math.Clamp(kf0raw, 1.0, 1.15), "");
            }
            KFbeta = T.R("KFbeta-C", "Eq(32)", "Face load factor for bending stress", KHbeta / KF0, "");
        }

        // ============================================================= Clause 9 [1]: Transverse load factor
        T.Section("ISO 10300-1, Clause 9: Transverse load factors, KHalpha, KFalpha");
        double KHalpha;
        if (In.TransverseLoadFactorMode == TransverseLoadFactorMode.Manual)
        {
            KHalpha = T.R("KHalpha", "Manual", "Manually supplied transverse load factor (= KFalpha, a=0)", In.ManualKHalpha, "");
        }
        else if (In.TransverseLoadFactorMode == TransverseLoadFactorMode.MethodB)
        {
            double FmtH = T.R("FmtH", "Eq(47)", "Determinant tangential force at mid-facewidth", Fvmt * In.KA * Kv * KHbeta, "N");
            double cfLocal = (Fvmt * In.KA / v.bvEff) >= 100.0 ? 1.0 : (Fvmt * In.KA / v.bvEff) / 100.0;
            double cGamma20 = 20.0 * cfLocal;
            double bracket = cGamma20 * (fptWheel - yAlpha) / (FmtH / v.bv);
            double star = v.epsVGamma <= 2.0
                ? 0.9 + 0.4 * (v.epsVGamma / 2.0) * bracket
                : 0.9 + 0.4 * Math.Sqrt(2.0 * (v.epsVGamma - 1.0) / v.epsVGamma) * bracket;
            KHalpha = T.R("KHalpha", "Eq(46/48)", "Transverse load factor (Method B, a=0 so KHalpha=KHalpha*)", star, "");
        }
        else
        {
            KHalpha = TransverseLoadFactorTable(In.GearType, In.AccuracyGradeB, v.epsVGamma, T, warnings);
        }
        double KFalpha = KHalpha; // a = 0 -> KHalpha = KFalpha (Eq 40/41).
        if (KHalpha < 1.0) { warnings.Add("Computed KHalpha/KFalpha < 1.0 - clamped to 1.0 per ISO 10300-1, Note under Formula (41)."); KHalpha = KFalpha = 1.0; }

        // ============================================================= ISO 10300-2, Method B1: Pitting
        T.Section("ISO 10300-2, Clause 6: Contact-stress factors (Method B1)");
        double ZMB = PittingFactors.MidZoneFactor(v, T);
        double ZLS = PittingFactors.LoadSharingFactor(v, In.ProfileCrowning, T);
        double ZE = PittingFactors.ElasticityFactor(In.E_Pinion, In.Nu_Pinion, In.E_Wheel, In.Nu_Wheel, T);
        var (ZL, Zv, ZR) = PittingFactors.LubricantFilmFactors(In.SigmaHlim, In.Viscosity40, vmt2, In.Rz_Flank_Pinion, In.Rz_Flank_Wheel, v.rhoRel, T);
        double ZW = PittingFactors.WorkHardeningFactor(In.HardnessRatioMode, In.HBW_Pinion, In.HBW_Wheel, u, T);
        double ZKP = T.R("ZKP", "Eq(11)", "Bevel gear factor", 1.2, "");
        double ZX = T.R("ZX", "6.5.2", "Size factor (contact)", 1.0, "");
        double ZHyp = T.R("ZHyp", "Eq(12)", "Hypoid factor (a=0)", 1.0, "");
        double ZNT1 = PittingFactors.LifeFactor(In.MaterialFamily, In.PinionLifeCycles, T, "pinion");
        double ZNT2 = PittingFactors.LifeFactor(In.MaterialFamily, In.WheelLifeCycles, T, "wheel");

        T.Section("ISO 10300-2, Clause 6: Contact stress");
        double Fn = T.R("Fn", "Eq(3)", "Nominal normal force at mean point (corrected form, see ISO/TR 10300-30 note)", Fmt1 / (Cos(In.Alpha_nD) * Cos(In.Beta_m1)), "N");
        double sigmaH0 = T.R("sigmaH0-B1", "Eq(2)", "Nominal contact stress", Math.Sqrt(Fn / (v.lbm * v.rhoRel)) * ZMB * ZLS * ZE, "N/mm^2");
        double sigmaH = T.R("sigmaH-B1", "Eq(1)", "CALCULATED CONTACT STRESS", sigmaH0 * Math.Sqrt(In.KA * Kv * KHbeta * KHalpha), "N/mm^2");
        double sigmaHP1 = T.R("sigmaHP1-B1", "Eq(4)", "Permissible contact stress, pinion", In.SigmaHlim * ZNT1 * ZX * ZL * Zv * ZR * 1.0 * ZKP * ZHyp, "N/mm^2");
        double sigmaHP2 = T.R("sigmaHP2-B1", "Eq(4)", "Permissible contact stress, wheel", In.SigmaHlim * ZNT2 * ZX * ZL * Zv * ZR * ZW * ZKP * ZHyp, "N/mm^2");
        double SH1 = T.R("SH1-B1", "Eq(5)", "Calculated safety factor for contact stress, pinion", sigmaHP1 / sigmaH, "");
        double SH2 = T.R("SH2-B1", "Eq(5)", "Calculated safety factor for contact stress, wheel", sigmaHP2 / sigmaH, "");
        if (SH1 < In.SH_min) warnings.Add($"Pinion pitting safety factor SH1 ({SH1:0.###}) is below SH,min ({In.SH_min:0.##}).");
        if (SH2 < In.SH_min) warnings.Add($"Wheel pitting safety factor SH2 ({SH2:0.###}) is below SH,min ({In.SH_min:0.##}).");

        // ============================================================= ISO 10300-3, Method B1: Bending
        T.Section("ISO 10300-3, Clause 6: Tooth-form factors (Method B1, pinion)");
        double ha0 = T.R("ha0", "6.4.1.2.2", "Tool addendum (= basic crown gear dedendum)", In.k_hfp * In.mmn, "mm");
        var pinionForm = BendingFactors.ToothForm(In.mmn, ha0, In.x_hm1, In.x_sm1, In.Rho_a01, In.Spr1, In.Alpha_eD, In.Alpha_nD, v.zvn1, v.dvan1, v.dvbn1, T, "pinion");

        T.Section("ISO 10300-3, Clause 6: Tooth-form factors (Method B1, wheel)");
        var wheelForm = BendingFactors.ToothForm(In.mmn, ha0, In.x_hm2, In.x_sm2, In.Rho_a02, In.Spr2, In.Alpha_eD, In.Alpha_nD, v.zvn2, v.dvan2, v.dvbn2, T, "wheel");

        T.Section("ISO 10300-3, Clause 6: Shared bending factors");
        double YLS = T.R("YLS", "Eq(36)", "Load sharing factor (bending)", ZLS * ZLS, "");
        double Yeps = BendingFactors.ContactRatioFactor(v.epsVAlpha, v.epsVBeta, T);
        double YBS = BendingFactors.BevelSpiralAngleFactor(In.GearType, v.betav, v.betaVb, v.lbm, v.bv, In.h_am1 + In.h_fm1, In.h_am2 + In.h_fm2, T);
        double YRrelT = BendingFactors.SurfaceConditionFactor(In.MaterialFamily, In.Rz_Root, T);
        double YX = BendingFactors.SizeFactor(In.MaterialFamily, In.mmn, T);
        const double YST = 2.0;
        T.R("YST", "6.2", "Stress correction factor for standard test gear", YST, "");
        double YdeltaRelT1 = BendingFactors.NotchSensitivityFactor(In.MaterialFamily, pinionForm.qs, T, "pinion");
        double YdeltaRelT2 = BendingFactors.NotchSensitivityFactor(In.MaterialFamily, wheelForm.qs, T, "wheel");
        double YNT1 = BendingFactors.LifeFactor(In.MaterialFamily, In.PinionLifeCycles, T, "pinion");
        double YNT2 = BendingFactors.LifeFactor(In.MaterialFamily, In.WheelLifeCycles, T, "wheel");

        T.Section("ISO 10300-3, Clause 6: Tooth root stress");
        double sigmaF01 = T.R("sigmaF01-B1", "Eq(2)", "Nominal tooth root stress, pinion", (Fvmt / (v.bv * In.mmn)) * pinionForm.YFa * pinionForm.YSa * Yeps * YBS * YLS, "N/mm^2");
        double sigmaF02 = T.R("sigmaF02-B1", "Eq(2)", "Nominal tooth root stress, wheel", (Fvmt / (v.bv * In.mmn)) * wheelForm.YFa * wheelForm.YSa * Yeps * YBS * YLS, "N/mm^2");
        double sigmaF1 = T.R("sigmaF1-B1", "Eq(1)", "CALCULATED TOOTH ROOT STRESS, pinion", sigmaF01 * In.KA * Kv * KFbeta * KFalpha, "N/mm^2");
        double sigmaF2 = T.R("sigmaF2-B1", "Eq(1)", "CALCULATED TOOTH ROOT STRESS, wheel", sigmaF02 * In.KA * Kv * KFbeta * KFalpha, "N/mm^2");
        double sigmaFP1 = T.R("sigmaFP1-B1", "Eq(4)", "Permissible tooth root stress, pinion", In.SigmaFlim * YST * YNT1 * YdeltaRelT1 * YRrelT * YX, "N/mm^2");
        double sigmaFP2 = T.R("sigmaFP2-B1", "Eq(4)", "Permissible tooth root stress, wheel", In.SigmaFlim * YST * YNT2 * YdeltaRelT2 * YRrelT * YX, "N/mm^2");
        double SF1 = T.R("SF1-B1", "Eq(5)", "Calculated safety factor for tooth root strength, pinion", sigmaFP1 / sigmaF1, "");
        double SF2 = T.R("SF2-B1", "Eq(5)", "Calculated safety factor for tooth root strength, wheel", sigmaFP2 / sigmaF2, "");
        if (SF1 < In.SF_min) warnings.Add($"Pinion bending safety factor SF1 ({SF1:0.###}) is below SF,min ({In.SF_min:0.##}).");
        if (SF2 < In.SF_min) warnings.Add($"Wheel bending safety factor SF2 ({SF2:0.###}) is below SF,min ({In.SF_min:0.##}).");

        // ============================================================= Summary
        T.Section("SUMMARY - Governing safety factors");
        double SHmin = Math.Min(SH1, SH2);
        double SFmin = Math.Min(SF1, SF2);
        T.R("SH (governing)", "", "GOVERNING PITTING SAFETY FACTOR (lesser of pinion/wheel)", SHmin, "");
        T.R("SF (governing)", "", "GOVERNING BENDING SAFETY FACTOR (lesser of pinion/wheel)", SFmin, "");
        T.Note(SHmin < In.SH_min || SFmin < In.SF_min
            ? "This design does NOT meet the specified minimum safety factors."
            : "This design meets the specified minimum safety factors.");

        return (T, warnings);
    }

    /// <summary>Table 3 [1]: influence factors cv1..cv7 for the Method B dynamic factor. cv1-cv6
    /// branch on eps_vgamma &lt;= 2 vs &gt; 2; cv7 branches independently on its own three ranges.</summary>
    private static (double cv12, double cv3, double cv4, double cv56, double cv7) DynamicFactorCoefficients(double epsVGamma)
    {
        double cv1, cv2, cv3, cv4, cv5, cv6;
        if (epsVGamma <= 2.0)
        {
            cv1 = 0.32; cv2 = 0.34; cv3 = 0.23;
            cv4 = 0.90;
            cv5 = 0.47; cv6 = 0.47;
        }
        else
        {
            cv1 = 0.32; cv2 = 0.57 / (epsVGamma - 0.3); cv3 = 0.096;
            cv4 = (epsVGamma - 1.56) / (0.57 - 0.05 * epsVGamma);
            cv5 = 0.47; cv6 = 0.12 / (epsVGamma - 1.74);
        }
        double cv7 = epsVGamma <= 1.5 ? 0.75
                   : epsVGamma <= 2.5 ? 0.125 * Math.Sin(Math.PI * (epsVGamma - 2.0)) + 0.875
                   : 1.0;
        return (cv1 + cv2, cv3, cv4, cv5 + cv6, cv7);
    }

    /// <summary>Table 4 [1]: mounting factor, KHbeta-be.</summary>
    private static double MountingFactorTable(MountingCondition m, ContactPatternVerification v)
    {
        double[,] table =
        {
            { 1.00, 1.00, 1.00 },
            { 1.05, 1.10, 1.25 },
            { 1.20, 1.32, 1.50 },
        };
        return table[(int)v, (int)m];
    }

    /// <summary>Table 5 [1]: transverse load factors, Method C.</summary>
    private static double TransverseLoadFactorTable(GearType gearType, int gradeB, double epsVGamma, CalcTrace T, List<string> warnings)
    {
        if (gearType == GearType.StraightBevel)
        {
            double val = gradeB switch { <= 6 => 1.0, 7 => 1.1, _ => 1.2 };
            if (gradeB >= 9)
            {
                warnings.Add("Straight bevel, accuracy grade >= 9: Method C transverse load factor uses max(1.2, 1/eps_vgamma^2) as an approximation of the standard's '1/ZLS^2 or 1.2, whichever is greater' rule (9.4.3, Table 5).");
                val = Math.Max(1.2, 1.0 / (epsVGamma * epsVGamma));
            }
            return T.R("KHalpha*-C", "T:5", "Transverse load factor (Method C, straight bevel)", val, "");
        }
        else
        {
            double val = gradeB switch { <= 6 => 1.0, 7 => 1.1, 8 => 1.2, 9 => 1.4, _ => Math.Max(epsVGamma, 1.4) };
            return T.R("KHalpha*-C", "T:5", "Transverse load factor (Method C, helical/spiral bevel)", val, "");
        }
    }
}
