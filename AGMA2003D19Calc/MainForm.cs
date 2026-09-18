using AGMA2003D19Calc.Calc;
using AGMA2003D19Calc.Models;

namespace AGMA2003D19Calc;

public partial class MainForm : Form
{
    private AgmaInputs _inputs = new();

    public MainForm()
    {
        InitializeComponent();
        propertyGrid1.SelectedObject = _inputs;
        exampleComboBox.SelectedIndex = 0;
    }

    private void CalculateButton_Click(object sender, EventArgs e) => RunCalculation();

    private void LoadExampleButton_Click(object sender, EventArgs e) => LoadAnnexEExample();

    private void SaveReportButton_Click(object sender, EventArgs e) => SaveReport();

    private void RunCalculation()
    {
        try
        {
            var calculator = new AgmaCalculator(_inputs);
            var (trace, warnings) = calculator.Calculate();

            resultsBox.Text = trace.BuildReport("ANSI/AGMA 2003-D19 CALCULATION RESULTS");

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

    private void LoadAnnexEExample()
    {
        // ANSI/AGMA 2003-D19, Annex E worked example, converted to SI, with I/J entered
        // manually (Annex E itself reads them off an Annex D chart for this exact
        // 14/39-tooth, 20 deg pressure angle, 35 deg spiral angle, 90 deg shaft angle design).
        const double lbfInToNm = 0.112984829;
        double t1Nm = 1440.0 * lbfInToNm;
        double powerKw = Math.PI * 1750.0 * t1Nm / 30000.0;

        _inputs = new AgmaInputs
        {
            GearType = GearType.SpiralBevel,
            ToothSide = ToothSide.Concave,
            ShaftAngle = 90.0,
            z1 = 14,
            z2 = 39,

            Power_kW = powerKw,
            PinionSpeed_rpm = 1750.0,

            Re = 93.98,
            b = 25.4,
            m_et = 25.4 / 5.6,
            Alpha_n = 20.0,
            Beta_m = 35.0,
            CutterRadius = 114.3,

            GeometryFactorMode = GeometryFactorMode.ManualEntry,
            ManualI = 0.109,
            ManualJ1 = 0.222,
            ManualJ2 = 0.217,
            StaticallyLoaded = false,

            Ko = 1.0,
            DynamicFactorMode = DynamicFactorMode.FromTransmissionAccuracy,
            Qv = 11,

            CSF = 1.0,
            KSF = 1.0,

            Mounting = MountingCondition.OneMemberStraddleMounted,
            ManualKmOverride = false,

            ProperlyCrowned = true,

            PinionLifeCycles = 1.0e9,
            GearLifeCycles = 3.59e8,
            CriticalService = false,
            Reliability = ReliabilityLevel.OneFailureIn100,
            OperatingTemperatureC = 20.0,

            HardnessRatioMode = HardnessRatioMode.EqualHardness,

            E_Pinion = 207000.0,
            E_Gear = 207000.0,
            Mu_Pinion = 0.3,
            Mu_Gear = 0.3,

            PinionMaterial = MaterialSelection.SteelCarburizedCaseHardened,
            PinionGrade = MaterialGrade.Grade1,
            GearMaterial = MaterialSelection.SteelCarburizedCaseHardened,
            GearGrade = MaterialGrade.Grade1,

            SH = 1.0,
            SF = 1.0,
        };
        propertyGrid1.SelectedObject = _inputs;
        statusLabel.ForeColor = Color.DarkBlue;
        statusLabel.Text = "Loaded ANSI/AGMA 2003-D19 Annex E worked example (SI units, manual I/J). Click Calculate to reproduce the published results.";
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
            FileName = "AGMA2003-D19_Results.txt"
        };
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            File.WriteAllText(dlg.FileName, resultsBox.Text);
        }
    }
}
