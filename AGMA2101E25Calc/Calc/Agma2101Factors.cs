using System;
using System.Collections.Generic;
using AGMA2101E25Calc.Models;

// NOTE: this file is deliberately written in C# 7.3 style (block namespace, no records, no
// target-typed new, no switch expressions) so that it can be ported unchanged into IGDv4.0,
// which targets .NET Framework 4.8 / C# 7.3.
namespace AGMA2101E25Calc.Calc
{
    /// <summary>
    /// Individual rating factors of ANSI/AGMA 2101-E25 as pure functions. Clause, equation, table
    /// and figure numbers refer to the published standard. SI units: mm, N/mm^2, m/s, rpm, um.
    /// </summary>
    public static class Agma2101Factors
    {
        // ================================================================ material classification

        public static bool IsSteel(GearMaterial m)
        {
            return m <= GearMaterial.SteelNitridedNitralloyN;
        }

        public static bool IsNitrided(GearMaterial m)
        {
            return m == GearMaterial.SteelNitridedThroughHardenedAlloy || m == GearMaterial.SteelNitrided2p5Chrome
                || m == GearMaterial.SteelNitridedNitralloy135M || m == GearMaterial.SteelNitridedNitralloyN;
        }

        public static bool IsFlameOrInduction(GearMaterial m)
        {
            return m == GearMaterial.SteelFlameOrInductionHardenedTypeA || m == GearMaterial.SteelFlameOrInductionHardenedTypeB;
        }

        /// <summary>Surface or case hardened steel (carburized, flame/induction or nitrided), as used by clause 13.2.</summary>
        public static bool IsSurfaceHardenedSteel(GearMaterial m)
        {
            return IsSteel(m) && m != GearMaterial.SteelThroughHardened;
        }

        // ================================================================ clause 11: dynamic factor, method B

        /// <summary>Dynamic factor parameter B, Eq 22.</summary>
        public static double DynamicFactorB(int av)
        {
            return 0.25 * Math.Pow(av - 5.0, 0.667);
        }

        /// <summary>Dynamic factor parameter C, Eq 22.</summary>
        public static double DynamicFactorC(double b)
        {
            return 50.0 + 56.0 * (1.0 - b);
        }

        /// <summary>K_V-B, Eq 21.</summary>
        public static double DynamicFactorKvB(double b, double c, double vt)
        {
            return Math.Pow(c / (c + Math.Sqrt(196.85 * vt)), -b);
        }

        /// <summary>Maximum recommended pitch line velocity (end point of the Figure 1 curve), Eq 23, m/s.</summary>
        public static double MaxPitchLineVelocity(double c, int av)
        {
            return Math.Pow(c + (14.0 - av), 2) / 196.85;
        }

        /// <summary>Flank tolerance class number from the single pitch deviation, Eq 24, before rounding up.</summary>
        public static double FlankToleranceClassFromPitchDeviation(double fptMicrons, double referenceDiameter, double normalModule)
        {
            return 5.0 + Math.Log(fptMicrons / (0.001 * referenceDiameter + 0.4 * normalModule + 5.0)) / Math.Log(Math.Sqrt(2.0));
        }

        // ================================================================ clause 12: elastic coefficient

        /// <summary>Modulus of elasticity and Poisson's ratio from Table 2, where the table lists the material.</summary>
        public static bool TryTable2Properties(GearMaterial m, out double e, out double nu)
        {
            if (IsSteel(m)) { e = 2.05e5; nu = 0.3; return true; }
            switch (m)
            {
                case GearMaterial.DuctileIron60_40_18:
                case GearMaterial.DuctileIron80_55_06:
                case GearMaterial.DuctileIron100_70_03:
                case GearMaterial.DuctileIron120_90_02:
                    e = 1.85e5; nu = 0.3; return true;
                case GearMaterial.GrayCastIronClass40:
                    e = 1.25e5; nu = 0.24; return true;
                case GearMaterial.GrayCastIronClass30:
                    e = 1.17e5; nu = 0.19; return true;
            }
            e = 0.0; nu = 0.0;
            return false;
        }

        private static int Table2Family(GearMaterial m)
        {
            if (IsSteel(m)) return 1;
            if (m >= GearMaterial.DuctileIron60_40_18 && m <= GearMaterial.DuctileIron120_90_02) return 2;
            if (m == GearMaterial.GrayCastIronClass40) return 3;
            if (m == GearMaterial.GrayCastIronClass30) return 4;
            return 0;
        }

        /// <summary>Tabulated elastic coefficient for the material pairs listed in Table 2.</summary>
        public static bool TryElasticCoefficientTable2(GearMaterial m1, GearMaterial m2, out double ze)
        {
            int f1 = Table2Family(m1), f2 = Table2Family(m2);
            int lo = Math.Min(f1, f2), hi = Math.Max(f1, f2);
            ze = 0.0;
            if (lo == 1 && hi == 1) ze = 190.0;            // steel - steel
            else if (lo == 1 && hi == 2) ze = 183.0;       // ductile iron - steel
            else if (lo == 2 && hi == 2) ze = 178.0;       // ductile iron - ductile iron
            else if (lo == 3 && hi == 3) ze = 146.0;       // class 40 - class 40
            else if (lo == 4 && hi == 4) ze = 139.0;       // class 30 - class 30
            return ze > 0.0;
        }

        /// <summary>Elastic coefficient, Eq 26, unrounded, (N/mm^2)^0.5.</summary>
        public static double ElasticCoefficientEq26(double e1, double nu1, double e2, double nu2)
        {
            return Math.Sqrt(1.0 / (Math.PI * ((1.0 - nu1 * nu1) / e1 + (1.0 - nu2 * nu2) / e2)));
        }

        public static double RoundToSignificantFigures(double value, int figures)
        {
            if (value == 0.0) return 0.0;
            double scale = Math.Pow(10.0, figures - 1 - (int)Math.Floor(Math.Log10(Math.Abs(value))));
            return Math.Round(value * scale) / scale;
        }

        // ================================================================ clause 13: hardness ratio factor

        /// <summary>Empirical constant A for through hardened pinion and wheel, Eq 28 with its range limits.</summary>
        public static double HardnessRatioConstantA(double hb1, double hb2)
        {
            double ratio = hb1 / hb2;
            if (ratio < 1.2) return 0.0;
            if (ratio > 1.7) return 0.00698;
            return 0.00898 * ratio - 0.00829;
        }

        /// <summary>Z_W for through hardened pinions with through hardened wheels, Eq 27.</summary>
        public static double HardnessRatioFactorThroughHardened(double hb1, double hb2, double u)
        {
            return 1.0 + HardnessRatioConstantA(hb1, hb2) * (u - 1.0);
        }

        /// <summary>Z_W for surface or case hardened pinions (>= 48 HRC) with through hardened wheels, Eq 29.</summary>
        public static double HardnessRatioFactorSurfaceHardenedPinion(double hb2, double ra1Microns)
        {
            if (ra1Microns > 1.6) return 1.0;
            return 1.0 + (450.0 - hb2) * 0.00075 * Math.Exp(-0.448 * ra1Microns);
        }

        // ================================================================ clause 14: load distribution factor, method B

        /// <summary>Lead correction factor K_Hmc.</summary>
        public static double LeadCorrectionFactor(LeadModification lead)
        {
            return lead == LeadModification.CrownedOrLeadCorrected ? 0.8 : 1.0;
        }

        /// <summary>Pinion proportion factor K_Hpf, Eq 32 - 34 (b in mm).</summary>
        public static double PinionProportionFactor(double b, double dw1)
        {
            double ratio = Math.Max(b / (10.0 * dw1), 0.05);   // note to Eq 32-34
            if (b <= 25.0) return ratio - 0.025;                                    // Eq 32
            if (b <= 432.0) return ratio - 0.0375 + 0.000492 * b;                   // Eq 33
            return ratio - 0.1109 + 0.000815 * b - 0.000000353 * b * b;             // Eq 34
        }

        /// <summary>Pinion proportion modifier K_Hpm from the offset ratio S1/S.</summary>
        public static double PinionProportionModifier(double s1, double s)
        {
            return s1 / s < 0.175 ? 1.0 : 1.1;
        }

        /// <summary>Empirical constants of Table 3 for the mesh alignment factor curves.</summary>
        public static void MeshAlignmentConstants(MeshAlignmentCurve curve, out double a, out double b, out double c)
        {
            switch (curve)
            {
                case MeshAlignmentCurve.Curve1OpenGearing: a = 2.47e-1; b = 0.657e-3; c = -1.186e-7; break;
                case MeshAlignmentCurve.Curve2CommercialEnclosed: a = 1.27e-1; b = 0.622e-3; c = -1.69e-7; break;
                case MeshAlignmentCurve.Curve3PrecisionEnclosed: a = 0.675e-1; b = 0.504e-3; c = -1.44e-7; break;
                default: a = 0.380e-1; b = 0.402e-3; c = -1.27e-7; break;
            }
        }

        /// <summary>Mesh alignment factor K_Hma, Eq 35 (b in mm).</summary>
        public static double MeshAlignmentFactor(MeshAlignmentCurve curve, double b)
        {
            double a, bb, c;
            MeshAlignmentConstants(curve, out a, out bb, out c);
            return a + bb * b + c * b * b;
        }

        // ================================================================ clause 15: allowable stress numbers

        /// <summary>
        /// Allowable contact stress number sigma_HP, Table 4 / Figure 7 (steel) and Table 6 (iron, bronze).
        /// hb is the Brinell hardness used by Figure 7; hrc the surface hardness of flame/induction or
        /// carburized steel; hr15n the surface hardness of nitrided steel.
        /// </summary>
        public static double AllowableContactStress(GearMaterial m, MaterialGrade grade, double hb, double hrc, double hr15n,
            bool useUpperValue, List<string> notes, List<string> warnings, string member)
        {
            int g = (int)grade;
            switch (m)
            {
                case GearMaterial.SteelThroughHardened:
                    RequireGrade(g, 2, m, warnings, member);
                    CheckRange(hb, 180.0, 400.0, "Brinell hardness for Figure 7", member, warnings);
                    if (hb > 400.0)
                        warnings.Add(member + ": through hardened gears above 400 HBW may vary widely in endurance strength (clause 15).");
                    notes.Add(g == 1 ? "Figure 7, Grade 1: sigma_HP = 2.22 HB + 200" : "Figure 7, Grade 2: sigma_HP = 2.41 HB + 237");
                    return g == 1 ? 2.22 * hb + 200.0 : 2.41 * hb + 237.0;

                case GearMaterial.SteelFlameOrInductionHardenedTypeA:
                case GearMaterial.SteelFlameOrInductionHardenedTypeB:
                    RequireGrade(g, 2, m, warnings, member);
                    if (hrc < 50.0)
                        warnings.Add(string.Format("{0}: surface hardness {1:0.#} HRC is below the 50 HRC minimum of Table 4 (50 HRC row used).", member, hrc));
                    if (hrc >= 54.0)
                    {
                        notes.Add("Table 4, flame or induction hardened, 54 HRC");
                        return g == 1 ? 1205.0 : 1345.0;
                    }
                    notes.Add("Table 4, flame or induction hardened, 50 HRC");
                    return g == 1 ? 1170.0 : 1310.0;

                case GearMaterial.SteelCarburizedAndHardened:
                    {
                        double minHrc = g == 1 ? 55.0 : 58.0;
                        if (hrc < minHrc)
                            warnings.Add(string.Format("{0}: surface hardness {1:0.#} HRC is below the {2:0} HRC minimum of Table 4 for carburized Grade {3}.", member, hrc, minHrc, g));
                        notes.Add("Table 4, carburized and hardened");
                        return g == 1 ? 1240.0 : g == 2 ? 1550.0 : 1895.0;
                    }

                case GearMaterial.SteelNitridedThroughHardenedAlloy:
                    if (hr15n < 83.5)
                        warnings.Add(string.Format("{0}: surface hardness {1:0.0} HR15N is below the 83.5 HR15N minimum of Table 4 (83.5 HR15N row used).", member, hr15n));
                    if (hr15n >= 84.5)
                    {
                        notes.Add("Table 4, nitrided through hardened alloy steel, 84.5 HR15N");
                        return g == 1 ? 1070.0 : g == 2 ? 1160.0 : 1240.0;
                    }
                    notes.Add("Table 4, nitrided through hardened alloy steel, 83.5 HR15N");
                    return g == 1 ? 1035.0 : g == 2 ? 1125.0 : 1205.0;

                case GearMaterial.SteelNitrided2p5Chrome:
                    if (hr15n < 87.5)
                        warnings.Add(string.Format("{0}: surface hardness {1:0.0} HR15N is below the 87.5 HR15N minimum of Table 4 (87.5 HR15N row used).", member, hr15n));
                    if (hr15n >= 90.0)
                    {
                        notes.Add("Table 4, 2.5% chrome nitrided, 90.0 HR15N");
                        return g == 1 ? 1215.0 : g == 2 ? 1350.0 : 1490.0;
                    }
                    notes.Add("Table 4, 2.5% chrome nitrided, 87.5 HR15N");
                    return g == 1 ? 1070.0 : g == 2 ? 1185.0 : 1305.0;

                case GearMaterial.SteelNitridedNitralloy135M:
                    if (hr15n < 90.0)
                        warnings.Add(string.Format("{0}: surface hardness {1:0.0} HR15N is below the 90.0 HR15N minimum of Table 4.", member, hr15n));
                    notes.Add("Table 4, Nitralloy 135M nitrided, 90.0 HR15N");
                    return g == 1 ? 1170.0 : g == 2 ? 1260.0 : 1345.0;

                case GearMaterial.SteelNitridedNitralloyN:
                    if (hr15n < 90.0)
                        warnings.Add(string.Format("{0}: surface hardness {1:0.0} HR15N is below the 90.0 HR15N minimum of Table 4.", member, hr15n));
                    notes.Add("Table 4, Nitralloy N nitrided, 90.0 HR15N");
                    return g == 1 ? 1185.0 : g == 2 ? 1300.0 : 1415.0;
            }

            double lo, hi;
            IronBronzeContact(m, out lo, out hi);
            notes.Add(string.Format("Table 6, {0} ({1})", m, lo == hi ? "single value" : useUpperValue ? "upper value of range" : "lower value of range"));
            return useUpperValue ? hi : lo;
        }

        /// <summary>
        /// Allowable bending stress number sigma_FP, Table 5 / Figures 8 to 10 (steel) and Table 7 (iron,
        /// bronze). hb is the Brinell hardness (through hardened) or core hardness (nitrided, Figures 9/10).
        /// </summary>
        public static double AllowableBendingStress(GearMaterial m, MaterialGrade grade, double hb, double hrc, bool carburizedBainiteGrade3,
            bool useUpperValue, List<string> notes, List<string> warnings, string member)
        {
            int g = (int)grade;
            switch (m)
            {
                case GearMaterial.SteelThroughHardened:
                    RequireGrade(g, 2, m, warnings, member);
                    CheckRange(hb, 180.0, 400.0, "Brinell hardness for Figure 8", member, warnings);
                    notes.Add(g == 1 ? "Figure 8, Grade 1: sigma_FP = 0.533 HB + 88.3" : "Figure 8, Grade 2: sigma_FP = 0.703 HB + 113");
                    return g == 1 ? 0.533 * hb + 88.3 : 0.703 * hb + 113.0;

                case GearMaterial.SteelFlameOrInductionHardenedTypeA:
                    RequireGrade(g, 2, m, warnings, member);
                    notes.Add("Table 5, flame or induction hardened with type A pattern");
                    return g == 1 ? 310.0 : 380.0;

                case GearMaterial.SteelFlameOrInductionHardenedTypeB:
                    RequireGrade(g, 2, m, warnings, member);
                    notes.Add("Table 5, flame or induction hardened with type B pattern");
                    return 150.0;

                case GearMaterial.SteelCarburizedAndHardened:
                    if (g == 2 && carburizedBainiteGrade3)
                    {
                        notes.Add("Table 5, carburized Grade 2 with bainite / broken needles meeting Grade 3 levels (note 6)");
                        return 485.0;
                    }
                    notes.Add("Table 5, carburized and hardened");
                    return g == 1 ? 380.0 : g == 2 ? 450.0 : 515.0;

                case GearMaterial.SteelNitridedThroughHardenedAlloy:
                    RequireGrade(g, 2, m, warnings, member);
                    CheckRange(hb, 270.0, 340.0, "core hardness for Figure 9", member, warnings);
                    notes.Add(g == 1 ? "Figure 9, Grade 1: sigma_FP = 0.568 HB + 83.8" : "Figure 9, Grade 2: sigma_FP = 0.749 HB + 110");
                    return g == 1 ? 0.568 * hb + 83.8 : 0.749 * hb + 110.0;

                case GearMaterial.SteelNitridedNitralloy135M:
                case GearMaterial.SteelNitridedNitralloyN:
                    RequireGrade(g, 2, m, warnings, member);
                    CheckRange(hb, 270.0, 340.0, "core hardness for Figure 10", member, warnings);
                    notes.Add(g == 1 ? "Figure 10, Grade 1 Nitralloy: sigma_FP = 0.594 HB + 87.76" : "Figure 10, Grade 2 Nitralloy: sigma_FP = 0.7848 HB + 114.81");
                    return g == 1 ? 0.594 * hb + 87.76 : 0.7848 * hb + 114.81;

                case GearMaterial.SteelNitrided2p5Chrome:
                    CheckRange(hb, 300.0, 340.0, "core hardness for Figure 10", member, warnings);
                    notes.Add(string.Format("Figure 10, Grade {0} 2.5% chrome: sigma_FP = 0.7255 HB + {1}", g, g == 1 ? "63.98" : g == 2 ? "153.63" : "201.91"));
                    return 0.7255 * hb + (g == 1 ? 63.98 : g == 2 ? 153.63 : 201.91);
            }

            double lo, hi;
            IronBronzeBending(m, out lo, out hi);
            notes.Add(string.Format("Table 7, {0} ({1})", m, lo == hi ? "single value" : useUpperValue ? "upper value of range" : "lower value of range"));
            return useUpperValue ? hi : lo;
        }

        private static void IronBronzeContact(GearMaterial m, out double lo, out double hi)
        {
            switch (m)
            {
                case GearMaterial.GrayCastIronClass20: lo = 345; hi = 415; return;
                case GearMaterial.GrayCastIronClass30: lo = 450; hi = 520; return;
                case GearMaterial.GrayCastIronClass40: lo = 520; hi = 585; return;
                case GearMaterial.DuctileIron60_40_18: lo = 530; hi = 635; return;
                case GearMaterial.DuctileIron80_55_06: lo = 530; hi = 635; return;
                case GearMaterial.DuctileIron100_70_03: lo = 635; hi = 770; return;
                case GearMaterial.DuctileIron120_90_02: lo = 710; hi = 870; return;
                case GearMaterial.BronzeSandCast: lo = 205; hi = 205; return;
                case GearMaterial.BronzeAlloy954HeatTreated: lo = 450; hi = 450; return;
            }
            throw new ArgumentException("Material " + m + " is not covered by Table 6.");
        }

        private static void IronBronzeBending(GearMaterial m, out double lo, out double hi)
        {
            switch (m)
            {
                case GearMaterial.GrayCastIronClass20: lo = 34.5; hi = 34.5; return;
                case GearMaterial.GrayCastIronClass30: lo = 59; hi = 59; return;
                case GearMaterial.GrayCastIronClass40: lo = 90; hi = 90; return;
                case GearMaterial.DuctileIron60_40_18: lo = 150; hi = 230; return;
                case GearMaterial.DuctileIron80_55_06: lo = 150; hi = 230; return;
                case GearMaterial.DuctileIron100_70_03: lo = 185; hi = 275; return;
                case GearMaterial.DuctileIron120_90_02: lo = 215; hi = 305; return;
                case GearMaterial.BronzeSandCast: lo = 39.5; hi = 39.5; return;
                case GearMaterial.BronzeAlloy954HeatTreated: lo = 165; hi = 165; return;
            }
            throw new ArgumentException("Material " + m + " is not covered by Table 7.");
        }

        private static void RequireGrade(int grade, int maxGrade, GearMaterial m, List<string> warnings, string member)
        {
            if (grade > maxGrade)
                throw new ArgumentException(string.Format("{0}: Grade {1} is not defined in clause 15 for {2} (highest grade: {3}).", member, grade, m, maxGrade));
        }

        private static void CheckRange(double value, double lo, double hi, string what, string member, List<string> warnings)
        {
            if (value < lo || value > hi)
                warnings.Add(string.Format("{0}: {1} = {2:0} HBW is outside the plotted range {3:0}-{4:0} HBW (value extrapolated).", member, what, value, lo, hi));
        }

        // ================================================================ clause 17: stress cycle factors

        /// <summary>Number of stress cycles, Eq 49.</summary>
        public static double StressCycles(double lifeHours, double speedRpm, double contactsPerRevolution)
        {
            return 60.0 * lifeHours * speedRpm * contactsPerRevolution;
        }

        /// <summary>Stress cycle factor for macropitting Z_N, Table 12. Gray/ductile iron and bronze use 1.0 (17.2).</summary>
        public static double StressCycleFactorZN(GearMaterial m, double nL, StressCycleCurve curve, out string basis)
        {
            if (!IsSteel(m)) { basis = "17.2: Z_N = 1.0 for iron and bronze"; return 1.0; }
            bool critical = curve == StressCycleCurve.CriticalService;
            if (IsNitrided(m))
            {
                if (nL < 1e4) { basis = "Table 12, nitrided, n_L < 1e4"; return 1.0999; }
                if (nL < 1e7) { basis = "Table 12, nitrided: 1.249 n_L^-0.0138"; return 1.249 * Math.Pow(nL, -0.0138); }
            }
            else
            {
                if (nL < 1e4) { basis = "Table 12, n_L < 1e4"; return 1.4723; }
                if (nL <= 1e7 || critical) { basis = "Table 12: 2.466 n_L^-0.056"; return 2.466 * Math.Pow(nL, -0.056); }
            }
            if (critical) { basis = "Table 12, critical service: 2.466 n_L^-0.056"; return 2.466 * Math.Pow(nL, -0.056); }
            basis = "Table 12, general application: 1.4488 n_L^-0.023";
            return 1.4488 * Math.Pow(nL, -0.023);
        }

        /// <summary>
        /// Stress cycle factor for bending Y_N, Table 13 / Figure 18. Through hardened steel uses the
        /// 160/250/400 HBW curves, with the logarithmic interpolation described below Figure 18 for
        /// intermediate hardness. Carburized and flame/induction hardened steel use the "case carb"
        /// curve; all nitrided steels the "nitrided" curve. Iron and bronze use 1.0 (17.2).
        /// </summary>
        public static double StressCycleFactorYN(GearMaterial m, double hb, double nL, StressCycleCurve curve, out string basis)
        {
            if (!IsSteel(m)) { basis = "17.2: Y_N = 1.0 for iron and bronze"; return 1.0; }
            if (nL > 3e6)
            {
                if (curve == StressCycleCurve.CriticalService) { basis = "Table 13, critical service: 1.6831 n_L^-0.0323"; return 1.6831 * Math.Pow(nL, -0.0323); }
                basis = "Table 13, general application: 1.3558 n_L^-0.0178";
                return 1.3558 * Math.Pow(nL, -0.0178);
            }

            double coef, exponent, plateau;
            string name;
            if (IsNitrided(m)) { coef = 3.517; exponent = -0.0817; plateau = 2.0002; name = "nitrided"; }
            else if (m == GearMaterial.SteelCarburizedAndHardened || IsFlameOrInduction(m)) { coef = 6.1514; exponent = -0.1192; plateau = 2.7001; name = "case carb"; }
            else
            {
                ThroughHardenedYNCurve(hb, out coef, out exponent, out plateau, out name);
            }

            if (nL < 1e3) { basis = string.Format("Table 13, {0}, n_L < 1e3", name); return plateau; }
            basis = string.Format("Table 13, {0}: {1:0.####} n_L^{2:0.######}", name, coef, exponent);
            return coef * Math.Pow(nL, exponent);
        }

        /// <summary>
        /// Y_N curve for through hardened steel between 1e3 and 3e6 cycles. At 160, 250 and 400 HBW the
        /// Table 13 equations are used directly. For other hardness values the value at 1e3 cycles is
        /// interpolated logarithmically between the neighbouring curves and joined by a straight line on
        /// the log-log plot to Y_N = 1.04 at 3e6 cycles, as described below Figure 18.
        /// </summary>
        private static void ThroughHardenedYNCurve(double hb, out double coef, out double exponent, out double plateau, out string name)
        {
            double[] hbs = { 160.0, 250.0, 400.0 };
            double[] coefs = { 2.3194, 4.9404, 9.4518 };
            double[] exps = { -0.0538, -0.1045, -0.148 };
            double[] plateaus = { 1.5995, 2.3826, 3.4003 };
            for (int i = 0; i < 3; i++)
            {
                if (Math.Abs(hb - hbs[i]) < 1e-9 || (i == 0 && hb < hbs[0]) || (i == 2 && hb > hbs[2]))
                {
                    coef = coefs[i]; exponent = exps[i]; plateau = plateaus[i];
                    name = string.Format("{0:0} HBW", hbs[i]);
                    return;
                }
            }
            int k = hb < 250.0 ? 0 : 1;
            double t = (hb - hbs[k]) / (hbs[k + 1] - hbs[k]);
            plateau = Math.Exp(Math.Log(plateaus[k]) + t * (Math.Log(plateaus[k + 1]) - Math.Log(plateaus[k])));
            exponent = Math.Log(1.04 / plateau) / Math.Log(3e6 / 1e3);
            coef = plateau / Math.Pow(1e3, exponent);
            name = string.Format("{0:0} HBW (log-interpolated between {1:0} and {2:0} HBW)", hb, hbs[k], hbs[k + 1]);
        }

        // ================================================================ Annex D: cycles to failure (inverse of clause 17)
        // The stress cycle curves above solved for the number of cycles, segment by segment with the same coefficients, so
        // that Eq D.6 / D.7 (Z_N) and D.13 / D.14 (Y_N) are the cases of the curves of Annex D.2. A value above the plateau
        // (the static region: fewer than 1e4 cycles for Z_N, 1e3 for Y_N) gives the start of the curve, the conservative end,
        // and is flagged through isStatic. (Written together with IGDv4.0's copy, 2026-10-20.)

        /// <summary>Number of cycles to failure at the stress cycle factor <paramref name="zn"/> for macropitting (Table 12 inverted).</summary>
        public static double CyclesToFailureZN(GearMaterial m, double zn, StressCycleCurve curve, out string basis, out bool isStatic)
        {
            if (!IsSteel(m))
                throw new ArgumentException("Miner's rule needs the stress cycle curves of steel; Z_N = 1.0 for iron and bronze has none (17.2).");
            if (!(zn > 0.0))
                throw new ArgumentException("The stress cycle factor must be positive.");
            isStatic = false;
            bool critical = curve == StressCycleCurve.CriticalService;
            double c2, e2, nKnee, plateau;
            if (IsNitrided(m)) { c2 = 1.249; e2 = -0.0138; nKnee = 1e7; plateau = 1.0999; }
            else { c2 = 2.466; e2 = -0.056; nKnee = critical ? double.PositiveInfinity : 1e7; plateau = 1.4723; }

            double zStart = c2 * Math.Pow(1e4, e2);
            if (zn > zStart)
            {
                isStatic = true;
                basis = string.Format("static region (Z_N above {0:0.####}, the curve at 1e4 cycles; plateau {1:0.####}): N_f = 1e4", zStart, plateau);
                return 1e4;
            }
            double zKnee = double.IsInfinity(nKnee) ? 0.0 : c2 * Math.Pow(nKnee, e2);
            if (zn >= zKnee)
            {
                basis = string.Format("Table 12 inverted: N_f = ({0:0.####} / Z_N)^(1/{1:0.####})", c2, -e2);
                return Math.Pow(c2 / zn, 1.0 / -e2);
            }
            double c3 = critical ? 2.466 : 1.4488, e3 = critical ? -0.056 : -0.023;
            basis = string.Format("Table 12 inverted, {0}: N_f = ({1:0.####} / Z_N)^(1/{2:0.####})", critical ? "critical service" : "general application", c3, -e3);
            return Math.Pow(c3 / zn, 1.0 / -e3);
        }

        /// <summary>Number of cycles to failure at the stress cycle factor <paramref name="yn"/> for bending (Table 13 inverted).</summary>
        public static double CyclesToFailureYN(GearMaterial m, double hb, double yn, StressCycleCurve curve, out string basis, out bool isStatic)
        {
            if (!IsSteel(m))
                throw new ArgumentException("Miner's rule needs the stress cycle curves of steel; Y_N = 1.0 for iron and bronze has none (17.2).");
            if (!(yn > 0.0))
                throw new ArgumentException("The stress cycle factor must be positive.");
            isStatic = false;
            bool critical = curve == StressCycleCurve.CriticalService;
            double c3 = critical ? 1.6831 : 1.3558, e3 = critical ? -0.0323 : -0.0178;
            double yKnee = c3 * Math.Pow(3e6, e3);
            if (yn < yKnee)
            {
                basis = string.Format("Table 13 inverted, {0}: N_f = ({1:0.####} / Y_N)^(1/{2:0.####})", critical ? "critical service" : "general application", c3, -e3);
                return Math.Pow(c3 / yn, 1.0 / -e3);
            }

            double coef, exponent, plateau;
            string name;
            if (IsNitrided(m)) { coef = 3.517; exponent = -0.0817; plateau = 2.0002; name = "nitrided"; }
            else if (m == GearMaterial.SteelCarburizedAndHardened || IsFlameOrInduction(m)) { coef = 6.1514; exponent = -0.1192; plateau = 2.7001; name = "case carb"; }
            else
                ThroughHardenedYNCurve(hb, out coef, out exponent, out plateau, out name);

            double yStart = coef * Math.Pow(1e3, exponent);
            if (yn > yStart)
            {
                isStatic = true;
                basis = string.Format("static region (Y_N above {0:0.####}, the {1} curve at 1e3 cycles): N_f = 1e3", yStart, name);
                return 1e3;
            }
            // The two segments meet at 3e6 cycles only to the 4 decimals of the standard: the knee value of the upper curve
            // decides the segment, and the lower one is kept within its range.
            basis = string.Format("Table 13 inverted, {0}: N_f = ({1:0.####} / Y_N)^(1/{2:0.######})", name, coef, -exponent);
            return Math.Min(Math.Pow(coef / yn, 1.0 / -exponent), 3e6);
        }

        // ================================================================ clause 18: reliability factor

        /// <summary>Reliability factor Y_Z, Table 14.</summary>
        public static double ReliabilityFactor(ReliabilityRequirement r, double userValue)
        {
            switch (r)
            {
                case ReliabilityRequirement.FewerThanOneFailureIn10000: return 1.50;
                case ReliabilityRequirement.FewerThanOneFailureIn1000: return 1.25;
                case ReliabilityRequirement.FewerThanOneFailureIn100: return 1.00;
                case ReliabilityRequirement.FewerThanOneFailureIn10: return 0.85;
                case ReliabilityRequirement.FewerThanOneFailureIn2: return 0.70;
                default: return userValue;
            }
        }

        // ================================================================ Annex A: rim thickness factor

        /// <summary>Rim thickness factor Y_B from the backup ratio m_B, Figure A.1. m_B &lt;= 0 means a solid blank (Y_B = 1).</summary>
        public static double RimThicknessFactor(double mB)
        {
            if (mB <= 0.0 || mB >= 1.2) return 1.0;
            return Math.Max(1.0, 1.6 * Math.Log(2.242 / mB));
        }
    }
}
