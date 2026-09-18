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
