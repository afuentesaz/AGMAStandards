using AGMA929B22Calc.Calc;
using AGMA929B22Calc.Models;

namespace AGMA929B22Calc;

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

    private void LoadExampleButton_Click(object sender, EventArgs e)
    {
        switch (exampleComboBox.SelectedIndex)
        {
            case 1: LoadAnnexDExample(); break;
            case 2: LoadAnnexEExample(); break;
            default: LoadAnnexCExample(); break;
        }
    }

    private void SaveReportButton_Click(object sender, EventArgs e) => SaveReport();

    private void RunCalculation()
    {
        try
        {
            var calculator = new AgmaCalculator(_inputs);
            var (trace, warnings) = calculator.Calculate();

            resultsBox.Text = trace.BuildReport("AGMA 929-B22 CALCULATION RESULTS");

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

    private void LoadAnnexCExample()
    {
        // AGMA 929-B22, Annex C — spiral bevel example (face milling, generated, completing, Gleason).
        _inputs = new AgmaInputs
        {
            CuttingMethod = CuttingMethod.FaceMilling,
            WheelGeneration = WheelGeneration.Generated,
            PinionProcess = MemberProcess.Completing,
            WheelProcess = MemberProcess.Completing,
            KeMethod = KeMethod.FaceMillingSpreadBlade,

            Sigma = 90.0,
            Offset_a = 0.0,
            z1 = 14,
            z2 = 39,
            m_mn = 3.21331,
            b1 = 25.4,
            b2 = 25.4,
            Re1 = 93.973,
            Re2 = 93.973,
            Rm1 = 81.273,
            Rm2 = 81.273,
            Delta1 = 19.7468,
            Delta2 = 70.2532,
            ThetaF1 = 2.2259,
            ThetaF2 = 6.4935,
            Clearance_c = 0.964,

            Alpha_cv1 = 20.0,
            Alpha_cx1 = -20.0,
            Beta_m2 = 35.0,
            h_am1 = 4.83648,
            h_am2 = 1.58937,
            h_fm1 = 2.55337,
            h_fm2 = 5.80048,
            s_mn1 = 6.2788,
            s_mn2 = 3.67226,

            r_c0 = 114.3,
            z0 = 0,
            Alpha_BX2 = 0.0,
            W_finishingPointWidth = 0.0,
            S_A1 = 0.0,
            S_A2 = 0.0,
            y_mutilation = 0.05,
            Delta_f = 0.20,

            CalculatePinionChamfer = false,
            DesiredChamferTopLand = 1.6
        };
        propertyGrid1.SelectedObject = _inputs;
        statusLabel.ForeColor = Color.DarkBlue;
        statusLabel.Text = "Loaded AGMA 929-B22 Annex C spiral bevel example. Click Calculate to reproduce the published results.";
        RunCalculation();
    }

    private void LoadAnnexDExample()
    {
        // AGMA 929-B22, Annex D — hypoid example (1): face milling (spread blade),
        // non-generated wheel, completing.
        _inputs = new AgmaInputs
        {
            CuttingMethod = CuttingMethod.FaceMilling,
            WheelGeneration = WheelGeneration.NonGenerated,
            PinionProcess = MemberProcess.Completing,
            WheelProcess = MemberProcess.Completing,
            KeMethod = KeMethod.FaceMillingSpreadBlade,

            Sigma = 90.0,
            Offset_a = 38.10000,
            z1 = 11,
            z2 = 45,
            m_mn = 4.49574,
            b1 = 46.71200,
            b2 = 40.64000,
            Re1 = 151.29400,
            Re2 = 143.57000,
            Rm1 = 127.82600,
            Rm2 = 123.10500,
            Delta1 = 16.86930,
            Delta2 = 72.31850,
            ThetaF1 = 1.53490,
            ThetaF2 = 4.62770,
            Clearance_c = 1.77781,

            Alpha_cv1 = 18.14480,
            Alpha_cx1 = -21.85520,
            Beta_m2 = 30.41010,
            h_am1 = 7.27566,
            h_am2 = 1.53072,
            h_fm1 = 3.18216,
            h_fm2 = 9.05347,
            s_mn1 = 9.11957,
            s_mn2 = 4.85460,

            r_c0 = 114.30000,
            Alpha_BX2 = 25.00000,
            S_A1 = 0.0,
            S_A2 = 0.0,
            y_mutilation = 0.05,
            Delta_f = 0.20,

            CalculatePinionChamfer = false,
            DesiredChamferTopLand = 1.6
        };
        propertyGrid1.SelectedObject = _inputs;
        statusLabel.ForeColor = Color.DarkBlue;
        statusLabel.Text = "Loaded AGMA 929-B22 Annex D hypoid example (non-generated wheel). Click Calculate to reproduce the published results.";
        RunCalculation();
    }

    private void LoadAnnexEExample()
    {
        // AGMA 929-B22, Annex E — hypoid example (2): face hobbing (Gleason),
        // non-generated wheel, completing. This example also works the pinion tip
        // chamfer (4.8) all the way through, so it is loaded with the chamfer enabled.
        _inputs = new AgmaInputs
        {
            CuttingMethod = CuttingMethod.FaceHobbing,
            WheelGeneration = WheelGeneration.NonGenerated,
            PinionProcess = MemberProcess.Completing,
            WheelProcess = MemberProcess.Completing,
            KeMethod = KeMethod.FaceHobbingGleasonOrKlingelnberg,

            Sigma = 70.00000,
            Offset_a = 25.40000,
            z1 = 13,
            z2 = 44,
            m_mn = 4.04412,
            b1 = 38.10200,
            b2 = 35.00000,
            Re1 = 118.74600,
            Re2 = 142.87900,
            Rm1 = 99.56800,
            Rm2 = 125.25100,
            Delta1 = 18.80470,
            Delta2 = 50.34310,
            ThetaF1 = 0.00000,
            ThetaF2 = 0.00000,
            Clearance_c = 1.26400,

            Alpha_cv1 = 18.98480,
            Alpha_cx1 = -21.01520,
            Beta_m2 = 22.68220,
            h_am1 = 6.71300,
            h_am2 = 1.37500,
            h_fm1 = 2.63900,
            h_fm2 = 7.97700,
            s_mn1 = 8.98499,
            s_mn2 = 3.56128,

            r_c0 = 76.00000,
            z0 = 7,
            Alpha_BX2 = 25.00000,
            S_A1 = 0.0,
            S_A2 = 0.0,
            y_mutilation = 0.05,
            Delta_f = 0.20,

            CalculatePinionChamfer = true,
            DesiredChamferTopLand = 0.80000
        };
        propertyGrid1.SelectedObject = _inputs;
        statusLabel.ForeColor = Color.DarkBlue;
        statusLabel.Text = "Loaded AGMA 929-B22 Annex E hypoid example (face hobbing, non-generated wheel, with pinion tip chamfer). Click Calculate to reproduce the published results.";
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
            FileName = "AGMA929-B22_Results.txt"
        };
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            File.WriteAllText(dlg.FileName, resultsBox.Text);
        }
    }
}
