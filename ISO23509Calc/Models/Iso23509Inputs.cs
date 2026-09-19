using System.ComponentModel;

namespace ISO23509Calc.Models;

/// <summary>
/// All input data for ISO 23509:2016 (ANSI/AGMA ISO 23509-B17), "Bevel and Hypoid Gear
/// Geometry": Clause 6 (pitch cone parameters, Methods 0-3), Clause 7 (gear dimensions) and
/// Clause 8 (undercut check). Covers both non-offset bevel gears (hypoid offset a = 0) and
/// general hypoid gears. Bound to a PropertyGrid. SI units throughout (mm, deg), matching the
/// standard's own equations.
/// </summary>
public class Iso23509Inputs
{
    // ----------------------------------------------------------------- 1. Method and process
    [Category("01. Method and manufacturing process")]
    [DisplayName("Pitch cone method")]
    [Description("6.2 / Annex A.3. Method 0: bevel gears without hypoid offset (no iteration). Method 1: Gleason. Method 2: Oerlikon. Method 3: Klingelnberg. Methods 1-3 all cover the general hypoid case; setting a = 0 reduces any of them to the equivalent bevel-gear geometry.")]
    public PitchConeMethod Method { get; set; } = PitchConeMethod.Method0;

    [Category("01. Method and manufacturing process")]
    [DisplayName("Manufacturing process")]
    [Description("5.6/5.9, 7.6, 7.7.1, Clause 8. Face milling: circular lengthwise tooth curvature of radius rc0. Face hobbing: extended-epicycloid lengthwise curvature (needs the number of blade groups, z0).")]
    public ManufacturingProcess Process { get; set; } = ManufacturingProcess.FaceMilling;

    // ----------------------------------------------------------------- 2. Basic data (Table 2)
    [Category("02. Basic data (Table 2)")]
    [DisplayName("Shaft angle, Sigma (deg)")]
    public double Sigma { get; set; } = 90.0;

    [Category("02. Basic data (Table 2)")]
    [DisplayName("Hypoid offset, a (mm)")]
    [Description("B.3: positive if the pinion offset follows the hand of spiral of the wheel. Zero for a bevel gear without offset.")]
    public double a { get; set; } = 0.0;

    [Category("02. Basic data (Table 2)")]
    [DisplayName("Number of pinion teeth, z1")]
    public int z1 { get; set; } = 14;

    [Category("02. Basic data (Table 2)")]
    [DisplayName("Number of wheel teeth, z2")]
    public int z2 { get; set; } = 39;

    [Category("02. Basic data (Table 2)")]
    [DisplayName("Wheel mean pitch diameter, dm2 (mm)")]
    [Description("Required by Method 2 only.")]
    public double dm2 { get; set; } = 146.7;

    [Category("02. Basic data (Table 2)")]
    [DisplayName("Wheel outer pitch diameter, de2 (mm)")]
    [Description("Required by Methods 0, 1 and 3.")]
    public double de2 { get; set; } = 176.893;

    [Category("02. Basic data (Table 2)")]
    [DisplayName("Wheel face width, b2 (mm)")]
    [Description("Required by all methods.")]
    public double b2 { get; set; } = 25.4;

    [Category("02. Basic data (Table 2)")]
    [DisplayName("Desired pinion mean spiral angle, beta_m1 (deg)")]
    [Description("Required by Method 1 only (the target spiral angle the iteration aims for).")]
    public double beta_m1_desired { get; set; } = 35.0;

    [Category("02. Basic data (Table 2)")]
    [DisplayName("Wheel mean spiral angle, beta_m2 (deg)")]
    [Description("Required by Methods 0, 2 and 3.")]
    public double beta_m2 { get; set; } = 35.0;

    [Category("02. Basic data (Table 2)")]
    [DisplayName("Cutter radius, rc0 (mm)")]
    [Description("Required by all methods.")]
    public double rc0 { get; set; } = 114.3;

    [Category("02. Basic data (Table 2)")]
    [DisplayName("Number of blade groups, z0")]
    [Description("Face hobbing only; also used internally by the Method 1 face-hobbed iteration branch.")]
    public int z0 { get; set; } = 5;

    // ----------------------------------------------------------------- 3. Pressure angle (Table 3)
    [Category("03. Pressure angle (Table 3)")]
    [DisplayName("Nominal design pressure angle, drive side, alpha_dD (deg)")]
    public double alpha_dD { get; set; } = 20.0;

    [Category("03. Pressure angle (Table 3)")]
    [DisplayName("Nominal design pressure angle, coast side, alpha_dC (deg)")]
    public double alpha_dC { get; set; } = 20.0;

    [Category("03. Pressure angle (Table 3)")]
    [DisplayName("Influence factor of limit pressure angle, f_alim")]
    [Description("C.2.1/C.2.5: 0 for a standard (non-duplex) cutting system; 1 to fully balance the coast/drive mesh conditions on a hypoid (Formulae 118/119).")]
    public double f_alim { get; set; } = 0.0;

    // ----------------------------------------------------------------- 4. Tooth depth data (Table 3)
    [Category("04. Tooth depth data (Table 3)")]
    [DisplayName("Input data type")]
    [Description("Data type I (xhm1/khap/khfp/xsmn) is used directly by every Clause 7 formula. Data type II (cham/kd/kc/kt, the AGMA-style form) is converted to type I via Table 4 before Clause 7 is evaluated.")]
    public InputDataType DataType { get; set; } = InputDataType.TypeI;

    [Category("04. Tooth depth data (Table 3)")]
    [DisplayName("[I] Profile shift coefficient, xhm1")]
    public double x_hm1 { get; set; } = 0.505;

    [Category("04. Tooth depth data (Table 3)")]
    [DisplayName("[I] Basic crown gear addendum factor, khap")]
    [Description("C.3.1.1: commonly khap = 1.")]
    public double k_hap { get; set; } = 1.0;

    [Category("04. Tooth depth data (Table 3)")]
    [DisplayName("[I] Basic crown gear dedendum factor, khfp")]
    [Description("C.3.1.1: commonly khfp = 1.25.")]
    public double k_hfp { get; set; } = 1.25;

    [Category("04. Tooth depth data (Table 3)")]
    [DisplayName("[I] Theoretical thickness modification coefficient, xsmn")]
    public double x_smn { get; set; } = 0.046;

    [Category("04. Tooth depth data (Table 3)")]
    [DisplayName("[II] Mean addendum factor of wheel, cham")]
    [Description("C.3.2.3/Table C.2. Only used when input data type = II.")]
    public double c_ham { get; set; } = 0.24737;

    [Category("04. Tooth depth data (Table 3)")]
    [DisplayName("[II] Depth factor, kd")]
    [Description("C.3.2.1/Table C.1: commonly kd = 2.000. Only used when input data type = II.")]
    public double k_d { get; set; } = 2.000;

    [Category("04. Tooth depth data (Table 3)")]
    [DisplayName("[II] Clearance factor, kc")]
    [Description("C.3.2.2: commonly kc = 0.125. Only used when input data type = II.")]
    public double k_c { get; set; } = 0.125;

    [Category("04. Tooth depth data (Table 3)")]
    [DisplayName("[II] Thickness factor, kt")]
    [Description("C.4.2.1/Figure C.1. Only used when input data type = II.")]
    public double k_t { get; set; } = 0.0915;

    // ----------------------------------------------------------------- 5. Backlash (Table 3)
    [Category("05. Backlash (Table 3)")]
    [DisplayName("Backlash quantity supplied")]
    [Description("Selects which of Formulae (202)-(205)/(207)-(210) is used to turn xsmn into xsm1/xsm2. \"Theoretical / no backlash\" uses xsmn unmodified (A.4).")]
    public BacklashMode Backlash { get; set; } = BacklashMode.OuterNormal_jen;

    [Category("05. Backlash (Table 3)")]
    [DisplayName("Backlash value, j (mm)")]
    [Description("Value of whichever backlash quantity was selected above (jen, jet2, jmn or jmt2); ignored for the theoretical/no-backlash option.")]
    public double BacklashValue { get; set; } = 0.127;

    // ----------------------------------------------------------------- 6. Wheel addendum/dedendum angle (Table 3)
    [Category("06. Wheel addendum/dedendum angle (Table 3)")]
    [DisplayName("theta_a2 / theta_f2 source")]
    [Description("Manual: enter theta_a2 and theta_f2 directly (Table 3). Auto: derive them from the depthwise taper type via Annex C.5 (Tables C.4/C.5).")]
    public ThetaA2Source ThetaSource { get; set; } = ThetaA2Source.Manual;

    [Category("06. Wheel addendum/dedendum angle (Table 3)")]
    [DisplayName("[Manual] Addendum angle of wheel, theta_a2 (deg)")]
    public double theta_a2 { get; set; } = 2.1342;

    [Category("06. Wheel addendum/dedendum angle (Table 3)")]
    [DisplayName("[Manual] Dedendum angle of wheel, theta_f2 (deg)")]
    public double theta_f2 { get; set; } = 6.4934;

    [Category("06. Wheel addendum/dedendum angle (Table 3)")]
    [DisplayName("[Auto] Depthwise taper type")]
    [Description("5.3, Annex C.5. Only used when the source above is Auto.")]
    public DepthwiseTaper Taper { get; set; } = DepthwiseTaper.StandardDepth;

    // ----------------------------------------------------------------- 7. Undercut check (Clause 8)
    [Category("07. Undercut check (Clause 8)")]
    [DisplayName("Check for undercut")]
    [Description("Runs Clause 8 at the toe (Ri) and heel (Re) of both members and reports the allowable range of xhm1.")]
    public bool CheckUndercut { get; set; } = true;
}
