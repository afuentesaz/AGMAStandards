using ISO23509Calc.Models;
using static ISO23509Calc.Calc.MathUtil;

namespace ISO23509Calc.Calc;

/// <summary>Results of ISO 23509:2016, Clause 7 ("Gear dimensions"): every quantity needed to
/// draw the finished blank and check the tooth data, computed once from the pitch cone
/// parameters (Clause 6) and the tooth-profile initial data (Table 3), the same for all four
/// pitch cone methods (Annex A.4).</summary>
public class GearDimensionsResult
{
    public double dm1, dm2, DeltaSigma, zeta_m, zeta_mp, ap, m_mn, alpha_lim;
    public double alpha_nD, alpha_nC, alpha_eD, alpha_eC;
    public double Re2, Ri2, de2, di2, met2, be2, bi2, tzm2, tzm1, tz1, tz2;
    public double hmw, ham2, hfm2, ham1, hfm1, c, hm;
    public double delta_a2, delta_f2, theta_a2, theta_f2, phi_R, phi_o, zeta_R, zeta_o, delta_a1, delta_f1, theta_a1, theta_f1;
    public double tzF2, tzR2, tzF1, tzR1;
    public double bp1, b1A, b1, be1, bi1;
    public double Re21, Ri21, beta_e21, beta_i21, zeta_ep21, zeta_ip21, beta_e1, beta_i1;
    public double beta_e2, beta_i2;
    public double hae1, hae2, hfe1, hfe2, he1, he2, hai1, hai2, hfi1, hfi2, hi1, hi2;
    public double alpha_n, x_sm1, s_mn1, x_sm2, s_mn2, s_mt1, s_mt2, d_mn1, d_mn2, s_mnc1, s_mnc2, h_amc1, h_amc2;
    public double Re1, Ri1, de1, di1, dae1, dae2, dfe1, dfe2, dai1, dai2, dfi1, dfi2, txo1, txo2, txi1, txi2, ht1;
    public double x_hm1, k_hap, k_hfp, x_smn; // Data type I values actually used (after any II->I conversion)
    public double rho_P0, rho_b; // face-hobbing epicycloid geometry, needed by Clause 8 too
}

/// <summary>
/// Implements ISO 23509:2016, Clause 7 (Formulae (110)-(226)), common to all four pitch cone
/// methods once Rm1/Rm2/delta1/delta2/beta_m1/beta_m2/c_be2 (Clause 6) are known.
/// </summary>
public static class GearDimensions
{
    /// <summary>Table 4: converts data type II (cham, kd, kc, kt) into data type I (xhm1, khap,
    /// khfp, xsmn), the form every Clause 7 formula uses directly.</summary>
    public static (double x_hm1, double k_hap, double k_hfp, double x_smn) ConvertTypeIIToI(Iso23509Inputs In, CalcTrace T)
    {
        T.Section("Table 4: Conversion of data type II into data type I");
        double kHap = T.R("khap", "T:4", "Basic crown gear addendum factor", In.k_d / 2.0, "");
        double xHm1 = T.R("xhm1", "T:4", "Profile shift coefficient", In.k_d * (0.5 - In.c_ham), "");
        double kHfp = T.R("khfp", "T:4", "Basic crown gear dedendum factor", In.k_d * (In.k_c + 0.5), "");
        double xSmn = T.R("xsmn", "T:4", "Theoretical thickness modification coefficient", In.k_t / 2.0, "");
        return (xHm1, kHap, kHfp, xSmn);
    }

    public static GearDimensionsResult Calculate(Iso23509Inputs In, PitchConeResult pc, CalcTrace T, List<string> warnings)
    {
        var r = new GearDimensionsResult();

        if (In.DataType == InputDataType.TypeII)
        {
            (r.x_hm1, r.k_hap, r.k_hfp, r.x_smn) = ConvertTypeIIToI(In, T);
        }
        else
        {
            r.x_hm1 = In.x_hm1;
            r.k_hap = In.k_hap;
            r.k_hfp = In.k_hfp;
            r.x_smn = In.x_smn;
        }

        // ========================================================= 7.2 Determination of basic data
        T.Section("7.2: Determination of basic data");
        r.dm1 = T.R("dm1", "Eq(110)", "Pinion mean pitch diameter", 2.0 * pc.Rm1 * Sin(pc.delta1), "mm");
        r.dm2 = T.R("dm2", "Eq(111)", "Wheel mean pitch diameter", 2.0 * pc.Rm2 * Sin(pc.delta2), "mm");
        r.DeltaSigma = T.R("DeltaSigma", "Eq(112)", "Shaft angle departure from 90 deg", In.Sigma - 90.0, "deg");
        r.zeta_m = T.R("zeta_m", "Eq(113)", "Offset angle in the pinion axial plane",
            Asin(2.0 * In.a / (r.dm2 + r.dm1 * Cos(pc.delta2) / Cos(pc.delta1))), "deg");
        r.zeta_mp = T.R("zeta_mp", "Eq(114)", "Offset angle in the pitch plane", Asin(Sin(r.zeta_m) * Sin(In.Sigma) / Cos(pc.delta1)), "deg");
        r.ap = T.R("ap", "Eq(115)", "Offset in pitch plane", pc.Rm2 * Sin(r.zeta_mp), "mm");
        r.m_mn = T.R("m_mn", "Eq(116)", "Mean normal module", 2.0 * pc.Rm2 * Sin(pc.delta2) * Cos(pc.beta_m2) / In.z2, "mm");
        r.alpha_lim = T.R("alpha_lim", "Eq(117)", "Limit pressure angle",
            -Atan(Tan(pc.delta1) * Tan(pc.delta2) / Cos(r.zeta_mp) *
                ((pc.Rm1 * Sin(pc.beta_m1) - pc.Rm2 * Sin(pc.beta_m2)) / (pc.Rm1 * Tan(pc.delta1) + pc.Rm2 * Tan(pc.delta2)))), "deg");
        r.alpha_nD = T.R("alpha_nD", "Eq(118)", "Generated normal pressure angle, drive side", In.alpha_dD + In.f_alim * r.alpha_lim, "deg");
        r.alpha_nC = T.R("alpha_nC", "Eq(119)", "Generated normal pressure angle, coast side", In.alpha_dC - In.f_alim * r.alpha_lim, "deg");
        r.alpha_eD = T.R("alpha_eD", "Eq(120)", "Effective pressure angle, drive side", r.alpha_nD - r.alpha_lim, "deg");
        r.alpha_eC = T.R("alpha_eC", "Eq(121)", "Effective pressure angle, coast side", r.alpha_nC + r.alpha_lim, "deg");
        r.Re2 = T.R("Re2", "Eq(122)", "Outer pitch cone distance, wheel", pc.Rm2 + pc.c_be2 * In.b2, "mm");
        r.Ri2 = T.R("Ri2", "Eq(123)", "Inner pitch cone distance, wheel", r.Re2 - In.b2, "mm");
        r.de2 = T.R("de2", "Eq(124)", "Outer pitch diameter, wheel", 2.0 * r.Re2 * Sin(pc.delta2), "mm");
        r.di2 = T.R("di2", "Eq(125)", "Inner pitch diameter, wheel", 2.0 * r.Ri2 * Sin(pc.delta2), "mm");
        r.met2 = T.R("met2", "Eq(126)", "Outer transverse module", r.de2 / In.z2, "mm");
        r.be2 = T.R("be2", "Eq(127)", "Wheel face width, calc point to outside", r.Re2 - pc.Rm2, "mm");
        r.bi2 = T.R("bi2", "Eq(128)", "Wheel face width, calc point to inside", pc.Rm2 - r.Ri2, "mm");
        r.tzm2 = T.R("tzm2", "Eq(129)", "Crossing point to calc point along wheel axis",
            r.dm1 * Sin(pc.delta2) / (2.0 * Cos(pc.delta1)) - 0.5 * Cos(r.zeta_m) * Tan(r.DeltaSigma) * (r.dm2 + r.dm1 * Cos(pc.delta2) / Cos(pc.delta1)), "mm");
        r.tzm1 = T.R("tzm1", "Eq(130)", "Crossing point to calc point along pinion axis",
            (r.dm2 / 2.0) * Cos(r.zeta_m) * Cos(r.DeltaSigma) - r.tzm2 * Sin(r.DeltaSigma), "mm");
        r.tz1 = T.R("tz1", "Eq(131)", "Pitch apex beyond crossing point along axis, pinion", pc.Rm1 * Cos(pc.delta1) - r.tzm1, "mm");
        r.tz2 = T.R("tz2", "Eq(131)", "Pitch apex beyond crossing point along axis, wheel", pc.Rm2 * Cos(pc.delta2) - r.tzm2, "mm");

        // ========================================================= 7.3 Tooth depth at calculation point
        T.Section("7.3: Determination of tooth depth at calculation point");
        r.hmw = T.R("hmw", "Eq(132)", "Mean working depth", 2.0 * r.m_mn * r.k_hap, "mm");
        r.ham2 = T.R("ham2", "Eq(133)", "Mean addendum, wheel", r.m_mn * (r.k_hap - r.x_hm1), "mm");
        r.hfm2 = T.R("hfm2", "Eq(134)", "Mean dedendum, wheel", r.m_mn * (r.k_hfp + r.x_hm1), "mm");
        r.ham1 = T.R("ham1", "Eq(135)", "Mean addendum, pinion", r.m_mn * (r.k_hap + r.x_hm1), "mm");
        r.hfm1 = T.R("hfm1", "Eq(136)", "Mean dedendum, pinion", r.m_mn * (r.k_hfp - r.x_hm1), "mm");
        r.c = T.R("c", "Eq(137)", "Clearance", r.m_mn * (r.k_hfp - r.k_hap), "mm");
        r.hm = T.R("hm", "Eq(138/139)", "Mean whole depth", r.m_mn * (r.k_hap + r.k_hfp), "mm");

        // ---- theta_a2 / theta_f2: manual, or Annex C.5 auto-derivation from the taper type ----
        double thetaA2 = In.theta_a2, thetaF2 = In.theta_f2;
        if (In.ThetaSource == ThetaA2Source.AutoFromTaper)
        {
            double alphaNForTaper = (r.alpha_nD + r.alpha_nC) / 2.0;
            (thetaA2, thetaF2) = AnnexC5.DeriveThetaA2ThetaF2(In.Taper, r.hfm1, r.hfm2, r.ham2, r.hmw, pc.Rm2, r.Re2, r.met2, alphaNForTaper, pc.beta_m2, In.rc0, T);
        }

        // ========================================================= 7.4 Root angles and face angles
        T.Section("7.4: Determination of root angles and face angles");
        r.theta_a2 = thetaA2;
        r.theta_f2 = thetaF2;
        r.delta_a2 = T.R("delta_a2", "Eq(140)", "Face angle, wheel", pc.delta2 + thetaA2, "deg");
        r.delta_f2 = T.R("delta_f2", "Eq(141)", "Root angle, wheel", pc.delta2 - thetaF2, "deg");
        r.phi_R = T.R("phi_R", "Eq(142)", "Auxiliary angle (root plane)", Atan(In.a * Tan(r.DeltaSigma) * Cos(r.delta_f2) / (pc.Rm2 * Cos(thetaF2) - r.tz2 * Cos(r.delta_f2))), "deg");
        r.phi_o = T.R("phi_o", "Eq(143)", "Auxiliary angle (face plane)", Atan(In.a * Tan(r.DeltaSigma) * Cos(r.delta_a2) / (pc.Rm2 * Cos(thetaA2) - r.tz2 * Cos(r.delta_a2))), "deg");
        r.zeta_R = T.R("zeta_R", "Eq(144)", "Pinion offset angle in root plane",
            Asin(In.a * Cos(r.phi_R) * Sin(r.delta_f2) / (pc.Rm2 * Cos(thetaF2) - r.tz2 * Cos(r.delta_f2))) - r.phi_R, "deg");
        r.zeta_o = T.R("zeta_o", "Eq(145)", "Pinion offset angle in face plane",
            Asin(In.a * Cos(r.phi_o) * Sin(r.delta_a2) / (pc.Rm2 * Cos(thetaA2) - r.tz2 * Cos(r.delta_a2))) - r.phi_o, "deg");
        r.delta_a1 = T.R("delta_a1", "Eq(146)", "Face angle, pinion", Asin(Sin(r.DeltaSigma) * Sin(r.delta_f2) + Cos(r.DeltaSigma) * Cos(r.delta_f2) * Cos(r.zeta_R)), "deg");
        r.delta_f1 = T.R("delta_f1", "Eq(147)", "Root angle, pinion", Asin(Sin(r.DeltaSigma) * Sin(r.delta_a2) + Cos(r.DeltaSigma) * Cos(r.delta_a2) * Cos(r.zeta_o)), "deg");
        r.theta_a1 = T.R("theta_a1", "Eq(148)", "Addendum angle, pinion", r.delta_a1 - pc.delta1, "deg");
        r.theta_f1 = T.R("theta_f1", "Eq(149)", "Dedendum angle, pinion", pc.delta1 - r.delta_f1, "deg");
        r.tzF2 = T.R("tzF2", "Eq(150)", "Wheel face apex beyond crossing point", r.tz2 - (pc.Rm2 * Sin(thetaA2) - r.ham2 * Cos(thetaA2)) / Sin(r.delta_a2), "mm");
        r.tzR2 = T.R("tzR2", "Eq(151)", "Wheel root apex beyond crossing point", r.tz2 + (pc.Rm2 * Sin(thetaF2) - r.hfm2 * Cos(thetaF2)) / Sin(r.delta_f2), "mm");
        r.tzF1 = T.R("tzF1", "Eq(152)", "Pinion face apex beyond crossing point", (In.a * Sin(r.zeta_R) * Cos(r.delta_f2) - r.tzR2 * Sin(r.delta_f2) - r.c) / Sin(r.delta_a1), "mm");
        r.tzR1 = T.R("tzR1", "Eq(153)", "Pinion root apex beyond crossing point", (In.a * Sin(r.zeta_o) * Cos(r.delta_a2) - r.tzF2 * Sin(r.delta_a2) - r.c) / Sin(r.delta_f1), "mm");

        // ========================================================= 7.5 Pinion face width, b1
        T.Section("7.5: Determination of pinion face width, b1");
        r.bp1 = T.R("bp1", "Eq(154)", "Pinion face width in pitch plane", Math.Sqrt(r.Re2 * r.Re2 - r.ap * r.ap) - Math.Sqrt(r.Ri2 * r.Ri2 - r.ap * r.ap), "mm");
        r.b1A = T.R("b1A", "Eq(155)", "Pinion face width, calc point to front crown", Math.Sqrt(pc.Rm2 * pc.Rm2 - r.ap * r.ap) - Math.Sqrt(r.Ri2 * r.Ri2 - r.ap * r.ap), "mm");

        switch (In.Method)
        {
            case PitchConeMethod.Method0:
                T.Note("Method 0:");
                r.b1 = T.R("b1", "Eq(156)", "Pinion face width", In.b2, "mm");
                r.be1 = T.R("be1", "Eq(157)", "Pinion face width, calc point to outside", pc.c_be2 * r.b1, "mm");
                r.bi1 = T.R("bi1", "Eq(158)", "Pinion face width, calc point to inside", r.b1 - r.be1, "mm");
                break;
            case PitchConeMethod.Method1:
                {
                    T.Note("Method 1:");
                    double lambdaP = T.R("lambda'", "Eq(159)", "Auxiliary angle",
                        Atan(Sin(r.zeta_mp) * Cos(pc.delta2) / (pc.u * Cos(pc.delta1) + Cos(pc.delta2) * Cos(r.zeta_mp))), "deg");
                    double bReri1 = T.R("b_reri1", "Eq(160)", "Pinion face width", In.b2 * Cos(lambdaP) / Cos(r.zeta_mp - lambdaP), "mm");
                    double dBx1 = T.R("Delta bx1", "Eq(161)", "Pinion face width increment along pinion axis", r.hmw * Sin(r.zeta_R) * (1.0 - 1.0 / pc.u), "mm");
                    double dGxe = T.R("Delta gxe", "Eq(162)", "Increment along pinion axis, calc point to outside",
                        (pc.c_be2 * bReri1 / Cos(r.theta_a1)) * Cos(r.delta_a1) + dBx1 - (r.hfm2 - r.c) * Sin(pc.delta1), "mm");
                    double dGxi = T.R("Delta gxi", "Eq(163)", "Increment along pinion axis, calc point to inside",
                        ((1.0 - pc.c_be2) * bReri1 / Cos(r.theta_a1)) * Cos(r.delta_a1) + dBx1 + (r.hfm2 - r.c) * Sin(pc.delta1), "mm");
                    r.be1 = T.R("be1", "Eq(164)", "Pinion face width, calc point to outside", (dGxe + r.ham1 * Sin(pc.delta1)) / Cos(r.delta_a1) * Cos(r.theta_a1), "mm");
                    r.bi1 = T.R("bi1", "Eq(165)", "Pinion face width, calc point to inside", (dGxi - r.ham1 * Sin(pc.delta1)) / (Cos(pc.delta1) - Tan(r.theta_a1) * Sin(pc.delta1)), "mm");
                    r.b1 = T.R("b1", "Eq(166)", "Pinion face width along pitch cone", r.bi1 + r.be1, "mm");
                    break;
                }
            case PitchConeMethod.Method2:
                T.Note("Method 2:");
                r.b1 = T.R("b1", "Eq(167)", "Pinion face width along pitch cone", In.b2 * (1.0 + Tan(r.zeta_mp) * Tan(r.zeta_mp)), "mm");
                r.be1 = T.R("be1", "Eq(168)", "Pinion face width, calc point to outside", pc.c_be2 * r.b1, "mm");
                r.bi1 = T.R("bi1", "Eq(169)", "Pinion face width, calc point to inside", r.b1 - r.be1, "mm");
                break;
            case PitchConeMethod.Method3:
            default:
                {
                    T.Note("Method 3:");
                    r.b1 = T.R("b1", "Eq(170)", "Pinion face width along pitch cone", Math.Floor(r.bp1 + 3.0 * r.m_mn * Math.Abs(Tan(r.zeta_mp)) + 1.0), "mm");
                    double bx = T.R("bx", "Eq(171)", "Additional pinion face width", (r.b1 - r.bp1) / 2.0, "mm");
                    r.bi1 = T.R("bi1", "Eq(172)", "Pinion face width, calc point to inside", r.b1A + bx, "mm");
                    r.be1 = T.R("be1", "Eq(173)", "Pinion face width, calc point to outside", r.b1 - r.bi1, "mm");
                    break;
                }
        }

        // ========================================================= 7.6 Inner and outer spiral angles
        T.Section("7.6.1: Determination of inner and outer spiral angles, pinion");
        r.Re21 = T.R("Re21", "Eq(174)", "Wheel cone distance of outer pinion boundary point", Math.Sqrt(pc.Rm2 * pc.Rm2 + r.be1 * r.be1 + 2.0 * pc.Rm2 * r.be1 * Cos(r.zeta_mp)), "mm");
        r.Ri21 = T.R("Ri21", "Eq(175)", "Wheel cone distance of inner pinion boundary point", Math.Sqrt(pc.Rm2 * pc.Rm2 + r.bi1 * r.bi1 - 2.0 * pc.Rm2 * r.bi1 * Cos(r.zeta_mp)), "mm");

        if (In.Process == ManufacturingProcess.FaceHobbing)
        {
            double nu = T.R("nu", "Eq(176)", "Lead angle of cutter", Asin(In.z0 * r.m_mn / (2.0 * In.rc0)), "deg");
            r.rho_P0 = T.R("rho_P0", "Eq(177)", "Crown gear to cutter centre distance", Math.Sqrt(pc.Rm2 * pc.Rm2 + In.rc0 * In.rc0 - 2.0 * pc.Rm2 * In.rc0 * Sin(pc.beta_m2 - nu)), "mm");
            r.rho_b = T.R("rho_b", "Eq(178)", "Epicycloid base circle radius", r.rho_P0 / (1.0 + ((double)In.z0 / In.z2) * Sin(pc.delta2)), "mm");
            double phiE21 = T.R("phi_e21", "Eq(179)", "Auxiliary angle", Acos((r.Re21 * r.Re21 + r.rho_P0 * r.rho_P0 - In.rc0 * In.rc0) / (2.0 * r.Re21 * r.rho_P0)), "deg");
            double phiI21 = T.R("phi_i21", "Eq(180)", "Auxiliary angle", Acos((r.Ri21 * r.Ri21 + r.rho_P0 * r.rho_P0 - In.rc0 * In.rc0) / (2.0 * r.Ri21 * r.rho_P0)), "deg");
            r.beta_e21 = T.R("beta_e21", "Eq(181)", "Wheel spiral angle at outer boundary point", Atan((r.Re21 - r.rho_b * Cos(phiE21)) / (r.rho_b * Sin(phiE21))), "deg");
            r.beta_i21 = T.R("beta_i21", "Eq(182)", "Wheel spiral angle at inner boundary point", Atan((r.Ri21 - r.rho_b * Cos(phiI21)) / (r.rho_b * Sin(phiI21))), "deg");
        }
        else
        {
            r.beta_e21 = T.R("beta_e21", "Eq(183)", "Wheel spiral angle at outer boundary point",
                Asin((2.0 * pc.Rm2 * In.rc0 * Sin(pc.beta_m2) - pc.Rm2 * pc.Rm2 + r.Re21 * r.Re21) / (2.0 * r.Re21 * In.rc0)), "deg");
            r.beta_i21 = T.R("beta_i21", "Eq(184)", "Wheel spiral angle at inner boundary point",
                Asin((2.0 * pc.Rm2 * In.rc0 * Sin(pc.beta_m2) - pc.Rm2 * pc.Rm2 + r.Ri21 * r.Ri21) / (2.0 * r.Ri21 * In.rc0)), "deg");
        }
        r.zeta_ep21 = T.R("zeta_ep21", "Eq(185)", "Pinion offset angle in pitch plane at outer boundary", Asin(r.ap / r.Re21), "deg");
        r.zeta_ip21 = T.R("zeta_ip21", "Eq(186)", "Pinion offset angle in pitch plane at inner boundary", Asin(r.ap / r.Ri21), "deg");
        r.beta_e1 = T.R("beta_e1", "Eq(187)", "Outer pinion spiral angle", r.beta_e21 + r.zeta_ep21, "deg");
        r.beta_i1 = T.R("beta_i1", "Eq(188)", "Inner pinion spiral angle", r.beta_i21 + r.zeta_ip21, "deg");

        T.Section("7.6.2: Determination of inner and outer spiral angles, wheel");
        if (In.Process == ManufacturingProcess.FaceHobbing)
        {
            double phiE2 = T.R("phi_e2", "Eq(189)", "Auxiliary angle", Acos((r.Re2 * r.Re2 + r.rho_P0 * r.rho_P0 - In.rc0 * In.rc0) / (2.0 * r.Re2 * r.rho_P0)), "deg");
            double phiI2 = T.R("phi_i2", "Eq(190)", "Auxiliary angle", Acos((r.Ri2 * r.Ri2 + r.rho_P0 * r.rho_P0 - In.rc0 * In.rc0) / (2.0 * r.Ri2 * r.rho_P0)), "deg");
            r.beta_e2 = T.R("beta_e2", "Eq(191)", "Outer wheel spiral angle", Atan((r.Re2 - r.rho_b * Cos(phiE2)) / (r.rho_b * Sin(phiE2))), "deg");
            r.beta_i2 = T.R("beta_i2", "Eq(192)", "Inner wheel spiral angle", Atan((r.Ri2 - r.rho_b * Cos(phiI2)) / (r.rho_b * Sin(phiI2))), "deg");
        }
        else
        {
            r.beta_e2 = T.R("beta_e2", "Eq(193)", "Outer wheel spiral angle",
                Asin((2.0 * pc.Rm2 * In.rc0 * Sin(pc.beta_m2) - pc.Rm2 * pc.Rm2 + r.Re2 * r.Re2) / (2.0 * r.Re2 * In.rc0)), "deg");
            r.beta_i2 = T.R("beta_i2", "Eq(194)", "Inner wheel spiral angle",
                Asin((2.0 * pc.Rm2 * In.rc0 * Sin(pc.beta_m2) - pc.Rm2 * pc.Rm2 + r.Ri2 * r.Ri2) / (2.0 * r.Ri2 * In.rc0)), "deg");
        }

        // ========================================================= 7.7 Tooth depth
        T.Section("7.7: Determination of tooth depth");
        r.hae1 = T.R("hae1", "Eq(195)", "Outer addendum, pinion", r.ham1 + r.be1 * Tan(r.theta_a1), "mm");
        r.hae2 = T.R("hae2", "Eq(195)", "Outer addendum, wheel", r.ham2 + r.be2 * Tan(thetaA2), "mm");
        r.hfe1 = T.R("hfe1", "Eq(196)", "Outer dedendum, pinion", r.hfm1 + r.be1 * Tan(r.theta_f1), "mm");
        r.hfe2 = T.R("hfe2", "Eq(196)", "Outer dedendum, wheel", r.hfm2 + r.be2 * Tan(thetaF2), "mm");
        r.he1 = T.R("he1", "Eq(197)", "Outer whole depth, pinion", r.hae1 + r.hfe1, "mm");
        r.he2 = T.R("he2", "Eq(197)", "Outer whole depth, wheel", r.hae2 + r.hfe2, "mm");
        r.hai1 = T.R("hai1", "Eq(198)", "Inner addendum, pinion", r.ham1 - r.bi1 * Tan(r.theta_a1), "mm");
        r.hai2 = T.R("hai2", "Eq(198)", "Inner addendum, wheel", r.ham2 - r.bi2 * Tan(thetaA2), "mm");
        r.hfi1 = T.R("hfi1", "Eq(199)", "Inner dedendum, pinion", r.hfm1 - r.bi1 * Tan(r.theta_f1), "mm");
        r.hfi2 = T.R("hfi2", "Eq(199)", "Inner dedendum, wheel", r.hfm2 - r.bi2 * Tan(thetaF2), "mm");
        r.hi1 = T.R("hi1", "Eq(200)", "Inner whole depth, pinion", r.hai1 + r.hfi1, "mm");
        r.hi2 = T.R("hi2", "Eq(200)", "Inner whole depth, wheel", r.hai2 + r.hfi2, "mm");

        // ========================================================= 7.8 Tooth thickness
        T.Section("7.8: Determination of tooth thickness");
        r.alpha_n = T.R("alpha_n", "Eq(201)", "Mean normal pressure angle", (r.alpha_nD + r.alpha_nC) / 2.0, "deg");

        double CosBeta2OverRe2 = pc.Rm2 * Cos(pc.beta_m2) / (r.Re2 * Cos(r.beta_e2));
        switch (In.Backlash)
        {
            case BacklashMode.OuterNormal_jen:
                r.x_sm1 = T.R("xsm1", "Eq(202)", "Thickness modification coefficient, pinion (outer normal backlash)",
                    r.x_smn - In.BacklashValue * (1.0 / (4.0 * r.m_mn * Cos(r.alpha_n))) * CosBeta2OverRe2, "");
                r.x_sm2 = T.R("xsm2", "Eq(207)", "Thickness modification coefficient, wheel (outer normal backlash)",
                    -r.x_smn - In.BacklashValue * (1.0 / (4.0 * r.m_mn * Cos(r.alpha_n))) * CosBeta2OverRe2, "");
                break;
            case BacklashMode.OuterTransverse_jet2:
                r.x_sm1 = T.R("xsm1", "Eq(203)", "Thickness modification coefficient, pinion (outer transverse backlash)",
                    r.x_smn - In.BacklashValue * pc.Rm2 * Cos(pc.beta_m2) / (4.0 * r.m_mn * r.Re2), "");
                r.x_sm2 = T.R("xsm2", "Eq(208)", "Thickness modification coefficient, wheel (outer transverse backlash)",
                    -r.x_smn - In.BacklashValue * pc.Rm2 * Cos(pc.beta_m2) / (4.0 * r.m_mn * r.Re2), "");
                break;
            case BacklashMode.MeanNormal_jmn:
                r.x_sm1 = T.R("xsm1", "Eq(204)", "Thickness modification coefficient, pinion (mean normal backlash)",
                    r.x_smn - In.BacklashValue * (1.0 / (4.0 * r.m_mn * Cos(r.alpha_n))), "");
                r.x_sm2 = T.R("xsm2", "Eq(209)", "Thickness modification coefficient, wheel (mean normal backlash)",
                    -r.x_smn - In.BacklashValue * (1.0 / (4.0 * r.m_mn * Cos(r.alpha_n))), "");
                break;
            case BacklashMode.MeanTransverse_jmt2:
                r.x_sm1 = T.R("xsm1", "Eq(205)", "Thickness modification coefficient, pinion (mean transverse backlash)",
                    r.x_smn - In.BacklashValue * Cos(pc.beta_m2) / (4.0 * r.m_mn), "");
                r.x_sm2 = T.R("xsm2", "Eq(210)", "Thickness modification coefficient, wheel (mean transverse backlash)",
                    -r.x_smn - In.BacklashValue * Cos(pc.beta_m2) / (4.0 * r.m_mn), "");
                break;
            default: // TheoreticalNoBacklash (A.4: backlash set to zero for the theoretical tooth thickness)
                r.x_sm1 = T.R("xsm1", "", "Thickness modification coefficient, pinion (theoretical, no backlash)", r.x_smn, "");
                r.x_sm2 = T.R("xsm2", "", "Thickness modification coefficient, wheel (theoretical, no backlash)", -r.x_smn, "");
                break;
        }

        r.s_mn1 = T.R("smn1", "Eq(206)", "Mean normal circular tooth thickness, pinion", 0.5 * r.m_mn * Math.PI + 2.0 * r.m_mn * (r.x_sm1 + r.x_hm1 * Tan(r.alpha_n)), "mm");
        r.s_mn2 = T.R("smn2", "Eq(211)", "Mean normal circular tooth thickness, wheel", 0.5 * r.m_mn * Math.PI + 2.0 * r.m_mn * (r.x_sm2 - r.x_hm1 * Tan(r.alpha_n)), "mm");
        r.s_mt1 = T.R("smt1", "Eq(212)", "Mean transverse circular thickness, pinion", r.s_mn1 / Cos(pc.beta_m1), "mm");
        r.s_mt2 = T.R("smt2", "Eq(212)", "Mean transverse circular thickness, wheel", r.s_mn2 / Cos(pc.beta_m2), "mm");
        r.d_mn1 = T.R("dmn1", "Eq(213)", "Mean normal diameter, pinion", r.dm1 / ((1.0 - Sin(pc.beta_m1) * Sin(pc.beta_m1) * Cos(r.alpha_n) * Cos(r.alpha_n)) * Cos(pc.delta1)), "mm");
        r.d_mn2 = T.R("dmn2", "Eq(213)", "Mean normal diameter, wheel", r.dm2 / ((1.0 - Sin(pc.beta_m2) * Sin(pc.beta_m2) * Cos(r.alpha_n) * Cos(r.alpha_n)) * Cos(pc.delta2)), "mm");
        // NOTE: s_mn/d_mn is a radian angle (arc length / diameter), so the sin/cos below take
        // it directly (Math.Sin/Cos), not this file's degree-based Sin()/Cos() helpers.
        r.s_mnc1 = T.R("smnc1", "Eq(214)", "Mean normal chordal tooth thickness, pinion", r.d_mn1 * Math.Sin(r.s_mn1 / r.d_mn1), "mm");
        r.s_mnc2 = T.R("smnc2", "Eq(214)", "Mean normal chordal tooth thickness, wheel", r.d_mn2 * Math.Sin(r.s_mn2 / r.d_mn2), "mm");
        r.h_amc1 = T.R("hamc1", "Eq(215)", "Mean chordal addendum, pinion", r.ham1 + 0.5 * r.d_mn1 * Cos(pc.delta1) * (1.0 - Math.Cos(r.s_mn1 / r.d_mn1)), "mm");
        r.h_amc2 = T.R("hamc2", "Eq(215)", "Mean chordal addendum, wheel", r.ham2 + 0.5 * r.d_mn2 * Cos(pc.delta2) * (1.0 - Math.Cos(r.s_mn2 / r.d_mn2)), "mm");

        // ========================================================= 7.9 Remaining dimensions
        T.Section("7.9: Determination of remaining dimensions");
        r.Re1 = T.R("Re1", "Eq(216)", "Outer pitch cone distance, pinion", pc.Rm1 + r.be1, "mm");
        r.Ri1 = T.R("Ri1", "Eq(217)", "Inner pitch cone distance, pinion", pc.Rm1 - r.bi1, "mm");
        r.de1 = T.R("de1", "Eq(218)", "Outer pitch diameter, pinion", 2.0 * r.Re1 * Sin(pc.delta1), "mm");
        r.di1 = T.R("di1", "Eq(219)", "Inner pitch diameter, pinion", 2.0 * r.Ri1 * Sin(pc.delta1), "mm");
        r.dae1 = T.R("dae1", "Eq(220)", "Outside diameter, pinion", r.de1 + 2.0 * r.hae1 * Cos(pc.delta1), "mm");
        r.dae2 = T.R("dae2", "Eq(220)", "Outside diameter, wheel", r.de2 + 2.0 * r.hae2 * Cos(pc.delta2), "mm");
        r.dfe1 = T.R("dfe1", "Eq(221)", "Diameter, pinion", r.de1 - 2.0 * r.hfe1 * Cos(pc.delta1), "mm");
        r.dfe2 = T.R("dfe2", "Eq(221)", "Diameter, wheel", r.de2 - 2.0 * r.hfe2 * Cos(pc.delta2), "mm");
        r.dai1 = T.R("dai1", "Eq(222)", "Diameter, pinion", r.di1 + 2.0 * r.hai1 * Cos(pc.delta1), "mm");
        r.dai2 = T.R("dai2", "Eq(222)", "Diameter, wheel", r.di2 + 2.0 * r.hai2 * Cos(pc.delta2), "mm");
        r.dfi1 = T.R("dfi1", "Eq(223)", "Diameter, pinion", r.di1 - 2.0 * r.hfi1 * Cos(pc.delta1), "mm");
        r.dfi2 = T.R("dfi2", "Eq(223)", "Diameter, wheel", r.di2 - 2.0 * r.hfi2 * Cos(pc.delta2), "mm");
        r.txo1 = T.R("txo1", "Eq(224)", "Crossing point to crown along axis, pinion", r.tzm1 + r.be1 * Cos(pc.delta1) - r.hae1 * Sin(pc.delta1), "mm");
        r.txo2 = T.R("txo2", "Eq(224)", "Crossing point to crown along axis, wheel", r.tzm2 + r.be2 * Cos(pc.delta2) - r.hae2 * Sin(pc.delta2), "mm");
        r.txi1 = T.R("txi1", "Eq(225)", "Crossing point to front crown along axis, pinion", r.tzm1 - r.bi1 * Cos(pc.delta1) - r.hai1 * Sin(pc.delta1), "mm");
        r.txi2 = T.R("txi2", "Eq(225)", "Crossing point to front crown along axis, wheel", r.tzm2 - r.bi2 * Cos(pc.delta2) - r.hai2 * Sin(pc.delta2), "mm");
        r.ht1 = T.R("ht1", "Eq(226)", "Pinion whole depth, perpendicular to root cone",
            (r.tzF1 + r.txo1) / Cos(r.delta_a1) * Sin(r.theta_a1 + r.theta_f1) - (r.tzR1 - r.tzF1) * Sin(r.delta_f1), "mm");

        return r;
    }
}
