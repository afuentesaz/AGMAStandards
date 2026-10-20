using System.Globalization;
using AGMA2101E25Calc.Models;
using static AGMA2101E25Calc.Calc.Agma2101Factors;

namespace AGMA2101E25Calc.Calc;

/// <summary>
/// Implements ANSI/AGMA 2101-E25 for external spur and helical gear pairs: clause 6.7
/// (preliminary calculations), 7.1 (service factor method), 7.2 (safety factor method), 7.5
/// (uniform load) and the influence factors of clauses 8 to 22 and Annex A. The geometry
/// factors Z_I and Y_J are calculated with the AGMA 908-B89 analytical method (clause 10.3) or
/// entered directly. Equation numbers in comments refer to ANSI/AGMA 2101-E25 unless prefixed
/// with "908".
/// </summary>
public class AgmaCalculator
{
    private readonly AgmaInputs In;
    private readonly CalcTrace T = new();

    public AgmaCalculator(AgmaInputs inputs) => In = inputs;

    public (CalcTrace trace, List<string> warnings) Calculate()
    {
        if (In.Goal == RatingGoal.FatigueLifeForLoadSpectrum)
            return CalculateFatigueLife();

        var warnings = new List<string>();
        Validate();

        double mn = In.NormalModule;
        double alphaN = In.NormalPressureAngle * Math.PI / 180.0;
        bool isSpur = Math.Abs(In.HelixAngle) < 1e-9;

        // ============================================================= Clause 6.7: preliminary calculations
        T.Section("Clause 6.7 / 7.5: Preliminary calculations");

        double beta = T.R("beta", "", "Helix angle at reference diameter, radians", In.HelixAngle * Math.PI / 180.0, "rad");
        if (In.HelixAngle > 50.0)
            warnings.Add($"Helix angle {In.HelixAngle:0.##} deg exceeds 50 deg: ANSI/AGMA 2101-E25 does not apply (clause 1.2).");

        double u = T.R("u", "Eq(1)", "Gear ratio", (double)In.z2 / In.z1, "");
        double n1 = In.PinionSpeed;
        double n2 = T.R("n2", "", "Wheel speed", n1 / u, "rpm");
        double d1 = T.R("d1", "Eq(25)", "Reference diameter, pinion", In.z1 * mn / Math.Cos(beta), "mm");
        double d2 = T.R("d2", "Eq(25)", "Reference diameter, wheel", In.z2 * mn / Math.Cos(beta), "mm");

        // Tight-mesh center distance from the profile shift sum (involute function of the operating pressure angle).
        double alphaT = Math.Atan(Math.Tan(alphaN) / Math.Cos(beta));
        double invAlphaWt = Math.Tan(alphaT) - alphaT + 2.0 * Math.Tan(alphaN) * (In.x1 + In.x2) / (In.z1 + In.z2);
        double alphaWt0 = Agma908GeometryFactors.ArcInv(invAlphaWt);
        double aTight = (d1 + d2) / 2.0 * Math.Cos(alphaT) / Math.Cos(alphaWt0);
        double aw;
        if (In.OperatingCenterDistance > 0.0)
        {
            aw = T.R("aw", "input", "Operating center distance", In.OperatingCenterDistance, "mm");
            if (In.GeometryFactors == GeometryFactorSource.CalculatedPerAgma908 && Math.Abs(aw - aTight) > 0.001 * mn)
                T.Note($"Note: aw differs from the tight-mesh center distance for x1 + x2 = {In.x1 + In.x2:0.####} ({aTight:0.0000} mm) by {aw - aTight:0.0000} mm.");
        }
        else
            aw = T.R("aw", "inv", "Operating center distance, tight mesh from x1 + x2", aTight, "mm");

        double dw1 = T.R("dw1", "Eq(2)", "Operating pitch diameter, pinion", 2.0 * aw / (u + 1.0), "mm");
        double b = T.R("b", "Eq(3)", "Net face width of narrowest member", Math.Min(In.FaceWidthPinion, In.FaceWidthWheel), "mm");
        double vt = T.R("vt", "Eq(19)", "Pitch line velocity at operating pitch diameter", Math.PI * n1 * dw1 / 60000.0, "m/s");

        double torque, power;
        if (In.Goal == RatingGoal.RatedLoadForRequiredSafetyFactors && In.LoadInput == LoadSpecification.PinionTorque)
        {
            double pSF = PowerForRequiredSafetyFactors();
            torque = T.R("T", "7.2 solved", "Transmitted pinion torque = torque for the required safety factors (section 'Load rating')", 30000.0 * pSF / (Math.PI * n1), "Nm");
            power = T.R("P", "", "Transmitted power, P = T n1 pi / 30000", pSF, "kW");
        }
        else if (In.Goal == RatingGoal.RatedLoadForRequiredSafetyFactors)
        {
            power = T.R("P", "7.2 solved", "Transmitted power = power for the required safety factors (section 'Load rating')", PowerForRequiredSafetyFactors(), "kW");
            torque = T.R("T", "", "Transmitted pinion torque, T = 30000 P / (pi n1)", 30000.0 * power / (Math.PI * n1), "Nm");
        }
        else if (In.LoadInput == LoadSpecification.PinionTorque)
        {
            torque = T.R("T", "input", "Transmitted pinion torque", In.PinionTorque, "Nm");
            power = T.R("P", "", "Transmitted power, P = T n1 pi / 30000", torque * n1 * Math.PI / 30000.0, "kW");
        }
        else
        {
            power = T.R("P", "input", "Transmitted power", In.TransmittedPower, "kW");
            torque = T.R("T", "", "Transmitted pinion torque, T = 30000 P / (pi n1)", 30000.0 * power / (Math.PI * n1), "Nm");
        }
        double Ft = T.R("Ft", "Eq(18)", "Transmitted tangential load, Ft = 2000 T / dw1", 2000.0 * torque / dw1, "N");

        // ============================================================= Clause 10: geometry factors (AGMA 908)
        double ZI, YJ1, YJ2;
        if (In.GeometryFactors == GeometryFactorSource.CalculatedPerAgma908)
        {
            var g = Calculate908(mn, isSpur, aw, b, warnings);
            ZI = g.I; YJ1 = g.J1; YJ2 = g.J2;
        }
        else
        {
            T.Section("Clause 10: Geometry factors (user specified)");
            ZI = T.R("ZI", "input", "Geometry factor for macropitting resistance", In.ZIUser, "");
            YJ1 = T.R("YJ1", "input", "Geometry factor for bending strength, pinion", In.YJ1User, "");
            YJ2 = T.R("YJ2", "input", "Geometry factor for bending strength, wheel", In.YJ2User, "");
            T.Note("Contact ratio, interference, undercut and pointed-tooth scope checks (clause 1.2) need the AGMA 908 geometry and are not performed.");
        }

        // ============================================================= Clause 11: dynamic factor
        T.Section("Clause 11: Dynamic factor KV");
        double Kv;
        if (In.KvMethod == DynamicFactorMethod.UserValue)
        {
            Kv = T.R("KV", "input", "Dynamic factor (method A or very accurate gearing, 11.2.1)", In.KvUser, "");
            if (Kv < 1.0) warnings.Add("The dynamic factor KV shall be greater than 1.0 (clause 11).");
        }
        else
        {
            int av;
            if (In.KvMethod == DynamicFactorMethod.MethodBFromPitchDeviation)
            {
                if (mn < 0.5 || mn > 70.0) warnings.Add($"Normal module {mn} mm is outside the 0.5-70 mm limit of Eq 24.");
                double av1 = T.R("Av1", "Eq(24)", "Flank tolerance class from f_pt, pinion (unrounded)", FlankToleranceClassFromPitchDeviation(In.SinglePitchDeviationPinion, d1, mn), "");
                double av2 = T.R("Av2", "Eq(24)", "Flank tolerance class from f_pt, wheel (unrounded)", FlankToleranceClassFromPitchDeviation(In.SinglePitchDeviationWheel, d2, mn), "");
                av = (int)Math.Ceiling(Math.Max(av1, av2) - 1e-9);
            }
            else
                av = In.Av;
            if (av < 6)
            {
                warnings.Add($"Av = {av} is below the range of Figure 1 (6 to 12); Av = 6 used. For 'very accurate gearing' use a user KV of 1.02-1.11 (11.2.1).");
                av = 6;
            }
            if (av > 12)
                throw new ArgumentException($"Av = {av} is outside the range 6 to 12 covered by Eq 21/22 (Figure 1).");
            T.R("Av", "", "Transmission flank tolerance class number used", av, "");
            double B = T.R("B", "Eq(22)", "Dynamic factor parameter B", DynamicFactorB(av), "");
            double C = T.R("C", "Eq(22)", "Dynamic factor parameter C", DynamicFactorC(B), "");
            Kv = T.R("KV", "Eq(21)", "Dynamic factor method B, KV-B", DynamicFactorKvB(B, C, vt), "");
            double vtMax = T.R("vt max", "Eq(23)", "Maximum recommended pitch line velocity for Av", MaxPitchLineVelocity(C, av), "m/s");
            if (vt > vtMax)
                warnings.Add($"Pitch line velocity {vt:0.00} m/s exceeds vt max = {vtMax:0.00} m/s for Av = {av} (Eq 23, end of the Figure 1 curve).");
        }

        // ============================================================= Clause 12: elastic coefficient
        T.Section("Clause 12: Elastic coefficient ZE");
        double ZE = ElasticCoefficient(warnings);

        // ============================================================= Clause 13: hardness ratio factor
        T.Section("Clause 13: Hardness ratio factor ZW");
        double ZW = HardnessRatio(u);

        // ============================================================= Clause 14: load distribution factor
        T.Section("Clause 14: Load distribution factor KH");
        double KH;
        if (In.KhMethod == LoadDistributionMethod.UserValue)
            KH = T.R("KH", "input", "Load distribution factor (method A)", In.KhUser, "");
        else
        {
            double bOverDw1 = T.R("b/dw1", "", "Net face width to pinion operating pitch diameter ratio", b / dw1, "");
            if (bOverDw1 > 2.0)
                warnings.Add($"b/dw1 = {bOverDw1:0.###} exceeds 2.0: method B for KH is not recommended (14.2); use method A.");
            if (b >= 1020.0)
                warnings.Add($"b = {b:0.#} mm is not below 1020 mm: method B for KH is not recommended (14.2).");
            double Kfactor = Ft / (dw1 * b) * (u + 1.0) / u;     // Eq C.1 with CG = u / (u + 1)
            T.Note($"14.2 caution: b/dw1 = {bOverDw1:0.###} vs 2.4 - 0.29 K = {2.4 - 0.29 * Kfactor:0.###} (K = {Kfactor:0.####} N/mm^2 from Eq C.1).");
            double KHmc = T.R("KHmc", "14.2", "Lead correction factor", LeadCorrectionFactor(In.Lead), "");
            double KHpf = T.R("KHpf", PinionProportionEquation(b), "Pinion proportion factor", PinionProportionFactor(b, dw1), "");
            double KHpm = T.R("KHpm", "14.2", $"Pinion proportion modifier (S1/S = {In.PinionOffset / In.BearingSpan:0.####})", PinionProportionModifier(In.PinionOffset, In.BearingSpan), "");
            double KHma = T.R("KHma", "Eq(35)", $"Mesh alignment factor, {In.AlignmentCurve}", MeshAlignmentFactor(In.AlignmentCurve, b), "");
            double KHe = T.R("KHe", "14.2", "Mesh alignment correction factor", In.AdjustedOrLapped ? 0.8 : 1.0, "");
            double KHbeta = T.R("KHbeta", "Eq(31)", "Face load distribution factor", 1.0 + KHmc * (KHpf * KHpm + KHma * KHe), "");
            KH = T.R("KH", "Eq(30)", "Load distribution factor, KH = KHbeta", KHbeta, "");
        }

        // ============================================================= Clause 15: allowable stress numbers
        T.Section("Clause 15: Allowable stress numbers");
        double sigmaHP1 = AllowableContact(true, warnings);
        double sigmaHP2 = AllowableContact(false, warnings);
        double sigmaFP1 = AllowableBending(true, warnings);
        double sigmaFP2 = AllowableBending(false, warnings);

        // ============================================================= Clause 17: stress cycle factors
        T.Section("Clause 17: Stress cycle factors");
        double nL1 = T.R("nL1", "Eq(49)", "Number of stress cycles, pinion", StressCycles(In.LifeHours, n1, In.ContactsPerRevPinion), "");
        double nL2 = T.R("nL2", "Eq(49)", "Number of stress cycles, wheel", StressCycles(In.LifeHours, n2, In.ContactsPerRevWheel), "");
        foreach (var (nL, who) in new[] { (nL1, "pinion"), (nL2, "wheel") })
            if (nL > 1e10)
                warnings.Add($"Number of stress cycles of the {who} ({nL:0.###E+0}) exceeds 1e10, the end of Figures 17 and 18; application beyond this point shall be reviewed (17.2).");
        double ZN1 = StressCycleFactorZN(In.MaterialPinion, nL1, In.CycleCurve, out string basisZN1);
        double ZN2 = StressCycleFactorZN(In.MaterialWheel, nL2, In.CycleCurve, out string basisZN2);
        double YN1 = StressCycleFactorYN(In.MaterialPinion, In.HardnessHBPinion, nL1, In.CycleCurve, out string basisYN1);
        double YN2 = StressCycleFactorYN(In.MaterialWheel, In.HardnessHBWheel, nL2, In.CycleCurve, out string basisYN2);
        T.R("ZN1", "Table 12", "Stress cycle factor for macropitting, pinion: " + basisZN1, ZN1, "");
        T.R("ZN2", "Table 12", "Stress cycle factor for macropitting, wheel: " + basisZN2, ZN2, "");
        T.R("YN1", "Table 13", "Stress cycle factor for bending, pinion: " + basisYN1, YN1, "");
        T.R("YN2", "Table 13", "Stress cycle factor for bending, wheel: " + basisYN2, YN2, "");
        if (In.MaterialPinion == GearMaterial.SteelFlameOrInductionHardenedTypeA || In.MaterialPinion == GearMaterial.SteelFlameOrInductionHardenedTypeB
            || In.MaterialWheel == GearMaterial.SteelFlameOrInductionHardenedTypeA || In.MaterialWheel == GearMaterial.SteelFlameOrInductionHardenedTypeB)
            T.Note("Table 13 has no separate Y_N curve for flame or induction hardened steel; the 'case carb' curve is used.");
        double localYield1 = sigmaFP1 * YN1, localYield2 = sigmaFP2 * YN2;
        T.Note($"17.3: sigma_FP x Y_N = {localYield1:0.0} N/mm^2 (pinion), {localYield2:0.0} N/mm^2 (wheel); localized yielding may occur if this exceeds the allowable yield strength of Figure 19.");

        // ============================================================= Clauses 8, 9, 18, 20, 21, 22: remaining factors
        T.Section("Clauses 8, 9, 18, 20, 21, 22: Other influence factors");
        double KO = T.R("KO", "Clause 8", "Overload factor (safety factor method only)", In.OverloadFactor, "");
        double Kgamma = T.R("Kgamma", "Clause 21", "Mesh load factor", In.MeshLoadFactor, "");
        if (Kgamma < 1.0) warnings.Add("The mesh load factor K_gamma is equal to or greater than 1.0 (clause 21).");
        double Ytheta = T.R("Ytheta", "Clause 20", "Temperature factor", In.TemperatureFactor, "");
        double YZ = T.R("YZ", "Table 14", $"Reliability factor, {In.Reliability} (safety factor method only)", ReliabilityFactor(In.Reliability, In.ReliabilityFactorUser), "");
        double YI1 = T.R("YI1", "Clause 22", "Reverse bending factor, pinion", In.ReverseLoadedPinion ? 0.7 : 1.0, "");
        double YI2 = T.R("YI2", "Clause 22", "Reverse bending factor, wheel", In.ReverseLoadedWheel ? 0.7 : 1.0, "");
        double YB1 = T.R("YB1", "Fig A.1", "Rim thickness factor, pinion" + (In.BackupRatioPinion > 0 ? $" (mB = {In.BackupRatioPinion:0.###})" : " (solid)"), RimThicknessFactor(In.BackupRatioPinion), "");
        double YB2 = T.R("YB2", "Fig A.1", "Rim thickness factor, wheel" + (In.BackupRatioWheel > 0 ? $" (mB = {In.BackupRatioWheel:0.###})" : " (solid)"), RimThicknessFactor(In.BackupRatioWheel), "");
        foreach (var (mB, who) in new[] { (In.BackupRatioPinion, "pinion"), (In.BackupRatioWheel, "wheel") })
            if (mB > 0.0 && mB < 0.75)
                warnings.Add($"Backup ratio of the {who} mB = {mB:0.###} is below 0.75: special analysis required (Annex A).");

        // ============================================================= Clause 7.1: service factor method
        T.Section("Clause 7.1: Service factor method (KO = 1, YZ = 1)");
        double cosB = Math.Cos(beta);
        double Pazu1 = T.R("Pazu1", "Eq(6)", "Allowable power for macropitting at unity service factor, pinion",
            Math.PI * n1 * b / 6e7 * ZI / (Kv * Kgamma * KH) * Math.Pow(dw1 * sigmaHP1 * ZN1 / (ZE * Ytheta), 2), "kW");
        double Pazu2 = T.R("Pazu2", "Eq(7)", "Allowable power for macropitting at unity service factor, wheel",
            Math.PI * n1 * b / 6e7 * ZI / (Kv * Kgamma * KH) * Math.Pow(dw1 * sigmaHP2 * ZN2 * ZW / (ZE * Ytheta), 2), "kW");
        double Pazu = T.R("Pazu", "Eq(8)", "Allowable power for macropitting at unity service factor", Math.Min(Pazu1, Pazu2), "kW");
        double CSF = T.R("CSF", "Eq(4)", "Service factor for macropitting resistance", Pazu / power, "");
        double Payu1 = T.R("Payu1", "Eq(9)", "Allowable power for bending at unity service factor, pinion",
            Math.PI * n1 * dw1 / 6e7 * b * mn / (Kv * Kgamma * cosB) * YJ1 / (KH * YB1) * YI1 * sigmaFP1 * YN1 / Ytheta, "kW");
        double Payu2 = T.R("Payu2", "Eq(10)", "Allowable power for bending at unity service factor, wheel",
            Math.PI * n1 * dw1 / 6e7 * b * mn / (Kv * Kgamma * cosB) * YJ2 / (KH * YB2) * YI2 * sigmaFP2 * YN2 / Ytheta, "kW");
        double Payu = T.R("Payu", "Eq(11)", "Allowable power for bending at unity service factor", Math.Min(Payu1, Payu2), "kW");
        double KSF = T.R("KSF", "Eq(5)", "Service factor for bending strength", Payu / power, "");
        double Prated = T.R("P rated", "", "Rated power at unity service factor, min(Pazu, Payu)", Math.Min(Pazu, Payu), "kW");

        // ============================================================= Clause 7.2: safety factor method
        T.Section("Clause 7.2: Safety factor method");
        double sigmaH = T.R("sigma_H", "Eq(16)", "Contact stress number", ZE * Math.Sqrt(Ft * KO * Kv * Kgamma * KH / (dw1 * b * ZI)), "N/mm2");
        double SH1 = T.R("SH1", "Eq(12)", "Safety factor for macropitting, pinion", sigmaHP1 / sigmaH * ZN1 / Ytheta * 1.0 / YZ, "");
        double SH2 = T.R("SH2", "Eq(12)", "Safety factor for macropitting, wheel", sigmaHP2 / sigmaH * ZN2 / Ytheta * ZW / YZ, "");
        double sigmaF1 = T.R("sigma_F1", "Eq(17)", "Bending stress number, pinion", Ft * KO * Kv * Kgamma * cosB / (b * mn) * KH * YB1 / YJ1, "N/mm2");
        double sigmaF2 = T.R("sigma_F2", "Eq(17)", "Bending stress number, wheel", Ft * KO * Kv * Kgamma * cosB / (b * mn) * KH * YB2 / YJ2, "N/mm2");
        double SF1 = T.R("SF1", "Eq(14)", "Safety factor for bending, pinion", YI1 * sigmaFP1 / sigmaF1 * YN1 / (Ytheta * YZ), "");
        double SF2 = T.R("SF2", "Eq(14)", "Safety factor for bending, wheel", YI2 * sigmaFP2 / sigmaF2 * YN2 / (Ytheta * YZ), "");
        double SH = Math.Min(SH1, SH2), SF = Math.Min(SF1, SF2);

        // ============================================================= Clause 7.2 solved for the load
        // No factor depends on the load at a given speed and life (K_V on the pitch line velocity, K_H method B on the
        // geometry, Z_N / Y_N on the cycles): sigma_H grows with sqrt(P) and sigma_F with P, so S_H = S_Hmin and
        // S_F = S_Fmin give the power of each member and mode in closed form from the powers of 7.1.
        T.Section("Load rating for the required safety factors (clause 7.2 solved for the load)");
        double SHreq = In.MinSafetyFactorPitting, SFreq = In.MinSafetyFactorBending;
        double PH1 = T.R("P_H1", "7.2 solved", $"Power for S_H1 = {SHreq:0.###}, P_azu1 / (KO (SHmin YZ)^2)", Pazu1 / (KO * Math.Pow(SHreq * YZ, 2)), "kW");
        double PH2 = T.R("P_H2", "7.2 solved", $"Power for S_H2 = {SHreq:0.###}, P_azu2 / (KO (SHmin YZ)^2)", Pazu2 / (KO * Math.Pow(SHreq * YZ, 2)), "kW");
        double PF1 = T.R("P_F1", "7.2 solved", $"Power for S_F1 = {SFreq:0.###}, P_ayu1 / (KO SFmin YZ)", Payu1 / (KO * SFreq * YZ), "kW");
        double PF2 = T.R("P_F2", "7.2 solved", $"Power for S_F2 = {SFreq:0.###}, P_ayu2 / (KO SFmin YZ)", Payu2 / (KO * SFreq * YZ), "kW");
        double PSF = T.R("P_SF", "7.2 solved", "Power for the required safety factors, min(P_H1, P_H2, P_F1, P_F2)", Math.Min(Math.Min(PH1, PH2), Math.Min(PF1, PF2)), "kW");
        double TSF = T.R("T_SF", "", "Pinion torque for the required safety factors, 30000 P_SF / (pi n1)", 30000.0 * PSF / (Math.PI * n1), "Nm");
        string governs = GoverningMode(PSF, PH1, PH2, PF1, PF2);
        T.Note($"Governed by {governs}. The load holds for n1 = {n1:0.###} rpm, L = {In.LifeHours:0.###} h and KO = {KO:0.###}; the yield check under peak loads (clause 19) is not made.");
        if (In.Goal == RatingGoal.RatedLoadForRequiredSafetyFactors)
            T.Note($"Rated at this load: min(SH1, SH2) = {SH:0.000000} for SHmin = {SHreq:0.######}, min(SF1, SF2) = {SF:0.000000} for SFmin = {SFreq:0.######}.");

        // ============================================================= Summary
        T.Section("Summary");
        T.Note($"Service factor method: CSF = {CSF:0.0000}, KSF = {KSF:0.0000}; rated power at unity service factor = {Prated:0.000} kW " +
               $"({(Pazu <= Payu ? "macropitting" : "bending")} governs) for P = {power:0.000} kW.");
        // Led by the quantity chosen in 'Load as' (power or pinion torque), the other in brackets
        T.Note(In.LoadInput == LoadSpecification.PinionTorque
            ? $"Pinion torque for the required safety factors SHmin = {SHreq:0.###}, SFmin = {SFreq:0.###}: {TSF:0.00} Nm at {n1:0.###} rpm ({PSF:0.000} kW), {governs} governs."
            : $"Power for the required safety factors SHmin = {SHreq:0.###}, SFmin = {SFreq:0.###}: {PSF:0.000} kW at {n1:0.###} rpm ({TSF:0.00} Nm), {governs} governs.");
        // A relative slack of 1e-9 so that the safety factor of the rated power, equal to its minimum, is not a warning
        bool shOk = SH >= In.MinSafetyFactorPitting * (1.0 - 1e-9), sfOk = SF >= In.MinSafetyFactorBending * (1.0 - 1e-9);
        T.Note($"Macropitting: SH = min(SH1, SH2) = {SH:0.0000} {(shOk ? ">=" : "<")} SHmin = {In.MinSafetyFactorPitting:0.###} -> {(shOk ? "OK" : "NOT OK")} (Eq 13)");
        T.Note($"Bending:      SF = min(SF1, SF2) = {SF:0.0000} {(sfOk ? ">=" : "<")} SFmin = {In.MinSafetyFactorBending:0.###} -> {(sfOk ? "OK" : "NOT OK")} (Eq 15)");
        if (!shOk)
            warnings.Add($"Macropitting safety factor SH = {SH:0.000} is below the minimum required SHmin = {In.MinSafetyFactorPitting:0.###}.");
        if (!sfOk)
            warnings.Add($"Bending safety factor SF = {SF:0.000} is below the minimum required SFmin = {In.MinSafetyFactorBending:0.###}.");

        return (T, warnings);
    }

    /// <summary>The member and mode whose power is the smallest.</summary>
    private static string GoverningMode(double p, double ph1, double ph2, double pf1, double pf2) =>
        p == ph1 ? "macropitting of the pinion" : p == ph2 ? "macropitting of the wheel" : p == pf1 ? "bending of the pinion" : "bending of the wheel";

    /// <summary>
    /// The power for the required safety factors, from a rating of the same inputs at 1 kW: the powers of 7.1 and so
    /// P_SF do not depend on the load, so this pass only provides them before the rating at P_SF.
    /// </summary>
    private double PowerForRequiredSafetyFactors()
    {
        AgmaInputs probe = In.Clone();
        probe.Goal = RatingGoal.CheckTransmittedLoad;
        probe.LoadInput = LoadSpecification.TransmittedPower;
        probe.TransmittedPower = 1.0;
        var (trace, _) = new AgmaCalculator(probe).Calculate();
        return trace.Last("P_SF");
    }

    // ================================================================= Clause 7.3 / Annex D: fatigue life by Miner's rule
    // Written together with IGDv4.0's copy (2026-10-20). Each bin is rated as a transmitted load (clause 7.2 at its torque
    // and speed), so K_V follows the speed of the bin and every other factor is the rating's own. The fatigue stress
    // numbers are those of D.4 and D.11 (no K_O: the spectrum stands for the loads; K_S = 1, the metric edition has no size
    // factor), the stress cycle factors D.5 / D.12 (Y_N divided by the reverse bending factor Y_I as in Eq 14; D.2 has
    // Y_I = 1), the cycles to failure the curves of clause 17 inverted (CyclesToFailureZN / YN), and Miner's sum D.2, D.3,
    // D.8 to D.10, which reduces to L = (total time) / (damage of one pass of the spectrum). With K_O = 1 a single bin at
    // the load for the required safety factors lasts exactly the design life (the continuity of 7.2 and Annex D).

    /// <summary>
    /// The fatigue lives of the pinion and the wheel for macropitting and bending under the load spectrum of
    /// <see cref="AgmaInputs.LoadSpectrum"/>: the rating at the bin of the largest torque, then a rating per bin and
    /// Miner's sum of Annex D.
    /// </summary>
    private (CalcTrace trace, List<string> warnings) CalculateFatigueLife()
    {
        var warnings = new List<string>();
        List<SpectrumBin> bins = ParseLoadSpectrum(In.LoadSpectrum);
        if (In.LifeHours <= 0)
            throw new ArgumentException("The design life L (the required life) must be positive.");
        double totalHours = bins.Sum(bin => bin.Hours);
        if (!(totalHours > 0.0))
            throw new ArgumentException("The load spectrum needs a time: every bin has 0 h.");

        // The rating at the peak load, then one rating per bin
        int peak = 0;
        for (int i = 1; i < bins.Count; i++)
            if (bins[i].Torque > bins[peak].Torque)
                peak = i;
        var (report, peakWarnings) = new AgmaCalculator(BinInputs(bins[peak])).Calculate();
        AddBinWarnings(warnings, peakWarnings, null);
        var rated = new CalcTrace[bins.Count];
        for (int i = 0; i < bins.Count; i++)
        {
            if (i == peak) { rated[i] = report; continue; }
            var (binTrace, binWarnings) = new AgmaCalculator(BinInputs(bins[i])).Calculate();
            rated[i] = binTrace;
            AddBinWarnings(warnings, binWarnings, i + 1);
        }

        CalcTrace T = report;  // the sections of Annex D are added to the rating of the peak bin
        T.Section("Clause 7.3 / Annex D: Load spectrum, a rating per bin");
        T.Note(string.Format(CultureInfo.InvariantCulture, "The sections above rate bin {0}, the largest torque ({1:0.###} Nm at {2:0.###} rpm), with the design life L = {3:0.###} h; " +
            "the fatigue life below uses every bin (the safety factors of a bin are not checked against SHmin / SFmin: the life is the criterion).",
            peak + 1, bins[peak].Torque, bins[peak].Speed, In.LifeHours));
        for (int i = 0; i < bins.Count; i++)
        {
            string k = "[" + (i + 1) + "]";
            T.R("T " + k, "input", "Pinion torque of the bin", bins[i].Torque, "Nm");
            T.R("n1 " + k, "input", "Pinion speed of the bin", bins[i].Speed, "rpm");
            T.R("t " + k, "input", "Time of the bin", bins[i].Hours, "h");
            T.R("vt " + k, "Eq(19)", "Pitch line velocity", rated[i].Last("vt"), "m/s");
            T.R("KV " + k, "Eq(21)", "Dynamic factor at the speed of the bin", rated[i].Last("KV"), "");
            T.R("SH1 " + k, "Eq(12)", "Safety factor for macropitting, pinion, at the load of the bin", rated[i].Last("SH1"), "");
            T.R("SH2 " + k, "Eq(12)", "Safety factor for macropitting, wheel", rated[i].Last("SH2"), "");
            T.R("SF1 " + k, "Eq(14)", "Safety factor for bending, pinion", rated[i].Last("SF1"), "");
            T.R("SF2 " + k, "Eq(14)", "Safety factor for bending, wheel", rated[i].Last("SF2"), "");
        }

        double u = report.Last("u"), dw1 = report.Last("dw1"), b = report.Last("b"), beta = report.Last("beta");
        double ZE = report.Last("ZE"), ZI = report.Last("ZI"), ZW = report.Last("ZW"), KH = report.Last("KH");
        double YJ1 = report.Last("YJ1"), YJ2 = report.Last("YJ2"), YB1 = report.Last("YB1"), YB2 = report.Last("YB2");
        double YI1 = report.Last("YI1"), YI2 = report.Last("YI2"), Kgamma = report.Last("Kgamma"), Ytheta = report.Last("Ytheta"), YZ = report.Last("YZ");
        double SH = In.MinSafetyFactorPitting, SF = In.MinSafetyFactorBending;
        double mn = In.NormalModule;

        double LH1 = MinerLife(T, bins, rated, "H1", "Annex D.1.2: Fatigue life, macropitting of the pinion", warnings, "the pinion's macropitting", 1.0, In.ContactsPerRevPinion,
            (i, kv) => SH * Ytheta * YZ * ZE * Math.Sqrt(2000.0 * bins[i].Torque / dw1 * kv * Kgamma * KH / (dw1 * b * ZI)), report.Last("sigma_HP1"),
            "D.4", "D.5", "Z_N");
        double LH2 = MinerLife(T, bins, rated, "H2", "Annex D.1.2: Fatigue life, macropitting of the wheel", warnings, "the wheel's macropitting", u, In.ContactsPerRevWheel,
            (i, kv) => SH * Ytheta * YZ / ZW * ZE * Math.Sqrt(2000.0 * bins[i].Torque / dw1 * kv * Kgamma * KH / (dw1 * b * ZI)), report.Last("sigma_HP2"),
            "D.4", "D.5", "Z_N");
        double LF1 = MinerLife(T, bins, rated, "F1", "Annex D.1.3: Fatigue life, bending of the pinion", warnings, "the pinion's bending", 1.0, In.ContactsPerRevPinion,
            (i, kv) => SF * Ytheta * YZ * 2000.0 * bins[i].Torque / dw1 * kv * Kgamma * Math.Cos(beta) / (b * mn) * KH * YB1 / YJ1, YI1 * report.Last("sigma_FP1"),
            "D.11", "D.12", "Y_N");
        double LF2 = MinerLife(T, bins, rated, "F2", "Annex D.1.3: Fatigue life, bending of the wheel", warnings, "the wheel's bending", u, In.ContactsPerRevWheel,
            (i, kv) => SF * Ytheta * YZ * 2000.0 * bins[i].Torque / dw1 * kv * Kgamma * Math.Cos(beta) / (b * mn) * KH * YB2 / YJ2, YI2 * report.Last("sigma_FP2"),
            "D.11", "D.12", "Y_N");

        T.Section("Summary of the fatigue life (Annex D)");
        double L = T.R("L min", "", "Fatigue life of the gear set, the smallest of the four", Math.Min(Math.Min(LH1, LH2), Math.Min(LF1, LF2)), "h");
        string governs = L == LH1 ? "macropitting of the pinion" : L == LH2 ? "macropitting of the wheel" : L == LF1 ? "bending of the pinion" : "bending of the wheel";
        bool ok = L >= In.LifeHours * (1.0 - 1e-9);
        T.Note(string.Format(CultureInfo.InvariantCulture, "Lives for SH = {0:0.###}, SF = {1:0.###}: macropitting {2} h (pinion), {3} h (wheel); bending {4} h (pinion), {5} h (wheel).",
            SH, SF, FormatHours(LH1), FormatHours(LH2), FormatHours(LF1), FormatHours(LF2)));
        T.Note(string.Format(CultureInfo.InvariantCulture, "Fatigue life L = {0} h, {1} governs; {2} the required life L = {3:0.###} h -> {4}.",
            FormatHours(L), governs, ok ? ">=" : "<", In.LifeHours, ok ? "OK" : "NOT OK"));
        if (!ok)
            warnings.Add(string.Format(CultureInfo.InvariantCulture, "The fatigue life by Miner's rule, {0} h ({1}), is below the required life L = {2:0.###} h.", FormatHours(L), governs, In.LifeHours));
        return (T, warnings);
    }

    /// <summary>
    /// Miner's sum of one member and mode (Annex D.1.2 / D.1.3) logged under the suffix <paramref name="tag"/>: the fatigue
    /// stress number of each bin, its stress cycle factor, the cycles to failure and the cycles applied, then D.2, D.3,
    /// D.8 to D.10. <paramref name="ratio"/> divides the pinion speed (u for the wheel). Returns the life in hours.
    /// </summary>
    private double MinerLife(CalcTrace T, List<SpectrumBin> bins, CalcTrace[] rated, string tag, string title, List<string> warnings,
        string who, double ratio, double q, Func<int, double, double> stress, double allowable,
        string stressEq, string factorEq, string factorName)
    {
        T.Section(title);
        bool pitting = tag[0] == 'H';
        double damage = 0.0, sumCycles = 0.0, sumCyclesOverSpeed = 0.0;
        bool beyondCurves = false;
        for (int i = 0; i < bins.Count; i++)
        {
            string k = "[" + (i + 1) + "]";
            double sigma = T.R("sigma_" + tag + " " + k, "Eq(" + stressEq + ")", "Fatigue " + (pitting ? "contact" : "bending") + " stress number", stress(i, rated[i].Last("KV")), "N/mm2");
            double factor = T.R(factorName + tag.Substring(1) + " " + k, "Eq(" + factorEq + ")", "Stress cycle factor, sigma / " + (pitting ? "sigma_HP" : "(Y_I sigma_FP)"), sigma / allowable, "");
            string basis;
            bool isStatic;
            double nf = pitting
                ? CyclesToFailureZN(tag[1] == '1' ? In.MaterialPinion : In.MaterialWheel, factor, In.CycleCurve, out basis, out isStatic)
                : CyclesToFailureYN(tag[1] == '1' ? In.MaterialPinion : In.MaterialWheel, tag[1] == '1' ? In.HardnessHBPinion : In.HardnessHBWheel, factor, In.CycleCurve, out basis, out isStatic);
            T.R("Nf_" + tag + " " + k, "Clause 17", "Number of cycles to failure: " + basis, nf, "");
            if (isStatic)
                warnings.Add(string.Format(CultureInfo.InvariantCulture, "Bin {0}: the stress number of {1} is in the static region of its stress cycle curve; " +
                    "its cycles to failure are taken at the start of the curve and the yield check of clause 19 is not made.", i + 1, who));
            if (nf > 1e10) beyondCurves = true;
            double speed = bins[i].Speed / ratio;
            double n = T.R("n_" + tag + " " + k, "Eq(D.2)", "Number of cycles of the bin, 60 w t q", 60.0 * speed * bins[i].Hours * q, "");
            sumCycles += n;
            sumCyclesOverSpeed += n / speed;
            damage += n / nf;
        }
        if (beyondCurves)
            T.Note("Some cycles to failure are beyond 1e10, the end of Figures 17 and 18: the curves are extended (17.2).");
        double total = T.R("Sum n_" + tag, "Eq(D.2)", "Total number of cycles of the load spectrum", sumCycles, "");
        T.R("D_" + tag, "Miner", "Damage of one pass of the load spectrum, sum n_i / N_fi", damage, "");
        // a_i = n_i / sum n (D.3): N = sum n / D (D.8), w_b = sum n / sum(n_i / w_i) (D.9)
        double nLife = T.R("N_" + tag, "Eq(D.8)", "Resultant fatigue life in cycles, 1 / sum(a_i / N_fi)", total / damage, "");
        double wb = T.R("wb_" + tag, "Eq(D.9)", "Equivalent baseline speed, 1 / sum(a_i / w_i)", total / sumCyclesOverSpeed, "rpm");
        return T.R("L_" + tag, "Eq(D.10)", "Fatigue life in hours, N / (60 w_b q)", nLife / (60.0 * wb * q), "h");
    }

    /// <summary>The inputs of a bin: the transmitted load of the bin checked by clause 7.2.</summary>
    private AgmaInputs BinInputs(SpectrumBin bin)
    {
        AgmaInputs inputs = In.Clone();
        inputs.Goal = RatingGoal.CheckTransmittedLoad;
        inputs.LoadInput = LoadSpecification.PinionTorque;
        inputs.PinionTorque = bin.Torque;
        inputs.PinionSpeed = bin.Speed;
        return inputs;
    }

    /// <summary>The warnings of a bin's rating, without the checks of the safety factors against their minimums.</summary>
    private static void AddBinWarnings(List<string> warnings, List<string> binWarnings, int? bin)
    {
        foreach (string w in binWarnings)
        {
            if (w.StartsWith("Macropitting safety factor SH") || w.StartsWith("Bending safety factor SF"))
                continue;
            if (warnings.Contains(w) || warnings.Any(x => x.EndsWith(": " + w)))
                continue;
            warnings.Add(bin.HasValue ? "Bin " + bin.Value + ": " + w : w);
        }
    }

    private static string FormatHours(double hours) =>
        hours >= 1e7 ? hours.ToString("0.000E+0", CultureInfo.InvariantCulture) : hours.ToString("0.#", CultureInfo.InvariantCulture);

    /// <summary>
    /// The bins of a load spectrum: "T n t; T n t; ..." (pinion torque in Nm, pinion speed in rpm, time in h; the values
    /// separated by spaces, tabs or commas, the bins by semicolons or new lines; invariant culture). Throws
    /// <see cref="ArgumentException"/> with the bin at fault.
    /// </summary>
    public static List<SpectrumBin> ParseLoadSpectrum(string text)
    {
        var bins = new List<SpectrumBin>();
        string[] parts = (text ?? "").Split(new[] { ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string part in parts)
        {
            if (part.Trim().Length == 0)
                continue;
            string[] values = part.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            int k = bins.Count + 1;
            if (values.Length != 3)
                throw new ArgumentException(string.Format("Bin {0} of the load spectrum, \"{1}\", needs three values: the pinion torque (Nm), the pinion speed (rpm) and the time (h).", k, part.Trim()));
            double[] v = new double[3];
            for (int j = 0; j < 3; j++)
                if (!double.TryParse(values[j], NumberStyles.Float, CultureInfo.InvariantCulture, out v[j]) || double.IsNaN(v[j]) || double.IsInfinity(v[j]))
                    throw new ArgumentException(string.Format("Bin {0} of the load spectrum: \"{1}\" is not a number (use a point for the decimals).", k, values[j]));
            if (!(v[0] > 0.0)) throw new ArgumentException(string.Format("Bin {0} of the load spectrum: the pinion torque must be positive.", k));
            if (!(v[1] > 0.0)) throw new ArgumentException(string.Format("Bin {0} of the load spectrum: the pinion speed must be positive.", k));
            if (v[2] < 0.0) throw new ArgumentException(string.Format("Bin {0} of the load spectrum: the time cannot be negative.", k));
            bins.Add(new SpectrumBin { Torque = v[0], Speed = v[1], Hours = v[2] });
        }
        if (bins.Count == 0)
            throw new ArgumentException("The fatigue life needs a load spectrum: enter at least one bin (pinion torque Nm, pinion speed rpm, time h).");
        return bins;
    }

    /// <summary>The text of a load spectrum, as <see cref="ParseLoadSpectrum"/> reads it (round-trip precision).</summary>
    public static string FormatLoadSpectrum(IEnumerable<SpectrumBin> bins) =>
        string.Join("; ", bins.Select(bin => string.Format(CultureInfo.InvariantCulture, "{0:R} {1:R} {2:R}", bin.Torque, bin.Speed, bin.Hours)));

    private static string PinionProportionEquation(double b) => b <= 25.0 ? "Eq(32)" : b <= 432.0 ? "Eq(33)" : "Eq(34)";

    private void Validate()
    {
        if (In.z1 < 5 || In.z2 < 5) throw new ArgumentException("Tooth numbers must be at least 5.");
        if (In.z2 < In.z1) throw new ArgumentException("The pinion is the member with fewer teeth: enter z2 >= z1 (the gear ratio u is never less than 1.0, Eq 1).");
        if (In.NormalModule <= 0 || In.PinionSpeed <= 0 || In.FaceWidthPinion <= 0 || In.FaceWidthWheel <= 0 || In.LifeHours <= 0)
            throw new ArgumentException("Module, pinion speed, face widths and life must be positive.");
        if (In.HelixAngle < 0) throw new ArgumentException("Enter the helix angle as a positive magnitude.");
        if (In.Goal == RatingGoal.CheckTransmittedLoad && (In.LoadInput == LoadSpecification.PinionTorque ? In.PinionTorque : In.TransmittedPower) <= 0)
            throw new ArgumentException("The transmitted torque / power must be positive.");
        if (In.MinSafetyFactorPitting <= 0 || In.MinSafetyFactorBending <= 0)
            throw new ArgumentException("The required safety factors SHmin and SFmin must be positive.");
        if (In.KhMethod == LoadDistributionMethod.MethodBEmpirical && (In.BearingSpan <= 0 || In.PinionOffset < 0))
            throw new ArgumentException("Bearing span S must be positive and pinion offset S1 non-negative for KH method B.");
    }

    // ----------------------------------------------------------------- AGMA 908-B89
    private Agma908Result Calculate908(double mn, bool isSpur, double aw, double b, List<string> warnings)
    {
        T.Section("Clause 10.3: Geometry factors per AGMA 908-B89 (lengths in mm, angles in deg)");
        double cosBeta = Math.Cos(In.HelixAngle * Math.PI / 180.0);
        double da1 = In.TipDiameterPinion > 0 ? In.TipDiameterPinion : mn * (In.z1 / cosBeta + 2.0 * (In.AddendumCoefficient + In.x1));
        double da2 = In.TipDiameterWheel > 0 ? In.TipDiameterWheel : mn * (In.z2 / cosBeta + 2.0 * (In.AddendumCoefficient + In.x2));
        T.R("da1", In.TipDiameterPinion > 0 ? "input" : "", "Tip diameter, pinion", da1, "mm");
        T.R("da2", In.TipDiameterWheel > 0 ? "input" : "", "Tip diameter, wheel", da2, "mm");

        var input = new Agma908Input
        {
            Z1 = In.z1,
            Z2 = In.z2,
            HelixAngleDeg = In.HelixAngle,
            NormalPressureAngleDeg = In.NormalPressureAngle,
            CenterDistance = aw / mn,
            OutsideRadius1 = da1 / (2.0 * mn),
            OutsideRadius2 = da2 / (2.0 * mn),
            FaceWidth = b / mn,
            X1 = In.x1,
            X2 = In.x2,
            Thinning1 = In.ThinningPinion / mn,
            Thinning2 = In.ThinningWheel / mn,
            Tool1 = BuildTool(In.ToolTypePinion, In.CutterTeethPinion, In.ToolAddendumPinion, In.ToolTipRadiusPinion, In.ToolProtuberancePinion, In.ToolStockAllowancePinion, In.ToolShiftPinion),
            Tool2 = BuildTool(In.ToolTypeWheel, In.CutterTeethWheel, In.ToolAddendumWheel, In.ToolTipRadiusWheel, In.ToolProtuberanceWheel, In.ToolStockAllowanceWheel, In.ToolShiftWheel),
            SpurLoad = In.SpurLoadPoint,
            FullButtressing1 = In.ButtressingPinion,
            FullButtressing2 = In.ButtressingWheel,
        };
        var r = Agma908GeometryFactors.Calculate(input);
        const double deg = 180.0 / Math.PI;

        T.R("phi", "908 3.4", "Standard transverse pressure angle", r.TransversePressureAngle * deg, "deg");
        T.R("Rb1", "908 3.5", "Base radius, pinion", r.BaseRadius1 * mn, "mm");
        T.R("Rb2", "908 3.6", "Base radius, wheel", r.BaseRadius2 * mn, "mm");
        T.R("phi_r", "908 3.7", "Operating transverse pressure angle", r.OperatingPressureAngle * deg, "deg");
        T.R("pb", "908 3.8", "Transverse base pitch", r.BasePitch * mn, "mm");
        T.R("pN", "908 3.9", "Normal base pitch", r.NormalBasePitch * mn, "mm");
        T.R("psi_b", "908 3.10", "Base helix angle", r.BaseHelixAngle * deg, "deg");
        T.R("C6", "908 3.11", "Sixth distance along line of action", r.C6 * mn, "mm");
        T.R("C1", "908 3.12", "First distance along line of action", r.C1 * mn, "mm");
        T.R("C3", "908 3.13", "Third distance along line of action (pitch point)", r.C3 * mn, "mm");
        T.R("C4", "908 3.14", "Fourth distance along line of action (HPSTC)", r.C4 * mn, "mm");
        T.R("C5", "908 3.15", "Fifth distance along line of action", r.C5 * mn, "mm");
        T.R("C2", "908 3.16", "Second distance along line of action (LPSTC)", r.C2 * mn, "mm");
        T.R("Z", "908 3.17", "Active length of line of contact", r.ActiveLength * mn, "mm");
        double mp = T.R("mp", "908 3.18", "Transverse contact ratio (epsilon_alpha)", r.TransverseContactRatio, "");
        if (!isSpur)
        {
            T.R("px", "908 3.19", "Axial pitch", r.AxialPitch * mn, "mm");
            T.R("mF", "908 3.20", "Axial contact ratio (epsilon_beta)" + (r.IsLACR ? ", LACR helical" : ", conventional helical"), r.AxialContactRatio, "");
            T.R("Lmin", "908 3.22/23", "Minimum length of the lines of contact", r.MinContactLength * mn, "mm");
        }
        T.R("mN", "908 3.24-26", "Load sharing ratio", r.LoadSharingRatio, "");
        T.R("psi_r", "908 3.27", "Operating helix angle", r.OperatingHelixAngle * deg, "deg");
        T.R("phi_nr", "908 3.28", "Operating normal pressure angle", r.OperatingNormalPressureAngle * deg, "deg");
        T.R("d", "908 4.2", "Operating pitch diameter of pinion", r.OperatingPitchDiameter * mn, "mm");
        T.R("rho1", isSpur || r.IsLACR ? "908 4.6" : "908 4.4", "Radius of curvature of pinion profile at stress calculation point", r.Rho1 * mn, "mm");
        T.R("rho2", isSpur || r.IsLACR ? "908 4.7" : "908 4.5", "Radius of curvature of wheel profile at stress calculation point", r.Rho2 * mn, "mm");
        T.R("C_psi", r.IsLACR ? "908 4.8" : "908 4.11", "Helical overlap factor", r.HelicalOverlapFactor, "");
        double ZI = T.R("ZI", "908 4.1", "Geometry factor for macropitting resistance, I", r.I, "");

        LogBending("pinion", "1", r.Bending1, mn, warnings);
        LogBending("wheel", "2", r.Bending2, mn, warnings);
        if (r.IsLACR)
        {
            T.Note("LACR helical (mF <= 1): 908 clause 5 recommends also rating as a conventional helical with F slightly greater than px and using the more conservative J.");
            LogBending("pinion, as conventional helical", "1c", r.Bending1Conventional, mn, warnings);
            LogBending("wheel, as conventional helical", "2c", r.Bending2Conventional, mn, warnings);
        }
        T.R("YJ1", "908 5.1", "Geometry factor for bending strength, pinion, J1", r.J1, "");
        T.R("YJ2", "908 5.1", "Geometry factor for bending strength, wheel, J2", r.J2, "");

        // Clause 1.2 scope checks that need the geometry.
        if (isSpur && mp < 1.0)
            warnings.Add($"Spur transverse contact ratio {mp:0.###} is less than 1.0: ANSI/AGMA 2101-E25 does not apply (clause 1.2).");
        if (mp > 2.0)
            warnings.Add($"Transverse contact ratio {mp:0.###} is greater than 2.0: ANSI/AGMA 2101-E25 does not apply (clause 1.2; see AGMA 908 Appendix F).");
        foreach (var w in r.Warnings)
            warnings.Add("AGMA 908: " + w + " ANSI/AGMA 2101-E25 does not apply when interference or undercut above the start of active profile exists (clause 1.2).");

        TopLand("pinion", In.z1, da1, r.Bending1.GeneratingRackShift, mn, warnings);
        TopLand("wheel", In.z2, da2, r.Bending2.GeneratingRackShift, mn, warnings);
        return r;
    }

    private static Agma908Tool BuildTool(GeneratingToolType type, int cutterTeeth, double ha0, double rho, double delta, double us, double x0) => new()
    {
        ToothNumber = type == GeneratingToolType.RackTypeTool ? 10000.0 : cutterTeeth,
        Addendum = ha0,
        TipRadius = rho,
        Protuberance = delta,
        StockAllowance = us,
        AddendumModification = type == GeneratingToolType.RackTypeTool ? 0.0 : x0,
    };

    private void LogBending(string who, string idx, Agma908BendingResult j, double mn, List<string> warnings)
    {
        const double deg = 180.0 / Math.PI;
        T.Note($"Bending geometry factor, {who}: {j.Case}");
        T.R("n" + idx, "908 5.2", "Virtual tooth number", j.VirtualTeeth, "");
        T.R("r_nb" + idx, "908 5.4", "Virtual base radius", j.VirtualBaseRadius * mn, "mm");
        T.R("r_na" + idx, "908 5.5", "Virtual outside radius", j.VirtualOutsideRadius * mn, "mm");
        T.R("tan phi_nW" + idx, "908 5.10-18", "Tangent of pressure angle at load point", j.TanPhiNW, "");
        T.R("x_g" + idx, "908 5.19", "Generating rack shift coefficient", j.GeneratingRackShift, "");
        T.R("s_n" + idx, "908 5.21", "Normal circular tooth thickness at reference diameter", j.ToothThickness * mn, "mm");
        T.R("phi_nL" + idx, "908 5.27", "Load angle", j.LoadAngle * deg, "deg");
        T.R("r_nL" + idx, "908 5.28", "Virtual load radius", j.LoadRadius * mn, "mm");
        T.R("n0" + idx, "908 5.29", "Virtual tooth number of tool", j.ToolVirtualTeeth, "");
        T.R("r^s_n0" + idx, "908 5.39", "Radius to center of tool tip radius", j.ToolCenterRadius * mn, "mm");
        T.R("lambda_ns/2 " + idx, "908 5.47", "Angle to center of tool tip radius", j.HalfAngleToToolCenter * deg, "deg");
        T.R("phi''_n" + idx, "908 5.48-50", "Generating pressure angle", j.GeneratingPressureAngle * deg, "deg");
        T.R("r''_n" + idx, "908 5.51", "Generating pitch radius of virtual gear", j.GeneratingPitchRadius * mn, "mm");
        T.R("r''_n0" + idx, "908 5.52", "Generating pitch radius of virtual tool", j.ToolGeneratingPitchRadius * mn, "mm");
        T.R("alpha_n" + idx, "908 5.65", j.UsedBisection ? "Angle of surface normal at critical point (Newton failed, bisection fallback)" : $"Angle of surface normal at critical point ({j.Iterations} Newton iterations)", j.AlphaN * deg, "deg");
        T.R("s_F" + idx, "908 5.73", "Tooth thickness at critical section", j.CriticalThickness * mn, "mm");
        T.R("h_F" + idx, "908 5.62", "Height of Lewis parabola", j.ParabolaHeight * mn, "mm");
        T.R("rho_F" + idx, "908 5.68", "Minimum radius of curvature of fillet", j.FilletRadius * mn, "mm");
        T.R("C_h" + idx, "908 5.69/70", "Helical factor", j.HelicalFactor, "");
        T.R("K_f" + idx, "908 5.72", "Stress correction factor", j.StressCorrection, "");
        T.R("K_psi" + idx, "908 5.77", "Helix angle factor", j.HelixAngleFactor, "");
        T.R("Y" + idx, "908 5.78", "Tooth form factor", j.ToothForm, "");
        T.R("J" + idx, "908 5.1", "Bending strength geometry factor", j.J, "");
        T.R("x_g min" + idx, "908 7.8", "Minimum generating rack shift to avoid undercut (rack criterion)", j.MinGeneratingRackShift, "");
    }

    /// <summary>Normal tooth thickness at the tip, from the generated tooth thickness (908 Eq 5.21).</summary>
    private void TopLand(string who, int z, double da, double xg, double mn, List<string> warnings)
    {
        double beta = In.HelixAngle * Math.PI / 180.0;
        double alphaN = In.NormalPressureAngle * Math.PI / 180.0;
        double alphaT = Math.Atan(Math.Tan(alphaN) / Math.Cos(beta));
        double d = z * mn / Math.Cos(beta);
        double db = d * Math.Cos(alphaT);
        double st = (Math.PI / 2.0 + 2.0 * xg * Math.Tan(alphaN)) * mn / Math.Cos(beta);
        double alphaAt = Math.Acos(db / da);
        double sat = da * (st / d + (Math.Tan(alphaT) - alphaT) - (Math.Tan(alphaAt) - alphaAt));
        double betaA = Math.Atan(Math.Tan(beta) * da / d);
        double sna = T.R("s_na " + who, "", $"Normal tooth thickness at the tip, {who}", sat * Math.Cos(betaA), "mm");
        if (sna <= 0.0)
            warnings.Add($"The {who} teeth are pointed (s_na = {sna:0.000} mm): ANSI/AGMA 2101-E25 does not apply (clause 1.2).");
        else if (sna < 0.3 * mn)
            T.Note($"The {who} top land {sna:0.000} mm is below 0.3 mn = {0.3 * mn:0.000} mm (AGMA 908 clause 7.8 table criterion).");
    }

    // ----------------------------------------------------------------- clause 12
    private double ElasticCoefficient(List<string> warnings)
    {
        if (In.ZESource == ElasticCoefficientSource.UserValue)
            return T.R("ZE", "input", "Elastic coefficient", In.ZEUser, "(N/mm2)^0.5");
        if (In.ZESource == ElasticCoefficientSource.Table2 && TryElasticCoefficientTable2(In.MaterialPinion, In.MaterialWheel, out double zeTable))
            return T.R("ZE", "Table 2", "Elastic coefficient for the material pair", zeTable, "(N/mm2)^0.5");
        if (In.ZESource == ElasticCoefficientSource.Table2)
            T.Note("Table 2 does not list this material pair: Z_E calculated from Eq 26.");
        var (e1, nu1) = ElasticProperties(In.MaterialPinion, In.ElasticModulusPinion, In.PoissonPinion, "pinion");
        var (e2, nu2) = ElasticProperties(In.MaterialWheel, In.ElasticModulusWheel, In.PoissonWheel, "wheel");
        T.R("E1", "", "Modulus of elasticity, pinion", e1, "N/mm2");
        T.R("nu1", "", "Poisson's ratio, pinion", nu1, "");
        T.R("E2", "", "Modulus of elasticity, wheel", e2, "N/mm2");
        T.R("nu2", "", "Poisson's ratio, wheel", nu2, "");
        double raw = ElasticCoefficientEq26(e1, nu1, e2, nu2);
        T.R("ZE unrounded", "Eq(26)", "Elastic coefficient before rounding", raw, "(N/mm2)^0.5");
        return T.R("ZE", "Eq(26)", "Elastic coefficient, rounded to three significant figures", RoundToSignificantFigures(raw, 3), "(N/mm2)^0.5");
    }

    private static (double e, double nu) ElasticProperties(GearMaterial m, double eUser, double nuUser, string who)
    {
        TryTable2Properties(m, out double eTab, out double nuTab);
        double e = eUser > 0 ? eUser : eTab;
        double nu = nuUser > 0 ? nuUser : nuTab;
        if (e <= 0 || nu <= 0)
            throw new ArgumentException($"Table 2 does not list E and Poisson's ratio for the {who} material ({m}); enter them in the {who} data or use a user Z_E.");
        return (e, nu);
    }

    // ----------------------------------------------------------------- clause 13
    private double HardnessRatio(double u)
    {
        if (In.ZWUser > 0)
            return T.R("ZW", "input", "Hardness ratio factor", In.ZWUser, "");
        if (In.MaterialPinion == GearMaterial.SteelThroughHardened && In.MaterialWheel == GearMaterial.SteelThroughHardened)
        {
            double ratio = T.R("HB1/HB2", "", "Hardness ratio, through hardened pinion and wheel", In.HardnessHBPinion / In.HardnessHBWheel, "");
            double A = T.R("A", "Eq(28)", "Empirical constant for hardness ratio factor" + (ratio < 1.2 ? " (HB1/HB2 < 1.2)" : ratio > 1.7 ? " (HB1/HB2 > 1.7)" : ""),
                HardnessRatioConstantA(In.HardnessHBPinion, In.HardnessHBWheel), "");
            return T.R("ZW", "Eq(27)", "Hardness ratio factor, 13.1", 1.0 + A * (u - 1.0), "");
        }
        if (IsSurfaceHardenedSteel(In.MaterialPinion) && In.MaterialWheel == GearMaterial.SteelThroughHardened)
        {
            if (In.HardnessHRCPinion >= 48.0 && In.HardnessHBWheel >= 180.0 && In.HardnessHBWheel <= 400.0)
                return T.R("ZW", "Eq(29)", $"Hardness ratio factor, 13.2 (Ra1 = {In.RoughnessPinion:0.###} um{(In.RoughnessPinion > 1.6 ? " > 1.6" : "")})",
                    HardnessRatioFactorSurfaceHardenedPinion(In.HardnessHBWheel, In.RoughnessPinion), "");
            T.Note("13.2 requires a pinion of 48 HRC or harder and a wheel of 180 to 400 HBW; Z_W = 1.0.");
        }
        return T.R("ZW", "Clause 13", "Hardness ratio factor (13.1 / 13.2 not applicable)", 1.0, "");
    }

    // ----------------------------------------------------------------- clause 15
    private double AllowableContact(bool pinion, List<string> warnings)
    {
        string who = pinion ? "Pinion" : "Wheel";
        double user = pinion ? In.SigmaHPUserPinion : In.SigmaHPUserWheel;
        string sym = pinion ? "sigma_HP1" : "sigma_HP2";
        if (user > 0) return T.R(sym, "input", $"Allowable contact stress number, {who.ToLower()}", user, "N/mm2");
        var notes = new List<string>();
        double v = AllowableContactStress(pinion ? In.MaterialPinion : In.MaterialWheel, pinion ? In.GradePinion : In.GradeWheel,
            pinion ? In.HardnessHBPinion : In.HardnessHBWheel, pinion ? In.HardnessHRCPinion : In.HardnessHRCWheel,
            pinion ? In.HardnessHR15NPinion : In.HardnessHR15NWheel, pinion ? In.UpperAllowablePinion : In.UpperAllowableWheel, notes, warnings, who);
        return T.R(sym, "Table 4/6", $"Allowable contact stress number, {who.ToLower()}: {string.Join("; ", notes)}", v, "N/mm2");
    }

    private double AllowableBending(bool pinion, List<string> warnings)
    {
        string who = pinion ? "Pinion" : "Wheel";
        double user = pinion ? In.SigmaFPUserPinion : In.SigmaFPUserWheel;
        string sym = pinion ? "sigma_FP1" : "sigma_FP2";
        if (user > 0) return T.R(sym, "input", $"Allowable bending stress number, {who.ToLower()}", user, "N/mm2");
        var notes = new List<string>();
        double v = AllowableBendingStress(pinion ? In.MaterialPinion : In.MaterialWheel, pinion ? In.GradePinion : In.GradeWheel,
            pinion ? In.HardnessHBPinion : In.HardnessHBWheel, pinion ? In.HardnessHRCPinion : In.HardnessHRCWheel,
            pinion ? In.BainiteGrade3Pinion : In.BainiteGrade3Wheel, pinion ? In.UpperAllowablePinion : In.UpperAllowableWheel, notes, warnings, who);
        return T.R(sym, "Table 5/7", $"Allowable bending stress number, {who.ToLower()}: {string.Join("; ", notes)}", v, "N/mm2");
    }
}
