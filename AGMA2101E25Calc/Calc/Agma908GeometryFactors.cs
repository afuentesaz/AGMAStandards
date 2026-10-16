using System;
using System.Collections.Generic;

// NOTE: this file is deliberately written in C# 7.3 style (block namespace, no records, no
// target-typed new, no nullable annotations) so that it can be ported unchanged into IGDv4.0,
// which targets .NET Framework 4.8 / C# 7.3.
namespace AGMA2101E25Calc.Calc
{
    /// <summary>Load application point used for the bending geometry factor J of spur gears (908 clause 5.2).</summary>
    public enum SpurLoadPoint
    {
        /// <summary>Highest point of single tooth contact (load sharing exists, Eq 5.11).</summary>
        HPSTC,
        /// <summary>Tip of the tooth (no load sharing, Eq 5.10; see Table 5-1).</summary>
        Tip
    }

    /// <summary>
    /// Generating tool data for one gear, AGMA 908-B89 clause 5.5. All lengths are dimensionless
    /// (divided by the normal module). A rack-type tool (hob, rack cutter, generating grinding
    /// wheel) is modelled as a shaper cutter with a very large number of teeth (nc = 10 000).
    /// </summary>
    public class Agma908Tool
    {
        /// <summary>Tool tooth number nc (10 000 for a rack-type tool).</summary>
        public double ToothNumber = 10000.0;
        /// <summary>Nominal tool addendum ha0 (addendum where the tool tooth thickness equals pi/2).</summary>
        public double Addendum = 1.25;
        /// <summary>Tool tip (edge) radius rho_a0.</summary>
        public double TipRadius = 0.25;
        /// <summary>Amount of protuberance delta_0.</summary>
        public double Protuberance = 0.0;
        /// <summary>Stock allowance per side u_s for a pre-grind tool (Eq 5.37/5.38). 0 = finishing tool.</summary>
        public double StockAllowance = 0.0;
        /// <summary>Addendum modification coefficient of the tool x0 (Eq 5.35); 0 for a hob or a mid-life cutter.</summary>
        public double AddendumModification = 0.0;

        public Agma908Tool Clone() { return (Agma908Tool)MemberwiseClone(); }
    }

    /// <summary>
    /// Input to the AGMA 908-B89 geometry factor calculation. As in the Information Sheet, every
    /// length is given for unity normal module (mn = 1.0).
    /// </summary>
    public class Agma908Input
    {
        public int Z1;                       // pinion tooth number n1
        public int Z2;                       // gear tooth number n2 (external gears only)
        public double HelixAngleDeg;         // standard helix angle psi
        public double NormalPressureAngleDeg;// standard normal pressure angle phi_n
        public double CenterDistance;        // operating center distance Cr
        public double OutsideRadius1;        // R01
        public double OutsideRadius2;        // R02
        public double FaceWidth;             // effective face width F
        public double X1;                    // addendum modification coefficient at zero backlash, pinion
        public double X2;                    // addendum modification coefficient at zero backlash, gear
        public double Thinning1;             // tooth thinning for backlash Delta s_n1
        public double Thinning2;             // tooth thinning for backlash Delta s_n2
        public Agma908Tool Tool1 = new Agma908Tool();
        public Agma908Tool Tool2 = new Agma908Tool();
        public SpurLoadPoint SpurLoad = SpurLoadPoint.HPSTC;
        public bool FullButtressing1;        // clause 5.10: Ch may be increased 10 % (conventional helicals only)
        public bool FullButtressing2;
    }

    /// <summary>Intermediate and final values of the J factor calculation for one member.</summary>
    public class Agma908BendingResult
    {
        public double VirtualTeeth;          // n            Eq 5.2
        public double VirtualPitchRadius;    // r_n          Eq 5.3
        public double VirtualBaseRadius;     // r_nb         Eq 5.4
        public double VirtualOutsideRadius;  // r_na         Eq 5.5
        public double TanPhiNW;              // tan phi_nW   Eq 5.10 / 5.11 / 5.18
        public double GeneratingRackShift;   // x_g          Eq 5.19
        public double ToothThickness;        // s_n          Eq 5.21
        public double LoadAngle;             // phi_nL       Eq 5.27 (rad)
        public double LoadRadius;            // r_nL         Eq 5.28
        public double ToolVirtualTeeth;      // n0           Eq 5.29
        public double ToolCenterRadius;      // r^s_n0       Eq 5.39
        public double HalfAngleToToolCenter; // lambda_ns/2  Eq 5.47 (rad)
        public double GeneratingPressureAngle; // phi''_n    Eq 5.48-5.50 (rad)
        public double GeneratingPitchRadius; // r''_n        Eq 5.51
        public double ToolGeneratingPitchRadius; // r''_n0   Eq 5.52
        public double AlphaN;                // alpha_n at the critical point (rad)
        public int Iterations;
        public bool UsedBisection;           // true when the Newton iteration of Eq 5.65 failed and the bracketing fallback was used
        public double CriticalThickness;     // s_F          Eq 5.73
        public double ParabolaHeight;        // h_F          Eq 5.62
        public double FilletRadius;          // rho_F        Eq 5.68
        public double HelicalFactor;         // C_h          Eq 5.69 / 5.70
        public double StressCorrection;      // K_f          Eq 5.72
        public double HelixAngleFactor;      // K_psi        Eq 5.77
        public double ToothForm;             // Y            Eq 5.78
        public double J;                     // J            Eq 5.1
        public double MinGeneratingRackShift;// x_g min      Eq 7.8 (undercut criterion)
        public string Case = "";             // description of the case used (spur HPSTC, tip, LACR, ...)
    }

    /// <summary>Results of the AGMA 908-B89 geometry factor calculation for a gear pair.</summary>
    public class Agma908Result
    {
        public double GearRatio;             // mG           Eq 3.1
        public double PitchRadius1;          // R1           Eq 3.2
        public double PitchRadius2;          // R2           Eq 3.3
        public double TransversePressureAngle; // phi        Eq 3.4 (rad)
        public double BaseRadius1;           // Rb1          Eq 3.5
        public double BaseRadius2;           // Rb2          Eq 3.6
        public double OperatingPressureAngle;// phi_r        Eq 3.7 (rad)
        public double BasePitch;             // p_b          Eq 3.8
        public double NormalBasePitch;       // p_N          Eq 3.9
        public double BaseHelixAngle;        // psi_b        Eq 3.10 (rad)
        public double C1, C2, C3, C4, C5, C6;// distances along the line of action, Eq 3.11-3.16
        public double ActiveLength;          // Z            Eq 3.17
        public double TransverseContactRatio;// m_p          Eq 3.18
        public double AxialPitch;            // p_x          Eq 3.19
        public double AxialContactRatio;     // m_F          Eq 3.20
        public double MinContactLength;      // L_min        Eq 3.21-3.23
        public double LoadSharingRatio;      // m_N          Eq 3.24-3.26
        public double OperatingHelixAngle;   // psi_r        Eq 3.27 (rad)
        public double OperatingNormalPressureAngle; // phi_nr Eq 3.28 (rad)
        public double OperatingPitchDiameter;// d            Eq 4.2
        public double Rho1, Rho2;            // rho_1, rho_2 Eq 4.4-4.7
        public double HelicalOverlapFactor;  // C_psi        Eq 4.8 / 4.11
        public double I;                     // I            Eq 4.1
        public bool IsSpur;
        public bool IsLACR;                  // helical with m_F <= 1.0
        public Agma908BendingResult Bending1 = new Agma908BendingResult();
        public Agma908BendingResult Bending2 = new Agma908BendingResult();
        /// <summary>For LACR helicals: J computed as a conventional helical with F slightly above p_x (clause 5 note).</summary>
        public Agma908BendingResult Bending1Conventional = new Agma908BendingResult();
        public Agma908BendingResult Bending2Conventional = new Agma908BendingResult();
        public double J1, J2;
        public List<string> Warnings = new List<string>();
    }

    /// <summary>
    /// AGMA 908-B89 analytical method for the pitting resistance geometry factor I (clause 4) and
    /// the bending strength geometry factor J (clause 5) of external spur and helical gears
    /// generated by rack-type tools or shaper cutters. Equation numbers refer to AGMA 908-B89.
    /// </summary>
    public static class Agma908GeometryFactors
    {
        private const double ConvergenceTolerance = 1e-12;
        private const int MaxIterations = 100;

        private static double Inv(double phi) { return Math.Tan(phi) - phi; }

        /// <summary>Arc involute by the iteration of Eq 5.49 / 5.50.</summary>
        public static double ArcInv(double inv)
        {
            if (inv <= 0.0) return 0.0;
            double phi = Math.Pow(3.0 * inv, 1.0 / 3.0);     // Eq 5.49 (0.33 exponent in the text)
            for (int i = 0; i < 100; i++)
            {
                double t = Math.Tan(phi);
                double next = phi + (inv + phi - t) / (t * t); // Eq 5.50
                if (Math.Abs(next - phi) < 1e-15) { phi = next; break; }
                phi = next;
            }
            return phi;
        }

        public static Agma908Result Calculate(Agma908Input input)
        {
            var r = new Agma908Result();
            double psi = input.HelixAngleDeg * Math.PI / 180.0;
            double phiN = input.NormalPressureAngleDeg * Math.PI / 180.0;
            r.IsSpur = Math.Abs(input.HelixAngleDeg) < 1e-9;
            psi = Math.Abs(psi);

            // ------------------------------------------------------------ Clause 3: basic geometry
            double n1 = input.Z1, n2 = input.Z2;
            double mG = r.GearRatio = n2 / n1;                                     // 3.1
            double R1 = r.PitchRadius1 = n1 / (2.0 * Math.Cos(psi));                // 3.2
            double R2 = r.PitchRadius2 = R1 * mG;                                   // 3.3
            double phi = r.TransversePressureAngle = Math.Atan(Math.Tan(phiN) / Math.Cos(psi)); // 3.4
            double Rb1 = r.BaseRadius1 = R1 * Math.Cos(phi);                        // 3.5
            double Rb2 = r.BaseRadius2 = Rb1 * mG;                                  // 3.6
            double Cr = input.CenterDistance;
            double phiR = r.OperatingPressureAngle = Math.Acos((Rb2 + Rb1) / Cr);   // 3.7
            double pb = r.BasePitch = 2.0 * Math.PI * Rb1 / n1;                     // 3.8
            double pN = r.NormalBasePitch = Math.PI * Math.Cos(phiN);               // 3.9
            double psiB = r.BaseHelixAngle = r.IsSpur ? 0.0 : Math.Acos(Math.Min(1.0, pN / pb)); // 3.10

            double R01 = input.OutsideRadius1, R02 = input.OutsideRadius2;
            double C6 = r.C6 = Cr * Math.Sin(phiR);                                 // 3.11
            double C1 = r.C1 = C6 - Math.Sqrt(R02 * R02 - Rb2 * Rb2);               // 3.12
            r.C3 = C6 / (mG + 1.0);                                                 // 3.13
            double C4 = r.C4 = C1 + pb;                                             // 3.14
            double C5 = r.C5 = Math.Sqrt(R01 * R01 - Rb1 * Rb1);                    // 3.15
            double C2 = r.C2 = C5 - pb;                                             // 3.16
            double Z = r.ActiveLength = C5 - C1;                                    // 3.17

            if (C1 <= 0.0)
                r.Warnings.Add("C1 <= 0: the gear tip reaches beyond the pinion's interference point (tip-to-fillet interference on the pinion).");
            if (C5 >= C6)
                r.Warnings.Add("C5 >= C6: the pinion tip reaches beyond the gear's interference point (tip-to-fillet interference on the gear).");

            // ------------------------------------------------------------ 3.1 contact ratios
            double mp = r.TransverseContactRatio = Z / pb;                          // 3.18
            double F = input.FaceWidth;
            if (r.IsSpur)
            {
                r.AxialPitch = double.PositiveInfinity;
                r.AxialContactRatio = 0.0;
            }
            else
            {
                r.AxialPitch = Math.PI / Math.Sin(psi);                             // 3.19
                r.AxialContactRatio = F / r.AxialPitch;                             // 3.20
            }
            double mF = r.AxialContactRatio;
            r.IsLACR = !r.IsSpur && mF <= 1.0;

            // ------------------------------------------------------------ 3.2-3.5
            r.MinContactLength = MinContactLength(mp, mF, F, r.AxialPitch, psiB, r.IsSpur);
            r.LoadSharingRatio = (r.IsSpur || r.IsLACR) ? 1.0 : F / r.MinContactLength; // 3.24-3.26
            r.OperatingHelixAngle = Math.Atan(Math.Tan(psiB) / Math.Cos(phiR));     // 3.27
            r.OperatingNormalPressureAngle = Math.Asin(Math.Cos(psiB) * Math.Sin(phiR)); // 3.28

            // ------------------------------------------------------------ Clause 4: I factor
            double d = r.OperatingPitchDiameter = 2.0 * Cr / (mG + 1.0);            // 4.2
            double Rm1 = 0.5 * (R01 + (Cr - R02));                                  // 4.3
            double rhoM1 = Math.Sqrt(Rm1 * Rm1 - Rb1 * Rb1);                        // 4.9
            double rhoM2 = C6 - rhoM1;                                              // 4.10
            if (r.IsSpur || r.IsLACR)
            {
                r.Rho1 = C2;                                                        // 4.6
                r.Rho2 = C6 - r.Rho1;                                               // 4.7
            }
            else
            {
                r.Rho1 = rhoM1;                                                     // 4.4
                r.Rho2 = C6 - r.Rho1;                                               // 4.5
            }
            if (r.IsLACR)
                r.HelicalOverlapFactor = Math.Sqrt(1.0 - mF * (1.0 - rhoM1 * rhoM2 * Z / (r.Rho1 * r.Rho2 * pN))); // 4.8
            else
                r.HelicalOverlapFactor = 1.0;                                       // 4.11
            double Cpsi = r.HelicalOverlapFactor;
            r.I = Math.Cos(phiR) * Cpsi * Cpsi / ((1.0 / r.Rho1 + 1.0 / r.Rho2) * d * r.LoadSharingRatio); // 4.1

            // ------------------------------------------------------------ Clause 5: J factor, both members
            var pinion = new Member { Teeth = n1, MateTeeth = n2, PitchRadius = R1, MatePitchRadius = R2, BaseRadius = Rb1, MateBaseRadius = Rb2,
                                      OutsideRadius = R01, MateOutsideRadius = R02, X = input.X1, Thinning = input.Thinning1, Tool = input.Tool1,
                                      FullButtressing = input.FullButtressing1 };
            var gear = new Member { Teeth = n2, MateTeeth = n1, PitchRadius = R2, MatePitchRadius = R1, BaseRadius = Rb2, MateBaseRadius = Rb1,
                                    OutsideRadius = R02, MateOutsideRadius = R01, X = input.X2, Thinning = input.Thinning2, Tool = input.Tool2,
                                    FullButtressing = input.FullButtressing2 };

            if (r.IsLACR)
            {
                // Clause 5 note: compute both as LACR and as a conventional helical with F slightly
                // greater than p_x, and use the more conservative (smaller) value.
                r.Bending1 = BendingFactor(pinion, r, input, psi, phiN, Cr, pb, pN, mF, Cpsi, 1.0, BendingCase.LACR);
                r.Bending2 = BendingFactor(gear, r, input, psi, phiN, Cr, pb, pN, mF, Cpsi, 1.0, BendingCase.LACR);
                double Fconv = 1.001 * r.AxialPitch;
                double mFconv = Fconv / r.AxialPitch;
                double mNconv = Fconv / MinContactLength(mp, mFconv, Fconv, r.AxialPitch, psiB, false);
                r.Bending1Conventional = BendingFactor(pinion, r, input, psi, phiN, Cr, pb, pN, mFconv, 1.0, mNconv, BendingCase.ConventionalHelical);
                r.Bending2Conventional = BendingFactor(gear, r, input, psi, phiN, Cr, pb, pN, mFconv, 1.0, mNconv, BendingCase.ConventionalHelical);
                r.J1 = Math.Min(r.Bending1.J, r.Bending1Conventional.J);
                r.J2 = Math.Min(r.Bending2.J, r.Bending2Conventional.J);
            }
            else
            {
                BendingCase bc = r.IsSpur
                    ? (input.SpurLoad == SpurLoadPoint.HPSTC ? BendingCase.SpurHPSTC : BendingCase.SpurTip)
                    : BendingCase.ConventionalHelical;
                r.Bending1 = BendingFactor(pinion, r, input, psi, phiN, Cr, pb, pN, mF, Cpsi, r.LoadSharingRatio, bc);
                r.Bending2 = BendingFactor(gear, r, input, psi, phiN, Cr, pb, pN, mF, Cpsi, r.LoadSharingRatio, bc);
                r.J1 = r.Bending1.J;
                r.J2 = r.Bending2.J;
            }

            if (r.Bending1.GeneratingRackShift < r.Bending1.MinGeneratingRackShift)
                r.Warnings.Add(string.Format("Pinion may be undercut: x_g = {0:0.0000} < x_g min = {1:0.0000} (AGMA 908 Eq 7.8).", r.Bending1.GeneratingRackShift, r.Bending1.MinGeneratingRackShift));
            if (r.Bending2.GeneratingRackShift < r.Bending2.MinGeneratingRackShift)
                r.Warnings.Add(string.Format("Gear may be undercut: x_g = {0:0.0000} < x_g min = {1:0.0000} (AGMA 908 Eq 7.8).", r.Bending2.GeneratingRackShift, r.Bending2.MinGeneratingRackShift));
            return r;
        }

        /// <summary>Minimum length of the lines of contact, Eq 3.21 - 3.23.</summary>
        private static double MinContactLength(double mp, double mF, double F, double px, double psiB, bool isSpur)
        {
            if (isSpur) return F;                                                   // 3.21
            double nr = mp - Math.Floor(mp);
            double na = mF - Math.Floor(mF);
            if (na <= 1.0 - nr)
                return (mp * F - na * nr * px) / Math.Cos(psiB);                    // 3.22
            return (mp * F - (1.0 - na) * (1.0 - nr) * px) / Math.Cos(psiB);       // 3.23
        }

        private enum BendingCase { SpurHPSTC, SpurTip, LACR, ConventionalHelical }

        /// <summary>One member of the pair seen as "the pinion" of clause 5 (the clause is written for the
        /// pinion and repeated for the gear with the roles exchanged).</summary>
        private class Member
        {
            public double Teeth, MateTeeth, PitchRadius, MatePitchRadius, BaseRadius, MateBaseRadius;
            public double OutsideRadius, MateOutsideRadius, X, Thinning;
            public Agma908Tool Tool = new Agma908Tool();
            public bool FullButtressing;
        }

        private static Agma908BendingResult BendingFactor(Member m, Agma908Result pair, Agma908Input input, double psi, double phiN,
            double Cr, double pb, double pN, double mF, double Cpsi, double mN, BendingCase bc)
        {
            var b = new Agma908BendingResult();
            bool spur = pair.IsSpur;
            double cos3 = Math.Pow(Math.Cos(psi), 3);

            // ------------------------------------------------------------ 5.1 virtual spur gear
            double n, rn, rnb, rna;
            if (spur)
            {
                n = m.Teeth; rn = m.PitchRadius; rnb = m.BaseRadius; rna = m.OutsideRadius;   // 5.6-5.9
            }
            else
            {
                n = m.Teeth / cos3;                                                 // 5.2
                rn = n / 2.0;                                                       // 5.3
                rnb = rn * Math.Cos(phiN);                                          // 5.4
                rna = rn + m.OutsideRadius - m.PitchRadius;                         // 5.5
            }
            b.VirtualTeeth = n; b.VirtualPitchRadius = rn; b.VirtualBaseRadius = rnb; b.VirtualOutsideRadius = rna;

            // ------------------------------------------------------------ 5.2 pressure angle at the load point
            switch (bc)
            {
                case BendingCase.SpurHPSTC:
                    {
                        // C4 for this member: from its own base-circle tangent point to its HPSTC.
                        double C1m = pair.C6 - Math.Sqrt(m.MateOutsideRadius * m.MateOutsideRadius - m.MateBaseRadius * m.MateBaseRadius);
                        b.TanPhiNW = (C1m + pb) / rnb;                              // 5.11
                        b.Case = "spur, load at HPSTC (Eq 5.11)";
                        break;
                    }
                case BendingCase.LACR:
                    {
                        double mGm = m.MateTeeth / m.Teeth;
                        double rn2 = rn * mGm;                                      // 5.12
                        double rnb2 = rnb * mGm;                                    // 5.13
                        double rna2 = rn2 + m.MateOutsideRadius - m.MatePitchRadius;// 5.14
                        double Cn6 = (rnb2 + rnb) * Math.Tan(pair.OperatingNormalPressureAngle); // 5.15
                        double Cn1 = Cn6 - Math.Sqrt(rna2 * rna2 - rnb2 * rnb2);    // 5.16
                        double Cn4 = Cn1 + pN;                                      // 5.17
                        b.TanPhiNW = Cn4 / rnb;                                     // 5.18
                        b.Case = "LACR helical, load at virtual HPSTC (Eq 5.18)";
                        break;
                    }
                default:
                    b.TanPhiNW = Math.Sqrt(Math.Pow(rna / rnb, 2) - 1.0);           // 5.10
                    b.Case = bc == BendingCase.SpurTip ? "spur, load at tip (Eq 5.10)" : "conventional helical, load at tip (Eq 5.10)";
                    break;
            }

            // ------------------------------------------------------------ 5.3 generating rack shift, 5.4 load angle/radius
            double xg = b.GeneratingRackShift = m.X - m.Thinning / (2.0 * Math.Tan(phiN)); // 5.19
            double sn = b.ToothThickness = Math.PI / 2.0 + 2.0 * xg * Math.Tan(phiN);    // 5.21
            double phiNL = b.LoadAngle = b.TanPhiNW - Math.Tan(phiN) + phiN - sn / n;    // 5.27
            double rnL = b.LoadRadius = rnb / Math.Cos(phiNL);                           // 5.28

            // ------------------------------------------------------------ 5.5 tool geometry
            Agma908Tool tool = m.Tool;
            double n0 = spur ? tool.ToothNumber : tool.ToothNumber / cos3;          // 5.29 / 5.32
            double rn0 = n0 / 2.0;                                                  // 5.30 / 5.33
            double rnb0 = rn0 * Math.Cos(phiN);                                     // 5.31 / 5.34
            double x0 = tool.AddendumModification;
            double deltaA0 = tool.Protuberance;
            if (tool.StockAllowance > 0.0)
            {
                x0 = 0.0;                                                           // 5.38
                deltaA0 = tool.Protuberance - tool.StockAllowance * Math.Cos(phiN);
            }
            double ha0 = tool.Addendum;
            double rhoA0 = tool.TipRadius;
            double rsn0 = b.ToolCenterRadius = rn0 + ha0 + x0 - rhoA0;              // 5.39
            double phiNs = Math.Acos(rnb0 / rsn0);                                  // 5.40
            double sn0 = Math.PI / 2.0 + 2.0 * x0 * Math.Tan(phiN);                 // 5.42
            double invPhiNp0 = Inv(phiN) + sn0 / n0;                                // 5.46
            double halfLambda = b.HalfAngleToToolCenter = invPhiNp0 - Inv(phiNs) + (deltaA0 - rhoA0) / rnb0; // 5.47
            b.ToolVirtualTeeth = n0;

            // ------------------------------------------------------------ 5.6 generating pressure angle and pitch radii
            double invGen = Inv(phiN) + 2.0 * (xg + x0) * Math.Tan(phiN) / (n + n0); // 5.48
            double phiGen = b.GeneratingPressureAngle = ArcInv(invGen);             // 5.49-5.50
            double rnG = b.GeneratingPitchRadius = rn * Math.Cos(phiN) / Math.Cos(phiGen);    // 5.51
            double rn0G = b.ToolGeneratingPitchRadius = rn0 * Math.Cos(phiN) / Math.Cos(phiGen); // 5.52

            // ------------------------------------------------------------ 5.7/5.8 critical point (Newton iteration on alpha_n)
            var pt = new FilletPoint();
            double alpha = Math.PI / 4.0;
            bool converged = false;
            for (int it = 1; it <= MaxIterations; it++)
            {
                Evaluate(alpha, rn0G, rsn0, rnG, rhoA0, halfLambda, n0, n, rnL, pt);
                b.Iterations = it;
                if (Math.Abs(pt.Y) < ConvergenceTolerance) { converged = true; break; }
                double next = alpha - pt.Y / pt.Yprime;                             // 5.65
                if (double.IsNaN(next) || next <= 0.0 || next >= Math.PI / 2.0) break;
                if (Math.Abs(next - alpha) < 1e-15) { alpha = next; converged = true; break; }
                alpha = next;
            }
            if (!converged)
            {
                // Safeguard: bracket the root between alpha_n ~ 0 and pi/2 (Figure 5-9) and bisect.
                alpha = BisectRoot(rn0G, rsn0, rnG, rhoA0, halfLambda, n0, n, rnL, pt);
                b.UsedBisection = true;
                Evaluate(alpha, rn0G, rsn0, rnG, rhoA0, halfLambda, n0, n, rnL, pt);
            }
            b.AlphaN = alpha;
            double sF = b.CriticalThickness = 2.0 * pt.Xi;                          // 5.73
            double hF = b.ParabolaHeight = pt.HF;                                   // 5.62

            // ------------------------------------------------------------ 5.9 minimum radius of curvature of the fillet
            double ks = rn0G - rsn0;                                                // 5.67
            b.FilletRadius = rhoA0 + ks * ks / (rnG * rn0G / (rnG + rn0G) - ks);    // 5.68

            // ------------------------------------------------------------ 5.10 helical factor
            bool conventional = !spur && mF > 1.0;
            if (conventional)
            {
                double omega = Math.Atan(Math.Tan(psi) * Math.Sin(phiN)) * 180.0 / Math.PI; // 5.71 (degrees)
                double w = omega / 100.0;
                b.HelicalFactor = 1.0 / (1.0 - Math.Sqrt(w * (1.0 - w)));          // 5.70
                if (m.FullButtressing) b.HelicalFactor *= 1.10;
            }
            else
                b.HelicalFactor = 1.0;                                              // 5.69

            // ------------------------------------------------------------ 5.11 stress correction factor
            double H = 0.331 - 0.436 * phiN;                                        // 5.74
            double L = 0.324 - 0.492 * phiN;                                        // 5.75
            double M = 0.261 + 0.545 * phiN;                                        // 5.76
            b.StressCorrection = H + Math.Pow(sF / b.FilletRadius, L) * Math.Pow(sF / hF, M); // 5.72

            // ------------------------------------------------------------ 5.12 helix angle factor, 5.13 tooth form factor
            b.HelixAngleFactor = conventional ? Math.Cos(pair.OperatingHelixAngle) * Math.Cos(psi) : 1.0; // 5.77
            double phiNr = pair.OperatingNormalPressureAngle;
            b.ToothForm = b.HelixAngleFactor /
                (Math.Cos(phiNL) / Math.Cos(phiNr) * (6.0 * hF / (sF * sF * b.HelicalFactor) - Math.Tan(phiNL) / sF)); // 5.78

            b.J = b.ToothForm * Cpsi / (b.StressCorrection * mN);                  // 5.1

            // ------------------------------------------------------------ 7.7 undercut criterion (informative)
            b.MinGeneratingRackShift = ha0 - rhoA0 * (1.0 - Math.Sin(phiN)) - n / 2.0 * Math.Pow(Math.Sin(phiN), 2); // 7.8
            return b;
        }

        private class FilletPoint
        {
            public double Y, Yprime, Xi, Eta, HF, Beta;
        }

        /// <summary>Eq 5.53 - 5.64 for a given angle of the surface normal alpha_n.</summary>
        private static void Evaluate(double alpha, double rn0G, double rsn0, double rnG, double rhoA0, double halfLambda,
            double n0, double n, double rnL, FilletPoint p)
        {
            double mu = Math.Acos(Math.Max(-1.0, Math.Min(1.0, rn0G * Math.Cos(alpha) / rsn0))) - alpha; // 5.53
            double Ks = rn0G * Math.Sin(alpha) - rsn0 * Math.Sin(alpha + mu);     // 5.54
            double Kf = Ks - rhoA0;                                                 // 5.55
            double theta0 = mu - halfLambda + Math.PI / n0;                         // 5.56
            double theta = n0 / n * theta0;                                         // 5.57/5.58
            double beta = alpha - theta;                                            // 5.59
            double xi = rnG * Math.Sin(theta) + Kf * Math.Cos(beta);                // 5.60
            double eta = rnG * Math.Cos(theta) + Kf * Math.Sin(beta);               // 5.61
            double hF = rnL - eta;                                                  // 5.62
            double tanB = Math.Tan(beta), cosB = Math.Cos(beta), sinB = Math.Sin(beta);
            p.Y = 2.0 * hF * tanB - xi;                                             // 5.63
            p.Yprime = 2.0 * hF / (cosB * cosB) - Kf * sinB                         // 5.64
                     + n0 / n * (rn0G * Math.Sin(alpha) / (rsn0 * Math.Sin(alpha + mu)) - 1.0)
                       * (2.0 * xi * tanB - eta - 2.0 * hF / (cosB * cosB))
                     - rn0G * (Math.Cos(alpha) - Math.Sin(alpha) / Math.Tan(alpha + mu)) * ((1.0 + sinB * sinB) / cosB);
            p.Xi = xi; p.Eta = eta; p.HF = hF; p.Beta = beta;
        }

        private static double BisectRoot(double rn0G, double rsn0, double rnG, double rhoA0, double halfLambda,
            double n0, double n, double rnL, FilletPoint p)
        {
            // Scan downward from just below pi/2, where y > 0 (Figure 5-9), for the first sign change.
            double hi = Math.PI / 2.0 - 1e-6;
            Evaluate(hi, rn0G, rsn0, rnG, rhoA0, halfLambda, n0, n, rnL, p);
            double yHi = p.Y;
            const int steps = 2000;
            double lo = hi;
            bool found = false;
            for (int i = 1; i < steps; i++)
            {
                double a = hi - i * (Math.PI / 2.0) / steps;
                if (a <= 1e-6) break;
                Evaluate(a, rn0G, rsn0, rnG, rhoA0, halfLambda, n0, n, rnL, p);
                if (Math.Sign(p.Y) != Math.Sign(yHi)) { lo = a; found = true; break; }
                hi = a; yHi = p.Y;
            }
            if (!found)
                throw new InvalidOperationException("AGMA 908 critical point iteration (Eq 5.63) did not converge: no root of y(alpha_n) in (0, pi/2).");
            for (int i = 0; i < 200; i++)
            {
                double mid = 0.5 * (lo + hi);
                Evaluate(mid, rn0G, rsn0, rnG, rhoA0, halfLambda, n0, n, rnL, p);
                if (Math.Sign(p.Y) == Math.Sign(yHi)) { hi = mid; yHi = p.Y; } else lo = mid;
                if (hi - lo < 1e-14) break;
            }
            return 0.5 * (lo + hi);
        }
    }
}
