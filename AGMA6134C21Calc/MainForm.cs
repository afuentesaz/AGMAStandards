using AGMA6134C21Calc.Calc;
using AGMA6134C21Calc.Models;

namespace AGMA6134C21Calc;

public partial class MainForm : Form
{
    private AgmaInputs _inputs = new();

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
    private void SetInputs(AgmaInputs inputs)
    {
        _inputs = inputs;
        propertyGrid1.SelectedObject = inputs;
        propertyGrid1.PropertySort = PropertySort.Alphabetical;
        propertyGrid1.PropertySort = PropertySort.Categorized;
    }

    private void CalculateButton_Click(object sender, EventArgs e) => RunCalculation();

    private void LoadExampleButton_Click(object sender, EventArgs e) => LoadAnnexCExample();

    private void SaveReportButton_Click(object sender, EventArgs e) => SaveReport();

    private void RunCalculation()
    {
        try
        {
            var calculator = new AgmaCalculator(_inputs);
            var (trace, warnings) = calculator.Calculate();

            resultsBox.Text = trace.BuildReport("ANSI/AGMA 6134-C21 CALCULATION RESULTS");

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
        // ANSI/AGMA 6134-C21, Annex C worked example (Table C.1 input data).
        var inputs = new AgmaInputs
        {
            NormalModule = 4.0,
            z1 = 1,
            z2 = 30,
            LeadAngleWorm = 6.0,
            HelixAngleGear = 6.0,
            NormalPressureAngle = 20.0,
            WormSpeed = 1750.0,
            ActualFaceWidthGear = 27.94,
            RootDiameterGearThroat = 110.66,
            WormMaxRadialDeflection = 0.076,

            WormHeatTreatment = WormHeatTreat.Carburized,
            WormHardnessHRC = 58.0,
            GearCasting = GearCastingMethod.Centrifugal,

            Lubricant = OilType.PAO,

            CheckOverhungLoad = false,
            ManualServiceFactorOverride = true,
            ManualServiceFactor = 1.0,
            SpecifiedInputPower_kW = 1.9,
        };
        SetInputs(inputs);
        statusLabel.ForeColor = Color.DarkBlue;
        statusLabel.Text = "Loaded ANSI/AGMA 6134-C21 Annex C worked example (worm: 8620 steel, 58 HRC; wormgear: C90700 bronze, centrifugal cast; PAO oil). Click Calculate to reproduce the published results.";
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
            FileName = "AGMA6134-C21_Results.txt"
        };
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            File.WriteAllText(dlg.FileName, resultsBox.Text);
        }
    }
}
