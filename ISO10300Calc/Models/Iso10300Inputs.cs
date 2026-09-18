using System.ComponentModel;

namespace ISO10300Calc.Models;

/// <summary>
/// All input data for ISO 10300:2023 (parts 1-3), Method B1, restricted like AGMA2003D19Calc
/// to bevel gears without hypoid offset (a = 0, shaft angle = 90 deg). Bound to a PropertyGrid.
/// SI units throughout (mm, N/mm^2, kW, rpm, deg), matching the standard's own SI equations.
/// The pinion/wheel tooth geometry (clause groups 04-05) is the ISO 23509 output data that
/// ISO 10300-1 Annex A itself takes as a given starting point - not computed by this tool,
/// exactly as AGMA2003D19Calc takes its own Annex C(M) "initial data" as given inputs.
/// </summary>
public class Iso10300Inputs
{
    // ----------------------------------------------------------------- 1. Gear type & rating
    [Category("01. Gear type and rating basics")]
    [DisplayName("Gear type")]
    [Description("Governs whether the lengthwise curvature factor KF0 (10300-1, 8.4.4) and bending-spiral-angle factor YBS (10300-3, 6.4.4) apply; both default to 1.0 for straight/Zerol bevel gears.")]
    public GearType GearType { get; set; } = GearType.SpiralBevel;

    [Category("01. Gear type and rating basics")]
    [DisplayName("Shaft angle, Sigma (deg)")]
    [Description("For reference/reporting only. This tool's formulas apply exclusively to non-offset bevel gears (hypoid offset a = 0); use 90 deg unless analyzing a non-offset crossed-axis design.")]
    public double ShaftAngle { get; set; } = 90.0;

    [Category("01. Gear type and rating basics")]
    [DisplayName("Number of pinion teeth, z1")]
    public int z1 { get; set; } = 14;

    [Category("01. Gear type and rating basics")]
    [DisplayName("Number of wheel teeth, z2")]
    public int z2 { get; set; } = 39;

    // ----------------------------------------------------------------- 2. Loads
    [Category("02. Loads and speed")]
    [DisplayName("Pinion torque, T1 (Nm)")]
    [Description("Clause 6, Formula (1). Nominal pinion torque flowing through the mesh.")]
    public double T1_Nm { get; set; } = 300.0;

    [Category("02. Loads and speed")]
    [DisplayName("Pinion speed, n1 (rpm)")]
    public double PinionSpeed_rpm { get; set; } = 1200.0;

    // ----------------------------------------------------------------- 3. Common geometry
    [Category("03. Common geometry")]
    [DisplayName("Outer cone distance, Re (mm)")]
    public double Re { get; set; } = 93.973;

    [Category("03. Common geometry")]
    [DisplayName("Facewidth, b (mm)")]
    [Description("Wheel facewidth b2. For non-offset bevel gears the virtual cylindrical gear facewidth bv equals b exactly (10300-1 Annex A).")]
    public double b { get; set; } = 25.4;

    [Category("03. Common geometry")]
    [DisplayName("Effective facewidth fraction, b_eff/b")]
    [Description("10300-1, A.2.4: at the design stage, 0.85 is a reasonable estimate for the length of contact pattern under load if it has not been measured or obtained from an LTCA.")]
    public double EffectiveFacewidthFraction { get; set; } = 0.85;

    [Category("03. Common geometry")]
    [DisplayName("Mean normal module, mmn (mm)")]
    public double mmn { get; set; } = 3.213;

    [Category("03. Common geometry")]
    [DisplayName("Outer transverse module, met2 (mm)")]
    [Description("Used only for reporting/cross-check against AGMA-style outer geometry; not used directly by the ISO Method B1 equations implemented here.")]
    public double met2 { get; set; } = 4.536;

    [Category("03. Common geometry")]
    [DisplayName("Generated pressure angle, drive side, alphaND (deg)")]
    public double Alpha_nD { get; set; } = 20.0;

    [Category("03. Common geometry")]
    [DisplayName("Generated pressure angle, coast side, alphaNC (deg)")]
    public double Alpha_nC { get; set; } = 20.0;

    [Category("03. Common geometry")]
    [DisplayName("Effective pressure angle, drive side, alphaED (deg)")]
    [Description("Equal to the generated pressure angle unless the flank has been modified after generation (e.g. by lapping); set equal to alphaND for standard designs.")]
    public double Alpha_eD { get; set; } = 20.0;

    [Category("03. Common geometry")]
    [DisplayName("Effective pressure angle, coast side, alphaEC (deg)")]
    public double Alpha_eC { get; set; } = 20.0;

    [Category("03. Common geometry")]
    [DisplayName("Limit pressure angle, alphaLim (deg)")]
    [Description("10300-1, A.2.8. Zero for a standard (non-duplex) cutting system.")]
    public double Alpha_lim { get; set; } = 0.0;

    [Category("03. Common geometry")]
    [DisplayName("Mean spiral angle, pinion, betam1 (deg)")]
    [Description("Use 0 for straight bevel and Zerol bevel gears.")]
    public double Beta_m1 { get; set; } = 35.0;

    [Category("03. Common geometry")]
    [DisplayName("Mean spiral angle, wheel, betam2 (deg)")]
    public double Beta_m2 { get; set; } = 35.0;

    [Category("03. Common geometry")]
    [DisplayName("Cutter radius, rc0 (mm)")]
    [Description("Face-milled cutter head radius; only used for the lengthwise-curvature factor KF0 (10300-1, 8.4.4). Face-hobbing (Formula 37) is not implemented, matching AGMA2003D19Calc's own Kx simplification.")]
    public double CutterRadius { get; set; } = 114.3;

    // ----------------------------------------------------------------- 4. Pinion geometry
    [Category("04. Pinion geometry (ISO 23509 data)")]
    [DisplayName("Pinion mean pitch diameter, dm1 (mm)")]
    public double dm1 { get; set; } = 54.918;

    [Category("04. Pinion geometry (ISO 23509 data)")]
    [DisplayName("Pinion pitch angle, delta1 (deg)")]
    public double Delta1 { get; set; } = 19.747;

    [Category("04. Pinion geometry (ISO 23509 data)")]
    [DisplayName("Pinion mean addendum, ham1 (mm)")]
    public double h_am1 { get; set; } = 4.837;

    [Category("04. Pinion geometry (ISO 23509 data)")]
    [DisplayName("Pinion mean dedendum, hfm1 (mm)")]
    public double h_fm1 { get; set; } = 2.393;

    [Category("04. Pinion geometry (ISO 23509 data)")]
    [DisplayName("Pinion addendum modification coefficient, xhm1")]
    [Description("Long-and-short addendum system: for equal-strength design, xhm1 = -xhm2 (nonzero); use 0 for a standard/equal-addendum design.")]
    public double x_hm1 { get; set; } = 0.505;

    [Category("04. Pinion geometry (ISO 23509 data)")]
    [DisplayName("Pinion tooth-thickness modification coefficient, xsm1")]
    public double x_sm1 { get; set; } = 0.036;

    [Category("04. Pinion geometry (ISO 23509 data)")]
    [DisplayName("Pinion cutter edge radius, rho_a01 (mm)")]
    public double Rho_a01 { get; set; } = 0.8;

    [Category("04. Pinion geometry (ISO 23509 data)")]
    [DisplayName("Pinion protuberance, spr1 (mm)")]
    public double Spr1 { get; set; } = 0.0;

    // ----------------------------------------------------------------- 5. Wheel geometry
    [Category("05. Wheel geometry (ISO 23509 data)")]
    [DisplayName("Wheel mean pitch diameter, dm2 (mm)")]
    public double dm2 { get; set; } = 152.987;

    [Category("05. Wheel geometry (ISO 23509 data)")]
    [DisplayName("Wheel pitch angle, delta2 (deg)")]
    public double Delta2 { get; set; } = 70.253;

    [Category("05. Wheel geometry (ISO 23509 data)")]
    [DisplayName("Wheel mean addendum, ham2 (mm)")]
    public double h_am2 { get; set; } = 1.590;

    [Category("05. Wheel geometry (ISO 23509 data)")]
    [DisplayName("Wheel mean dedendum, hfm2 (mm)")]
    public double h_fm2 { get; set; } = 5.640;

    [Category("05. Wheel geometry (ISO 23509 data)")]
    [DisplayName("Wheel addendum modification coefficient, xhm2")]
    public double x_hm2 { get; set; } = -0.505;

    [Category("05. Wheel geometry (ISO 23509 data)")]
    [DisplayName("Wheel tooth-thickness modification coefficient, xsm2")]
    public double x_sm2 { get; set; } = -0.055;

    [Category("05. Wheel geometry (ISO 23509 data)")]
    [DisplayName("Wheel cutter edge radius, rho_a02 (mm)")]
    public double Rho_a02 { get; set; } = 1.2;

    [Category("05. Wheel geometry (ISO 23509 data)")]
    [DisplayName("Wheel protuberance, spr2 (mm)")]
    public double Spr2 { get; set; } = 0.0;

    // ----------------------------------------------------------------- 6. Tool / generation
    [Category("06. Tool and generation data")]
    [DisplayName("Basic crown gear dedendum factor, khfp")]
    [Description("10300-3, 6.4.1.2.2: sets the tool addendum ha0 = hfp = khfp * mmn (same for pinion and wheel). 1.25 is the standard Gleason-system value.")]
    public double k_hfp { get; set; } = 1.25;

    [Category("06. Tool and generation data")]
    [DisplayName("Gear material density, rho (kg/mm^3)")]
    [Description("10300-1, 7.7.3.3. 7.86e-6 kg/mm^3 for steel.")]
    public double MaterialDensity { get; set; } = 7.86e-6;

    // ----------------------------------------------------------------- 7. Application/dynamic
    [Category("07. Application and dynamic factor")]
    [DisplayName("Application factor, KA")]
    [Description("10300-1, Clause 6 / Annex C. Use Annex C Table C.1 for typical values based on driving/driven machine working characteristics.")]
    public double KA { get; set; } = 1.1;

    [Category("07. Application and dynamic factor")]
    [DisplayName("Dynamic factor source")]
    public DynamicFactorMode DynamicFactorMode { get; set; } = DynamicFactorMode.MethodC;

    [Category("07. Application and dynamic factor")]
    [DisplayName("ISO accuracy grade, B (Method C)")]
    [Description("10300-1, 7.7.4. ISO 17485 accuracy grade; lower is more accurate (opposite convention to AGMA's Qv). Valid range for the empirical Kv-C curve is 5 to 8.")]
    public int AccuracyGradeB { get; set; } = 6;

    [Category("07. Application and dynamic factor")]
    [DisplayName("Single pitch deviation, pinion, fpt1 (um)")]
    [Description("10300-1, 9.3.1 / Method B dynamic factor. Measured or from ISO 17485 tolerance tables.")]
    public double fpt1_um { get; set; } = 12.0;

    [Category("07. Application and dynamic factor")]
    [DisplayName("Single pitch deviation, wheel, fpt2 (um)")]
    public double fpt2_um { get; set; } = 26.0;

    [Category("07. Application and dynamic factor")]
    [DisplayName("Manual Kv")]
    public double ManualKv { get; set; } = 1.0;

    // ----------------------------------------------------------------- 8. Face load factor
    [Category("08. Face load factor, KHbeta")]
    [DisplayName("Mounting condition")]
    public MountingCondition Mounting { get; set; } = MountingCondition.OneMemberCantileverMounted;

    [Category("08. Face load factor, KHbeta")]
    [DisplayName("Contact pattern verification")]
    public ContactPatternVerification ContactVerification { get; set; } = ContactPatternVerification.CheckedUnderLightTestLoad;

    [Category("08. Face load factor, KHbeta")]
    [DisplayName("Override KHbeta manually")]
    public bool ManualKHbetaOverride { get; set; } = false;

    [Category("08. Face load factor, KHbeta")]
    [DisplayName("Manual KHbeta")]
    public double ManualKHbeta { get; set; } = 1.5;

    // ----------------------------------------------------------------- 9. Transverse load factor
    [Category("09. Transverse load factor, KHalpha/KFalpha")]
    [DisplayName("Transverse load factor source")]
    public TransverseLoadFactorMode TransverseLoadFactorMode { get; set; } = TransverseLoadFactorMode.MethodC;

    [Category("09. Transverse load factor, KHalpha/KFalpha")]
    [DisplayName("Profile crowning")]
    [Description("10300-2, Table 5. Sets the load-sharing exponent eLS used by ZLS/YLS, and (with the accuracy grade) the Method C table lookup.")]
    public ProfileCrowning ProfileCrowning { get; set; } = ProfileCrowning.Low;

    [Category("09. Transverse load factor, KHalpha/KFalpha")]
    [DisplayName("Manual KHalpha = KFalpha")]
    public double ManualKHalpha { get; set; } = 1.2;

    // ----------------------------------------------------------------- 10. Life, material family
    [Category("10. Life and material family")]
    [DisplayName("Material family")]
    [Description("Selects the ZNT/YNT life-factor curve family, the YdeltarelT slip-layer thickness, and the YR,relT surface-condition formula.")]
    public MaterialFamily MaterialFamily { get; set; } = MaterialFamily.CaseOrThroughHardenedSteel;

    [Category("10. Life and material family")]
    [DisplayName("Required pinion life (load cycles), NL1")]
    public double PinionLifeCycles { get; set; } = 1.0e7;

    [Category("10. Life and material family")]
    [DisplayName("Required wheel life (load cycles), NL2")]
    public double WheelLifeCycles { get; set; } = 1.0e7;

    // ----------------------------------------------------------------- 11. Hardness ratio ZW
    [Category("11. Hardness ratio factor, ZW")]
    [DisplayName("Hardness ratio mode")]
    public HardnessRatioMode HardnessRatioMode { get; set; } = HardnessRatioMode.EqualHardness;

    [Category("11. Hardness ratio factor, ZW")]
    [DisplayName("Minimum pinion Brinell hardness, HBW1")]
    public double HBW_Pinion { get; set; } = 400.0;

    [Category("11. Hardness ratio factor, ZW")]
    [DisplayName("Minimum wheel Brinell hardness, HBW2")]
    public double HBW_Wheel { get; set; } = 300.0;

    // ----------------------------------------------------------------- 12. Elastic coefficient
    [Category("12. Elasticity factor, ZE")]
    [DisplayName("Pinion Young's modulus, E1 (N/mm^2)")]
    public double E_Pinion { get; set; } = 210000.0;

    [Category("12. Elasticity factor, ZE")]
    [DisplayName("Wheel Young's modulus, E2 (N/mm^2)")]
    public double E_Wheel { get; set; } = 210000.0;

    [Category("12. Elasticity factor, ZE")]
    [DisplayName("Pinion Poisson's ratio, nu1")]
    public double Nu_Pinion { get; set; } = 0.3;

    [Category("12. Elasticity factor, ZE")]
    [DisplayName("Wheel Poisson's ratio, nu2")]
    public double Nu_Wheel { get; set; } = 0.3;

    // ----------------------------------------------------------------- 13. Lubrication/roughness
    [Category("13. Lubrication and roughness")]
    [DisplayName("Oil kinematic viscosity at 40C, v40 (mm^2/s)")]
    public double Viscosity40 { get; set; } = 150.0;

    [Category("13. Lubrication and roughness")]
    [DisplayName("Flank roughness, pinion/wheel, Rz1/Rz2 (um)")]
    public double Rz_Flank_Pinion { get; set; } = 8.0;

    [Category("13. Lubrication and roughness")]
    [DisplayName("Flank roughness, wheel, Rz2 (um)")]
    public double Rz_Flank_Wheel { get; set; } = 8.0;

    [Category("13. Lubrication and roughness")]
    [DisplayName("Root roughness, pinion/wheel, Rz (um)")]
    [Description("10300-3, 6.5.1. Used for both pinion and wheel (single YR,relT surface-condition factor per the formula family).")]
    public double Rz_Root { get; set; } = 16.0;

    // ----------------------------------------------------------------- 14. Allowable stress numbers
    [Category("14. Allowable stress numbers (ISO 6336-5)")]
    [DisplayName("Allowable stress number, contact, sigmaHlim (N/mm^2)")]
    [Description("From ISO 6336-5 test-gear data for the chosen material/heat-treatment/quality grade. This tool does not embed the ISO 6336-5 tables (not available) - enter directly; 1500 N/mm^2 is representative of MQ-grade case-carburized steel.")]
    public double SigmaHlim { get; set; } = 1500.0;

    [Category("14. Allowable stress numbers (ISO 6336-5)")]
    [DisplayName("Nominal stress number, bending, sigmaFlim (N/mm^2)")]
    public double SigmaFlim { get; set; } = 480.0;

    [Category("14. Allowable stress numbers (ISO 6336-5)")]
    [DisplayName("Yield/proof stress, sigmaS or sigma0.2 (N/mm^2, static YdeltarelT only)")]
    [Description("Only used if a static (NL below the endurance knee) safety check is added later; not used by the reference-life SelfTest.")]
    public double YieldStress { get; set; } = 800.0;

    // ----------------------------------------------------------------- 15. Safety factors
    [Category("15. Safety factors")]
    [DisplayName("Minimum contact safety factor, SH,min")]
    public double SH_min { get; set; } = 1.0;

    [Category("15. Safety factors")]
    [DisplayName("Minimum bending safety factor, SF,min")]
    [Description("10300-3, Clause 5.2/6.3: >=1.3 for spiral bevel, >=1.5 for straight bevel or betam <= 5 deg.")]
    public double SF_min { get; set; } = 1.3;
}
