using System.Drawing.Drawing2D;

namespace AGMAStandardsHub;

/// <summary>
/// A clickable card summarizing one implemented standard: code, title, a short scope
/// description, and an "Open" button. Clicking anywhere on the card raises <see cref="Activated"/>.
/// </summary>
internal sealed class StandardCard : Panel
{
    private const int CornerRadius = 10;
    private const int AccentBarWidth = 5;

    private static readonly Color BorderColor = Color.FromArgb(224, 226, 230);
    private static readonly Color HoverBorderColor = Color.FromArgb(190, 194, 200);
    private static readonly Color HoverBackColor = Color.FromArgb(249, 250, 251);

    private readonly StandardEntry _entry;
    private readonly Label _codeLabel;
    private readonly Label _titleLabel;
    private readonly Label _descriptionLabel;
    private readonly Button _openButton;
    private bool _hovered;

    public event EventHandler? Activated;

    public StandardCard(StandardEntry entry)
    {
        _entry = entry;

        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

        BackColor = Color.White;
        Cursor = Cursors.Hand;

        _codeLabel = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = entry.AccentColor,
            Text = entry.Code,
            BackColor = Color.Transparent,
        };

        _titleLabel = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = Color.FromArgb(32, 33, 36),
            Text = entry.Title,
            BackColor = Color.Transparent,
        };

        _descriptionLabel = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(102, 105, 112),
            Text = entry.Description,
            BackColor = Color.Transparent,
        };

        _openButton = new Button
        {
            Text = "Open  →",
            FlatStyle = FlatStyle.Flat,
            BackColor = entry.AccentColor,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Size = new Size(100, 34),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            UseVisualStyleBackColor = false,
        };
        _openButton.FlatAppearance.BorderSize = 0;
        _openButton.FlatAppearance.MouseOverBackColor = ControlPaint.Light(entry.AccentColor, 0.15f);
        _openButton.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(entry.AccentColor, 0.1f);
        _openButton.Click += (_, _) => Activated?.Invoke(this, EventArgs.Empty);

        Controls.Add(_openButton);
        Controls.Add(_descriptionLabel);
        Controls.Add(_titleLabel);
        Controls.Add(_codeLabel);

        foreach (var control in new Control[] { this, _codeLabel, _titleLabel, _descriptionLabel })
        {
            control.Click += (_, _) => Activated?.Invoke(this, EventArgs.Empty);
            control.MouseEnter += (_, _) => SetHovered(true);
            control.MouseLeave += OnPossibleLeave;
        }

        _openButton.MouseEnter += (_, _) => SetHovered(true);
        _openButton.MouseLeave += OnPossibleLeave;

        Padding = new Padding(24, 14, 20, 14);
        Margin = new Padding(0, 0, 0, 16);
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);

        if (_openButton is null)
        {
            // A layout pass can be triggered by base-class/control-collection setup
            // before the constructor has finished building the child controls.
            return;
        }

        var contentLeft = Padding.Left;
        var contentTop = Padding.Top;
        var buttonAreaWidth = _openButton.Width + 16;
        var textWidth = Math.Max(80, ClientSize.Width - contentLeft - Padding.Right - buttonAreaWidth);

        _codeLabel.Location = new Point(contentLeft, contentTop);
        _titleLabel.MaximumSize = new Size(textWidth, 0);
        _titleLabel.Location = new Point(contentLeft, _codeLabel.Bottom + 2);
        _descriptionLabel.MaximumSize = new Size(textWidth, 0);
        _descriptionLabel.Location = new Point(contentLeft, _titleLabel.Bottom + 5);

        _openButton.Location = new Point(
            ClientSize.Width - Padding.Right - _openButton.Width,
            Math.Max(contentTop, (ClientSize.Height - _openButton.Height) / 2));
    }

    private void OnPossibleLeave(object? sender, EventArgs e)
    {
        if (!ClientRectangle.Contains(PointToClient(MousePosition)))
        {
            SetHovered(false);
        }
    }

    private void SetHovered(bool hovered)
    {
        if (_hovered == hovered)
        {
            return;
        }

        _hovered = hovered;
        Invalidate();
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        // Painting is fully handled in OnPaint so the rounded card reads cleanly
        // against the hub's background instead of showing square corners underneath.
        pevent.Graphics.Clear(Parent?.BackColor ?? BackColor);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = RoundedRect(bounds, CornerRadius);

        using (var fill = new SolidBrush(_hovered ? HoverBackColor : BackColor))
        {
            g.FillPath(fill, path);
        }

        using (var border = new Pen(_hovered ? HoverBorderColor : BorderColor, 1f))
        {
            g.DrawPath(border, path);
        }

        using (var clip = new Region(path))
        {
            g.SetClip(clip, CombineMode.Replace);
            using var accentBrush = new SolidBrush(_entry.AccentColor);
            g.FillRectangle(accentBrush, 0, 0, AccentBarWidth, Height);
            g.ResetClip();
        }

        base.OnPaint(e);
    }

    private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var diameter = radius * 2;
        var path = new GraphicsPath();
        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }
}
