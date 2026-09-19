using ISO10300Calc.Calc;

namespace ISO10300Calc;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--selftest")
        {
            SelfTest.RunSample1Check();
            SelfTest.RunSample2Check();
            SelfTest.RunSample3Check();
            return;
        }

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
