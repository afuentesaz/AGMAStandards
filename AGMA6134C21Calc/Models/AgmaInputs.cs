using System.ComponentModel;

namespace AGMA6134C21Calc.Models;

/// <summary>
/// All input data for ANSI/AGMA 6134-C21 (clauses 4 through 7 and 12). Bound to a
/// PropertyGrid. SI units throughout (mm, N, Nm, kW, rpm, deg). Default values reproduce
/// the Annex C worked example.
/// </summary>
public class AgmaInputs
{
    // ----------------------------------------------------------------- 1. Mesh geometry and speed
    [Category("01. Mesh geometry and speed")]
    [DisplayName("Normal module, mn (mm)")]
    [Description("Clause 4.3, Eq 5. Common to worm and wormgear.")]
    public double NormalModule { get; set; } = 4.0;

    [Category("01. Mesh geometry and speed")]
    [DisplayName("Number of starts on the worm, z1")]
    public int z1 { get; set; } = 1;

    [Category("01. Mesh geometry and speed")]
    [DisplayName("Number of teeth on the wormgear, z2")]
    [Description("Clause 1.2 b): the gear ratio z2/z1 must be >= 3.0 for this standard to apply.")]
    public int z2 { get; set; } = 30;

    [Category("01. Mesh geometry and speed")]
    [DisplayName("Lead angle of the worm, gamma1 (deg)")]
    [Description("Eq 4. The helix angle is its complement, beta1 = 90 - gamma1.")]
    public double LeadAngleWorm { get; set; } = 6.0;

    [Category("01. Mesh geometry and speed")]
    [DisplayName("Helix angle of the wormgear, beta2 (deg)")]
    [Description("Eq 6, used for the wormgear pitch diameter d2. Numerically equal to the worm lead angle for a conjugate mesh at standard center distance.")]
    public double HelixAngleGear { get; set; } = 6.0;

    [Category("01. Mesh geometry and speed")]
    [DisplayName("Normal pressure angle, alpha_n (deg)")]
    [Description("Eq 16, 21, 22, 24.")]
    public double NormalPressureAngle { get; set; } = 20.0;

    [Category("01. Mesh geometry and speed")]
    [DisplayName("Rotational speed of the worm, n1 (rpm)")]
    [Description("Clause 1.1: this standard applies for n1 <= 3600 rpm.")]
    public double WormSpeed { get; set; } = 1750.0;

    [Category("01. Mesh geometry and speed")]
    [DisplayName("Actual face width of the wormgear, b2 (mm)")]
    [Description("Eq 11. The effective face width used in rating is the lesser of b2 and 2*d1/3.")]
    public double ActualFaceWidthGear { get; set; } = 27.94;

    [Category("01. Mesh geometry and speed")]
    [DisplayName("Root diameter of the wormgear at the throat, df2 (mm)")]
    [Description("Eq 12, length of the gear tooth root.")]
    public double RootDiameterGearThroat { get; set; } = 110.66;

    [Category("01. Mesh geometry and speed")]
    [DisplayName("Maximum radial deflection of the worm under load, dy1max (mm)")]
    [Description("Clause 1.2 h) scope check only (Eq 3): dy1max <= 0.025*sqrt(px1). See ANSI/AGMA 6022 for the deflection calculation itself.")]
    public double WormMaxRadialDeflection { get; set; } = 0.076;

    // ----------------------------------------------------------------- 2. Materials
    [Category("02. Materials")]
    [DisplayName("Worm heat treatment")]
    [Description("Table 1, scope check only (clause 1.2 d): carburized 58-62 HRC, or flame/induction hardened 50-62 HRC.")]
    public WormHeatTreat WormHeatTreatment { get; set; } = WormHeatTreat.Carburized;

    [Category("02. Materials")]
    [DisplayName("Worm surface hardness, HRC")]
    [Description("Table 1, scope check only.")]
    public double WormHardnessHRC { get; set; } = 58.0;

    [Category("02. Materials")]
    [DisplayName("Wormgear casting method")]
    [Description("Table 3: determines K_d, the materials factor based only on casting method and pitch diameter.")]
    public GearCastingMethod GearCasting { get; set; } = GearCastingMethod.Centrifugal;

    // ----------------------------------------------------------------- 3. Lubrication
    [Category("03. Lubrication")]
    [DisplayName("Oil type")]
    [Description("Table 7: sets Z_oil-wear (Eq 15) and Z_oil-mu (Eq 14).")]
    public OilType Lubricant { get; set; } = OilType.PAO;

    // ----------------------------------------------------------------- 4. Component design - overhung load (clause 7, optional)
    [Category("04. Overhung load (clause 7, optional)")]
    [DisplayName("Check overhung shaft loads")]
    [Description("Eq 27. Requires an applied input power (below) to establish worm/gear torque, plus the pitch diameter of whatever sprocket, gear, or pulley is mounted on each shaft extension.")]
    public bool CheckOverhungLoad { get; set; } = false;

    [Category("04. Overhung load (clause 7, optional)")]
    [DisplayName("Applied input power, P1 (kW)")]
    [Description("Used only to establish T1 and T2 (via the wear-rating efficiency, Eq 21) for the overhung load check, clause 10.1's own equivalent-input-power comparison uses this value too.")]
    public double AppliedInputPower_kW { get; set; } = 1.946;

    [Category("04. Overhung load (clause 7, optional)")]
    [DisplayName("Drive type on worm shaft (input)")]
    public OverhungDriveType WormShaftDrive { get; set; } = OverhungDriveType.SingleOrMultipleChain;

    [Category("04. Overhung load (clause 7, optional)")]
    [DisplayName("Manual K_O, worm shaft (if 'OtherManualKo')")]
    public double ManualKoWorm { get; set; } = 1.0;

    [Category("04. Overhung load (clause 7, optional)")]
    [DisplayName("Pitch diameter of sprocket/gear/pulley on worm shaft, D1 (mm)")]
    [Description("0 disables the worm-shaft overhung load calculation.")]
    public double SprocketDiameterWorm { get; set; } = 0.0;

    [Category("04. Overhung load (clause 7, optional)")]
    [DisplayName("Drive type on wormgear shaft (output)")]
    public OverhungDriveType GearShaftDrive { get; set; } = OverhungDriveType.SingleOrMultipleChain;

    [Category("04. Overhung load (clause 7, optional)")]
    [DisplayName("Manual K_O, wormgear shaft (if 'OtherManualKo')")]
    public double ManualKoGear { get; set; } = 1.0;

    [Category("04. Overhung load (clause 7, optional)")]
    [DisplayName("Pitch diameter of sprocket/gear/pulley on wormgear shaft, D2 (mm)")]
    [Description("0 disables the wormgear-shaft overhung load calculation.")]
    public double SprocketDiameterGear { get; set; } = 0.0;

    // ----------------------------------------------------------------- 5. Selection and service factor (clause 10, Annex A.1)
    [Category("05. Selection and service factor (clause 10)")]
    [DisplayName("Specified (nameplate) input power (kW)")]
    [Description("Clause 10.1: multiplied by the service factor to get the equivalent input power, compared against the rated input power (min of wear and bending ratings).")]
    public double SpecifiedInputPower_kW { get; set; } = 1.5;

    [Category("05. Selection and service factor (clause 10)")]
    [DisplayName("Use manual service factor")]
    [Description("If unchecked, the service factor is looked up from Annex A Table A.1 (electric/hydraulic motor prime mover) using the duration and shock class below. Table A.3's large list of specific applications is not reproduced here - consult the standard directly for an application-specific value.")]
    public bool ManualServiceFactorOverride { get; set; } = false;

    [Category("05. Selection and service factor (clause 10)")]
    [DisplayName("Manual service factor")]
    public double ManualServiceFactor { get; set; } = 1.0;

    [Category("05. Selection and service factor (clause 10)")]
    [DisplayName("Duration of service")]
    [Description("Annex A Table A.1.")]
    public ServiceDuration Duration { get; set; } = ServiceDuration.Hr3To10;

    [Category("05. Selection and service factor (clause 10)")]
    [DisplayName("Shock load classification")]
    [Description("Annex A Table A.1 / A.3.1.")]
    public ShockLoadClass ShockLoad { get; set; } = ShockLoadClass.UniformLoad;

    // ----------------------------------------------------------------- 6. Lubricant viscosity recommendation (Table 9, optional)
    [Category("06. Lubricant viscosity (Table 9, optional)")]
    [DisplayName("Ambient temperature band")]
    public AmbientTemperatureBand AmbientBand { get; set; } = AmbientTemperatureBand.Between10CAnd52C;
}
