using System.ComponentModel;

namespace AGMA2003D19Calc.Models;

/// <summary>
/// All input data for ANSI/AGMA 2003-D19 (clauses 5 through 21, plus Annex C(M)). Bound to a
/// PropertyGrid. SI units throughout (mm, N/mm^2, kW, rpm, deg), matching the "M"-suffixed
/// equations of the standard.
/// </summary>
public class AgmaInputs
{
    // ----------------------------------------------------------------- 1. Gear type & rating
    [Category("01. Gear type and rating basics")]
    [DisplayName("Gear type")]
    [Description("Clause 1.1. Straight bevel, Zerol bevel (zero spiral angle) or spiral bevel. Governs the lengthwise curvature factor Kx (Yβ), clause 14, and the Annex C(M) contact-ratio/load-sharing branch.")]
    public GearType GearType { get; set; } = GearType.SpiralBevel;

    [Category("01. Gear type and rating basics")]
    [DisplayName("Tooth side analyzed")]
    [Description("Annex C(M).2.2/.3.4.3. For spiral bevel gears only: pinion concave / gear convex are normally the loaded (driving) surfaces.")]
    public ToothSide ToothSide { get; set; } = ToothSide.Concave;

    [Category("01. Gear type and rating basics")]
    [DisplayName("Shaft angle, Σ (deg)")]
    [Description("For reference/reporting. This standard's formulas do not apply to offset (hypoid) gears; use 90° unless a non-offset crossed-axis design is being analyzed.")]
    public double ShaftAngle { get; set; } = 90.0;

    [Category("01. Gear type and rating basics")]
    [DisplayName("Number of pinion teeth, z1 (n)")]
    public int z1 { get; set; } = 14;

    [Category("01. Gear type and rating basics")]
    [DisplayName("Number of gear teeth, z2 (N)")]
    public int z2 { get; set; } = 39;

    // ----------------------------------------------------------------- 2. Loads
    [Category("02. Loads and speed")]
    [DisplayName("Transmitted power, P (kW)")]
    [Description("Design power flowing through the mesh (assumed the same at pinion and gear, clause 6).")]
    public double Power_kW { get; set; } = 12.5;

    [Category("02. Loads and speed")]
    [DisplayName("Pinion speed, n1 (rpm)")]
    public double PinionSpeed_rpm { get; set; } = 1750.0;

    // ----------------------------------------------------------------- 3. Common (outer) geometry
    [Category("03. Common outer geometry")]
    [DisplayName("Outer cone distance, Re (mm)")]
    [Description("Table 1 / Annex C(M).1.3.1.")]
    public double Re { get; set; } = 93.98;

    [Category("03. Common outer geometry")]
    [DisplayName("Net face width, b (mm)")]
    [Description("Eq 1M etc. The narrower of the pinion and gear face widths.")]
    public double b { get; set; } = 25.4;

    [Category("03. Common outer geometry")]
    [DisplayName("Outer transverse module, m_et (mm)")]
    [Description("= 25.4 / (outer transverse diametral pitch, in^-1).")]
    public double m_et { get; set; } = 4.536;

    [Category("03. Common outer geometry")]
    [DisplayName("Normal pressure angle, αn (deg)")]
    public double Alpha_n { get; set; } = 20.0;

    [Category("03. Common outer geometry")]
    [DisplayName("Mean spiral angle, βm (deg)")]
    [Description("Use 0 for straight bevel and Zerol bevel gears.")]
    public double Beta_m { get; set; } = 35.0;

    [Category("03. Common outer geometry")]
    [DisplayName("Cutter radius, rc0 (mm)")]
    [Description("Spiral bevel gears only (Eq 21M). Ignored for straight bevel / Zerol bevel (Kx = 1.0 per Eq 22).")]
    public double CutterRadius { get; set; } = 114.3;

    // ----------------------------------------------------------------- 4. Pinion geometry
    [Category("04. Pinion geometry (Annex C(M) initial data)")]
    [DisplayName("Pinion outer pitch diameter, de1 (mm)")]
    public double de1 { get; set; } = 63.50;

    [Category("04. Pinion geometry (Annex C(M) initial data)")]
    [DisplayName("Pinion pitch angle, δ1 (deg)")]
    public double Delta1 { get; set; } = 19.75;

    [Category("04. Pinion geometry (Annex C(M) initial data)")]
    [DisplayName("Pinion face (tip) angle, δa1 (deg)")]
    public double Delta_a1 { get; set; } = 22.29;

    [Category("04. Pinion geometry (Annex C(M) initial data)")]
    [DisplayName("Pinion outer addendum, hae1 (mm)")]
    public double h_ae1 { get; set; } = 4.978;

    [Category("04. Pinion geometry (Annex C(M) initial data)")]
    [DisplayName("Pinion outer dedendum, hfe1 (mm)")]
    public double h_fe1 { get; set; } = 3.454;

    [Category("04. Pinion geometry (Annex C(M) initial data)")]
    [DisplayName("Pinion dedendum angle, θf1 (deg)")]
    public double Theta_f1 { get; set; } = 2.10;

    [Category("04. Pinion geometry (Annex C(M) initial data)")]
    [DisplayName("Pinion tool edge radius, ρao1 (mm)")]
    public double Rho_ao1 { get; set; } = 0.75;

    [Category("04. Pinion geometry (Annex C(M) initial data)")]
    [DisplayName("Pinion mean normal circular tooth thickness, s1 (mm)")]
    public double s1 { get; set; } = 6.60;

    // ----------------------------------------------------------------- 5. Gear geometry
    [Category("05. Gear geometry (Annex C(M) initial data)")]
    [DisplayName("Gear outer pitch diameter, de2 (mm)")]
    public double de2 { get; set; } = 176.90;

    [Category("05. Gear geometry (Annex C(M) initial data)")]
    [DisplayName("Gear pitch angle, δ2 (deg)")]
    public double Delta2 { get; set; } = 70.25;

    [Category("05. Gear geometry (Annex C(M) initial data)")]
    [DisplayName("Gear face (tip) angle, δa2 (deg)")]
    public double Delta_a2 { get; set; } = 71.35;

    [Category("05. Gear geometry (Annex C(M) initial data)")]
    [DisplayName("Gear outer addendum, hae2 (mm)")]
    public double h_ae2 { get; set; } = 1.634;

    [Category("05. Gear geometry (Annex C(M) initial data)")]
    [DisplayName("Gear outer dedendum, hfe2 (mm)")]
    public double h_fe2 { get; set; } = 6.622;

    [Category("05. Gear geometry (Annex C(M) initial data)")]
    [DisplayName("Gear dedendum angle, θf2 (deg)")]
    public double Theta_f2 { get; set; } = 4.02;

    [Category("05. Gear geometry (Annex C(M) initial data)")]
    [DisplayName("Gear tool edge radius, ρao2 (mm)")]
    public double Rho_ao2 { get; set; } = 0.75;

    [Category("05. Gear geometry (Annex C(M) initial data)")]
    [DisplayName("Gear mean normal circular tooth thickness, s2 (mm)")]
    public double s2 { get; set; } = 3.55;

    // ----------------------------------------------------------------- 6. Geometry factors I, J
    [Category("06. Geometry factors I (Zi), J (YJ) - clause 15")]
    [DisplayName("Geometry factor source")]
    [Description("Computed: full Annex C(M) analytical method using the geometry above. Manual: enter I and J directly (e.g. read from an Annex D chart, as the standard's own Annex E example does).")]
    public GeometryFactorMode GeometryFactorMode { get; set; } = GeometryFactorMode.ComputedAnnexC;

    [Category("06. Geometry factors I (Zi), J (YJ) - clause 15")]
    [DisplayName("Manual I (Zi)")]
    public double ManualI { get; set; } = 0.109;

    [Category("06. Geometry factors I (Zi), J (YJ) - clause 15")]
    [DisplayName("Manual J1, pinion (YJ1)")]
    public double ManualJ1 { get; set; } = 0.222;

    [Category("06. Geometry factors I (Zi), J (YJ) - clause 15")]
    [DisplayName("Manual J2, gear (YJ2)")]
    public double ManualJ2 { get; set; } = 0.217;

    [Category("06. Geometry factors I (Zi), J (YJ) - clause 15")]
    [DisplayName("Statically loaded (e.g. differential)")]
    [Description("C(M).1.3.5/.2.4.10: forces the inertia factor Zi (Yi) = 1.0 even when the modified contact ratio is below 2.0, as for vehicle drive-axle differential gears.")]
    public bool StaticallyLoaded { get; set; } = false;

    // ----------------------------------------------------------------- 7. Overload & dynamic factor
    [Category("07. Overload and dynamic factor")]
    [DisplayName("Overload factor, Ko (KA)")]
    [Description("Clause 7 / Annex A. Use Annex A Table A.1 for typical values based on prime mover and driven machine character.")]
    public double Ko { get; set; } = 1.0;

    [Category("07. Overload and dynamic factor")]
    [DisplayName("Dynamic factor source")]
    public DynamicFactorMode DynamicFactorMode { get; set; } = DynamicFactorMode.FromTransmissionAccuracy;

    [Category("07. Overload and dynamic factor")]
    [DisplayName("Transmission accuracy number, Qv")]
    [Description("Clause 10.6, 5 ≤ Qv ≤ 11 (Figure 1). Higher is more accurate; Qv ≥ 11 corresponds to \"very accurate gearing\", 10.6.1.")]
    public int Qv { get; set; } = 11;

    [Category("07. Overload and dynamic factor")]
    [DisplayName("Manual Kv")]
    [Description("Clause 10.7/10.8. Use 1.0 when known dynamic loads are already included in the transmitted load.")]
    public double ManualKv { get; set; } = 1.0;

    // ----------------------------------------------------------------- 8. Service factors (clause 9, optional)
    [Category("08. Service factors (optional, clause 9)")]
    [DisplayName("Service factor, pitting, CSF")]
    [Description("Clause 9.2. Leave at 1.0 unless a service factor determined from Eq 10/12 at unity life factor is being used instead of explicit life-cycle stress-cycle factors.")]
    public double CSF { get; set; } = 1.0;

    [Category("08. Service factors (optional, clause 9)")]
    [DisplayName("Service factor, bending, KSF")]
    public double KSF { get; set; } = 1.0;

    // ----------------------------------------------------------------- 9. Load distribution factor Km
    [Category("09. Load distribution factor, Km (KHβ) - clause 12")]
    [DisplayName("Mounting condition")]
    public MountingCondition Mounting { get; set; } = MountingCondition.OneMemberStraddleMounted;

    [Category("09. Load distribution factor, Km (KHβ) - clause 12")]
    [DisplayName("Override Km manually")]
    public bool ManualKmOverride { get; set; } = false;

    [Category("09. Load distribution factor, Km (KHβ) - clause 12")]
    [DisplayName("Manual Km (KHβ)")]
    public double ManualKm { get; set; } = 1.10;

    // ----------------------------------------------------------------- 10. Crowning factor Cxc
    [Category("10. Crowning factor, Cxc (Zxc) - clause 13")]
    [DisplayName("Teeth properly crowned")]
    [Description("Clause 13. If true, Cxc = 1.5. If false, Cxc = 2.0 or larger (manual value below).")]
    public bool ProperlyCrowned { get; set; } = true;

    [Category("10. Crowning factor, Cxc (Zxc) - clause 13")]
    [DisplayName("Manual Cxc (non-crowned teeth)")]
    public double CxcManual { get; set; } = 2.0;

    // ----------------------------------------------------------------- 11. Life, reliability, temperature
    [Category("11. Life, reliability, temperature")]
    [DisplayName("Required pinion life (cycles), NL1")]
    public double PinionLifeCycles { get; set; } = 1.0e9;

    [Category("11. Life, reliability, temperature")]
    [DisplayName("Required gear life (cycles), NL2")]
    public double GearLifeCycles { get; set; } = 3.59e8;

    [Category("11. Life, reliability, temperature")]
    [DisplayName("Critical service (lower KL/YNT band)")]
    [Description("Clause 16.2 / Figure 6. Above ~3x10^6 cycles the bending stress-cycle factor lies in a shaded band; the upper edge (this box unchecked) is for general applications, the lower edge (checked) for critical service.")]
    public bool CriticalService { get; set; } = false;

    [Category("11. Life, reliability, temperature")]
    [DisplayName("Reliability")]
    public ReliabilityLevel Reliability { get; set; } = ReliabilityLevel.OneFailureIn100;

    [Category("11. Life, reliability, temperature")]
    [DisplayName("Operating gear blank temperature, TT (°C)")]
    [Description("Clause 18. KT = 1.0 for 0°C to 120°C. Above 120°C, KT = (273+TT)/393 (Eq 31M).")]
    public double OperatingTemperatureC { get; set; } = 20.0;

    // ----------------------------------------------------------------- 12. Hardness ratio factor CH
    [Category("12. Hardness ratio factor, CH (Zw) - clause 17")]
    [DisplayName("Hardness ratio mode")]
    public HardnessRatioMode HardnessRatioMode { get; set; } = HardnessRatioMode.EqualHardness;

    [Category("12. Hardness ratio factor, CH (Zw) - clause 17")]
    [DisplayName("Minimum pinion Brinell hardness, HBP")]
    public double HB_Pinion { get; set; } = 400.0;

    [Category("12. Hardness ratio factor, CH (Zw) - clause 17")]
    [DisplayName("Minimum gear Brinell hardness, HBG")]
    public double HB_Gear { get; set; } = 250.0;

    [Category("12. Hardness ratio factor, CH (Zw) - clause 17")]
    [DisplayName("Pinion surface roughness, fP (µm)")]
    [Description("Surface-hardened-pinion mode only (17.2, Eq 30M uses Ra1 in µm).")]
    public double PinionSurfaceRoughness_um { get; set; } = 0.8;

    // ----------------------------------------------------------------- 13. Elastic coefficient Cp
    [Category("13. Elastic coefficient, Cp (ZE) - clause 20")]
    [DisplayName("Pinion Young's modulus, EP (N/mm²)")]
    public double E_Pinion { get; set; } = 207000.0;

    [Category("13. Elastic coefficient, Cp (ZE) - clause 20")]
    [DisplayName("Gear Young's modulus, EG (N/mm²)")]
    public double E_Gear { get; set; } = 207000.0;

    [Category("13. Elastic coefficient, Cp (ZE) - clause 20")]
    [DisplayName("Pinion Poisson's ratio, μP")]
    public double Mu_Pinion { get; set; } = 0.3;

    [Category("13. Elastic coefficient, Cp (ZE) - clause 20")]
    [DisplayName("Gear Poisson's ratio, μG")]
    public double Mu_Gear { get; set; } = 0.3;

    // ----------------------------------------------------------------- 14. Allowable stress numbers, pinion
    [Category("14. Allowable stress - pinion (clause 21)")]
    [DisplayName("Pinion material / heat treatment")]
    public MaterialSelection PinionMaterial { get; set; } = MaterialSelection.SteelCarburizedCaseHardened;

    [Category("14. Allowable stress - pinion (clause 21)")]
    [DisplayName("Pinion material grade")]
    public MaterialGrade PinionGrade { get; set; } = MaterialGrade.Grade1;

    [Category("14. Allowable stress - pinion (clause 21)")]
    [DisplayName("Pinion Brinell hardness, HB (through/flame/induction)")]
    public double PinionHB { get; set; } = 400.0;

    [Category("14. Allowable stress - pinion (clause 21)")]
    [DisplayName("Pinion root hardening (flame/induction only)")]
    public RootHardening PinionRootHardening { get; set; } = RootHardening.HardenedRoots;

    [Category("14. Allowable stress - pinion (clause 21)")]
    [DisplayName("Manual sac (σHlim), pinion (N/mm²)")]
    public double PinionSacManual { get; set; } = 1380.0;

    [Category("14. Allowable stress - pinion (clause 21)")]
    [DisplayName("Manual sat (σFlim), pinion (N/mm²)")]
    public double PinionSatManual { get; set; } = 205.0;

    // ----------------------------------------------------------------- 15. Allowable stress numbers, gear
    [Category("15. Allowable stress - gear (clause 21)")]
    [DisplayName("Gear material / heat treatment")]
    public MaterialSelection GearMaterial { get; set; } = MaterialSelection.SteelCarburizedCaseHardened;

    [Category("15. Allowable stress - gear (clause 21)")]
    [DisplayName("Gear material grade")]
    public MaterialGrade GearGrade { get; set; } = MaterialGrade.Grade1;

    [Category("15. Allowable stress - gear (clause 21)")]
    [DisplayName("Gear Brinell hardness, HB (through/flame/induction)")]
    public double GearHB { get; set; } = 250.0;

    [Category("15. Allowable stress - gear (clause 21)")]
    [DisplayName("Gear root hardening (flame/induction only)")]
    public RootHardening GearRootHardening { get; set; } = RootHardening.HardenedRoots;

    [Category("15. Allowable stress - gear (clause 21)")]
    [DisplayName("Manual sac (σHlim), gear (N/mm²)")]
    public double GearSacManual { get; set; } = 1380.0;

    [Category("15. Allowable stress - gear (clause 21)")]
    [DisplayName("Manual sat (σFlim), gear (N/mm²)")]
    public double GearSatManual { get; set; } = 205.0;

    // ----------------------------------------------------------------- 16. Safety factors
    [Category("16. Safety factors (clause 8)")]
    [DisplayName("Contact (pitting) safety factor, SH")]
    public double SH { get; set; } = 1.0;

    [Category("16. Safety factors (clause 8)")]
    [DisplayName("Bending safety factor, SF")]
    public double SF { get; set; } = 1.0;
}
