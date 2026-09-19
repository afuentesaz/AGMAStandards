namespace AGMAStandardsHub;

/// <summary>
/// The landing screen: lists every implemented standard as a card. Opening one hides this
/// form and shows its calculator modally; closing the calculator brings this form back.
/// </summary>
public sealed class HubForm : Form
{
    private const int CardHeight = 118;

    public HubForm()
    {
        Text = "Gear Standards Calculator Suite";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(960, 640);
        MinimumSize = new Size(860, 560);
        BackColor = Color.FromArgb(241, 242, 245);
        Font = new Font("Segoe UI", 9f);
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? Icon;

        Controls.Add(BuildContentPanel());
        Controls.Add(BuildHeaderPanel());
    }

    private Control BuildHeaderPanel()
    {
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 92,
            BackColor = Color.White,
        };

        var titleLabel = new Label
        {
            AutoSize = true,
            Text = "Gear Standards Calculator Suite",
            Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            ForeColor = Color.FromArgb(32, 33, 36),
            Location = new Point(28, 16),
        };

        var subtitleLabel = new Label
        {
            AutoSize = true,
            Text = "Select a standard below to open its calculator.",
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(102, 105, 112),
            Location = new Point(30, 52),
        };

        var rule = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 1,
            BackColor = Color.FromArgb(228, 230, 234),
        };

        header.Controls.Add(titleLabel);
        header.Controls.Add(subtitleLabel);
        header.Controls.Add(rule);
        return header;
    }

    private Control BuildContentPanel()
    {
        var scrollHost = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = BackColor,
            Padding = new Padding(28, 20, 28, 20),
        };

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = StandardCatalog.All.Count,
            BackColor = scrollHost.BackColor,
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var row = 0;
        foreach (var entry in StandardCatalog.All)
        {
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, CardHeight + 16));

            var card = new StandardCard(entry)
            {
                Dock = DockStyle.Fill,
                Height = CardHeight,
            };
            card.Activated += (_, _) => OpenStandard(entry);

            table.Controls.Add(card, 0, row);
            row++;
        }

        scrollHost.Controls.Add(table);
        return scrollHost;
    }

    private void OpenStandard(StandardEntry entry)
    {
        Hide();
        try
        {
            using var form = entry.CreateForm();
            form.StartPosition = FormStartPosition.CenterScreen;
            form.ShowDialog(this);
        }
        finally
        {
            Show();
            Activate();
        }
    }
}
