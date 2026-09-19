namespace ISO23509Calc;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        splitContainerMain = new SplitContainer();
        propertyGrid1 = new PropertyGrid();
        resultsBox = new TextBox();
        buttonPanel = new FlowLayoutPanel();
        calculateButton = new Button();
        exampleComboBox = new ComboBox();
        loadExampleButton = new Button();
        saveReportButton = new Button();
        statusLabel = new Label();
        ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
        splitContainerMain.Panel1.SuspendLayout();
        splitContainerMain.Panel2.SuspendLayout();
        splitContainerMain.SuspendLayout();
        buttonPanel.SuspendLayout();
        SuspendLayout();
        //
        // splitContainerMain
        //
        splitContainerMain.Dock = DockStyle.Fill;
        splitContainerMain.Location = new Point(0, 40);
        splitContainerMain.Name = "splitContainerMain";
        //
        // splitContainerMain.Panel1
        //
        splitContainerMain.Panel1.Controls.Add(propertyGrid1);
        //
        // splitContainerMain.Panel2
        //
        splitContainerMain.Panel2.Controls.Add(resultsBox);
        splitContainerMain.Size = new Size(1500, 860);
        splitContainerMain.SplitterDistance = 560;
        splitContainerMain.TabIndex = 2;
        //
        // propertyGrid1
        //
        propertyGrid1.Dock = DockStyle.Fill;
        propertyGrid1.Location = new Point(0, 0);
        propertyGrid1.Name = "propertyGrid1";
        propertyGrid1.PropertySort = PropertySort.Categorized;
        propertyGrid1.Size = new Size(560, 860);
        propertyGrid1.TabIndex = 0;
        propertyGrid1.ToolbarVisible = false;
        //
        // resultsBox
        //
        resultsBox.Dock = DockStyle.Fill;
        resultsBox.Font = new Font("Consolas", 9.5F);
        resultsBox.Location = new Point(0, 0);
        resultsBox.Multiline = true;
        resultsBox.Name = "resultsBox";
        resultsBox.ReadOnly = true;
        resultsBox.ScrollBars = ScrollBars.Both;
        resultsBox.Size = new Size(936, 860);
        resultsBox.TabIndex = 0;
        resultsBox.WordWrap = false;
        //
        // buttonPanel
        //
        buttonPanel.Controls.Add(calculateButton);
        buttonPanel.Controls.Add(exampleComboBox);
        buttonPanel.Controls.Add(loadExampleButton);
        buttonPanel.Controls.Add(saveReportButton);
        buttonPanel.Dock = DockStyle.Top;
        buttonPanel.Location = new Point(0, 0);
        buttonPanel.Name = "buttonPanel";
        buttonPanel.Padding = new Padding(4);
        buttonPanel.Size = new Size(1500, 40);
        buttonPanel.TabIndex = 0;
        //
        // calculateButton
        //
        calculateButton.Location = new Point(7, 7);
        calculateButton.Name = "calculateButton";
        calculateButton.Size = new Size(110, 23);
        calculateButton.TabIndex = 0;
        calculateButton.Text = "Calculate";
        calculateButton.UseVisualStyleBackColor = true;
        calculateButton.Click += CalculateButton_Click;
        //
        // exampleComboBox
        //
        exampleComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        exampleComboBox.Items.AddRange(new object[] { "Annex F.2 (spiral bevel, Method 0, face milling)", "Annex F.3 (hypoid, Method 1/Gleason, face milling)", "Annex F.4 (hypoid, Method 2/Oerlikon, face hobbing)", "Annex F.5 (hypoid, Method 3/Klingelnberg, face hobbing)" });
        exampleComboBox.Location = new Point(123, 7);
        exampleComboBox.Name = "exampleComboBox";
        exampleComboBox.Size = new Size(440, 23);
        exampleComboBox.TabIndex = 1;
        //
        // loadExampleButton
        //
        loadExampleButton.Location = new Point(569, 7);
        loadExampleButton.Name = "loadExampleButton";
        loadExampleButton.Size = new Size(110, 23);
        loadExampleButton.TabIndex = 2;
        loadExampleButton.Text = "Load Example";
        loadExampleButton.UseVisualStyleBackColor = true;
        loadExampleButton.Click += LoadExampleButton_Click;
        //
        // saveReportButton
        //
        saveReportButton.Location = new Point(685, 7);
        saveReportButton.Name = "saveReportButton";
        saveReportButton.Size = new Size(110, 23);
        saveReportButton.TabIndex = 3;
        saveReportButton.Text = "Save Report...";
        saveReportButton.UseVisualStyleBackColor = true;
        saveReportButton.Click += SaveReportButton_Click;
        //
        // statusLabel
        //
        statusLabel.Dock = DockStyle.Bottom;
        statusLabel.ForeColor = Color.DarkRed;
        statusLabel.Location = new Point(0, 900);
        statusLabel.Name = "statusLabel";
        statusLabel.Padding = new Padding(6, 4, 6, 4);
        statusLabel.Size = new Size(1500, 26);
        statusLabel.TabIndex = 1;
        //
        // MainForm
        //
        ClientSize = new Size(1500, 926);
        // Dock=Fill control must be added before the Dock=Top/Bottom controls, otherwise the
        // top/bottom-docked controls' claimed edge space can visually overlap the fill control
        // instead of being excluded from it.
        Controls.Add(splitContainerMain);
        Controls.Add(buttonPanel);
        Controls.Add(statusLabel);
        MinimumSize = new Size(1100, 650);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "ISO 23509:2016 — Bevel and Hypoid Gear Geometry Calculator";
        splitContainerMain.Panel1.ResumeLayout(false);
        splitContainerMain.Panel2.ResumeLayout(false);
        splitContainerMain.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
        splitContainerMain.ResumeLayout(false);
        buttonPanel.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.SplitContainer splitContainerMain;
    private System.Windows.Forms.PropertyGrid propertyGrid1;
    private System.Windows.Forms.TextBox resultsBox;
    private System.Windows.Forms.FlowLayoutPanel buttonPanel;
    private System.Windows.Forms.Button calculateButton;
    private System.Windows.Forms.ComboBox exampleComboBox;
    private System.Windows.Forms.Button loadExampleButton;
    private System.Windows.Forms.Button saveReportButton;
    private System.Windows.Forms.Label statusLabel;
}
