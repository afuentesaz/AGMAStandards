using System.ComponentModel;

namespace AGMA929B22Calc.Models;

/// <summary>
/// All input data for AGMA 929-B22 (Table 2, Table 3, plus manufacturing-method
/// selectors and the optional pinion tip chamfer request). Bound to a PropertyGrid.
/// </summary>
public class AgmaInputs
{
    // ----------------------------------------------------------------- Manufacturing method
    [Category("1. Manufacturing method")]
    [DisplayName("Cutting method")]
    [Description("Face milling (fixed-radius circular cutter) or face hobbing (continuous indexing). Governs 4.2 spiral-angle and 4.3 cone-distance formulas.")]
    public CuttingMethod CuttingMethod { get; set; } = CuttingMethod.FaceMilling;

    [Category("1. Manufacturing method")]
    [DisplayName("Wheel generation")]
    [Description("Generated (conjugate rolling action) or non-generated / Formate wheel. Affects dedendum, interference, tip radius and top-land formulas.")]
    public WheelGeneration WheelGeneration { get; set; } = WheelGeneration.Generated;

    [Category("1. Manufacturing method")]
    [DisplayName("Pinion process")]
    [Description("Completing (single setup) or separate rough-and-finish operations for the pinion. Governs 4.5.5 / 4.6.2 blade-point and edge-radius chain.")]
    public MemberProcess PinionProcess { get; set; } = MemberProcess.Completing;

    [Category("1. Manufacturing method")]
    [DisplayName("Wheel process")]
    [Description("Completing (single setup) or separate rough-and-finish operations for the wheel.")]
    public MemberProcess WheelProcess { get; set; } = MemberProcess.Completing;

    [Category("1. Manufacturing method")]
    [DisplayName("Wheel rotation factor basis")]
    [Description("Selects which of Eq (65)-(71) defines the wheel rotation factor k_E, based on cutter/machine type (3.2, 4.5.2).")]
    public KeMethod KeMethod { get; set; } = KeMethod.FaceMillingStraightBevelPlaningGenerator;

    // ----------------------------------------------------------------- Basic gear data (Table 2)
    [Category("2. Basic gear data")]
    [DisplayName("Shaft angle, Σ (deg)")]
    [Description("Table 2. Shaft angle between pinion and wheel axes. 90 for most bevel/hypoid sets.")]
    public double Sigma { get; set; } = 90.0;

    [Category("2. Basic gear data")]
    [DisplayName("Hypoid offset, a (mm)")]
    [Description("Table 2. Offset distance for hypoid gears. Use 0 for bevel gears (spiral bevel, Zerol, straight bevel).")]
    public double Offset_a { get; set; } = 0.0;

    [Category("2. Basic gear data")]
    [DisplayName("Number of pinion teeth, z1")]
    [Description("Table 2.")]
    public int z1 { get; set; } = 14;

    [Category("2. Basic gear data")]
    [DisplayName("Number of wheel teeth, z2")]
    [Description("Table 2.")]
    public int z2 { get; set; } = 39;

    [Category("2. Basic gear data")]
    [DisplayName("Mean normal module, m_mn (mm)")]
    [Description("Table 2.")]
    public double m_mn { get; set; } = 3.21331;

    [Category("2. Basic gear data")]
    [DisplayName("Pinion face width, b1 (mm)")]
    [Description("Table 2.")]
    public double b1 { get; set; } = 25.4;

    [Category("2. Basic gear data")]
    [DisplayName("Wheel face width, b2 (mm)")]
    [Description("Table 2.")]
    public double b2 { get; set; } = 25.4;

    [Category("2. Basic gear data")]
    [DisplayName("Pinion outer cone distance, Re1 (mm)")]
    [Description("Table 2.")]
    public double Re1 { get; set; } = 93.973;

    [Category("2. Basic gear data")]
    [DisplayName("Wheel outer cone distance, Re2 (mm)")]
    [Description("Table 2.")]
    public double Re2 { get; set; } = 93.973;

    [Category("2. Basic gear data")]
    [DisplayName("Pinion mean cone distance, Rm1 (mm)")]
    [Description("Table 2.")]
    public double Rm1 { get; set; } = 81.273;

    [Category("2. Basic gear data")]
    [DisplayName("Wheel mean cone distance, Rm2 (mm)")]
    [Description("Table 2.")]
    public double Rm2 { get; set; } = 81.273;

    [Category("2. Basic gear data")]
    [DisplayName("Pinion pitch angle, δ1 (deg)")]
    [Description("Table 2.")]
    public double Delta1 { get; set; } = 19.7468;

    [Category("2. Basic gear data")]
    [DisplayName("Wheel pitch angle, δ2 (deg)")]
    [Description("Table 2.")]
    public double Delta2 { get; set; } = 70.2532;

    [Category("2. Basic gear data")]
    [DisplayName("Pinion dedendum angle, θf1 (deg)")]
    [Description("Table 2.")]
    public double ThetaF1 { get; set; } = 2.2259;

    [Category("2. Basic gear data")]
    [DisplayName("Wheel dedendum angle, θf2 (deg)")]
    [Description("Table 2.")]
    public double ThetaF2 { get; set; } = 6.4935;

    [Category("2. Basic gear data")]
    [DisplayName("Clearance, c (mm)")]
    [Description("Table 2.")]
    public double Clearance_c { get; set; } = 0.964;

    // ----------------------------------------------------------------- Tooth proportions (Table 2)
    [Category("3. Tooth proportions")]
    [DisplayName("Concave pinion normal pressure angle, αcv1 (deg)")]
    [Description("Table 2. Always positive.")]
    public double Alpha_cv1 { get; set; } = 20.0;

    [Category("3. Tooth proportions")]
    [DisplayName("Convex pinion normal pressure angle, αcx1 (deg)")]
    [Description("Table 2. Always negative.")]
    public double Alpha_cx1 { get; set; } = -20.0;

    [Category("3. Tooth proportions")]
    [DisplayName("Wheel mean spiral angle, βm2 (deg)")]
    [Description("Table 2. Use 0 for straight bevel gears.")]
    public double Beta_m2 { get; set; } = 35.0;

    [Category("3. Tooth proportions")]
    [DisplayName("Pinion mean addendum, ham1 (mm)")]
    [Description("Table 2.")]
    public double h_am1 { get; set; } = 4.83648;

    [Category("3. Tooth proportions")]
    [DisplayName("Wheel mean addendum, ham2 (mm)")]
    [Description("Table 2.")]
    public double h_am2 { get; set; } = 1.58937;

    [Category("3. Tooth proportions")]
    [DisplayName("Pinion mean dedendum, hfm1 (mm)")]
    [Description("Table 2.")]
    public double h_fm1 { get; set; } = 2.55337;

    [Category("3. Tooth proportions")]
    [DisplayName("Wheel mean dedendum, hfm2 (mm)")]
    [Description("Table 2.")]
    public double h_fm2 { get; set; } = 5.80048;

    [Category("3. Tooth proportions")]
    [DisplayName("Pinion mean normal circular thickness, smn1 (mm)")]
    [Description("Table 2.")]
    public double s_mn1 { get; set; } = 6.2788;

    [Category("3. Tooth proportions")]
    [DisplayName("Wheel mean normal circular thickness, smn2 (mm)")]
    [Description("Table 2.")]
    public double s_mn2 { get; set; } = 3.67226;

    // ----------------------------------------------------------------- Cutter / blade data (Table 2, Table 3)
    [Category("4. Cutter and blade data")]
    [DisplayName("Cutter radius, rc0 (mm)")]
    [Description("Table 2/3. Use 254000 mm as a default for straight bevel gears (per Table 3), otherwise per ANSI/AGMA ISO 23509 Annex D.")]
    public double r_c0 { get; set; } = 114.3;

    [Category("4. Cutter and blade data")]
    [DisplayName("Number of blade groups, z0 (face hobbing)")]
    [Description("Table 2. Only used for face hobbing (Eq 26).")]
    public int z0 { get; set; } = 5;

    [Category("4. Cutter and blade data")]
    [DisplayName("Inside blade angle, αBX2 (deg, non-generated wheel)")]
    [Description("Table 2/3. Only used for a non-generated wheel cutter (Eq 48). Suggested default: the average pressure angle.")]
    public double Alpha_BX2 { get; set; } = 0.0;

    [Category("4. Cutter and blade data")]
    [DisplayName("Wheel finishing point width, W (mm)")]
    [Description("Table 2. Unitool and single-setting methods only (Eq 69).")]
    public double W_finishingPointWidth { get; set; } = 0.0;

    [Category("4. Cutter and blade data")]
    [DisplayName("Pinion stock allowance, SA1 (mm)")]
    [Description("Table 2/3. Use 0 for completing; actual rough/finish stock otherwise (Annex B).")]
    public double S_A1 { get; set; } = 0.0;

    [Category("4. Cutter and blade data")]
    [DisplayName("Wheel stock allowance, SA2 (mm)")]
    [Description("Table 2/3. Use 0 for completing; actual rough/finish stock otherwise (Annex B).")]
    public double S_A2 { get; set; } = 0.0;

    [Category("4. Cutter and blade data")]
    [DisplayName("Fillet mutilation permitted, y (mm)")]
    [Description("Table 2/3. Suggested default 0.05 mm.")]
    public double y_mutilation { get; set; } = 0.05;

    [Category("4. Cutter and blade data")]
    [DisplayName("Width of blade flat, Δf (mm)")]
    [Description("Table 2/3. Suggested default 0.20 mm.")]
    public double Delta_f { get; set; } = 0.20;

    // ----------------------------------------------------------------- Optional pinion chamfer (4.8)
    [Category("5. Pinion tip chamfer (optional, 4.8)")]
    [DisplayName("Calculate inner-end chamfer")]
    [Description("If true, calculates the pinion tooth tip chamfer at the inner end (4.8) needed to achieve the desired top land below.")]
    public bool CalculatePinionChamfer { get; set; } = false;

    [Category("5. Pinion tip chamfer (optional, 4.8)")]
    [DisplayName("Desired top land at chamfer, sacnΔ1 (mm)")]
    [Description("Desired pinion normal top land thickness at the chamfer starting point / inner end. Must lie strictly between the (uncorrected) inner and outer top lands.")]
    public double DesiredChamferTopLand { get; set; } = 0.30;
}
