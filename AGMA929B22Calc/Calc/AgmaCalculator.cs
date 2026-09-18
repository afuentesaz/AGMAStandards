using AGMA929B22Calc.Models;
using static AGMA929B22Calc.Calc.MathUtil;

namespace AGMA929B22Calc.Calc;

/// <summary>
/// Implements every calculation in AGMA 929-B22, Calculation of Bevel Gear Top Land,
/// Slot Widths and Cutter Edge Radii, Clause 4 (Eq 1 through Eq 215).
/// Equation numbers in comments refer to the published information sheet.
/// </summary>
public class AgmaCalculator
{
    private readonly AgmaInputs In;
    private readonly CalcTrace T = new();

    // Constant range variables per Table 4.
    private const double Mu_i1 = 1.0, Mu_e1 = 0.0, Mu_i2 = 1.0, Mu_e2 = 0.0;

    public AgmaCalculator(AgmaInputs inputs) => In = inputs;

    public (CalcTrace trace, List<string> warnings) Calculate()
    {
        var warnings = new List<string>();

        // ============================================================= 4.1 General geometry
        T.Section("4.1 General geometry");

        double Mu1(double R1) => (In.Re1 - R1) / In.b1;                       // Eq (1)
        double Mu2(double R2) => (In.Re2 - R2) / In.b2;                       // Eq (2)
        double R1(double mu1) => In.Re1 - mu1 * In.b1;                        // Eq (5)
        double R2(double mu2) => In.Re2 - mu2 * In.b2;                        // Eq (6)

        double mu_m1 = T.R("mu_m1", "Eq(3)", "Pinion range variable at mean", Mu1(In.Rm1), "");
        double mu_m2 = T.R("mu_m2", "Eq(4)", "Wheel range variable at mean", Mu2(In.Rm2), "");

        double d_m1 = T.R("d_m1", "Eq(7)", "Pinion mean pitch diameter", 2 * In.Rm1 * Sin(In.Delta1));
        double d_m2 = T.R("d_m2", "Eq(8)", "Wheel mean pitch diameter", 2 * In.Rm2 * Sin(In.Delta2));
        double d_e1 = T.R("d_e1", "Eq(9)", "Pinion outer pitch diameter", 2 * In.Re1 * Sin(In.Delta1));
        double d_e2 = T.R("d_e2", "Eq(10)", "Wheel outer pitch diameter", 2 * In.Re2 * Sin(In.Delta2));
        double dSigma = T.R("DeltaSigma", "Eq(11)", "Shaft angle departure from 90 degrees", In.Sigma - 90.0, "deg");
        double delta_f2 = T.R("delta_f2", "Eq(12)", "Wheel root angle", In.Delta2 - In.ThetaF2, "deg");

        double zeta_m = T.R("zeta_m", "Eq(13)", "Pinion offset angle in axial plane",
            Asin(2 * In.Offset_a / (d_m2 + d_m1 * Cos(In.Delta2) / Cos(In.Delta1))), "deg");
        double zeta_mp = T.R("zeta_mp", "Eq(14)", "Offset angle in pitch plane, pinion and wheel",
            Asin(Sin(zeta_m) * Sin(In.Sigma) / Cos(In.Delta1)), "deg");
        double a_p = T.R("a_p", "Eq(15)", "Offset in the pitch plane", In.Rm2 * Sin(zeta_mp));

        // ============================================================= 4.2 Spiral angles
        T.Section("4.2 Determination of spiral angles");

        double R21(double mu1) => Math.Sqrt(
            In.Rm2 * In.Rm2 + Math.Pow((mu1 - mu_m1) * In.b1, 2)
            - 2 * In.Rm2 * (mu1 - mu_m1) * In.b1 * Cos(zeta_mp));            // Eq (16)
        double Mu21(double mu1) => Mu2(R21(mu1));                             // Eq (17)

        double Re21 = T.R("Re21", "Eq(18)", "Wheel cone distance of pinion outer boundary", R21(Mu_e1));
        double Ri21 = T.R("Ri21", "Eq(19)", "Wheel cone distance of pinion inner boundary", R21(Mu_i1));
        double mu_e21 = T.R("mu_e21", "Eq(20)", "Range variable for wheel cone distance Re21", Mu2(Re21), "");
        double mu_i21 = T.R("mu_i21", "Eq(21)", "Range variable for wheel cone distance Ri21", Mu2(Ri21), "");

        // Method-specific wheel spiral angle beta2{mu2} and generating angle q2{mu2}.
        Func<double, double> Beta2, Q2;
        double nu = 0, rho_P0 = 0, rho_b = 0;

        if (In.CuttingMethod == CuttingMethod.FaceMilling)
        {
            T.Note("Face milling formulas used for spiral angle (Eq 22-25).");
            Beta2 = mu2 =>
            {
                double R2v = R2(mu2);
                return Asin((2 * In.Rm2 * In.r_c0 * Sin(In.Beta_m2) - In.Rm2 * In.Rm2 + R2v * R2v) / (2 * R2v * In.r_c0)); // Eq(22)
            };
            // Eq (23) as published gives a dimensionless ratio; the arctan wrapper implied
            // by the "degrees" unit in Table 1 and the analogous Eq (30) relation is applied.
            // Not used downstream for face milling (see 4.3), reported for reference only.
            Q2 = mu2 =>
            {
                double R2v = R2(mu2);
                double b2v = Beta2(mu2);
                return Atan(In.r_c0 * Cos(b2v) / (R2v - In.r_c0 * Sin(b2v)));  // Eq(23)
            };
        }
        else
        {
            nu = T.R("nu", "Eq(26)", "Lead angle of cutter (face hobbing)", Asin(In.z0 * In.m_mn / (2 * In.r_c0)), "deg");
            rho_P0 = T.R("rho_P0", "Eq(27)", "Crown gear to cutter center distance", Math.Sqrt(In.Rm2 * In.Rm2 + In.r_c0 * In.r_c0 - 2 * In.Rm2 * In.r_c0 * Sin(In.Beta_m2 - nu)));
            rho_b = T.R("rho_b", "Eq(28)", "Epicycloid base circle radius", rho_P0 / (1 + (double)In.z0 / In.z2 * Sin(In.Delta2)));

            double rho_P0c = rho_P0, rho_bc = rho_b;
            Q2 = mu2 =>
            {
                double R2v = R2(mu2);
                return Acos((R2v * R2v + rho_P0c * rho_P0c - In.r_c0 * In.r_c0) / (2 * R2v * rho_P0c));  // Eq(29)
            };
            Beta2 = mu2 =>
            {
                double R2v = R2(mu2);
                double q2v = Q2(mu2);
                return Atan((R2v - rho_bc * Cos(q2v)) / (rho_bc * Sin(q2v)));  // Eq(30)
            };
        }

        double beta_e21 = T.R("beta_e21", In.CuttingMethod == CuttingMethod.FaceMilling ? "Eq(24)" : "Eq(31)", "Wheel spiral angle at point mu_e21", Beta2(mu_e21), "deg");
        double beta_i21 = T.R("beta_i21", In.CuttingMethod == CuttingMethod.FaceMilling ? "Eq(25)" : "Eq(32)", "Wheel spiral angle at point mu_i21", Beta2(mu_i21), "deg");

        double Zetap2(double mu2) => Asin(a_p / R2(mu2));                     // Eq(33)
        double Beta12(double mu2) => Beta2(mu2) + Zetap2(mu2);                // Eq(34)

        double beta_e1 = T.R("beta_e1", "Eq(35)", "Pinion outer spiral angle", Beta12(mu_e21), "deg");
        double beta_i1 = T.R("beta_i1", "Eq(36)", "Pinion inner spiral angle", Beta12(mu_i21), "deg");

        T.Section("4.2.2 Wheel outer and inner spiral angles");
        double beta_e2 = T.R("beta_e2", "Eq(37)", "Wheel outer spiral angle", Beta2(Mu_e2), "deg");
        double beta_i2 = T.R("beta_i2", "Eq(38)", "Wheel inner spiral angle", Beta2(Mu_i2), "deg");

        // ============================================================= 4.3 Cone distance for involute curvature
        T.Section("4.3 Cone distance for involute lengthwise curvature");
        double Rx2, mu_x2, beta_x2;
        if (In.CuttingMethod == CuttingMethod.FaceMilling)
        {
            Rx2 = T.R("Rx2", "Eq(39)", "Cone distance for involute lengthwise curvature", Math.Sqrt(In.Rm2 * In.Rm2 - 2 * In.Rm2 * In.r_c0 * Sin(In.Beta_m2) + 2 * In.r_c0 * In.r_c0));
            mu_x2 = T.R("mu_x2", "Eq(40)", "Range variable for involute lengthwise curvature", Mu2(Rx2), "");
            // Eq(47) itself is not method-specific — it is the final beta2{mu_x2} evaluation
            // common to both methods (only how mu_x2/Rx2 are obtained differs, Eq 39/40 here
            // vs. the Eq 41-46 iteration below). The Annex C worked example (face milling)
            // reports beta_x2 under "Eq(47)" too, so it is logged here for consistency.
            beta_x2 = T.R("beta_x2", "Eq(47)", "Wheel spiral angle at involute lengthwise curvature", Beta2(mu_x2), "deg");
        }
        else
        {
            T.Note("Face hobbing: iterating Eq(41)-(45) for mu_x2 where q2{mu_x2} = beta2{mu_x2}.");
            mu_x2 = SecantSolve(mu2 => Q2(mu2) - Beta2(mu2), mu_m2, 0.001, 0.001);   // Eq(41)-(45)
            Rx2 = T.R("Rx2", "Eq(46)", "Wheel cone distance at point mu_x2", R2(mu_x2));
            T.R("mu_x2", "Eq(44)", "Range variable at involute lengthwise curvature (converged)", mu_x2, "");
            beta_x2 = T.R("beta_x2", "Eq(47)", "Wheel spiral angle at involute lengthwise curvature", Beta2(mu_x2), "deg");
        }

        // ============================================================= 4.4 Additional geometry
        T.Section("4.4 Additional geometry calculations");

        double delta_alpha2 = double.NaN;
        if (In.WheelGeneration == WheelGeneration.NonGenerated)
            delta_alpha2 = T.R("Delta_alpha2", "Eq(48)", "Normal tilt of finishing cutter (non-generated wheel)", In.Alpha_BX2 - In.Alpha_cv1, "deg");

        double sigmaAlpha = T.R("Sigma_alpha", "Eq(49)", "Included pressure angle", In.Alpha_cv1 - In.Alpha_cx1, "deg");
        double p_n = T.R("p_n", "Eq(50)", "Mean normal circular pitch", Math.PI * In.m_mn);
        double j_en = T.R("j_en", "Eq(51)", "Outer normal backlash",
            (p_n - (In.s_mn1 + In.s_mn2)) * (In.Re2 * Cos(beta_e2) * Cos(sigmaAlpha / 2)) / (In.Rm2 * Cos(In.Beta_m2)));

        double delta_c1 = T.R("Delta_c1", "Eq(52)", "Change in clearance, pinion",
            j_en * R1(Mu_i1) * Cos(beta_i1) / (In.Re1 * Cos(beta_e1)) * (1.0 / (Sin(In.Alpha_cv1) - Sin(In.Alpha_cx1))));
        double delta_c2 = T.R("Delta_c2", "Eq(53)", "Change in clearance, wheel",
            j_en * R2(Mu_i2) * Cos(beta_i2) / (In.Re2 * Cos(beta_e2)) * (1.0 / (Sin(In.Alpha_cv1) - Sin(In.Alpha_cx1))));

        if (Math.Abs(delta_c1) > 0.75 * In.Clearance_c)
            warnings.Add($"Delta_c1 ({delta_c1:0.###} mm) exceeds 0.75*c — reduce j_en or increase clearance c.");
        if (Math.Abs(delta_c2) > 0.75 * In.Clearance_c)
            warnings.Add($"Delta_c2 ({delta_c2:0.###} mm) exceeds 0.75*c — reduce j_en or increase clearance c.");

        double q2_mean = Q2(mu_m2);
        double DeltaQ(double mu2) => Math.Abs(q2_mean - Q2(mu2));             // Eq(54)

        double R12(double mu2) => In.Rm1 + R2(mu2) * Cos(Zetap2(mu2)) - In.Rm2 * Cos(zeta_mp); // Eq(55)
        double Mu12(double mu2) => Mu1(R12(mu2));                             // Eq(56)

        double DeltaH2(double mu2)                                            // Eq(57)
        {
            double term1 = Math.Pow(In.b2 * (mu_m2 - mu2), 2) * Tan(delta_alpha2) / (2 * In.r_c0 * Math.Pow(Cos(In.Beta_m2), 2));
            double term2 = R2(mu2) * Math.Pow(Sin(DeltaQ(mu2)), 2) / (2 * Tan(delta_f2));
            return term1 + term2;
        }

        double Hf1(double mu1) => In.h_fm1 + (mu_m1 - mu1) * In.b1 * Tan(In.ThetaF1);  // Eq(58)
        double Hf2(double mu2) => In.WheelGeneration == WheelGeneration.NonGenerated
            ? In.h_fm2 + (mu_m2 - mu2) * In.b2 * Tan(In.ThetaF2) - DeltaH2(mu2)         // Eq(59)
            : In.h_fm2 + (mu_m2 - mu2) * In.b2 * Tan(In.ThetaF2);                       // Eq(60)

        double SumHf(double mu2) => Hf1(Mu12(mu2)) + Hf2(mu2);                // Eq(61)
        double Ha1(double mu1) => Hf2(Mu21(mu1)) - In.Clearance_c;            // Eq(62)
        double Ha2(double mu2) => Hf1(Mu12(mu2)) - In.Clearance_c;            // Eq(63)

        // ============================================================= 4.5 Slot width
        T.Section("4.5 Slot width calculations and blade specifications");

        double W_m2 = T.R("W_m2", "Eq(64)", "Mean wheel slot width",
            p_n - In.s_mn2 - SumHf(mu_m2) / 2 * (Tan(In.Alpha_cv1) - Tan(In.Alpha_cx1)) - (In.h_am1 - In.h_am2) * Tan(sigmaAlpha / 2));

        double kE = In.KeMethod switch
        {
            KeMethod.FaceHobbingGleasonOrKlingelnberg => 1.0,                 // Eq(65)
            KeMethod.FaceHobbingOerlikonNoRoughingBlade => 1.0,               // Eq(66)
            KeMethod.FaceHobbingOerlikonWithRoughingBlade => 1.3,             // Eq(67)
            KeMethod.FaceMillingSpreadBlade => 0.0,                          // Eq(68)
            KeMethod.FaceMillingUnitoolSingleCycle => 2 * (W_m2 - In.W_finishingPointWidth) / p_n, // Eq(69)
            KeMethod.FaceMillingVersacutStandardSingleSide => 2 * (W_m2 + In.h_fm2 * (Tan(In.Alpha_cv1) - Tan(In.Alpha_cx1))) / p_n, // Eq(70)
            KeMethod.FaceMillingStraightBevelPlaningGenerator => 2 * W_m2 / p_n, // Eq(71)
            _ => throw new ArgumentOutOfRangeException()
        };
        T.R("k_E", "Eq(65-71)", "Wheel rotation factor", kE, "");

        double W_eff = T.R("W_eff", "Eq(72)", "Effective wheel point width", W_m2 - kE * p_n / 2);

        double xi = In.KeMethod == KeMethod.FaceMillingVersacutStandardSingleSide ? In.ThetaF2 : 0.0; // Eq(73)/(74)
        T.R("xi", "Eq(73/74)", "Angle between wheel root plane and taper plane", xi, "deg");

        double W2(double mu2)                                                 // Eq(75)
        {
            double R2v = R2(mu2);
            double b2v = Beta2(mu2);
            double cosRatio = R2v * Cos(b2v) / (In.Rm2 * Cos(In.Beta_m2));
            double term1 = W_eff * (1 - cosRatio);
            double term2 = cosRatio * (In.s_mn1 - In.h_fm2 * (Tan(In.Alpha_cv1) - Tan(In.Alpha_cx1)));
            double term3 = (R2v * Cos(b2v) / (In.Re2 * Cos(beta_e2))) * (j_en / Cos(sigmaAlpha / 2));
            double term4 = (In.Rm2 - R2v) * (Tan(In.Alpha_cv1) - Tan(In.Alpha_cx1)) * Tan(xi);
            return term1 + term2 + term3 + term4;
        }

        double W12(double mu2)                                                // Eq(76)
        {
            double R2v = R2(mu2);
            double b2v = Beta2(mu2);
            double cosRatio = R2v * Cos(b2v) / (In.Rm2 * Cos(In.Beta_m2));
            double term1 = cosRatio * p_n - SumHf(mu2) * (Tan(In.Alpha_cv1) - Tan(In.Alpha_cx1)) - W2(mu2);
            double term2 = (R2v * Cos(b2v) / (In.Re2 * Cos(beta_e2))) * (j_en / Cos(sigmaAlpha / 2));
            return term1 + term2;
        }

        double W_e2 = T.R("W_e2", "Eq(77)", "Outer (wheel) slot width", W2(Mu_e2));
        double W_e1 = T.R("W_e1", "Eq(78)", "Outer (pinion) slot width", W12(mu_e21));
        double W_m1 = T.R("W_m1", "Eq(79)", "Mean pinion slot width", W12(mu_m2));
        double W_i2 = T.R("W_i2", "Eq(80)", "Inner (wheel) slot width", W2(Mu_i2));
        double W_i1 = T.R("W_i1", "Eq(81)", "Inner (pinion) slot width", W12(mu_i21));

        double Ri2 = R2(Mu_i2);
        double W_x2, W_x1;
        if (Ri2 < Rx2 && Rx2 < In.Re2)
        {
            W_x2 = T.R("W_x2", "Eq(82)", "Wheel slot width at involute curvature", W2(mu_x2));
            W_x1 = T.R("W_x1", "Eq(83)", "Pinion slot width at involute curvature", W12(mu_x2));
        }
        else
        {
            W_x2 = T.R("W_x2", "Eq(84)", "Wheel slot width at involute curvature (out of range)", 0.0);
            W_x1 = T.R("W_x1", "Eq(85)", "Pinion slot width at involute curvature (out of range)", 0.0);
        }

        double W_min2 = T.R("W_min2", "Eq(86)", "Minimum wheel slot width", new[] { W_e2, W_m2, W_i2 }.Min());
        double W_min1 = T.R("W_min1", "Eq(87)", "Minimum pinion slot width", new[] { W_e1, W_m1, W_i1 }.Min());
        // Eq(88)/(89) as published include W_m2/W_m1 in the maximum. Note: the AGMA 929-B22
        // Annex D worked example's own published W_max2 (2.67661) is smaller than its own
        // published W_m2 (2.71949), i.e. it does not include W_m2 in the max — but the
        // formula as printed in 4.5.4 unambiguously lists it, and Wmax/Wmin only feed the
        // informational slot-width-range summary (they do not affect any other calculation),
        // so the literal published formula is used here.
        double W_max2 = T.R("W_max2", "Eq(88)", "Maximum wheel slot width", new[] { W_e2, W_x2, W_m2, W_i2 }.Max());
        double W_max1 = T.R("W_max1", "Eq(89)", "Maximum pinion slot width", new[] { W_e1, W_x1, W_m1, W_i1 }.Max());

        T.Section("4.5.5 Point width and blade point calculations");
        double W_BR1 = 0, W_BR2 = 0;
        if (In.WheelProcess == MemberProcess.RoughAndFinish)
        {
            double W_r2 = T.R("W_r2", "Eq(90)", "Wheel roughing point width", W_min2 - In.S_A2);
            W_BR2 = T.R("W_BR2", "Eq(92)", "Minimum wheel roughing blade point", W_r2 / 2 + 0.025);
        }
        if (In.PinionProcess == MemberProcess.RoughAndFinish)
        {
            double W_r1 = T.R("W_r1", "Eq(91)", "Pinion roughing point width", W_min1 - In.S_A1);
            W_BR1 = T.R("W_BR1", "Eq(93)", "Minimum pinion roughing blade point", W_r1 / 2 + 0.025);
        }

        double W_B2, W_B1;
        if (In.WheelProcess == MemberProcess.Completing)
            W_B2 = T.R("W_B2", "Eq(94)", "Minimum wheel finishing blade point (completing)", W_min2 / 2 + 0.025);
        else
            W_B2 = T.R("W_B2", "Eq(96)", "Maximum wheel finishing blade point (rough & finish)", W_min2 - In.S_A2);

        if (In.PinionProcess == MemberProcess.Completing)
            W_B1 = T.R("W_B1", "Eq(95)", "Minimum pinion finishing blade point (completing)", W_min1 / 2 + 0.025);
        else
            W_B1 = T.R("W_B1", "Eq(97)", "Maximum pinion finishing blade point (rough & finish)", W_min1 - In.S_A1);

        // ============================================================= 4.6 Cutter edge radius
        T.Section("4.6.1 Cutter edge radius for no running interference");

        double r_i1 = T.R("r_i1", "Eq(100)", "Original inner pinion pitch radius", R1(Mu_i1) * Tan(In.Delta1) / Math.Pow(Cos(beta_i1), 2));
        double r_i2 = T.R("r_i2", "Eq(101)", "Original inner wheel pitch radius", R2(Mu_i2) * Tan(In.Delta2) / Math.Pow(Cos(beta_i2), 2));
        double h_ai1 = T.R("h_ai1", "Eq(102)", "Pinion inner addendum", Hf2(Mu_i2) - In.Clearance_c);
        double h_ai2 = T.R("h_ai2", "Eq(103)", "Wheel inner addendum", Hf1(Mu_i1) - In.Clearance_c);
        double dHai1 = T.R("Delta_h_ai1", "Eq(104)", "Change in inner pinion addendum", delta_c2 * In.z1 * Cos(In.Delta2) / (In.z1 * Cos(In.Delta2) + In.z2 * Cos(In.Delta1)));
        double dHai2 = T.R("Delta_h_ai2", "Eq(105)", "Change in inner wheel addendum", delta_c1 * In.z2 * Cos(In.Delta1) / (In.z1 * Cos(In.Delta2) + In.z2 * Cos(In.Delta1)));
        double h_aiadj1 = T.R("h_aiadj1", "Eq(106)", "Adjusted inner pinion addendum", h_ai1 + dHai1);
        double h_aiadj2 = T.R("h_aiadj2", "Eq(107)", "Adjusted inner wheel addendum", h_ai2 + dHai2);
        double r_iadj1 = T.R("r_iadj1", "Eq(108)", "Adjusted inner pinion pitch radius", r_i1 - dHai1);
        double r_iadj2 = T.R("r_iadj2", "Eq(109)", "Adjusted inner wheel pitch radius", r_i2 - dHai2);
        double r_ai1 = T.R("r_ai1", "Eq(110)", "Inner pinion outside (tip) radius", r_i1 + h_ai1);
        double r_ai2 = T.R("r_ai2", "Eq(111)", "Inner wheel outside (tip) radius", r_i2 + h_ai2);

        double r_bicv1 = T.R("r_bicv1", "Eq(112)", "Inner pinion base radius, concave", r_i1 * Cos(In.Alpha_cv1));
        double r_bicx1 = T.R("r_bicx1", "Eq(113)", "Inner pinion base radius, convex", r_i1 * Cos(In.Alpha_cx1));
        double r_bicx2 = T.R("r_bicx2", "Eq(114)", "Inner wheel base radius, convex", r_i2 * Cos(In.Alpha_cv1));
        double r_bicv2 = T.R("r_bicv2", "Eq(115)", "Inner wheel base radius, concave", r_i2 * Cos(In.Alpha_cx1));

        double alpha_icv1 = T.R("alpha_icv1", "Eq(116)", "Inner pinion pressure angle, concave", Acos(r_bicv1 / r_iadj1), "deg");
        double alpha_icx1 = T.R("alpha_icx1", "Eq(117)", "Inner pinion pressure angle, convex", Acos(r_bicx1 / r_iadj1), "deg");

        double rho_icv1 = T.R("rho_icv1", "Eq(118)", "Inner pinion profile radius of curvature, concave",
            r_iadj1 * Sin(alpha_icv1) - Math.Sqrt(r_ai2 * r_ai2 - r_bicx2 * r_bicx2) + r_iadj2 * Sin(alpha_icv1));
        double rho_icx1 = T.R("rho_icx1", "Eq(119)", "Inner pinion profile radius of curvature, convex",
            r_iadj1 * Sin(alpha_icx1) - Math.Sqrt(r_ai2 * r_ai2 - r_bicv2 * r_bicv2) + r_iadj2 * Sin(alpha_icx1));
        double rho_icx2 = T.R("rho_icx2", "Eq(120)", "Inner wheel profile radius of curvature, convex",
            r_iadj2 * Sin(alpha_icv1) - Math.Sqrt(r_ai1 * r_ai1 - r_bicv1 * r_bicv1) + r_iadj1 * Sin(alpha_icv1));
        double rho_icv2 = T.R("rho_icv2", "Eq(121)", "Inner wheel profile radius of curvature, concave",
            r_iadj2 * Sin(alpha_icx1) - Math.Sqrt(r_ai1 * r_ai1 - r_bicx1 * r_bicx1) + r_iadj1 * Sin(alpha_icx1));

        double h_infcv1 = In.WheelGeneration == WheelGeneration.Generated
            ? T.R("h_infcv1", "Eq(122)", "Limit tooth height for interference, pinion concave", r_bicv1 * Cos(In.Alpha_cv1) + rho_icv1 * Sin(In.Alpha_cv1) - r_i1 + Hf1(Mu_i1))
            : T.R("h_infcv1", "Eq(123)", "Limit tooth height for interference, pinion concave (non-gen. wheel)", In.Clearance_c - delta_c1);
        double h_infcx1 = In.WheelGeneration == WheelGeneration.Generated
            ? T.R("h_infcx1", "Eq(124)", "Limit tooth height for interference, pinion convex", r_bicx1 * Cos(In.Alpha_cx1) - rho_icx1 * Sin(In.Alpha_cx1) - r_i1 + Hf1(Mu_i1))
            : T.R("h_infcx1", "Eq(125)", "Limit tooth height for interference, pinion convex (non-gen. wheel)", In.Clearance_c - delta_c1);
        double h_infcx2 = T.R("h_infcx2", "Eq(126)", "Limit tooth height for interference, wheel convex", r_bicx2 * Cos(In.Alpha_cv1) + rho_icx2 * Sin(In.Alpha_cv1) - r_i2 + Hf2(Mu_i2));
        double h_infcv2 = T.R("h_infcv2", "Eq(127)", "Limit tooth height for interference, wheel concave", r_bicv2 * Cos(In.Alpha_cx1) - rho_icv2 * Sin(In.Alpha_cx1) - r_i2 + Hf2(Mu_i2));

        double r_a0cv11 = T.R("r_a0cv11", "Eq(128)", "Max blade edge radius, no interference, pinion concave", h_infcv1 / (1 - Sin(In.Alpha_cv1)));
        double r_a0cx11 = T.R("r_a0cx11", "Eq(129)", "Max blade edge radius, no interference, pinion convex", h_infcx1 / (1 + Sin(In.Alpha_cx1)));
        double r_a0cx12 = T.R("r_a0cx12", "Eq(130)", "Max blade edge radius, no interference, wheel convex", h_infcx2 / (1 - Sin(In.Alpha_cv1)));
        double r_a0cv12 = T.R("r_a0cv12", "Eq(131)", "Max blade edge radius, no interference, wheel concave", h_infcv2 / (1 + Sin(In.Alpha_cx1)));

        double r_a011 = T.R("r_a011", "Eq(132/133)", "Max blade edge radius, no interference, pinion", Math.Min(r_a0cv11, r_a0cx11));
        double r_a012 = T.R("r_a012", "Eq(134/135)", "Max blade edge radius, no interference, wheel", Math.Min(r_a0cv12, r_a0cx12));

        T.Section("4.6.2 Cutter edge radius which can be manufactured");
        double r_a0rh1 = double.NaN, r_a0rh2 = double.NaN;
        if (In.PinionProcess == MemberProcess.RoughAndFinish)
        {
            double r_a02R1 = T.R("r_a02R1", "Eq(137)", "Max pinion roughing blade edge radius (manufacture)", (W_BR1 - In.Delta_f) / (Sec(In.Alpha_cv1) - Tan(In.Alpha_cv1)));
            r_a0rh1 = T.R("r_a0rh1", "Eq(139)", "Pinion roughing cutter edge radius", Math.Max(r_a02R1, 0.254));
        }
        if (In.WheelProcess == MemberProcess.RoughAndFinish)
        {
            double r_a02R2 = T.R("r_a02R2", "Eq(136)", "Max wheel roughing blade edge radius (manufacture)", (W_BR2 - In.Delta_f) / (Sec(In.Alpha_cv1) - Tan(In.Alpha_cv1)));
            r_a0rh2 = T.R("r_a0rh2", "Eq(138)", "Wheel roughing cutter edge radius", Math.Max(r_a02R2, 0.254));
        }

        double r_fincv1 = Math.Max(0.0, (W_B1 - In.Delta_f) / (Sec(In.Alpha_cv1) - Tan(In.Alpha_cv1)));
        double r_fincx1 = Math.Max(0.0, (W_B1 - In.Delta_f) / (Sec(In.Alpha_cx1) + Tan(In.Alpha_cx1)));
        double r_fincx2 = Math.Max(0.0, (W_B2 - In.Delta_f) / (Sec(In.Alpha_cv1) - Tan(In.Alpha_cv1)));
        double r_fincv2 = Math.Max(0.0, (W_B2 - In.Delta_f) / (Sec(In.Alpha_cx1) + Tan(In.Alpha_cx1)));
        if (r_fincv1 == 0 || r_fincx1 == 0 || r_fincx2 == 0 || r_fincv2 == 0)
            warnings.Add("A finishing blade edge radius (Eq 144-147) would be negative — reduce the blade flat width (Delta_f).");
        T.R("r_fincv1", "Eq(144)", "Max pinion finishing blade edge radius, concave", r_fincv1);
        T.R("r_fincx1", "Eq(145)", "Max pinion finishing blade edge radius, convex", r_fincx1);
        T.R("r_fincx2", "Eq(146)", "Max wheel finishing blade edge radius, convex", r_fincx2);
        T.R("r_fincv2", "Eq(147)", "Max wheel finishing blade edge radius, concave", r_fincv2);

        double r_a021, r_a022;
        if (In.PinionProcess == MemberProcess.RoughAndFinish)
        {
            double r_a02cv1 = T.R("r_a02cv1", "Eq(140)", "Max pinion finishing blade edge radius, concave (rough+fin)", In.S_A1 / (2 * (Sec(In.Alpha_cv1) - Tan(In.Alpha_cv1))) + r_a0rh1);
            double r_a02cx1 = T.R("r_a02cx1", "Eq(141)", "Max pinion finishing blade edge radius, convex (rough+fin)", In.S_A1 / (2 * (Sec(In.Alpha_cx1) + Tan(In.Alpha_cx1))) + r_a0rh1);
            r_a021 = T.R("r_a021", "Eq(148)", "Max pinion blade edge radius, manufacturable (rough & finish)", new[] { r_a02cv1, r_a02cx1, r_fincv1, r_fincx1 }.Min());
        }
        else
        {
            r_a021 = T.R("r_a021", "Eq(149)", "Max pinion blade edge radius, manufacturable (completing)", Math.Min(r_fincv1, r_fincx1));
        }

        if (In.WheelProcess == MemberProcess.RoughAndFinish)
        {
            double r_a02cx2 = T.R("r_a02cx2", "Eq(142)", "Max wheel finishing blade edge radius, convex (rough+fin)", In.S_A2 / (2 * (Sec(In.Alpha_cv1) - Tan(In.Alpha_cv1))) + r_a0rh2);
            double r_a02cv2 = T.R("r_a02cv2", "Eq(143)", "Max wheel finishing blade edge radius, concave (rough+fin)", In.S_A2 / (2 * (Sec(In.Alpha_cx1) + Tan(In.Alpha_cx1))) + r_a0rh2);
            r_a022 = T.R("r_a022", "Eq(150)", "Max wheel blade edge radius, manufacturable (rough & finish)", new[] { r_a02cx2, r_a02cv2, r_fincx2, r_fincv2 }.Min());
        }
        else
        {
            r_a022 = T.R("r_a022", "Eq(151)", "Max wheel blade edge radius, manufacturable (completing)", Math.Min(r_fincx2, r_fincv2));
        }

        T.Section("4.6.3 Cutter edge radius to avoid mutilation");
        double dWmin1 = T.R("Delta_Wmin1", "Eq(152)", "Difference: pinion min slot width vs finishing blade point", W_min1 - W_B1);
        double dWmin2 = T.R("Delta_Wmin2", "Eq(153)", "Difference: wheel min slot width vs finishing blade point", W_min2 - W_B2);

        double omega_cv1 = In.y_mutilation * In.y_mutilation + 2 * In.y_mutilation * dWmin1 * (Sec(In.Alpha_cv1) - Tan(In.Alpha_cv1));
        double omega_cx1 = In.y_mutilation * In.y_mutilation + 2 * In.y_mutilation * dWmin1 * (Sec(In.Alpha_cx1) + Tan(In.Alpha_cx1));
        double omega_cx2 = In.y_mutilation * In.y_mutilation + 2 * In.y_mutilation * dWmin2 * (Sec(In.Alpha_cv1) - Tan(In.Alpha_cv1));
        double omega_cv2 = In.y_mutilation * In.y_mutilation + 2 * In.y_mutilation * dWmin2 * (Sec(In.Alpha_cx1) + Tan(In.Alpha_cx1));
        if (omega_cv1 < 0 || omega_cx1 < 0 || omega_cx2 < 0 || omega_cv2 < 0)
            warnings.Add("An intermediate blade mutilation value Omega (Eq 154-157) is negative — reduce W_B1 or W_B2.");
        T.R("Omega_cv1", "Eq(154)", "Pinion intermediate blade mutilation value, concave", omega_cv1, "mm^2");
        T.R("Omega_cx1", "Eq(155)", "Pinion intermediate blade mutilation value, convex", omega_cx1, "mm^2");
        T.R("Omega_cx2", "Eq(156)", "Wheel intermediate blade mutilation value, convex", omega_cx2, "mm^2");
        T.R("Omega_cv2", "Eq(157)", "Wheel intermediate blade mutilation value, concave", omega_cv2, "mm^2");

        double SafeSqrt(double x) => Math.Sqrt(Math.Max(0.0, x));
        double r_a0cx31 = T.R("r_a0cx31", "Eq(158)", "Max pinion blade edge radius to avoid mutilation, convex",
            (In.y_mutilation + dWmin1 * (Sec(In.Alpha_cx1) + Tan(In.Alpha_cx1)) + SafeSqrt(omega_cx1)) / Math.Pow(Sec(In.Alpha_cx1) + Tan(In.Alpha_cx1), 2));
        double r_a0cv31 = T.R("r_a0cv31", "Eq(159)", "Max pinion blade edge radius to avoid mutilation, concave",
            (In.y_mutilation + dWmin1 * (Sec(In.Alpha_cv1) - Tan(In.Alpha_cv1)) + SafeSqrt(omega_cv1)) / Math.Pow(Sec(In.Alpha_cv1) - Tan(In.Alpha_cv1), 2));
        double r_a0cx32 = T.R("r_a0cx32", "Eq(160)", "Max wheel blade edge radius to avoid mutilation, convex",
            (In.y_mutilation + dWmin2 * (Sec(In.Alpha_cx1) + Tan(In.Alpha_cx1)) + SafeSqrt(omega_cv2)) / Math.Pow(Sec(In.Alpha_cx1) + Tan(In.Alpha_cx1), 2));
        double r_a0cv32 = T.R("r_a0cv32", "Eq(161)", "Max wheel blade edge radius to avoid mutilation, concave",
            (In.y_mutilation + dWmin2 * (Sec(In.Alpha_cv1) - Tan(In.Alpha_cv1)) + SafeSqrt(omega_cx2)) / Math.Pow(Sec(In.Alpha_cv1) - Tan(In.Alpha_cv1), 2));

        double r_a031 = T.R("r_a031", "Eq(162)", "Max pinion blade edge radius to avoid mutilation", Math.Min(r_a0cv31, r_a0cx31));
        double r_a032 = T.R("r_a032", "Eq(163)", "Max wheel blade edge radius to avoid mutilation", Math.Min(r_a0cv32, r_a0cx32));

        T.Section("4.6 Summary — recommended maximum cutter edge radius");
        double r_a0max1 = T.R("r_a0max1", "Eq(98)", "Maximum pinion blade edge radius (governing)", new[] { r_a011, r_a021, r_a031 }.Min());
        double r_a0max2 = T.R("r_a0max2", "Eq(99)", "Maximum wheel blade edge radius (governing)", new[] { r_a012, r_a022, r_a032 }.Min());

        if (In.WheelGeneration == WheelGeneration.NonGenerated)
            warnings.Add("Wheel generation = Non-generated: the PINION top land (Eq 180) was validated against " +
                "AGMA 929-B22 Annex D and Annex E to within roughly 0.15-0.6 mm, not to full published precision " +
                "the way every other result in this calculator was. This also affects the pinion tip chamfer " +
                "(4.8), which is located by searching for where the top land equals a target value. The wheel's " +
                "own top land (Eq 182) and all other results for a non-generated wheel matched both published " +
                "examples exactly. Cross-check the pinion top land (and any chamfer built from it) against the " +
                "standard directly, or against manufacturer data, before relying on it.");

        // ============================================================= 4.7 Top land formulas
        // Reusable point-evaluation of Eq (164)-(182) at any (mu1, mu2) pair.
        // r_na2{mu2}: Eq(171) for a generated wheel, Eq(172) for a non-generated (Formate) wheel.
        double Rna2(double mu2v, double rNpt2v) => In.WheelGeneration == WheelGeneration.Generated
            ? rNpt2v + Ha2(mu2v)             // Eq(171)
            : rNpt2v - Ha1(Mu12(mu2v));      // Eq(172)

        // Full set of wheel-side quantities (Eq 165,168,169,171/172,175,176,178) at a given
        // mu2, shared between the wheel's own top-land calc and the Eq(180) pinion cross-term
        // so both use exactly the same formulas.
        (double r_npt2, double r_na2, double alpha_acx2, double alpha_acv2, double s_n2) WheelSide(double mu2v)
        {
            double r_npt2v = d_e2 / (2 * Cos(In.Delta2) * Math.Pow(Cos(Beta2(mu2v)), 2)) * R2(mu2v) / In.Re2; // Eq(165)
            double r_bcx2v = r_npt2v * Cos(In.Alpha_cv1);   // Eq(168)
            double r_bcv2v = r_npt2v * Cos(In.Alpha_cx1);   // Eq(169)
            double r_na2v = Rna2(mu2v, r_npt2v);
            double alpha_acx2v = Acos(r_bcx2v / r_na2v);    // Eq(175)
            double alpha_acv2v = Acos(r_bcv2v / r_na2v);    // Eq(176)
            double s_n2v = Hf1(Mu12(mu2v)) * (Tan(In.Alpha_cv1) - Tan(In.Alpha_cx1)) + W12(mu2v)
                           - (R2(mu2v) * Cos(Beta2(mu2v)) / (In.Re2 * Cos(beta_e2))) * (j_en / Cos(sigmaAlpha / 2)); // Eq(178)
            return (r_npt2v, r_na2v, alpha_acx2v, alpha_acv2v, s_n2v);
        }

        TopLandPoint EvalTopLand(double mu1, double mu2)
        {
            double r_npt1 = d_e1 / (2 * Cos(In.Delta1) * Math.Pow(Cos(Beta12(Mu21(mu1))), 2)) * R1(mu1) / In.Re1; // Eq(164)
            double r_bcv1 = r_npt1 * Cos(In.Alpha_cv1);   // Eq(166)
            double r_bcx1 = r_npt1 * Cos(In.Alpha_cx1);   // Eq(167)
            double r_na1 = r_npt1 + Ha1(mu1);             // Eq(170)
            double alpha_acv1 = Acos(r_bcv1 / r_na1);     // Eq(173)
            double alpha_acx1 = Acos(r_bcx1 / r_na1);     // Eq(174)

            var (r_npt2, r_na2, alpha_acx2, alpha_acv2, s_n2) = WheelSide(mu2);

            double s_n1 = Hf2(Mu21(mu1)) * (Tan(In.Alpha_cv1) - Tan(In.Alpha_cx1)) + W2(Mu21(mu1))
                          - (R1(mu1) * Cos(Beta12(Mu21(mu1))) / (In.Re1 * Cos(beta_e1))) * (j_en / Cos(sigmaAlpha / 2)); // Eq(177)
                          // NB: printed as "+" in the information sheet, but validated against the
                          // Annex C worked example (s_n1 at toe/mean/heel) only with a "-" sign,
                          // mirroring the minus sign in the analogous Eq(178) for the wheel.

            double s_an1;
            if (In.WheelGeneration == WheelGeneration.Generated)
            {
                s_an1 = (s_n1 / r_npt1 + Inv(In.Alpha_cv1) - Inv(alpha_acv1) + Inv(-In.Alpha_cx1) - Inv(alpha_acx1)) * r_na1; // Eq(179)
            }
            else
            {
                // Eq(180)'s second bracket mirrors Eq(181) (the generated-wheel top-land
                // formula) evaluated at the pinion-boundary-mapped point mu21 = mu21{mu1}; the
                // third term then reconciles it against the wheel's non-generated circular
                // thickness/addendum at that same point.
                //
                // This reading — using the PINION addendum function h_a1{mu21} (Eq 62), not the
                // wheel's h_a2, for both the tip radius in the second bracket and the final
                // correction term — was arrived at by numerical validation against AGMA 929-B22
                // Annex D and Annex E (the standard's two published non-generated-wheel worked
                // examples). It is the closest of every sign/subscript/function combination
                // tried (see git history for the ones ruled out), reproducing the published
                // pinion top land to within roughly 0.15-0.6 mm, but it does NOT reproduce it to
                // full published precision the way every other equation in this calculator does
                // (see the Annex C/D/E self-tests, "--selftest"). Treat this one case — pinion
                // top land, and the pinion tip chamfer built from it (4.8), under a
                // NON-GENERATED wheel — with extra caution; the wheel's own top land (Eq 182)
                // is unaffected and matches published examples exactly.
                double mu21 = Mu21(mu1);
                var (r_npt2b, _, _, _, s_n2b) = WheelSide(mu21);
                double h_a1_mu21 = Ha1(mu21);
                double r_na2b = r_npt2b + h_a1_mu21;
                double alpha_acx2b = Acos((r_npt2b * Cos(In.Alpha_cv1)) / r_na2b);
                double alpha_acv2b = Acos((r_npt2b * Cos(In.Alpha_cx1)) / r_na2b);

                double bracket1 = s_n1 / r_npt1 + Inv(In.Alpha_cv1) - Inv(alpha_acv1) + Inv(-In.Alpha_cx1) - Inv(alpha_acx1);
                double bracket2 = s_n2b / r_npt2b + Inv(In.Alpha_cv1) - Inv(alpha_acx2b) + Inv(-In.Alpha_cx1) - Inv(alpha_acv2b);
                s_an1 = bracket1 * r_na1 + bracket2 * r_na2b - (s_n2b - h_a1_mu21 * (Tan(In.Alpha_cv1) - Tan(In.Alpha_cx1))); // Eq(180)
            }

            double s_an2 = In.WheelGeneration == WheelGeneration.Generated
                ? (s_n2 / r_npt2 + Inv(In.Alpha_cv1) - Inv(alpha_acx2) + Inv(-In.Alpha_cx1) - Inv(alpha_acv2)) * r_na2 // Eq(181)
                : s_n2 - Ha2(mu2) * (Tan(In.Alpha_cv1) - Tan(In.Alpha_cx1));                                            // Eq(182)

            return new TopLandPoint(r_npt1, r_npt2, r_na1, r_na2, alpha_acv1, alpha_acx1, alpha_acx2, alpha_acv2, s_n1, s_n2, s_an1, s_an2);
        }

        T.Section("4.7 Top land — inner end (toe)");
        var inner = EvalTopLand(Mu_i1, Mu_i2);
        LogTopLand(T, inner, "183/186");
        double s_ain1 = inner.s_an1, s_ain2 = inner.s_an2;

        T.Section("4.7 Top land — mean");
        var mean = EvalTopLand(mu_m1, mu_m2);
        LogTopLand(T, mean, "184/187");
        double s_amn1 = mean.s_an1, s_amn2 = mean.s_an2;

        T.Section("4.7 Top land — outer end (heel)");
        var outer = EvalTopLand(Mu_e1, Mu_e2);
        LogTopLand(T, outer, "185/188");
        double s_aen1 = outer.s_an1, s_aen2 = outer.s_an2;

        // ============================================================= 4.8 Pinion tip chamfer (optional)
        if (In.CalculatePinionChamfer)
        {
            T.Section("4.8 Pinion tooth tip chamfer at inner end");

            double target = In.DesiredChamferTopLand;
            if (!(s_ain1 < target && target < s_aen1))
                warnings.Add($"Desired chamfer top land ({target:0.###} mm) should lie strictly between the inner ({s_ain1:0.###} mm) and outer ({s_aen1:0.###} mm) pinion top lands.");

            double mu_c1 = SecantSolve(mu1 => EvalTopLand(mu1, Mu_i2 /*unused for pinion s_an1 mu2 arg*/).s_an1 - target, Mu_i1, -0.001, 0.001);
            // Note: EvalTopLand needs a matching mu2; for the pinion-only chamfer search we
            // evaluate s_an1{mu1} which internally maps mu1 -> mu21{mu1} itself (Eq 177/179/180),
            // so the mu2 argument is not used by the s_an1 branch and is passed as a placeholder.
            double s_acn1_check = EvalTopLand(mu_c1, Mu_i2).s_an1;
            T.R("mu_c1", "Eq(189-192)", "Pinion range variable at chamfer starting point (converged)", mu_c1, "");
            T.R("s_acn1", "Eq(190)", "Pinion normal top land at chamfer starting point", s_acn1_check);

            double R_c1 = T.R("R_c1", "Eq(193)", "Pinion cone distance at chamfer starting point", R1(mu_c1));
            double t_c1 = T.R("t_c1", "Eq(194)", "Length of chamfer", R_c1 - R1(Mu_i1));

            double r_nipt1 = T.R("r_nipt1", "Eq(195)", "Normal pinion pitch radius at inner end", d_e1 / (2 * Cos(In.Delta1) * Math.Pow(Cos(Beta12(mu_i21)), 2)) * R1(Mu_i1) / In.Re1);
            double r_nipt21 = T.R("r_nipt21", "Eq(196)", "Normal wheel pitch radius of pinion inner boundary", d_e2 / (2 * Cos(In.Delta2) * Math.Pow(Cos(Beta2(mu_i21)), 2)) * R2(mu_i21) / In.Re2);

            double r_bicv1c = T.R("r_bicv1(chamfer)", "Eq(197)", "Normal pinion base radius at inner end, concave", r_nipt1 * Cos(In.Alpha_cv1));
            double r_bicx1c = T.R("r_bicx1(chamfer)", "Eq(198)", "Normal pinion base radius at inner end, convex", r_nipt1 * Cos(In.Alpha_cx1));
            double r_bicx21 = T.R("r_bicx21", "Eq(199)", "Normal wheel base radius of pinion inner boundary, convex", r_nipt21 * Cos(In.Alpha_cv1));
            double r_bicv21 = T.R("r_bicv21", "Eq(200)", "Normal wheel base radius of pinion inner boundary, concave", r_nipt21 * Cos(In.Alpha_cx1));

            double s_ni1 = T.R("s_ni1", "Eq(201)", "Normal pinion circular thickness at inner end", inner.s_n1);
            double s_ni21 = T.R("s_ni21", "Eq(202)", "Normal wheel circular thickness of pinion inner boundary", EvalTopLand(Mu_i1, mu_i21).s_n2);

            double SAnci(double h_aci1_trial)
            {
                double r_naci1 = r_nipt1 + h_aci1_trial;                       // Eq(204)
                double alpha_acicv1 = Acos(r_bicv1c / r_naci1);                // Eq(206)
                double alpha_acicx1 = Acos(r_bicx1c / r_naci1);                // Eq(207)

                if (In.WheelGeneration == WheelGeneration.Generated)
                {
                    return (s_ni1 / r_nipt1 + Inv(In.Alpha_cv1) - Inv(alpha_acicv1) + Inv(-In.Alpha_cx1) - Inv(alpha_acicx1)) * r_naci1; // Eq(210)
                }
                else
                {
                    double r_naci21 = r_nipt21 - h_aci1_trial;                 // Eq(205)
                    double alpha_acicx21 = Acos(r_bicx21 / r_naci21);          // Eq(208)
                    double alpha_acicv21 = Acos(r_bicv21 / r_naci21);          // Eq(209)
                    double bracket1 = s_ni1 / r_nipt1 + Inv(In.Alpha_cv1) - Inv(alpha_acicv1) + Inv(-In.Alpha_cx1) - Inv(alpha_acicx1);
                    double bracket2 = s_ni21 / r_nipt21 + Inv(In.Alpha_cv1) - Inv(alpha_acicx21) + Inv(-In.Alpha_cx1) - Inv(alpha_acicv21);
                    return bracket1 * r_naci1 + bracket2 * r_naci21
                           - (s_ni21 + h_aci1_trial * (Tan(In.Alpha_cv1) - Tan(In.Alpha_cx1))); // Eq(211)
                }
            }

            double h_aci1 = SecantSolve(h => SAnci(h) - target, Ha1(Mu_i1), -0.001, 0.001);    // Eq(203),(212),(213)
            double s_anci1 = T.R("s_anci1", "Eq(210/211)", "Normal pinion top land at inner chamfer (converged)", SAnci(h_aci1));
            T.R("h_aci1", "Eq(203-213)", "Pinion addendum at the inner end chamfer (converged)", h_aci1);

            double h_ac1_final = T.R("h_ac1", "Eq(214)", "Pinion addendum at the chamfer starting point", Ha1(mu_c1));
            double delta_ac1 = T.R("delta_ac1", "Eq(215)", "Pinion chamfer angle", Atan((h_ac1_final - h_aci1) / t_c1) + In.Delta1, "deg");
        }

        // ---- Final summary section, mirroring Annex C presentation.
        T.Section("Summary — Top land results (normal, mm)");
        T.R("s_ain1", "Eq(183)", "Pinion inner (toe) normal top land", s_ain1);
        T.R("s_amn1", "Eq(184)", "Pinion mean normal top land", s_amn1);
        T.R("s_aen1", "Eq(185)", "Pinion outer (heel) normal top land", s_aen1);
        T.R("s_ain2", "Eq(186)", "Wheel inner (toe) normal top land", s_ain2);
        T.R("s_amn2", "Eq(187)", "Wheel mean normal top land", s_amn2);
        T.R("s_aen2", "Eq(188)", "Wheel outer (heel) normal top land", s_aen2);
        T.R("r_a0max1", "Eq(98)", "Maximum recommended pinion blade edge radius", r_a0max1);
        T.R("r_a0max2", "Eq(99)", "Maximum recommended wheel blade edge radius", r_a0max2);
        T.R("W_min1", "Eq(87)", "Minimum pinion slot width", W_min1);
        T.R("W_max1", "Eq(89)", "Maximum pinion slot width", W_max1);
        T.R("W_min2", "Eq(86)", "Minimum wheel slot width", W_min2);
        T.R("W_max2", "Eq(88)", "Maximum wheel slot width", W_max2);

        return (T, warnings);
    }

    private static void LogTopLand(CalcTrace t, TopLandPoint p, string eqRef)
    {
        t.R("r_npt1", "Eq(164)", "Normal pinion pitch radius", p.r_npt1);
        t.R("r_npt2", "Eq(165)", "Normal wheel pitch radius", p.r_npt2);
        t.R("r_na1", "Eq(170)", "Normal pinion outside (tip) radius", p.r_na1);
        t.R("r_na2", "Eq(171/172)", "Normal wheel outside (tip) radius", p.r_na2);
        t.R("alpha_acv1", "Eq(173)", "Pressure angle at tip, pinion concave", p.alpha_acv1, "deg");
        t.R("alpha_acx1", "Eq(174)", "Pressure angle at tip, pinion convex", p.alpha_acx1, "deg");
        t.R("alpha_acx2", "Eq(175)", "Pressure angle at tip, wheel convex", p.alpha_acx2, "deg");
        t.R("alpha_acv2", "Eq(176)", "Pressure angle at tip, wheel concave", p.alpha_acv2, "deg");
        t.R("s_n1", "Eq(177)", "Normal pinion circular thickness at pitch line", p.s_n1);
        t.R("s_n2", "Eq(178)", "Normal wheel circular thickness at pitch line", p.s_n2);
        t.R($"s_an1 [{eqRef}]", "Eq(179/180)", "Normal pinion top land", p.s_an1);
        t.R($"s_an2 [{eqRef}]", "Eq(181/182)", "Normal wheel top land", p.s_an2);
    }

    private readonly record struct TopLandPoint(
        double r_npt1, double r_npt2, double r_na1, double r_na2,
        double alpha_acv1, double alpha_acx1, double alpha_acx2, double alpha_acv2,
        double s_n1, double s_n2, double s_an1, double s_an2);
}
