using AGMA2101E25Calc.Calc;
using AGMA2101E25Calc.Models;

namespace AGMA2101E25Calc;

public partial class MainForm : Form
{
    private AgmaInputs _inputs = Examples.AnnexB();

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

    private void LoadExampleButton_Click(object sender, EventArgs e) => LoadExample(exampleComboBox.SelectedIndex);

    private void SaveReportButton_Click(object sender, EventArgs e) => SaveReport();

    private void RunCalculation()
    {
        try
        {
            var calculator = new AgmaCalculator(_inputs);
            var (trace, warnings) = calculator.Calculate();

            resultsBox.Text = trace.BuildReport("ANSI/AGMA 2101-E25 CALCULATION RESULTS");

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

    private void LoadExample(int index)
    {
        if (index < 0) return;
        SetInputs(Examples.Create(index));
        statusLabel.ForeColor = Color.DarkBlue;
        statusLabel.Text = index switch
        {
            0 => "Loaded ANSI/AGMA 2101-E25 Annex B illustrative example (carburized Grade 2 pinion and wheel; Z_I, Y_J and Z_E as given in B.1). Click Calculate to reproduce the published results.",
            3 => $"Loaded example: {Examples.Names[index]}. The rating goal is the load for the required safety factors, given as a pinion torque (see 'Load rating' in the report).",
            _ => $"Loaded example: {Examples.Names[index]}. Z_I and Y_J are calculated per AGMA 908-B89 and should match the 908 table values.",
        };
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
            FileName = "AGMA2101-E25_Results.txt"
        };
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            File.WriteAllText(dlg.FileName, resultsBox.Text);
        }
    }
}
