using ISO23509Calc.Calc;
using ISO23509Calc.Models;

namespace ISO23509Calc;

public partial class MainForm : Form
{
    private Iso23509Inputs _inputs = new();

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
    private void SetInputs(Iso23509Inputs inputs)
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
        else LoadSample1Example();
    }

    private void SaveReportButton_Click(object sender, EventArgs e) => SaveReport();

    private void RunCalculation()
    {
        try
        {
            var calculator = new Iso23509Calculator(_inputs);
            var (trace, _, _, warnings) = calculator.Calculate();

            resultsBox.Text = trace.BuildReport("ISO 23509:2016 CALCULATION RESULTS");

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
        // ISO 23509:2016, Annex F.2: spiral bevel gear pair without hypoid offset (z1=14,
        // z2=39), Method 0, face milling. Every value below is taken directly from Tables
        // F.1-F.3.
        var inputs = new Iso23509Inputs
        {
            Method = PitchConeMethod.Method0,
            Process = ManufacturingProcess.FaceMilling,
            Sigma = 90.0,
            a = 0.0,
            z1 = 14,
            z2 = 39,
            de2 = 176.893,
            b2 = 25.4,
            beta_m2 = 35.0,
            rc0 = 114.3,

            DataType = InputDataType.TypeII,
            alpha_dD = 20.0,
            alpha_dC = 20.0,
            f_alim = 0.0,
            c_ham = 0.24737,
            k_d = 2.000,
            k_c = 0.125,
            k_t = 0.0915,

            Backlash = BacklashMode.OuterNormal_jen,
            BacklashValue = 0.127,

            ThetaSource = ThetaA2Source.Manual,
            theta_a2 = 2.1342,
            theta_f2 = 6.4934,

            CheckUndercut = true,
        };
        SetInputs(inputs);
        statusLabel.ForeColor = Color.DarkBlue;
        statusLabel.Text = "Loaded ISO 23509:2016 Annex F.2 (spiral bevel, Method 0, face milling). Click Calculate to reproduce the published results.";
        RunCalculation();
    }

    private void LoadSample2Example()
    {
        // ISO 23509:2016, Annex F.3: hypoid gear pair (z1=13, z2=42, a=15mm), Method 1
        // (Gleason), face milling. Every value below is taken directly from Tables F.4-F.6.
        var inputs = new Iso23509Inputs
        {
            Method = PitchConeMethod.Method1,
            Process = ManufacturingProcess.FaceMilling,
            Sigma = 90.0,
            a = 15.0,
            z1 = 13,
            z2 = 42,
            de2 = 170.0,
            b2 = 30.0,
            beta_m1_desired = 50.0,
            rc0 = 63.5,

            DataType = InputDataType.TypeII,
            alpha_dD = 20.0,
            alpha_dC = 20.0,
            f_alim = 1.0,
            c_ham = 0.35,
            k_d = 2.000,
            k_c = 0.125,
            k_t = 0.1,

            Backlash = BacklashMode.OuterTransverse_jet2,
            BacklashValue = 0.2,

            ThetaSource = ThetaA2Source.Manual,
            theta_a2 = 1.0,
            theta_f2 = 4.0,

            CheckUndercut = true,
        };
        SetInputs(inputs);
        statusLabel.ForeColor = Color.DarkBlue;
        statusLabel.Text = "Loaded ISO 23509:2016 Annex F.3 (hypoid, Method 1/Gleason, face milling). Click Calculate to reproduce the published results.";
        RunCalculation();
    }

    private void LoadSample3Example()
    {
        // ISO 23509:2016, Annex F.4: hypoid gear pair (z1=9, z2=34, a=31.75mm), Method 2
        // (Oerlikon), face hobbing. Every value below is taken directly from Tables F.7-F.9.
        var inputs = new Iso23509Inputs
        {
            Method = PitchConeMethod.Method2,
            Process = ManufacturingProcess.FaceHobbing,
            Sigma = 90.0,
            a = 31.75,
            z1 = 9,
            z2 = 34,
            dm2 = 146.7,
            b2 = 26.0,
            beta_m2 = 21.009,
            rc0 = 76.0,
            z0 = 13,

            DataType = InputDataType.TypeII,
            alpha_dD = 20.0,
            alpha_dC = 20.0,
            f_alim = 1.0,
            c_ham = 0.275,
            k_d = 2.000,
            k_c = 0.125,
            k_t = 0.1,

            Backlash = BacklashMode.OuterTransverse_jet2,
            BacklashValue = 0.2,

            ThetaSource = ThetaA2Source.Manual,
            theta_a2 = 0.0,
            theta_f2 = 0.0,

            CheckUndercut = true,
        };
        SetInputs(inputs);
        statusLabel.ForeColor = Color.DarkBlue;
        statusLabel.Text = "Loaded ISO 23509:2016 Annex F.4 (hypoid, Method 2/Oerlikon, face hobbing). Click Calculate to reproduce the published results.";
        RunCalculation();
    }

    private void LoadSample4Example()
    {
        // ISO 23509:2016, Annex F.5: hypoid gear pair (z1=12, z2=49, a=40mm), Method 3
        // (Klingelnberg), face hobbing, data type I. Every value below is taken directly
        // from Tables F.10-F.12.
        var inputs = new Iso23509Inputs
        {
            Method = PitchConeMethod.Method3,
            Process = ManufacturingProcess.FaceHobbing,
            Sigma = 90.0,
            a = 40.0,
            z1 = 12,
            z2 = 49,
            de2 = 400.0,
            b2 = 60.0,
            beta_m2 = 30.0,
            rc0 = 135.0,
            z0 = 5,

            DataType = InputDataType.TypeI,
            alpha_dD = 19.0,
            alpha_dC = 21.0,
            f_alim = 0.0,
            x_hm1 = 0.2,
            k_hap = 1.0,
            k_hfp = 1.25,
            x_smn = 0.031,

            Backlash = BacklashMode.OuterTransverse_jet2,
            BacklashValue = 0.0,

            ThetaSource = ThetaA2Source.Manual,
            theta_a2 = 0.0,
            theta_f2 = 0.0,

            CheckUndercut = true,
        };
        SetInputs(inputs);
        statusLabel.ForeColor = Color.DarkBlue;
        statusLabel.Text = "Loaded ISO 23509:2016 Annex F.5 (hypoid, Method 3/Klingelnberg, face hobbing, data type I). Click Calculate to reproduce the published results.";
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
            FileName = "ISO23509_Results.txt"
        };
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            File.WriteAllText(dlg.FileName, resultsBox.Text);
        }
    }
}
