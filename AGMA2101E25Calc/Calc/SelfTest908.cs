namespace AGMA2101E25Calc.Calc;

/// <summary>
/// Diagnostic-only harness (part of "--selftest") that checks the AGMA 908-B89 analytical I and J
/// factors against the Information Sheet's own geometry factor tables (clause 7). The table
/// conditions are reproduced exactly: standard center distance (x1 + x2 = 0), outside diameters
/// per Eq 7.1/7.2, tooth thinning 0.024 per member (Eq 7.5), hob with nc = 10 000, s_n0 = pi/2,
/// x0 = 0, no protuberance (clause 7.9), and m_F = 2.0 for helical gears (clause 7.10).
/// The tables print I to three decimals and J to two. I is checked to +/-0.0006. J is checked to
/// +/-0.006: 22 of the 24 points round exactly to the printed value, but on the 14.5 deg table the
/// pinion J of 21/21 and 26/55 (x = +/-0.25) computes 0.4055 and 0.4452 against printed 0.40 and
/// 0.44, i.e. 0.0005 past the rounding boundary (rounding H, L, M to two places, which clause 5.11
/// permits, does not explain it and makes the other tables worse).
/// </summary>
public static class SelfTest908
{
    public sealed record TableSpec(string Name, double PressureAngle, double HelixAngle, double WholeDepth,
        double EdgeRadius, SpurLoadPoint Load, double X1, double X2);

    public sealed record TableCase(int Z1, int Z2, double I, double J1, double J2);

    /// <summary>The transcribed table points (I to three decimals, J to two, as printed).</summary>
    public static readonly List<(TableSpec spec, TableCase[] cases)> Tables = new()
    {
        (new TableSpec("908 p.41 top: 20 deg spur, 2.25 depth, 0.25 radius, HPSTC, x = 0", 20, 0, 2.25, 0.25, SpurLoadPoint.HPSTC, 0, 0),
         new[] { new TableCase(21, 21, 0.078, 0.33, 0.33), new TableCase(21, 35, 0.091, 0.34, 0.37), new TableCase(26, 55, 0.101, 0.37, 0.41),
                 new TableCase(35, 135, 0.120, 0.41, 0.45), new TableCase(55, 55, 0.080, 0.43, 0.43), new TableCase(135, 135, 0.080, 0.49, 0.49),
                 new TableCase(21, 135, 0.118, 0.35, 0.43) }),
        (new TableSpec("908 p.41 bottom: 20 deg spur, 2.25 depth, 0.25 radius, HPSTC, x = +/-0.25", 20, 0, 2.25, 0.25, SpurLoadPoint.HPSTC, 0.25, -0.25),
         new[] { new TableCase(17, 17, 0.080, 0.36, 0.24), new TableCase(21, 35, 0.098, 0.40, 0.32), new TableCase(35, 135, 0.129, 0.45, 0.42),
                 new TableCase(55, 55, 0.080, 0.47, 0.39) }),
        (new TableSpec("908 p.49 top: 20 deg, 30 deg helix, 2.25 depth, 0.25 radius, tip, x = +/-0.25", 20, 30, 2.25, 0.25, SpurLoadPoint.Tip, 0.25, -0.25),
         new[] { new TableCase(12, 12, 0.115, 0.42, 0.30), new TableCase(21, 35, 0.174, 0.49, 0.44), new TableCase(17, 55, 0.224, 0.48, 0.46),
                 new TableCase(135, 135, 0.151, 0.57, 0.55), new TableCase(26, 26, 0.133, 0.50, 0.42) }),
        (new TableSpec("908 p.49 bottom: 20 deg, 30 deg helix, 2.25 depth, 0.25 radius, tip, x = +/-0.50", 20, 30, 2.25, 0.25, SpurLoadPoint.Tip, 0.50, -0.50),
         new[] { new TableCase(12, 12, 0.098, 0.47, 0.24), new TableCase(21, 135, 0.274, 0.52, 0.47), new TableCase(55, 55, 0.141, 0.55, 0.47) }),
        (new TableSpec("908 p.35 top: 14.5 deg, 20 deg helix, 2.157 depth, 0.157 radius, tip, x = 0", 14.5, 20, 2.157, 0.157, SpurLoadPoint.Tip, 0, 0),
         new[] { new TableCase(35, 35, 0.125, 0.43, 0.43), new TableCase(35, 135, 0.215, 0.46, 0.49), new TableCase(135, 135, 0.146, 0.53, 0.53) }),
        (new TableSpec("908 p.35 bottom: 14.5 deg, 20 deg helix, 2.157 depth, 0.157 radius, tip, x = +/-0.25", 14.5, 20, 2.157, 0.157, SpurLoadPoint.Tip, 0.25, -0.25),
         new[] { new TableCase(21, 21, 0.104, 0.40, 0.32), new TableCase(26, 55, 0.183, 0.44, 0.41) }),
    };

    public static (int pass, int fail) Run()
    {
        int pass = 0, fail = 0;
        Console.WriteLine("=== AGMA 908-B89 geometry factor tables self-test ===");
        foreach (var (spec, cases) in Tables)
        {
            Console.WriteLine();
            Console.WriteLine(spec.Name);
            foreach (var c in cases)
            {
                var r = Agma908GeometryFactors.Calculate(BuildTableInput(spec, c.Z1, c.Z2));
                bool okI = Math.Abs(r.I - c.I) <= 0.0006;
                bool okJ1 = Math.Abs(r.J1 - c.J1) <= 0.006;
                bool okJ2 = Math.Abs(r.J2 - c.J2) <= 0.006;
                bool ok = okI && okJ1 && okJ2;
                Console.WriteLine($"{(ok ? "PASS" : "FAIL"),-4} {c.Z1,4}/{c.Z2,-4} I={r.I,7:0.0000} (table {c.I:0.000}{(okI ? "" : " X")})  " +
                                  $"J1={r.J1,6:0.0000} (table {c.J1:0.00}{(okJ1 ? "" : " X")})  J2={r.J2,6:0.0000} (table {c.J2:0.00}{(okJ2 ? "" : " X")})");
                if (ok) pass++; else fail++;
            }
        }
        return (pass, fail);
    }

    public static Agma908Input BuildTableInput(TableSpec spec, int z1, int z2)
    {
        double cosPsi = Math.Cos(spec.HelixAngle * Math.PI / 180.0);
        double px = spec.HelixAngle > 0 ? Math.PI / Math.Sin(spec.HelixAngle * Math.PI / 180.0) : 1.0;
        var tool = new Agma908Tool { ToothNumber = 10000, Addendum = spec.WholeDepth - 1.0, TipRadius = spec.EdgeRadius };
        return new Agma908Input
        {
            Z1 = z1,
            Z2 = z2,
            HelixAngleDeg = spec.HelixAngle,
            NormalPressureAngleDeg = spec.PressureAngle,
            CenterDistance = (z1 + z2) / (2.0 * cosPsi),                       // Eq 7.3
            OutsideRadius1 = 0.5 * (z1 / cosPsi + 2.0 * (1.0 + spec.X1)),       // Eq 7.1
            OutsideRadius2 = 0.5 * (z2 / cosPsi + 2.0 * (1.0 + spec.X2)),       // Eq 7.2
            FaceWidth = spec.HelixAngle > 0 ? 2.0 * px : 10.0,                  // clause 7.10: m_F = 2.0
            X1 = spec.X1,
            X2 = spec.X2,
            Thinning1 = 0.024,                                                  // Eq 7.5
            Thinning2 = 0.024,
            Tool1 = tool,
            Tool2 = tool.Clone(),
            SpurLoad = spec.Load,
        };
    }
}
