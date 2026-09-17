using AGMA929B22Calc.Calc;
using AGMA929B22Calc.Models;

namespace AGMA929B22Calc;

public class MainForm : Form
{
    private readonly PropertyGrid _propertyGrid = new();
    private readonly TextBox _resultsBox = new();
    private readonly Button _calculateButton = new();
    private readonly Button _loadExampleButton = new();
    private readonly Button _saveReportButton = new();
    private readonly Label _statusLabel = new();
    private AgmaInputs _inputs = new();

    public MainForm()
    {
        Text = "AGMA 929-B22 — Bevel Gear Top Land, Slot Width and Cutter Edge Radius Calculator";
        Width = 1400;
        Height = 900;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1000, 650);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 460
        };

        // ---- Left panel: PropertyGrid + buttons
        var leftPanel = new Panel { Dock = DockStyle.Fill };

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 40,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(4)
        };

        _calculateButton.Text = "Calculate";
        _calculateButton.Width = 110;
        _calculateButton.Height = 30;
        _calculateButton.Click += (_, _) => RunCalculation();

        _loadExampleButton.Text = "Load AGMA Annex C Example";
        _loadExampleButton.Width = 190;
        _loadExampleButton.Height = 30;
        _loadExampleButton.Click += (_, _) => LoadAnnexCExample();

        _saveReportButton.Text = "Save Report...";
        _saveReportButton.Width = 110;
        _saveReportButton.Height = 30;
        _saveReportButton.Click += (_, _) => SaveReport();

        buttonPanel.Controls.Add(_calculateButton);
        buttonPanel.Controls.Add(_loadExampleButton);
        buttonPanel.Controls.Add(_saveReportButton);

        _propertyGrid.Dock = DockStyle.Fill;
        _propertyGrid.SelectedObject = _inputs;
        _propertyGrid.PropertySort = PropertySort.Categorized;
        _propertyGrid.ToolbarVisible = false;
        _propertyGrid.HelpVisible = true;

        leftPanel.Controls.Add(_propertyGrid);
        leftPanel.Controls.Add(buttonPanel);
        split.Panel1.Controls.Add(leftPanel);

        // ---- Right panel: results text box + status
        var rightPanel = new Panel { Dock = DockStyle.Fill };

        _statusLabel.Dock = DockStyle.Top;
        _statusLabel.Height = 26;
        _statusLabel.Padding = new Padding(6, 4, 6, 4);
        _statusLabel.ForeColor = Color.DarkRed;
        _statusLabel.Text = "";

        _resultsBox.Multiline = true;
        _resultsBox.ScrollBars = ScrollBars.Both;
        _resultsBox.Dock = DockStyle.Fill;
        _resultsBox.WordWrap = false;
        _resultsBox.Font = new Font(FontFamily.GenericMonospace, 9.5f);
        _resultsBox.ReadOnly = true;

        rightPanel.Controls.Add(_resultsBox);
        rightPanel.Controls.Add(_statusLabel);
        split.Panel2.Controls.Add(rightPanel);

        Controls.Add(split);
    }

    private void RunCalculation()
    {
        try
        {
            var calculator = new AgmaCalculator(_inputs);
            var (trace, warnings) = calculator.Calculate();

            _resultsBox.Text = trace.BuildReport("AGMA 929-B22 CALCULATION RESULTS");

            if (warnings.Count > 0)
            {
                _statusLabel.Text = "Warning: " + string.Join("  |  ", warnings);
                _statusLabel.ForeColor = Color.DarkRed;
            }
            else
            {
                _statusLabel.Text = "Calculation completed with no warnings.";
                _statusLabel.ForeColor = Color.DarkGreen;
            }
        }
        catch (Exception ex)
        {
            _statusLabel.ForeColor = Color.DarkRed;
            _statusLabel.Text = "Error: " + ex.Message;
            _resultsBox.Text = "Calculation failed:\r\n\r\n" + ex;
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
        _propertyGrid.SelectedObject = _inputs;
        _statusLabel.ForeColor = Color.DarkBlue;
        _statusLabel.Text = "Loaded AGMA 929-B22 Annex C spiral bevel example. Click Calculate to reproduce the published results.";
        RunCalculation();
    }

    private void SaveReport()
    {
        if (string.IsNullOrEmpty(_resultsBox.Text))
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
            File.WriteAllText(dlg.FileName, _resultsBox.Text);
        }
    }
}
