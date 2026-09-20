using ISO10300Calc.Calc;
using ISO10300Calc.Models;

namespace ISO10300Calc;

public partial class MainForm : Form
{
    private Iso10300Inputs _inputs = new();

    public MainForm()
    {
        InitializeComponent();
        exampleComboBox.SelectedIndex = 0;
        Load += (_, _) => SetInputs(_inputs);
    }

    /// <summary>
    /// Assigns a new input object to the PropertyGrid. WinForms' PropertyGrid has a known
    /// quirk where the very first category's header band can fail to paint (its properties
    /// still show, just without the gray category bar/label above them) - most reliably
    /// triggered by assigning SelectedObject before the control's window handle exists (e.g.
    /// from the form constructor), but it can recur on later reassignment too. Toggling
    /// PropertySort forces the grid to fully rebuild its category layout, which clears it.
    /// </summary>
    private void SetInputs(Iso10300Inputs inputs)
    {
        _inputs = inputs;
        propertyGrid1.SelectedObject = inputs;
        propertyGrid1.PropertySort = PropertySort.Alphabetical;
        propertyGrid1.PropertySort = PropertySort.Categorized;
    }

    private void CalculateButton_Click(object sender, EventArgs e) => RunCalculation();

    private void LoadExampleButton_Click(object sender, EventArgs e)
    {
        if (exampleComboBox.SelectedIndex == 1) LoadSample2Example();
        else if (exampleComboBox.SelectedIndex == 2) LoadSample3Example();
        else if (exampleComboBox.SelectedIndex == 3) LoadSample4Example();
        else if (exampleComboBox.SelectedIndex == 4) LoadSample5Example();
        else LoadSample1Example();
    }

    private void SaveReportButton_Click(object sender, EventArgs e) => SaveReport();

    private void RunCalculation()
    {
        try
        {
            var calculator = new Iso10300Calculator(_inputs);
            var (trace, warnings) = calculator.Calculate();

            resultsBox.Text = trace.BuildReport("ISO 10300:2023 CALCULATION RESULTS");

            if (warnings.Count > 0)
            {
                statusLabel.Text = "Warning: " + string.Join("  |  ", warnings);
                statusLabel.ForeColor = Color.DarkRed;
            }
            else
            {
                statusLabel.Text = "Calculation completed with no warnings.";
                statusLabel.ForeColor = Color.DarkGreen;
            }
        }
        catch (Exception ex)
        {
            statusLabel.ForeColor = Color.DarkRed;
            statusLabel.Text = "Error: " + ex.Message;
            resultsBox.Text = "Calculation failed:\r\n\r\n" + ex;
        }
    }

    private void LoadSample1Example()
    {
        // ISO/TR 10300-30:2024, Annex A, Sample 1: a spiral bevel gear pair without hypoid
        // offset (z1=14, z2=39), rated by Method B1. Every value below is taken directly from
        // the technical report's own Tables A.1 to A.4.
        var inputs = new Iso10300Inputs
        {
            GearType = GearType.SpiralBevel,
            ShaftAngle = 90.0,
            z1 = 14,
            z2 = 39,

            T1_Nm = 300.0,
            PinionSpeed_rpm = 1200.0,

            Re = 93.973,
            b = 25.4,
            EffectiveFacewidthFraction = 0.85,
            mmn = 3.213,
            met2 = 4.536,
            Alpha_nD = 20.0,
            Alpha_nC = 20.0,
            Alpha_eD = 20.0,
            Alpha_eC = 20.0,
            Alpha_lim = 0.0,
            Beta_m1 = 35.0,
            Beta_m2 = 35.0,
            CutterRadius = 114.3,

            dm1 = 54.918,
            Delta1 = 19.747,
            h_am1 = 4.837,
            h_fm1 = 2.393,
            x_hm1 = 0.505,
            x_sm1 = 0.036,
            Rho_a01 = 0.8,
            Spr1 = 0.0,

            dm2 = 152.987,
            Delta2 = 70.253,
            h_am2 = 1.590,
            h_fm2 = 5.640,
            x_hm2 = -0.505,
            x_sm2 = -0.055,
            Rho_a02 = 1.2,
            Spr2 = 0.0,

            k_hfp = 1.25,
            MaterialDensity = 7.86e-6,

            KA = 1.1,
            DynamicFactorMode = DynamicFactorMode.MethodB,
            fpt1_um = 12.0,
            fpt2_um = 26.0,

            Mounting = MountingCondition.OneMemberCantileverMounted,
            ContactVerification = ContactPatternVerification.CheckedUnderLightTestLoad,
            ManualKHbetaOverride = false,

            TransverseLoadFactorMode = TransverseLoadFactorMode.MethodB,
            ProfileCrowning = ProfileCrowning.Low,

            MaterialFamily = MaterialFamily.CaseOrThroughHardenedSteel,
            PinionLifeCycles = 3.0e6,
            WheelLifeCycles = 3.0e6,

            HardnessRatioMode = HardnessRatioMode.EqualHardness,

            E_Pinion = 210000.0,
            E_Wheel = 210000.0,
            Nu_Pinion = 0.3,
            Nu_Wheel = 0.3,

            Viscosity40 = 150.0,
            Rz_Flank_Pinion = 8.0,
            Rz_Flank_Wheel = 8.0,
            Rz_Root = 16.0,

            SigmaHlim = 1500.0,
            SigmaFlim = 480.0,

            SH_min = 1.0,
            SF_min = 1.3,
        };
        SetInputs(inputs);
        statusLabel.ForeColor = Color.DarkBlue;
        statusLabel.Text = "Loaded ISO/TR 10300-30:2024 Sample 1 (spiral bevel, no offset). Click Calculate to reproduce the published results.";
        RunCalculation();
    }

    private void LoadSample2Example()
    {
        // ISO/TR 10300-30:2024, Annex B, Sample 2: a hypoid gear pair (z1=13, z2=42, hypoid
        // offset a=15mm), rated by Method B1. Every value below is taken directly from the
        // technical report's own Tables B.1 to B.4 (xhm1/xhm2 back-derived from the given
        // absolute addenda - see the comment in Calc/SelfTest.cs, RunSample2Check).
        var inputs = new Iso10300Inputs
        {
            GearType = GearType.SpiralBevel,
            ShaftAngle = 90.0,
            z1 = 13,
            z2 = 42,

            T1_Nm = 250.0,
            PinionSpeed_rpm = 1200.0,

            HypoidOffset_a = 15.0,
            Zeta_mp = 11.390,
            Zeta_m = 10.603,
            Zeta_R = 10.319,

            Re = 89.600,
            de2 = 170.0,
            b = 30.0,
            EffectiveFacewidthFraction = 0.85,
            mmn = 2.640,
            met2 = 4.048,
            Alpha_nD = 17.748,
            Alpha_nC = 22.252,
            Alpha_eD = 20.0,
            Alpha_eC = 20.0,
            Alpha_lim = -2.252,
            Beta_m1 = 50.0,
            Beta_m2 = 38.610,
            CutterRadius = 63.5,

            dm1 = 53.384,
            Delta1 = 21.291,
            h_am1 = 3.432,
            h_fm1 = 2.508,
            x_hm1 = 0.30,
            x_sm1 = 0.038,
            Rho_a01 = 0.8,
            Spr1 = 0.0,

            dm2 = 141.877,
            Delta2 = 68.321,
            h_am2 = 1.848,
            h_fm2 = 4.092,
            x_hm2 = -0.30,
            x_sm2 = -0.062,
            Rho_a02 = 1.2,
            Spr2 = 0.0,

            k_hfp = 1.25,
            MaterialDensity = 7.86e-6,

            KA = 1.1,
            DynamicFactorMode = DynamicFactorMode.MethodB,
            fpt1_um = 11.6,
            fpt2_um = 23.5,

            Mounting = MountingCondition.OneMemberCantileverMounted,
            ContactVerification = ContactPatternVerification.CheckedUnderLightTestLoad,
            ManualKHbetaOverride = false,

            TransverseLoadFactorMode = TransverseLoadFactorMode.MethodB,
            ProfileCrowning = ProfileCrowning.Low,

            MaterialFamily = MaterialFamily.CaseOrThroughHardenedSteel,
            PinionLifeCycles = 3.0e6,
            WheelLifeCycles = 3.0e6,

            HardnessRatioMode = HardnessRatioMode.EqualHardness,

            E_Pinion = 210000.0,
            E_Wheel = 210000.0,
            Nu_Pinion = 0.3,
            Nu_Wheel = 0.3,

            Viscosity40 = 150.0,
            Rz_Flank_Pinion = 8.0,
            Rz_Flank_Wheel = 8.0,
            Rz_Root = 16.0,

            SigmaHlim = 1500.0,
            SigmaFlim = 480.0,

            SH_min = 1.0,
            SF_min = 1.3,
        };
        SetInputs(inputs);
        statusLabel.ForeColor = Color.DarkBlue;
        statusLabel.Text = "Loaded ISO/TR 10300-30:2024 Sample 2 (hypoid gear, a=15mm). Click Calculate to reproduce the published results (note: ZLS/YLS have a known ~10% high bias for this sample - see the implementation findings docx).";
        RunCalculation();
    }

    private void LoadSample3Example()
    {
        // ISO/TR 10300-30:2024, Annex C, Sample 3: a hypoid gear pair (z1=9, z2=34, hypoid
        // offset a=31.75mm), rated by Method B1. Every value below is taken directly from the
        // technical report's own Tables C.1 to C.4 (xhm1/xhm2 back-derived from the given
        // absolute addenda, same technique as Sample 2 - see Calc/SelfTest.cs,
        // RunSample3Check). Dynamic factor is supplied manually (matching the published
        // Kv-B = 1.000) because this sample's own table is internally inconsistent for Kv-B -
        // see the comment in RunSample3Check for the full trace of that finding.
        var inputs = new Iso10300Inputs
        {
            GearType = GearType.SpiralBevel,
            ShaftAngle = 90.0,
            z1 = 9,
            z2 = 34,

            T1_Nm = 250.0,
            PinionSpeed_rpm = 4500.0,

            HypoidOffset_a = 31.75,
            Zeta_mp = 23.981,
            Zeta_m = 21.647,
            Zeta_R = 21.647,

            Re = 76.756,
            de2 = 95.168,
            b = 26.0,
            EffectiveFacewidthFraction = 0.85,
            mmn = 4.028,
            met2 = 4.997,
            Alpha_nD = 15.868,
            Alpha_nC = 24.132,
            Alpha_eD = 20.0,
            Alpha_eC = 20.0,
            Alpha_lim = -4.132,
            Beta_m1 = 44.991,
            Beta_m2 = 21.009,
            CutterRadius = 76.0,

            dm1 = 51.258,
            Delta1 = 24.763,
            h_am1 = 5.840,
            h_fm1 = 3.222,
            x_hm1 = 0.450,
            x_sm1 = 0.040,
            Rho_a01 = 0.8,
            Spr1 = 0.0,

            dm2 = 146.700,
            Delta2 = 63.212,
            h_am2 = 2.215,
            h_fm2 = 6.847,
            x_hm2 = -0.450,
            x_sm2 = -0.060,
            Rho_a02 = 1.2,
            Spr2 = 0.0,

            k_hfp = 1.25,
            MaterialDensity = 7.86e-6,

            KA = 1.1,
            DynamicFactorMode = DynamicFactorMode.Manual,
            ManualKv = 1.000,
            fpt1_um = 12.0,
            fpt2_um = 25.0,

            Mounting = MountingCondition.OneMemberCantileverMounted,
            ContactVerification = ContactPatternVerification.CheckedUnderLightTestLoad,
            ManualKHbetaOverride = false,

            TransverseLoadFactorMode = TransverseLoadFactorMode.MethodB,
            ProfileCrowning = ProfileCrowning.Low,

            MaterialFamily = MaterialFamily.CaseOrThroughHardenedSteel,
            PinionLifeCycles = 3.0e6,
            WheelLifeCycles = 3.0e6,

            HardnessRatioMode = HardnessRatioMode.EqualHardness,

            E_Pinion = 210000.0,
            E_Wheel = 210000.0,
            Nu_Pinion = 0.3,
            Nu_Wheel = 0.3,

            Viscosity40 = 100.0,
            Rz_Flank_Pinion = 3.0,
            Rz_Flank_Wheel = 3.0,
            Rz_Root = 10.0,

            SigmaHlim = 1510.0,
            SigmaFlim = 500.0,

            SH_min = 1.0,
            SF_min = 1.3,
        };
        SetInputs(inputs);
        statusLabel.ForeColor = Color.DarkBlue;
        statusLabel.Text = "Loaded ISO/TR 10300-30:2024 Sample 3 (hypoid gear, a=31.75mm). Click Calculate to reproduce the published results (note: lbm/sigmaH0 have a known source-document discrepancy for this sample - see the implementation findings docx).";
        RunCalculation();
    }

    private void LoadSample4Example()
    {
        // ISO/TR 10300-30:2024, Annex D, Sample 4: a hypoid gear pair designed via ISO 23509
        // Method 3 (z1=12, z2=49, hypoid offset a=40mm), rated by Method B1, with a NON-GENERATED
        // (face-hobbed) wheel - the only sample exercising ISO 10300-3, 6.4.1.3's non-generated
        // tooth-form formulae. Every value below is taken directly from the technical report's
        // own Tables D.1 to D.4. Dynamic factor uses Method B directly (this sample's own table
        // is internally consistent for Kv-B, unlike Sample 3).
        var inputs = new Iso10300Inputs
        {
            GearType = GearType.SpiralBevel,
            ShaftAngle = 90.0,
            z1 = 12,
            z2 = 49,

            T1_Nm = 3000.0,
            PinionSpeed_rpm = 800.0,

            HypoidOffset_a = 40.0,
            Zeta_mp = 12.922,
            Zeta_m = 12.265,
            Zeta_R = 12.265,

            Re = 191.949,
            de2 = 400.0,
            b = 60.0,
            EffectiveFacewidthFraction = 0.85,
            mmn = 6.065,
            met2 = 8.163,
            Alpha_nD = 19.0,
            Alpha_nC = 21.0,
            Alpha_eD = 20.731,
            Alpha_eC = 19.269,
            Alpha_lim = -1.731,
            Beta_m1 = 42.922,
            Beta_m2 = 30.0,
            CutterRadius = 135.0,

            dm1 = 99.377,
            Delta1 = 18.200,
            h_am1 = 7.278,
            h_fm1 = 6.368,
            x_hm1 = 0.2,
            x_sm1 = 0.031,
            Rho_a01 = 0.8,
            Spr1 = 0.0,

            dm2 = 343.151,
            Delta2 = 71.360,
            h_am2 = 4.852,
            h_fm2 = 8.794,
            x_hm2 = 0.0,
            x_sm2 = -0.031,
            Rho_a02 = 1.2,
            Spr2 = 0.0,

            k_hfp = 1.25,
            MaterialDensity = 7.86e-6,
            WheelIsNonGenerated = true,

            KA = 1.1,
            DynamicFactorMode = DynamicFactorMode.MethodB,
            fpt1_um = 14.0,
            fpt2_um = 27.0,

            Mounting = MountingCondition.OneMemberCantileverMounted,
            ContactVerification = ContactPatternVerification.CheckedUnderLightTestLoad,
            ManualKHbetaOverride = false,

            TransverseLoadFactorMode = TransverseLoadFactorMode.MethodB,
            ProfileCrowning = ProfileCrowning.Low,

            MaterialFamily = MaterialFamily.CaseOrThroughHardenedSteel,
            PinionLifeCycles = 3.0e6,
            WheelLifeCycles = 3.0e6,

            HardnessRatioMode = HardnessRatioMode.EqualHardness,

            E_Pinion = 210000.0,
            E_Wheel = 210000.0,
            Nu_Pinion = 0.3,
            Nu_Wheel = 0.3,

            Viscosity40 = 150.0,
            Rz_Flank_Pinion = 8.0,
            Rz_Flank_Wheel = 8.0,
            Rz_Root = 16.0,

            SigmaHlim = 1500.0,
            SigmaFlim = 480.0,

            SH_min = 1.0,
            SF_min = 1.3,
        };
        SetInputs(inputs);
        statusLabel.ForeColor = Color.DarkBlue;
        statusLabel.Text = "Loaded ISO/TR 10300-30:2024 Sample 4 (hypoid gear, non-generated wheel). Click Calculate to reproduce the published results (note: ZLS/YLS have a known ~3-8% high bias for this sample's hypoid offset asymmetry - see the implementation findings docx).";
        RunCalculation();
    }

    private void LoadSample5Example()
    {
        // AGMA-vs-ISO comparison: ANSI/AGMA 2003-D19, Annex E's own worked example (z1=14,
        // z2=39, 20 deg pressure angle, 35 deg spiral angle, 90 deg shaft angle, Re=93.98mm,
        // b=25.4mm, cutter radius 114.3mm) is, by its basic proportions, the SAME reference
        // spiral bevel gearset as ISO/TR 10300-30 Sample 1 (Re=93.973mm, b=25.4mm, cutter
        // radius 114.3mm, identical teeth/angles - AGMA2003D19Calc/Calc/SelfTest.cs,
        // RunAnnexECheck). Annex E itself never publishes full tooth geometry (it bypasses
        // Annex C(M) via an Annex D chart lookup for I/J), so this example reuses Sample 1's
        // own full ISO 23509-derived geometry (dm1/dm2, delta1/delta2, ham/hfm, xhm/xsm) as-is,
        // and overrides only the load, life, material and dynamic-factor inputs with AGMA Annex
        // E's own published values, to rate the SAME physical gear under the SAME operating
        // point through both standards' formulas side by side. Judgment calls made explicit:
        //   - Kv is supplied manually as AGMA's own computed 1.081 (DynamicFactorMode.Manual)
        //     rather than derived from ISO's own Method B/C, because AGMA's quality number
        //     Qv=11 and ISO's accuracy grade B (valid range 5-8 for the Kv-C curve) have no
        //     official numeric conversion - holding Kv equal isolates the comparison to the
        //     pitting/bending STRESS formulas, which is the point of this example.
        //   - Mounting = OneMemberCantileverMounted: AGMA's "one member straddle mounted" and
        //     ISO's "one member cantilever mounted" describe the identical physical arrangement
        //     from opposite ends (AGMA counts straddle-mounted/good members, ISO counts
        //     cantilever-mounted/bad ones), so KHbeta-C is left to compute normally here rather
        //     than manually overridden - a genuine, meaningful structural comparison point.
        //   - SigmaHlim/SigmaFlim use AGMA's own sac1/sat1 = 1380/205 N/mm^2 (Grade 1 carburized
        //     case-hardened steel, clause 21) rather than ISO Sample 1's own 1500/480 N/mm^2, so
        //     the material input is held identical between the two calculations. Note the large
        //     gap between AGMA's sat=205 and ISO's typical sigmaFlim~480 for nominally similar
        //     case-hardened steel is itself a known, real difference in how the two standards
        //     define their respective bending allowable stress numbers (different reference test
        //     gears/methodologies - ISO 10300-2's own text notes ISO 6336-5 carries a formal
        //     cross-reference table to "equivalent ANSI/AGMA 2003 steel").
        //   - KA=1.0 (AGMA Ko), T1/n1, life cycles, elastic modulus and Poisson's ratio are
        //     AGMA Annex E's own published values, converted to SI.
        //   - Lubricant viscosity/roughness (ZL/Zv/ZR) and the transverse load factor KHalpha
        //     have no AGMA counterpart at all (AGMA 2003's rating formula does not separate
        //     these out), so Sample 1's own ISO defaults are kept unchanged.
        // This is a comparison example only - not part of --selftest, since there is no
        // independently published "ISO rating of the AGMA Annex E gear" to check against.
        const double lbfInToNm = 0.112984829;
        var inputs = new Iso10300Inputs
        {
            GearType = GearType.SpiralBevel,
            ShaftAngle = 90.0,
            z1 = 14,
            z2 = 39,

            T1_Nm = 1440.0 * lbfInToNm, // AGMA Annex E: T1 = 1440 lbf.in
            PinionSpeed_rpm = 1750.0,

            HypoidOffset_a = 0.0,
            Zeta_mp = 0.0,
            Zeta_m = 0.0,
            Zeta_R = 0.0,

            Re = 93.973,
            de2 = 176.90,
            b = 25.4,
            EffectiveFacewidthFraction = 0.85,
            mmn = 3.213,
            met2 = 4.536,
            Alpha_nD = 20.0,
            Alpha_nC = 20.0,
            Alpha_eD = 20.0,
            Alpha_eC = 20.0,
            Alpha_lim = 0.0,
            Beta_m1 = 35.0,
            Beta_m2 = 35.0,
            CutterRadius = 114.3,

            dm1 = 54.918,
            Delta1 = 19.747,
            h_am1 = 4.837,
            h_fm1 = 2.393,
            x_hm1 = 0.505,
            x_sm1 = 0.036,
            Rho_a01 = 0.8,
            Spr1 = 0.0,

            dm2 = 152.987,
            Delta2 = 70.253,
            h_am2 = 1.590,
            h_fm2 = 5.640,
            x_hm2 = -0.505,
            x_sm2 = -0.055,
            Rho_a02 = 1.2,
            Spr2 = 0.0,

            k_hfp = 1.25,
            MaterialDensity = 7.86e-6,

            KA = 1.0, // AGMA Annex E: Ko = 1.0
            DynamicFactorMode = DynamicFactorMode.Manual,
            ManualKv = 1.081, // AGMA Annex E's own computed Kv (Qv=11) - see class note above
            fpt1_um = 12.0,
            fpt2_um = 26.0,

            Mounting = MountingCondition.OneMemberCantileverMounted,
            ContactVerification = ContactPatternVerification.CheckedUnderLightTestLoad,
            ManualKHbetaOverride = false,

            TransverseLoadFactorMode = TransverseLoadFactorMode.MethodB,
            ProfileCrowning = ProfileCrowning.Low, // AGMA Annex E: properly crowned

            MaterialFamily = MaterialFamily.CaseOrThroughHardenedSteel,
            PinionLifeCycles = 1.0e9, // AGMA Annex E: NL1
            WheelLifeCycles = 3.59e8, // AGMA Annex E: NL2

            HardnessRatioMode = HardnessRatioMode.EqualHardness,
            HBW_Pinion = 400.0, // AGMA Annex E: HBP (informational only under EqualHardness)
            HBW_Wheel = 250.0,  // AGMA Annex E: HBG (informational only under EqualHardness)

            E_Pinion = 207000.0, // AGMA Annex E: EP
            E_Wheel = 207000.0,  // AGMA Annex E: EG
            Nu_Pinion = 0.3,
            Nu_Wheel = 0.3,

            Viscosity40 = 150.0,
            Rz_Flank_Pinion = 8.0,
            Rz_Flank_Wheel = 8.0,
            Rz_Root = 16.0,

            SigmaHlim = 1380.0, // AGMA Annex E: sac1 = sac2, Grade 1 carburized case-hardened steel
            SigmaFlim = 205.0,  // AGMA Annex E: sat1 = sat2 - see class note on the AGMA/ISO gap

            SH_min = 1.0, // matches AGMA Annex E's own SH = 1.0
            SF_min = 1.3, // ISO 10300-3, 5.2/6.3 mandated minimum for spiral bevel (not an AGMA figure)
        };
        SetInputs(inputs);
        statusLabel.ForeColor = Color.DarkBlue;
        statusLabel.Text = "Loaded ANSI/AGMA 2003-D19 Annex E example (same reference gearset as ISO/TR 10300-30 Sample 1), rated under ISO 10300 for direct AGMA-vs-ISO comparison. Click Calculate. See LoadSample5Example() for the judgment calls made in mapping AGMA's inputs onto this tool's fields.";
        RunCalculation();
    }

    private void SaveReport()
    {
        if (string.IsNullOrEmpty(resultsBox.Text))
        {
            MessageBox.Show(this, "Run a calculation first.", "Nothing to save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dlg = new SaveFileDialog
        {
            Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*",
            FileName = "ISO10300_Results.txt"
        };
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            File.WriteAllText(dlg.FileName, resultsBox.Text);
        }
    }
}
