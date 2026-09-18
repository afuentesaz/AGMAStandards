using AGMA2003D19Calc.Models;
using static AGMA2003D19Calc.Calc.MathUtil;

namespace AGMA2003D19Calc.Calc;

public readonly record struct GeometryFactorResult(double ZI, double YJ1, double YJ2);

/// <summary>
/// Implements ANSI/AGMA 2003-D19, Annex C(M) (SI units/symbols) in full: the pitting
/// resistance geometry factor Z_I (Eq C.1M-C.42M) and the bending strength geometry factors
/// Y_J1, Y_J2 (Eq C.43M-C.116M). Equation numbers in comments refer to Annex C(M).
/// </summary>
public class GeometryFactors
{
    private readonly AgmaInputs In;
    private readonly CalcTrace T;
    private readonly bool _spiral;

    public GeometryFactors(AgmaInputs inputs, CalcTrace trace)
    {
        In = inputs;
        T = trace;
        _spiral = inputs.GearType == GearType.SpiralBevel;
    }

    public GeometryFactorResult Calculate()
    {
        T.Section("Annex C(M).1.3.2 Initial formulas (common to Zi and YJ)");

        double Rm = T.R("Rm", "C.2M", "Mean cone distance", In.Re - 0.5 * In.b);
        double theta_a1 = T.R("theta_a1", "C.3M", "Pinion addendum angle", In.Delta_a1 - In.Delta1, "deg");
        double theta_a2 = T.R("theta_a2", "C.4M", "Gear addendum angle", In.Delta_a2 - In.Delta2, "deg");
        double h_am1 = T.R("h_am1", "C.5M", "Mean pinion addendum", In.h_ae1 - 0.5 * In.b * Tan(theta_a1));
        double h_am2 = T.R("h_am2", "C.6M", "Mean gear addendum", In.h_ae2 - 0.5 * In.b * Tan(theta_a2));
        double kPrime = T.R("k'", "C.7M", "Location constant", (double)(In.z2 - In.z1) / (3.2 * In.z2 + 4.0 * In.z1), "");
        double m_mt = T.R("m_mt", "C.8M", "Mean transverse module", (Rm / In.Re) * In.m_et);
        double p_e = T.R("p_e", "C.9M", "Outer transverse circular pitch", Math.PI * In.m_et);
        double p_mbn = T.R("p_mbn", "C.10M", "Mean normal base pitch", (Rm / In.Re) * p_e * Cos(In.Beta_m) * Cos(In.Alpha_n));
        double p_mn = T.R("p_mn", "C.11M", "Mean normal circular pitch", p_mbn / Cos(In.Alpha_n));
        double p2 = T.R("p2", "C.12M", "Auxiliary pitch", p_mn / (Cos(In.Alpha_n) * (Math.Pow(Cos(In.Beta_m), 2) + Math.Pow(Tan(In.Alpha_n), 2))));

        double r_mpt1 = T.R("r_mpt1", "C.13M", "Mean transverse pinion pitch radius", In.de1 / (2 * Cos(In.Delta1)) * (Rm / In.Re));
        double r_mpt2 = T.R("r_mpt2", "C.14M", "Mean transverse gear pitch radius", In.de2 / (2 * Cos(In.Delta2)) * (Rm / In.Re));
        double r_mpn1 = T.R("r_mpn1", "C.15M", "Mean normal pinion pitch radius", r_mpt1 / Math.Pow(Cos(In.Beta_m), 2));
        double r_mpn2 = T.R("r_mpn2", "C.16M", "Mean normal gear pitch radius", r_mpt2 / Math.Pow(Cos(In.Beta_m), 2));
        double r_mbn1 = T.R("r_mbn1", "C.17M", "Mean normal pinion base radius", r_mpn1 * Cos(In.Alpha_n));
        double r_mbn2 = T.R("r_mbn2", "C.18M", "Mean normal gear base radius", r_mpn2 * Cos(In.Alpha_n));
        double r_mne1 = T.R("r_mne1", "C.19M", "Mean normal pinion outside radius", r_mpn1 + h_am1);
        double r_mne2 = T.R("r_mne2", "C.20M", "Mean normal gear outside radius", r_mpn2 + h_am2);

        double g_a1 = T.R("g_alpha1", "C.21M", "Length of mean normal pinion addendum action", Math.Sqrt(Math.Max(0, r_mne1 * r_mne1 - r_mbn1 * r_mbn1)) - r_mpn1 * Sin(In.Alpha_n));
        double g_a2 = T.R("g_alpha2", "C.22M", "Length of mean normal gear addendum action", Math.Sqrt(Math.Max(0, r_mne2 * r_mne2 - r_mbn2 * r_mbn2)) - r_mpn2 * Sin(In.Alpha_n));
        double g_an = T.R("g_alphan", "C.23M", "Length of action in mean normal section", g_a1 + g_a2);

        double eps_alpha = T.R("eps_alpha", "C.24M", "Transverse contact ratio", g_an / p2, "");
        if (!_spiral && eps_alpha <= 1.0)
            T.Note("WARNING: transverse contact ratio <= 1.0 - formulas are invalid for straight bevel/Zerol bevel gears per the note under Eq C.24M.");

        double Kz = T.R("Kz", "C.25M", "Intermediate variable", (In.b / In.Re) * ((2 - In.b / In.Re) / (2 * (1 - In.b / In.Re))), "");
        double eps_beta = _spiral
            ? T.R("eps_beta", "C.26M", "Face contact ratio", (1.0 / (Math.PI * In.m_et)) * (Kz * Tan(In.Beta_m) - Math.Pow(Kz, 3) / 3.0 * Math.Pow(Tan(In.Beta_m), 3)) * In.Re, "")
            : T.R("eps_beta", "C.26M", "Face contact ratio (0 for straight/Zerol bevel)", 0.0, "");

        double eps_o = T.R("eps_o", "C.27M", "Modified contact ratio", Math.Sqrt(eps_alpha * eps_alpha + eps_beta * eps_beta), "");
        double cosBetaMb = Cos(In.Alpha_n) * Math.Sqrt(Math.Pow(Cos(In.Beta_m), 2) + Math.Pow(Tan(In.Alpha_n), 2));
        double beta_mb = T.R("beta_mb", "C.28M", "Mean base spiral angle", Acos(cosBetaMb), "deg");

        double rho_m1 = T.R("rho_m1", "C.30M", "Mean normal pinion profile radius of curvature at pitch circle", r_mpt1 * Sin(In.Alpha_n) / Math.Pow(Cos(beta_mb), 2));
        double rho_m2 = T.R("rho_m2", "C.31M", "Mean normal gear profile radius of curvature at pitch circle", r_mpt2 * Sin(In.Alpha_n) / Math.Pow(Cos(beta_mb), 2));

        bool lowContactRatio = eps_o < 2.0;
        double Zi = In.StaticallyLoaded ? 1.0 : (lowContactRatio ? 2.0 / eps_o : 1.0);
        Zi = T.R("Zi", "C.40M", "Inertia factor (Zi = Yi)", Zi, "");

        // ===================================================== Z_I (pitting) - Eq C.29M-C.42M
        T.Section("Annex C(M).1.3.3-1.3.6 Pitting resistance geometry factor, Zi");

        double gEtaSq = g_an * g_an * Math.Pow(Cos(beta_mb), 4) + In.b * In.b * Math.Pow(Sin(beta_mb), 2);
        double g_eta = Math.Sqrt(gEtaSq);
        T.R("g_eta", "C.29M", "Length of action within the contact ellipse", g_eta);

        double yI;
        if (!_spiral)
        {
            yI = T.R("yI", "C.32M", "Critical point location (straight/Zerol bevel)", g_an / 2.0 - p_mbn);
        }
        else
        {
            double ZIat(double y)
            {
                double gEtaI2Trial = gEtaSq - 4 * y * y;
                if (gEtaI2Trial < 0) return double.PositiveInfinity;
                double gEtaITrial = Math.Sqrt(gEtaI2Trial);
                double gyoTrial = g_an / 2.0 + (g_an * g_an * y * Math.Pow(Cos(beta_mb), 2)) / gEtaSq
                                  + (In.b * g_an * gEtaITrial * kPrime * Sin(beta_mb)) / gEtaSq - g_a2;
                double rho1Trial = rho_m1 + gyoTrial;
                double rho2Trial = rho_m2 - gyoTrial;
                if (rho1Trial <= 0 || rho2Trial <= 0) return double.PositiveInfinity;
                double rhoYoTrial = rho1Trial * rho2Trial / (rho1Trial + rho2Trial);
                double gcTrial = In.b * g_an * gEtaITrial * cosBetaMb / gEtaSq;
                double epsNITrial = LoadSharingRatio(gEtaITrial, p_mbn, y);
                if (epsNITrial <= 0) return double.PositiveInfinity;
                return (gcTrial * rhoYoTrial * Cos(In.Beta_m) * Cos(In.Alpha_n) / (In.b * In.de1 * Zi * epsNITrial)) * (m_mt / In.m_et);
            }
            yI = MinimizeStepHalving(ZIat, 0.0, -g_eta / 10.0, g_eta / 80.0, 0.001);
            T.R("yI", "C.33M", "Critical point location (spiral bevel, converged)", yI);
        }

        double g_etaI2 = gEtaSq - 4 * yI * yI;
        double g_etaI = Math.Sqrt(Math.Max(0, g_etaI2));
        T.R("g_etaI", "C.34M", "Length of action within contact ellipse at critical point", g_etaI);

        double g_yo = g_an / 2.0 + (g_an * g_an * yI * Math.Pow(Cos(beta_mb), 2)) / gEtaSq
                      + (In.b * g_an * g_etaI * kPrime * Sin(beta_mb)) / gEtaSq - g_a2;
        T.R("g_yo", "C.35M", "Distance along path of action from pitch line to critical point", g_yo);

        double rho_1 = T.R("rho_1", "C.36M", "Pinion profile radius of curvature at critical point", rho_m1 + g_yo);
        double rho_2 = T.R("rho_2", "C.37M", "Gear profile radius of curvature at critical point", rho_m2 - g_yo);
        double rho_yo = T.R("rho_yo", "C.38M", "Relative radius of profile curvature", rho_1 * rho_2 / (rho_1 + rho_2));
        double g_c = T.R("g_c", "C.39M", "Length of line of contact", In.b * g_an * g_etaI * cosBetaMb / gEtaSq);

        double epsNI = LoadSharingRatio(g_etaI, p_mbn, yI);
        T.R("eps_NI", "C.42M", "Load sharing ratio, pitting", epsNI, "");

        double ZI = T.R("ZI", "C.1M", "PITTING RESISTANCE GEOMETRY FACTOR", (g_c * rho_yo * Cos(In.Beta_m) * Cos(In.Alpha_n) / (In.b * In.de1 * Zi * epsNI)) * (m_mt / In.m_et), "");

        // ===================================================== Y_J (bending) - Eq C.43M-C.116M
        T.Section("Annex C(M).2.4.2-2.4.4 Bending: point of load application");
        double h_fm1 = T.R("h_fm1", "C.45M", "Mean pinion dedendum", In.h_fe1 - 0.5 * In.b * Tan(In.Theta_f1));
        double h_fm2 = T.R("h_fm2", "C.46M", "Mean gear dedendum", In.h_fe2 - 0.5 * In.b * Tan(In.Theta_f2));

        double yJ = eps_o <= 2.0
            ? T.R("yJ", "C.47M", "Point of load application param (modified contact ratio <= 2.0)", p_mbn - g_eta / 2.0)
            : T.R("yJ", "C.48M", "Point of load application param (modified contact ratio > 2.0)", 0.0);

        double g_etaJ2 = gEtaSq - 4 * yJ * yJ;
        double g_etaJ = Math.Sqrt(Math.Max(0, g_etaJ2));
        T.R("g_etaJ", "C.50M", "Length of action within contact ellipse at load point", g_etaJ);

        double y3;
        double gOPrimePrime;
        if (!_spiral)
        {
            y3 = T.R("y3", "C.51M", "Point of load application on path of action", g_an / 2.0 + (g_an * g_an * yJ) / gEtaSq);
            gOPrimePrime = T.R("g_o\"", "C.54M", "Lengthwise location of point of load application", In.b * g_an * g_etaJ * kPrime / gEtaSq);
        }
        else if (In.ToothSide == ToothSide.Concave)
        {
            y3 = T.R("y3", "C.52M", "Point of load application on path of action (concave)", g_an / 2.0 + (g_an * g_an * yJ * Math.Pow(Cos(beta_mb), 2) + In.b * g_an * g_etaJ * kPrime * Sin(beta_mb)) / gEtaSq);
            gOPrimePrime = T.R("g_o\"", "C.55M", "Lengthwise location of point of load application (concave)", (In.b * g_an * g_etaJ * kPrime * Math.Pow(Cos(beta_mb), 2) - g_an * g_an * yJ * Sin(beta_mb)) / gEtaSq);
        }
        else
        {
            y3 = T.R("y3", "C.53M", "Point of load application on path of action (convex)", g_an / 2.0 + (g_an * g_an * yJ * Math.Pow(Cos(beta_mb), 2) - In.b * g_an * g_etaJ * kPrime * Sin(beta_mb)) / gEtaSq);
            gOPrimePrime = T.R("g_o\"", "C.56M", "Lengthwise location of point of load application (convex)", (In.b * g_an * g_etaJ * kPrime * Math.Pow(Cos(beta_mb), 2) + g_an * g_an * yJ * Sin(beta_mb)) / gEtaSq);
        }

        double sumR = T.R("Sum(r_mpn)", "C.57M", "Sum of gear and pinion mean normal pitch radii", r_mpn1 + r_mpn2);

        double alpha_L1 = T.R("alpha_L1", "C.58M", "Normal pressure angle at load point, pinion", Atan((y3 + sumR * Sin(In.Alpha_n) - Math.Sqrt(Math.Max(0, r_mne2 * r_mne2 - r_mbn2 * r_mbn2))) / r_mbn1), "deg");
        double alpha_L2 = T.R("alpha_L2", "C.59M", "Normal pressure angle at load point, gear", Atan((y3 + sumR * Sin(In.Alpha_n) - Math.Sqrt(Math.Max(0, r_mne1 * r_mne1 - r_mbn1 * r_mbn1))) / r_mbn2), "deg");

        double zeta_h1 = 57.2958 * (0.5 * In.s1 / r_mpn1 - Inv(alpha_L1) + Inv(In.Alpha_n));
        T.R("zeta_h1", "C.60M", "Rotation angle used in bending calc, pinion", zeta_h1, "deg");
        double zeta_h2 = 57.2958 * (0.5 * In.s2 / r_mpn2 - Inv(alpha_L2) + Inv(In.Alpha_n));
        T.R("zeta_h2", "C.61M", "Rotation angle used in bending calc, gear", zeta_h2, "deg");

        double alpha_h1 = T.R("alpha_h1", "C.62M", "Normal pressure angle at load point on tooth centerline, pinion", alpha_L1 - zeta_h1, "deg");
        double alpha_h2 = T.R("alpha_h2", "C.63M", "Normal pressure angle at load point on tooth centerline, gear", alpha_L2 - zeta_h2, "deg");

        double dR_yo1 = T.R("dr_yo1", "C.64M", "Distance from pitch circle to load point, pinion", r_mbn1 / Cos(alpha_h1) - r_mpn1);
        double dR_yo2 = T.R("dr_yo2", "C.65M", "Distance from pitch circle to load point, gear", r_mbn2 / Cos(alpha_h2) - r_mpn2);

        double r_myo1 = T.R("r_myo1", "C.66M", "Mean transverse radius of pinion to point of load application", r_mpt1 * ((Rm + gOPrimePrime) / Rm) + dR_yo1);
        double r_myo2 = T.R("r_myo2", "C.67M", "Mean transverse radius of gear to point of load application", r_mpt2 * ((Rm + gOPrimePrime) / Rm) + dR_yo2);

        double r_mf1 = T.R("r_mf1", "C.68M", "Tooth fillet radius at root of pinion tooth", Math.Pow(h_fm1 - In.Rho_ao1, 2) / (r_mpn1 + h_fm1 - In.Rho_ao1) + In.Rho_ao1);
        double r_mf2 = T.R("r_mf2", "C.69M", "Tooth fillet radius at root of gear tooth", Math.Pow(h_fm2 - In.Rho_ao2, 2) / (r_mpn2 + h_fm2 - In.Rho_ao2) + In.Rho_ao2);

        T.Section("Annex C(M).2.4.6 Tooth form factor, Y (trial solution)");
        var pinionForm = ToothForm(r_mpn1, In.s1, h_fm1, In.Rho_ao1, alpha_h1);
        var gearForm = ToothForm(r_mpn2, In.s2, h_fm2, In.Rho_ao2, alpha_h2);
        T.R("X_N1", "C.81M", "Pinion tooth strength factor", pinionForm.XN);
        T.R("Y1", "C.82M", "Pinion tooth form factor (excl. stress concentration)", pinionForm.Y);
        T.R("X_N2", "C.94M", "Gear tooth strength factor", gearForm.XN);
        T.R("Y2", "C.95M", "Gear tooth form factor (excl. stress concentration)", gearForm.Y);

        T.Section("Annex C(M).2.4.7-2.4.8 Stress concentration/correction, tooth form factor YK");
        double H = 0.3254545 - 0.0072727 * In.Alpha_n;
        double L = 0.3318182 - 0.0090909 * In.Alpha_n;
        double M = 0.2681818 + 0.0090909 * In.Alpha_n;
        T.R("H", "C.98M", "Empirical constant", H, "");
        T.R("L", "C.99M", "Empirical exponent", L, "");
        T.R("M", "C.100M", "Empirical exponent", M, "");

        double Yf1 = H + Math.Pow(2 * pinionForm.sN / r_mf1, L) * Math.Pow(2 * pinionForm.sN / pinionForm.hN, M);
        double Yf2 = H + Math.Pow(2 * gearForm.sN / r_mf2, L) * Math.Pow(2 * gearForm.sN / gearForm.hN, M);
        T.R("Yf1", "C.96M", "Stress concentration/correction factor, pinion", Yf1, "");
        T.R("Yf2", "C.97M", "Stress concentration/correction factor, gear", Yf2, "");

        double YK1 = T.R("YK1", "C.101M", "Tooth form factor incl. stress concentration, pinion", pinionForm.Y / Yf1, "");
        double YK2 = T.R("YK2", "C.102M", "Tooth form factor incl. stress concentration, gear", gearForm.Y / Yf2, "");

        T.Section("Annex C(M).2.4.9-2.4.11 Load sharing, inertia and effective face width");
        double epsNJ = (!_spiral && eps_o <= 2.0)
            ? 1.0
            : LoadSharingRatio(g_etaJ, p_mbn, yJ);
        T.R("eps_NJ", "C.104M", "Load sharing ratio, bending", epsNJ, "");

        double Yi = Zi; // C.105M: Yi = Zi
        T.R("Yi", "C.105M", "Inertia factor for bending", Yi, "");

        double gK = T.R("gK", "C.106M", "Projected length of instantaneous line of contact", In.b * g_an * g_etaJ * Math.Pow(Cos(beta_mb), 2) / gEtaSq);

        double bPrime1 = EffectiveFaceWidth(In.b, gK, gOPrimePrime, pinionForm.hN);
        double bPrime2 = EffectiveFaceWidth(In.b, gK, gOPrimePrime, gearForm.hN);
        T.R("b'1", "C.111M", "Effective face width, pinion", bPrime1);
        T.R("b'2", "C.116M", "Effective face width, gear", bPrime2);

        double YJ1 = T.R("YJ1", "C.43M", "PINION BENDING STRENGTH GEOMETRY FACTOR", (YK1 / (epsNJ * Yi)) * (r_myo1 / r_mpt1) * (bPrime1 / In.b) * (m_mt / In.m_et), "");
        double YJ2 = T.R("YJ2", "C.44M", "GEAR BENDING STRENGTH GEOMETRY FACTOR", (YK2 / (epsNJ * Yi)) * (r_myo2 / r_mpt2) * (bPrime2 / In.b) * (m_mt / In.m_et), "");

        return new GeometryFactorResult(ZI, YJ1, YJ2);
    }

    /// <summary>
    /// Eq C.41M/C.103M: the "extra path of contact" cubic-radical summation used by both the
    /// pitting (eps_NI) and bending (eps_NJ) load-sharing ratios. Only real (non-negative
    /// radicand) terms are included, summing k = 1, 2, ... until the radicand goes negative in
    /// each of the two directions (the standard notes "for most designs a and b (x and y) will
    /// not be greater than 2").
    /// </summary>
    private static double LoadSharingRatio(double gEta, double pMbn, double y)
    {
        double gEtaCubed = Math.Pow(gEta, 3);
        double sum = gEtaCubed;
        for (int k = 1; k <= 6; k++)
        {
            double term = gEta * gEta - 4 * k * pMbn * (k * pMbn + 2 * y);
            if (term > 0) sum += Math.Pow(term, 1.5); else break;
        }
        for (int k = 1; k <= 6; k++)
        {
            double term = gEta * gEta - 4 * k * pMbn * (k * pMbn - 2 * y);
            if (term > 0) sum += Math.Pow(term, 1.5); else break;
        }
        return gEtaCubed / sum;
    }

    /// <summary>
    /// Eq C.70M-C.82M/C.83M-C.95M: the tooth-form-factor trial solution for one member.
    /// Iterates the assumed distance g_fo until s_N*cot(v)/h_N = 2.0 to within 0.001, using
    /// the standard's own secant recipe (Eq C.80M/C.93M: second trial changes g_fo by
    /// 0.005 m_et, third and later trials interpolate), then returns Y, s_N, h_N and X_N.
    /// </summary>
    private (double Y, double XN, double sN, double hN) ToothForm(double rMpn, double s, double hFm, double rhoAo, double alphaH)
    {
        double gYb = hFm - rhoAo;
        double gO = 0.5 * s + hFm * Tan(In.Alpha_n) + rhoAo * (Sec(In.Alpha_n) - Tan(In.Alpha_n));

        (double sN, double hN, double v) Eval(double gFo)
        {
            double zeta = 57.2958 * gFo / rMpn;
            double gX = gFo - gO;
            double gZa = gYb * Cos(zeta) - gX * Sin(zeta);
            double gZb = gYb * Sin(zeta) + gX * Cos(zeta);
            double v = Atan(gZa / gZb);
            double sN = rMpn * Sin(zeta) - rhoAo * Cos(v);
            double hN = rMpn * (1 - Cos(zeta)) + rhoAo * Sin(v) + gZa;
            return (sN, hN, v);
        }

        double F(double gFo)
        {
            var (sN, hN, v) = Eval(gFo);
            return sN * Cot(v) / hN - 2.0;
        }

        double gFo1 = gO + gYb;
        double gFoConverged = SecantSolve(F, gFo1, 0.005 * In.m_et, 0.001);
        var final = Eval(gFoConverged);

        double xN = final.sN * final.sN / final.hN;
        double Y = (2.0 / 3.0) / (In.m_et * (1.0 / xN - Tan(alphaH) / (3.0 * final.sN)));
        return (Y, xN, final.sN, final.hN);
    }

    /// <summary>Eq C.107M-C.115M: builds the effective face width from the projected line of
    /// contact and toe/heel increments, then Eq C.111M/C.116M. NOTE: the tan^-1(...) terms here
    /// are used directly as radian angle measures (not re-entered into another trig function),
    /// so this uses Math.Atan (radians) rather than the degrees-returning MathUtil.Atan.</summary>
    private double EffectiveFaceWidth(double bMember, double gK, double gOPrimePrime, double hN)
    {
        double cosBm = Cos(In.Beta_m);
        double dbToePrime = (bMember - gK) / (2 * cosBm) + gOPrimePrime / cosBm;
        double dbHeelPrime = (bMember - gK) / (2 * cosBm) - gOPrimePrime / cosBm;

        double dbToeFinal, dbHeelFinal;
        if (dbToePrime >= 0 && dbHeelPrime >= 0) { dbToeFinal = dbToePrime; dbHeelFinal = dbHeelPrime; }
        else if (dbToePrime >= 0 && dbHeelPrime < 0) { dbToeFinal = (bMember - gK) / cosBm; dbHeelFinal = 0; }
        else if (dbToePrime < 0 && dbHeelPrime >= 0) { dbToeFinal = 0; dbHeelFinal = (bMember - gK) / cosBm; }
        else { dbToeFinal = 0; dbHeelFinal = 0; }

        return hN * cosBm * (Math.Atan(dbToeFinal / hN) + Math.Atan(dbHeelFinal / hN)) + gK;
    }
}
