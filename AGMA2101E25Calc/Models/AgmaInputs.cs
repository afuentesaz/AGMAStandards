using System.ComponentModel;

namespace AGMA2101E25Calc.Models;

/// <summary>
/// All input data for ANSI/AGMA 2101-E25 (service factor method 7.1 and safety factor method 7.2)
/// with geometry factors per AGMA 908-B89. Bound to a PropertyGrid. SI units throughout
/// (mm, N, Nm, kW, rpm, deg, um). Default values reproduce the Annex B illustrative example.
/// Subscript 1 refers to the pinion, 2 to the wheel. External gears only.
/// </summary>
public class AgmaInputs
{
    // ----------------------------------------------------------------- 1. Gear pair geometry
    [Category("01. Gear pair geometry")]
    [DisplayName("Normal module, mn (mm)")]
    public double NormalModule { get; set; } = 4.5;

    [Category("01. Gear pair geometry")]
    [DisplayName("Normal pressure angle, alpha_n (deg)")]
    public double NormalPressureAngle { get; set; } = 20.0;

    [Category("01. Gear pair geometry")]
    [DisplayName("Helix angle at reference diameter, beta (deg)")]
    [Description("0 for spur gears. Clause 1.2: the standard does not apply above 50 deg.")]
    public double HelixAngle { get; set; } = 11.0;

    [Category("01. Gear pair geometry")]
    [DisplayName("Number of pinion teeth, z1")]
    public int z1 { get; set; } = 15;

    [Category("01. Gear pair geometry")]
    [DisplayName("Number of wheel teeth, z2")]
    [Description("External gears only. z2 >= z1 (the gear ratio u is never less than 1.0).")]
    public int z2 { get; set; } = 63;

    [Category("01. Gear pair geometry")]
    [DisplayName("Face width of the pinion, b1 (mm)")]
    public double FaceWidthPinion { get; set; } = 76.2;

    [Category("01. Gear pair geometry")]
    [DisplayName("Face width of the wheel, b2 (mm)")]
    [Description("The net face width of the narrowest member, b = min(b1, b2), Eq 3, is used in rating and as the AGMA 908 effective face width F.")]
    public double FaceWidthWheel { get; set; } = 76.2;

    [Category("01. Gear pair geometry")]
    [DisplayName("Operating center distance, aw (mm)")]
    [Description("0 = tight-mesh (zero backlash) center distance computed from x1 + x2. Eq 2 uses aw for the operating pitch diameter dw1.")]
    public double OperatingCenterDistance { get; set; } = 182.88;

    [Category("01. Gear pair geometry")]
    [DisplayName("Addendum coefficient, ha* (both gears)")]
    [Description("Used for the default tip diameters da = mn (z/cos(beta) + 2 (ha* + x)). Override with the tip diameters below.")]
    public double AddendumCoefficient { get; set; } = 1.0;

    // ----------------------------------------------------------------- 2. Operating conditions
    [Category("02. Operating conditions")]
    [DisplayName("Pinion speed, n1 (rpm)")]
    public double PinionSpeed { get; set; } = 1450.0;

    [Category("02. Operating conditions")]
    [DisplayName("Rating goal")]
    [Description("CheckTransmittedLoad: the safety and service factors of the load below. RatedLoadForRequiredSafetyFactors: the power or pinion torque (see 'Load as') the gear set can transmit at the pinion speed above with the required safety factors SHmin and SFmin (clause 7.2 solved for the load; the load values below are not used), and the rating at that load.")]
    public RatingGoal Goal { get; set; } = RatingGoal.CheckTransmittedLoad;

    [Category("02. Operating conditions")]
    [DisplayName("Load as (power or pinion torque)")]
    [Description("CheckTransmittedLoad: whether the load is given by the power or by the pinion torque below. RatedLoadForRequiredSafetyFactors: whether the result is the power or the pinion torque (the other is given too).")]
    public LoadSpecification LoadInput { get; set; } = LoadSpecification.PinionTorque;

    [Category("02. Operating conditions")]
    [DisplayName("Transmitted power, P (kW)")]
    [Description("Used when the goal is CheckTransmittedLoad and 'Load as' = TransmittedPower. The service factor method compares the rated power at unity service factor with this power (Eq 4, 5).")]
    public double TransmittedPower { get; set; } = 226.854406;

    [Category("02. Operating conditions")]
    [DisplayName("Transmitted pinion torque, T (Nm)")]
    [Description("Used when the goal is CheckTransmittedLoad and 'Load as' = PinionTorque.")]
    public double PinionTorque { get; set; } = 1494.0;

    [Category("02. Operating conditions")]
    [DisplayName("Design life, L (hours)")]
    [Description("Eq 49, number of stress cycles n_L = 60 L n q.")]
    public double LifeHours { get; set; } = 10000.0;

    [Category("02. Operating conditions")]
    [DisplayName("Stress cycle curve beyond the knee")]
    [Description("Tables 12 and 13 / Figures 17 and 18: general application (upper curves) or critical service (lower curves).")]
    public StressCycleCurve CycleCurve { get; set; } = StressCycleCurve.GeneralApplication;

    [Category("02. Operating conditions")]
    [DisplayName("Overload factor, KO")]
    [Description("Clause 8. Used by the safety factor method only; the service factor method assumes KO = 1.0 (7.1).")]
    public double OverloadFactor { get; set; } = 1.0;

    [Category("02. Operating conditions")]
    [DisplayName("Mesh load factor, K_gamma")]
    [Description("Clause 21, Eq 53. 1.0 for all single power path meshes.")]
    public double MeshLoadFactor { get; set; } = 1.0;

    [Category("02. Operating conditions")]
    [DisplayName("Temperature factor, Y_theta")]
    [Description("Clause 20: 1.0 up to 100 deg C oil or gear temperature; greater than 1.0 above.")]
    public double TemperatureFactor { get; set; } = 1.0;

    [Category("02. Operating conditions")]
    [DisplayName("Reliability requirement")]
    [Description("Table 14 reliability factor Y_Z. Used by the safety factor method only (7.1 assumes Y_Z = 1.0).")]
    public ReliabilityRequirement Reliability { get; set; } = ReliabilityRequirement.FewerThanOneFailureIn100;

    [Category("02. Operating conditions")]
    [DisplayName("Reliability factor Y_Z (if UserValue)")]
    public double ReliabilityFactorUser { get; set; } = 1.0;

    [Category("02. Operating conditions")]
    [DisplayName("Required safety factor, macropitting, SHmin")]
    [Description("Clause 7.2.1. Agreed between manufacturer and purchaser; recommended values are beyond the scope of the standard. The minimum the safety factors are checked against, and the safety factor the rated power is computed for.")]
    public double MinSafetyFactorPitting { get; set; } = 1.0;

    [Category("02. Operating conditions")]
    [DisplayName("Required safety factor, bending, SFmin")]
    [Description("Clause 7.2.1. The minimum the safety factors are checked against, and the safety factor the rated power is computed for.")]
    public double MinSafetyFactorBending { get; set; } = 1.0;

    // ----------------------------------------------------------------- 2b. Load spectrum (clause 7.3, Annex D)
    [Category("02b. Load spectrum (Miner's rule, clause 7.3)")]
    [DisplayName("Load spectrum bins (T Nm, n1 rpm, t h; ...)")]
    [Description("Used when the goal is Fatigue Life For Load Spectrum: one bin per load level, the pinion torque T (Nm), the pinion speed n1 (rpm) and the time t (h), separated by spaces, the bins by semicolons, e.g. \"700 3052 1000; 800 2670 2000\". The design life L above is the required life the Miner life is compared with.")]
    public string LoadSpectrum { get; set; } = "";

    // ----------------------------------------------------------------- 3. Dynamic factor
    [Category("03. Dynamic factor (clause 11)")]
    [DisplayName("K_V method")]
    public DynamicFactorMethod KvMethod { get; set; } = DynamicFactorMethod.MethodBFromAv;

    [Category("03. Dynamic factor (clause 11)")]
    [DisplayName("Flank tolerance class number, Av")]
    [Description("Integer 6 to 12 (Figure 1). The worst (highest) of pinion and wheel per ANSI/AGMA ISO 1328-1.")]
    public int Av { get; set; } = 6;

    [Category("03. Dynamic factor (clause 11)")]
    [DisplayName("Single pitch deviation, pinion, fpt1 (um)")]
    [Description("Used when K_V method = MethodBFromPitchDeviation (Eq 24).")]
    public double SinglePitchDeviationPinion { get; set; } = 10.0;

    [Category("03. Dynamic factor (clause 11)")]
    [DisplayName("Single pitch deviation, wheel, fpt2 (um)")]
    public double SinglePitchDeviationWheel { get; set; } = 10.0;

    [Category("03. Dynamic factor (clause 11)")]
    [DisplayName("Dynamic factor K_V (if UserValue)")]
    [Description("Method A value, or 1.02 to 1.11 for 'very accurate gearing' (11.2.1).")]
    public double KvUser { get; set; } = 1.05;

    // ----------------------------------------------------------------- 4. Load distribution
    [Category("04. Load distribution factor (clause 14)")]
    [DisplayName("K_H method")]
    public LoadDistributionMethod KhMethod { get; set; } = LoadDistributionMethod.MethodBEmpirical;

    [Category("04. Load distribution factor (clause 14)")]
    [DisplayName("Lead modification (K_Hmc)")]
    public LeadModification Lead { get; set; } = LeadModification.CrownedOrLeadCorrected;

    [Category("04. Load distribution factor (clause 14)")]
    [DisplayName("Mesh alignment curve (K_Hma)")]
    [Description("Figure 6 / Table 3.")]
    public MeshAlignmentCurve AlignmentCurve { get; set; } = MeshAlignmentCurve.Curve3PrecisionEnclosed;

    [Category("04. Load distribution factor (clause 14)")]
    [DisplayName("Adjusted at assembly or lapped (K_He = 0.8)")]
    [Description("K_He = 0.80 when the gearing is adjusted at assembly and/or lapped; 1.0 otherwise.")]
    public bool AdjustedOrLapped { get; set; } = false;

    [Category("04. Load distribution factor (clause 14)")]
    [DisplayName("Bearing span, S (mm)")]
    [Description("Figure 5. Distance between the bearing center lines.")]
    public double BearingSpan { get; set; } = 331.72;

    [Category("04. Load distribution factor (clause 14)")]
    [DisplayName("Pinion offset, S1 (mm)")]
    [Description("Figure 5. Distance from the bearing span centerline to the pinion mid-face. K_Hpm = 1.1 when S1/S >= 0.175.")]
    public double PinionOffset { get; set; } = 67.56;

    [Category("04. Load distribution factor (clause 14)")]
    [DisplayName("Load distribution factor K_H (if UserValue)")]
    public double KhUser { get; set; } = 1.2;

    // ----------------------------------------------------------------- 5. Geometry factors and elastic coefficient
    [Category("05. Geometry factors and Z_E")]
    [DisplayName("Geometry factors Z_I, Y_J")]
    [Description("CalculatedPerAgma908 uses the AGMA 908-B89 analytical method with the tooth and tool data below. UserSpecified uses the values below (the Annex B example gives Z_I and Y_J as inputs).")]
    public GeometryFactorSource GeometryFactors { get; set; } = GeometryFactorSource.UserSpecified;

    [Category("05. Geometry factors and Z_E")]
    [DisplayName("Spur gear load point for Y_J")]
    [Description("AGMA 908 clause 5.2: HPSTC when the teeth share load, Tip when they cannot (Table 5-1). Helical gears ignore this.")]
    public Calc.SpurLoadPoint SpurLoadPoint { get; set; } = Calc.SpurLoadPoint.HPSTC;

    [Category("05. Geometry factors and Z_E")]
    [DisplayName("Z_I (if UserSpecified)")]
    public double ZIUser { get; set; } = 0.231;

    [Category("05. Geometry factors and Z_E")]
    [DisplayName("Y_J1, pinion (if UserSpecified)")]
    public double YJ1User { get; set; } = 0.505;

    [Category("05. Geometry factors and Z_E")]
    [DisplayName("Y_J2, wheel (if UserSpecified)")]
    public double YJ2User { get; set; } = 0.521;

    [Category("05. Geometry factors and Z_E")]
    [DisplayName("Elastic coefficient Z_E source")]
    public ElasticCoefficientSource ZESource { get; set; } = ElasticCoefficientSource.Table2;

    [Category("05. Geometry factors and Z_E")]
    [DisplayName("Z_E ((N/mm^2)^0.5, if UserValue)")]
    public double ZEUser { get; set; } = 190.0;

    [Category("05. Geometry factors and Z_E")]
    [DisplayName("Hardness ratio factor Z_W (0 = calculated)")]
    [Description("Clause 13. 0 = calculated from 13.1 (through hardened pair) or 13.2 (surface hardened pinion, >= 48 HRC, with through hardened wheel, 180-400 HBW); otherwise 1.0.")]
    public double ZWUser { get; set; } = 0.0;

    // ----------------------------------------------------------------- 6. Pinion
    [Category("06. Pinion")]
    [DisplayName("Profile shift coefficient, x1")]
    [Description("Addendum modification coefficient at zero backlash (AGMA 908 Eq 5.20).")]
    public double x1 { get; set; } = 0.0;

    [Category("06. Pinion")]
    [DisplayName("Tip diameter, da1 (mm, 0 = from ha* and x1)")]
    public double TipDiameterPinion { get; set; } = 0.0;

    [Category("06. Pinion")]
    [DisplayName("Tooth thinning for backlash, Delta_sn1 (mm)")]
    [Description("Normal circular tooth thinning; generating rack shift x_g = x - Delta_sn / (2 tan alpha_n), AGMA 908 Eq 5.19.")]
    public double ThinningPinion { get; set; } = 0.0;

    [Category("06. Pinion")]
    [DisplayName("Generating tool")]
    public GeneratingToolType ToolTypePinion { get; set; } = GeneratingToolType.RackTypeTool;

    [Category("06. Pinion")]
    [DisplayName("Shaper cutter tooth number, nc (if ShaperCutter)")]
    public int CutterTeethPinion { get; set; } = 30;

    [Category("06. Pinion")]
    [DisplayName("Tool addendum coefficient, ha0/mn")]
    [Description("Nominal tool addendum (= gear dedendum at x = 0). Basic rack whole depth factor = 1 + ha0/mn, e.g. 1.25 for a 2.25 whole depth rack.")]
    public double ToolAddendumPinion { get; set; } = 1.25;

    [Category("06. Pinion")]
    [DisplayName("Tool tip radius coefficient, rho_a0/mn")]
    public double ToolTipRadiusPinion { get; set; } = 0.25;

    [Category("06. Pinion")]
    [DisplayName("Tool protuberance coefficient, delta_0/mn")]
    public double ToolProtuberancePinion { get; set; } = 0.0;

    [Category("06. Pinion")]
    [DisplayName("Grinding stock per side coefficient, u_s/mn")]
    [Description("Pre-grind tool: AGMA 908 Eq 5.37/5.38 (x0 = 0 and the protuberance is reduced by u_s cos alpha_n). 0 = finishing tool.")]
    public double ToolStockAllowancePinion { get; set; } = 0.0;

    [Category("06. Pinion")]
    [DisplayName("Tool addendum modification coefficient, x0")]
    [Description("AGMA 908 Eq 5.35: positive for a new shaper cutter, negative for a used one, 0 for a hob.")]
    public double ToolShiftPinion { get; set; } = 0.0;

    [Category("06. Pinion")]
    [DisplayName("Full buttressing (Ch +10%)")]
    [Description("AGMA 908 clause 5.10: conventional helical gears whose face extends at least one addendum beyond the loaded portion.")]
    public bool ButtressingPinion { get; set; } = false;

    [Category("06. Pinion")]
    [DisplayName("Material / heat treatment")]
    public GearMaterial MaterialPinion { get; set; } = GearMaterial.SteelCarburizedAndHardened;

    [Category("06. Pinion")]
    [DisplayName("Metallurgical grade")]
    public MaterialGrade GradePinion { get; set; } = MaterialGrade.Grade2;

    [Category("06. Pinion")]
    [DisplayName("Brinell hardness, HB (HBW)")]
    [Description("Through hardened steel: surface hardness (Figures 7, 8; Y_N curve; Z_W). Nitrided steel: core hardness (Figures 9, 10). Also used as the pinion hardness in Eq 28.")]
    public double HardnessHBPinion { get; set; } = 300.0;

    [Category("06. Pinion")]
    [DisplayName("Surface hardness, HRC")]
    [Description("Flame/induction hardened or carburized steel (Table 4 rows and minimums), and the >= 48 HRC check of clause 13.2.")]
    public double HardnessHRCPinion { get; set; } = 58.0;

    [Category("06. Pinion")]
    [DisplayName("Surface hardness, HR15N (nitrided)")]
    public double HardnessHR15NPinion { get; set; } = 90.0;

    [Category("06. Pinion")]
    [DisplayName("Carburized Grade 2 meets Grade 3 bainite levels")]
    [Description("Table 5 note 6: sigma_FP = 485 N/mm^2 instead of 450.")]
    public bool BainiteGrade3Pinion { get; set; } = false;

    [Category("06. Pinion")]
    [DisplayName("Iron/bronze: use upper allowable value")]
    [Description("Tables 6 and 7 note 3: the lower values are for general design purposes.")]
    public bool UpperAllowablePinion { get; set; } = false;

    [Category("06. Pinion")]
    [DisplayName("Allowable contact stress sigma_HP1 (0 = from clause 15)")]
    public double SigmaHPUserPinion { get; set; } = 0.0;

    [Category("06. Pinion")]
    [DisplayName("Allowable bending stress sigma_FP1 (0 = from clause 15)")]
    public double SigmaFPUserPinion { get; set; } = 0.0;

    [Category("06. Pinion")]
    [DisplayName("Modulus of elasticity, E1 (N/mm^2, 0 = Table 2)")]
    public double ElasticModulusPinion { get; set; } = 0.0;

    [Category("06. Pinion")]
    [DisplayName("Poisson's ratio, nu1 (0 = Table 2)")]
    public double PoissonPinion { get; set; } = 0.0;

    [Category("06. Pinion")]
    [DisplayName("Surface roughness, Ra1 (um)")]
    [Description("Pinion flank roughness, used by Z_W Eq 29 only.")]
    public double RoughnessPinion { get; set; } = 0.8;

    [Category("06. Pinion")]
    [DisplayName("Contacts per revolution, q1")]
    public double ContactsPerRevPinion { get; set; } = 1.0;

    [Category("06. Pinion")]
    [DisplayName("Fully reverse loaded (idler), Y_I1 = 0.7")]
    [Description("Clause 22.")]
    public bool ReverseLoadedPinion { get; set; } = false;

    [Category("06. Pinion")]
    [DisplayName("Backup ratio, mB1 (0 = solid)")]
    [Description("Annex A, Eq A.1: rim thickness below the root / whole depth. Y_B = 1.0 for mB >= 1.2.")]
    public double BackupRatioPinion { get; set; } = 0.0;

    // ----------------------------------------------------------------- 7. Wheel
    [Category("07. Wheel")]
    [DisplayName("Profile shift coefficient, x2")]
    public double x2 { get; set; } = 0.0;

    [Category("07. Wheel")]
    [DisplayName("Tip diameter, da2 (mm, 0 = from ha* and x2)")]
    public double TipDiameterWheel { get; set; } = 0.0;

    [Category("07. Wheel")]
    [DisplayName("Tooth thinning for backlash, Delta_sn2 (mm)")]
    public double ThinningWheel { get; set; } = 0.0;

    [Category("07. Wheel")]
    [DisplayName("Generating tool")]
    public GeneratingToolType ToolTypeWheel { get; set; } = GeneratingToolType.RackTypeTool;

    [Category("07. Wheel")]
    [DisplayName("Shaper cutter tooth number, nc (if ShaperCutter)")]
    public int CutterTeethWheel { get; set; } = 30;

    [Category("07. Wheel")]
    [DisplayName("Tool addendum coefficient, ha0/mn")]
    public double ToolAddendumWheel { get; set; } = 1.25;

    [Category("07. Wheel")]
    [DisplayName("Tool tip radius coefficient, rho_a0/mn")]
    public double ToolTipRadiusWheel { get; set; } = 0.25;

    [Category("07. Wheel")]
    [DisplayName("Tool protuberance coefficient, delta_0/mn")]
    public double ToolProtuberanceWheel { get; set; } = 0.0;

    [Category("07. Wheel")]
    [DisplayName("Grinding stock per side coefficient, u_s/mn")]
    public double ToolStockAllowanceWheel { get; set; } = 0.0;

    [Category("07. Wheel")]
    [DisplayName("Tool addendum modification coefficient, x0")]
    public double ToolShiftWheel { get; set; } = 0.0;

    [Category("07. Wheel")]
    [DisplayName("Full buttressing (Ch +10%)")]
    public bool ButtressingWheel { get; set; } = false;

    [Category("07. Wheel")]
    [DisplayName("Material / heat treatment")]
    public GearMaterial MaterialWheel { get; set; } = GearMaterial.SteelCarburizedAndHardened;

    [Category("07. Wheel")]
    [DisplayName("Metallurgical grade")]
    public MaterialGrade GradeWheel { get; set; } = MaterialGrade.Grade2;

    [Category("07. Wheel")]
    [DisplayName("Brinell hardness, HB (HBW)")]
    [Description("Through hardened steel: surface hardness (Figures 7, 8; Y_N curve; Z_W). Nitrided steel: core hardness (Figures 9, 10).")]
    public double HardnessHBWheel { get; set; } = 300.0;

    [Category("07. Wheel")]
    [DisplayName("Surface hardness, HRC")]
    public double HardnessHRCWheel { get; set; } = 58.0;

    [Category("07. Wheel")]
    [DisplayName("Surface hardness, HR15N (nitrided)")]
    public double HardnessHR15NWheel { get; set; } = 90.0;

    [Category("07. Wheel")]
    [DisplayName("Carburized Grade 2 meets Grade 3 bainite levels")]
    public bool BainiteGrade3Wheel { get; set; } = false;

    [Category("07. Wheel")]
    [DisplayName("Iron/bronze: use upper allowable value")]
    public bool UpperAllowableWheel { get; set; } = false;

    [Category("07. Wheel")]
    [DisplayName("Allowable contact stress sigma_HP2 (0 = from clause 15)")]
    public double SigmaHPUserWheel { get; set; } = 0.0;

    [Category("07. Wheel")]
    [DisplayName("Allowable bending stress sigma_FP2 (0 = from clause 15)")]
    public double SigmaFPUserWheel { get; set; } = 0.0;

    [Category("07. Wheel")]
    [DisplayName("Modulus of elasticity, E2 (N/mm^2, 0 = Table 2)")]
    public double ElasticModulusWheel { get; set; } = 0.0;

    [Category("07. Wheel")]
    [DisplayName("Poisson's ratio, nu2 (0 = Table 2)")]
    public double PoissonWheel { get; set; } = 0.0;

    [Category("07. Wheel")]
    [DisplayName("Contacts per revolution, q2")]
    public double ContactsPerRevWheel { get; set; } = 1.0;

    [Category("07. Wheel")]
    [DisplayName("Fully reverse loaded (idler), Y_I2 = 0.7")]
    public bool ReverseLoadedWheel { get; set; } = false;

    [Category("07. Wheel")]
    [DisplayName("Backup ratio, mB2 (0 = solid)")]
    public double BackupRatioWheel { get; set; } = 0.0;

    /// <summary>A copy of every input (all of them are values).</summary>
    public AgmaInputs Clone() => (AgmaInputs)MemberwiseClone();
}

/// <summary>One bin of a load spectrum (Annex D): pinion torque [Nm], pinion speed [rpm] and time [h].</summary>
public class SpectrumBin
{
    public double Torque;
    public double Speed;
    public double Hours;
}
