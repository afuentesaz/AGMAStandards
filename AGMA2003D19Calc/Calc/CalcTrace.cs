using System.Linq;
using System.Text;

namespace AGMA2003D19Calc.Calc;

public readonly record struct ReportRow(string Section, string Symbol, string Equation, string Description, double Value, string Unit)
{
    public bool IsSectionHeader => Symbol.Length == 0;
}

/// <summary>
/// Records every intermediate and final calculation as it is produced, in the same
/// "Symbol / Description / Equation / Value / Units" style as the AGMA 2003-D19 Annex E
/// worked example, so the whole audit trail can be dumped to the results text box.
/// </summary>
public class CalcTrace
{
    private readonly List<ReportRow> _rows = new();
    private string _section = "";

    public IReadOnlyList<ReportRow> Rows => _rows;

    public void Section(string title)
    {
        _section = title;
        _rows.Add(new ReportRow(_section, "", "", title, 0, ""));
    }

    /// <summary>Logs a value and returns it unchanged, so calls can be inlined into expressions.</summary>
    public double R(string symbol, string equation, string description, double value, string unit = "")
    {
        _rows.Add(new ReportRow(_section, symbol, equation, description, value, unit));
        return value;
    }

    /// <summary>Logs a plain text note (e.g. iteration convergence info) with no numeric value.</summary>
    public void Note(string text)
    {
        _rows.Add(new ReportRow(_section, "", "", text, double.NaN, ""));
    }

    public string BuildReport(string title)
    {
        int symWidth = Math.Max(16, _rows.Where(r => !r.IsSectionHeader && !double.IsNaN(r.Value)).Select(r => r.Symbol.Length).DefaultIfEmpty(0).Max()) + 1;
        int eqWidth = Math.Max(9, _rows.Where(r => !r.IsSectionHeader && !double.IsNaN(r.Value)).Select(r => r.Equation.Length).DefaultIfEmpty(0).Max()) + 1;

        var sb = new StringBuilder();
        sb.AppendLine(new string('=', 100));
        sb.AppendLine(title);
        sb.AppendLine(new string('=', 100));
        sb.AppendLine();

        string lastSection = null!;
        foreach (var row in _rows)
        {
            if (row.Section != lastSection)
            {
                sb.AppendLine();
                sb.AppendLine(row.Section);
                sb.AppendLine(new string('-', row.Section.Length));
                lastSection = row.Section;
                if (row.IsSectionHeader && row.Description == row.Section)
                    continue;
            }

            if (row.IsSectionHeader)
            {
                sb.AppendLine($"  {row.Description}");
            }
            else if (double.IsNaN(row.Value))
            {
                sb.AppendLine($"  {row.Description}");
            }
            else
            {
                string sym = row.Symbol.PadRight(symWidth);
                string eq = row.Equation.PadRight(eqWidth);
                string val = row.Value.ToString("0.00000").PadLeft(14);
                sb.AppendLine($"  {sym}{eq}{val} {row.Unit,-6} {row.Description}");
            }
        }
        return sb.ToString();
    }
}
