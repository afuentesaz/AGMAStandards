using AGMA2101E25Calc.Calc;

namespace AGMA2101E25Calc;

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
            Environment.ExitCode = SelfTest.RunAll();
            return;
        }
        if (args.Length > 1 && args[0] == "--report" && int.TryParse(args[1], out int example))
        {
            // Prints the full report of a built-in example (0 = Annex B) to the console.
            var (trace, warnings) = new AgmaCalculator(Models.Examples.Create(example)).Calculate();
            Console.WriteLine(trace.BuildReport("ANSI/AGMA 2101-E25 CALCULATION RESULTS - " + Models.Examples.Names[example]));
            foreach (var w in warnings) Console.WriteLine("WARNING: " + w);
            return;
        }

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
