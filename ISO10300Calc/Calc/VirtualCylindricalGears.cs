using ISO10300Calc.Models;
using static ISO10300Calc.Calc.MathUtil;

namespace ISO10300Calc.Calc;

/// <summary>
/// Results of ISO 10300-1:2023, Annex A ("Calculation of virtual cylindrical gears - Method
/// B1"), covering both non-offset bevel gears (hypoid offset a = 0) and hypoid gears (a != 0).
/// Only the loaded (drive) flank is evaluated, matching both worked examples in ISO/TR
/// 10300-30:2024 (Sample 1, bevel; Sample 2, hypoid), which likewise rate the drive flank only.
/// </summary>
public class VirtualGearResult
{
    public double u, dv1, dv2, av, betav, alphaVet, mvt, zv1, zv2, uv;
    public double betaVb, dva1, dva2, dvf1, dvf2, dvb1, dvb2, pvet, gvAlpha;
    public double epsVAlpha, bvEff, bv, epsVBeta, epsVGamma;
    public double rhoT, betaB, rhoRel, gamma;
    public double zvn1, zvn2, dvn1, dvn2, dvan1, dvan2, dvbn1, dvbn2, epsVAlphaN;
    public double fmax, lbm, ft, fm, fr;
}

public static class VirtualCylindricalGears
{
    /// <summary>Length of the contact line at a signed distance f from the centre of the zone
    /// of action (ISO 10300-1:2023, A.2.7, Formulae A.26 to A.36). gammaDeg is the auxiliary
    /// angle for length-of-contact-line calculation (0 identically for a = 0 - 10300-1,
    /// A.2.4/A.2.7 - nonzero for hypoid gears). Also returns fmax so callers computing several
    /// contact lines only need to evaluate it once.</summary>
    public static double ContactLineLength(double f, double betaVbDeg, double gammaDeg, double gvAlpha, double bvEff, double bv, double fmax)
    {
        double betaVb = ToRad(betaVbDeg);
        double x1, x2, y1, y2;
        if (betaVbDeg <= 1e-9)
        {
            // Straight/Zerol bevel (helix angle of virtual gear at base circle = 0): 10300-1, A.27/A.28.
            x1 = 0.0;
            x2 = bvEff;
            y1 = f;
            y2 = f;
        }
        else
        {
            double tanBvb = Math.Tan(betaVb);
            double cosBvb = Math.Cos(betaVb);
            double sinBvb = Math.Sin(betaVb);
            double tanGamma = Math.Tan(ToRad(gammaDeg));
            double bracket = f * sinBvb + bvEff / 2.0;
            double denom = tanGamma + tanBvb;
            x1 = (f * cosBvb + tanBvb * bracket + 0.5 * (gvAlpha + bvEff * tanGamma)) / denom;
            x2 = (f * cosBvb + tanBvb * bracket - 0.5 * (gvAlpha - bvEff * tanGamma)) / denom;
            y1 = -x1 * tanBvb + f * cosBvb + tanBvb * bracket;
            y2 = -x2 * tanBvb + f * cosBvb + tanBvb * bracket;
        }

        double lb0 = Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        double fRatio2 = fmax > 0 ? (f / fmax) * (f / fmax) : 0.0;
        double k = 1.0 - Math.Sqrt(bvEff / bv);
        // KNOWN LIMITATION: this form of Clb (as opposed to ISO 10300-1's own printed Formula
        // A.36, a plain product of the two square-root terms, which does not reproduce either
        // sample) was reverse-engineered against ISO/TR 10300-30 Sample 1 (a=0) and matches it
        // to <0.2% at every f (tip/middle/root). It also matches Sample 2 (hypoid, a=15mm) at
        // f=0 exactly, but over-estimates the tip/root contact-line length there by ~35%,
        // giving ZLS about 10% high for that markedly asymmetric hypoid geometry (fmaxB and
        // |fmax0| differing by a factor of ~2.6, vs. near-equal for Sample 1). The general
        // hypoid form of Eq A.36 could not be reconstructed with confidence from the source
        // material - see ISO10300_Implementation_Findings.docx. Verify ZLS/YLS independently
        // for hypoid designs with pronounced offset.
        double clb = Math.Sqrt(fRatio2 + k * k * (1.0 - fRatio2));
        return lb0 * (1.0 - clb);
    }

    public static VirtualGearResult Calculate(Iso10300Inputs In, CalcTrace T, List<string> warnings)
    {
        var r = new VirtualGearResult();
        double u = (double)In.z2 / In.z1;
        r.u = T.R("u", "", "Gear ratio", u, "");
        bool hypoid = Math.Abs(In.HypoidOffset_a) > 1e-9;

        // ---- A.2.2: diameters (Eq A.1: dv = dm/cos(delta), general for any offset) ----
        r.dv1 = T.R("dv1", "Eq(A.1)", "Reference diameter of virtual cylindrical gear, pinion", In.dm1 / Cos(In.Delta1), "mm");
        r.dv2 = T.R("dv2", "Eq(A.1)", "Reference diameter of virtual cylindrical gear, wheel", In.dm2 / Cos(In.Delta2), "mm");
        r.av = T.R("av", "Eq(A.5)", "Centre distance of virtual cylindrical gear pair", (r.dv1 + r.dv2) / 2.0, "mm");

        // ---- A.2.3: helix angle, base diameter, module, teeth, base pitch, path of contact ----
        r.betav = T.R("betav", "Eq(A.8)", "Helix angle of virtual cylindrical gear", (In.Beta_m1 + In.Beta_m2) / 2.0, "deg");
        r.alphaVet = T.R("alphavet", "Eq(A.10)", "Transverse pressure angle of virtual cylindrical gear (drive flank)", Atan(Tan(In.Alpha_eD) / Cos(r.betav)), "deg");
        r.dvb1 = T.R("dvb1", "Eq(A.9)", "Base diameter, pinion", r.dv1 * Cos(r.alphaVet), "mm");
        r.dvb2 = T.R("dvb2", "Eq(A.9)", "Base diameter, wheel", r.dv2 * Cos(r.alphaVet), "mm");
        r.mvt = T.R("mvt", "Eq(A.13)", "Transverse module", In.mmn / Cos(r.betav), "mm");
        r.zv1 = T.R("zv1", "", "Number of teeth, virtual pinion (= dv1/mvt)", r.dv1 / r.mvt, "");
        r.zv2 = T.R("zv2", "", "Number of teeth, virtual wheel (= dv2/mvt)", r.dv2 / r.mvt, "");
        r.uv = T.R("uv", "", "Gear ratio of virtual cylindrical gear", r.zv2 / r.zv1, "");
        r.betaVb = T.R("betavb", "Eq(A.16)", "Helix angle at base circle of virtual cylindrical gear", Asin(Sin(r.betav) * Cos(In.Alpha_eD)), "deg");
        r.dva1 = T.R("dva1", "Eq(A.6)", "Tip diameter, pinion", r.dv1 + 2.0 * In.h_am1, "mm");
        r.dva2 = T.R("dva2", "Eq(A.6)", "Tip diameter, wheel", r.dv2 + 2.0 * In.h_am2, "mm");
        r.dvf1 = T.R("dvf1", "Eq(A.7)", "Root diameter, pinion", r.dv1 - 2.0 * In.h_fm1, "mm");
        r.dvf2 = T.R("dvf2", "Eq(A.7)", "Root diameter, wheel", r.dv2 - 2.0 * In.h_fm2, "mm");
        r.pvet = T.R("pvet", "Eq(A.17)", "Transverse base pitch", Math.PI * In.mmn * Cos(r.alphaVet) / Cos(r.betav), "mm");
        r.gvAlpha = T.R("gv_alpha", "Eq(A.18)", "Length of path of contact (transverse section)",
            0.5 * ((Math.Sqrt(r.dva1 * r.dva1 - r.dvb1 * r.dvb1) - r.dv1 * Sin(r.alphaVet)) +
                   (Math.Sqrt(r.dva2 * r.dva2 - r.dvb2 * r.dvb2) - r.dv2 * Sin(r.alphaVet))), "mm");
        r.epsVAlpha = T.R("eps_valpha", "Eq(A.23)", "Transverse contact ratio", r.gvAlpha / r.pvet, "");
        if (r.epsVAlpha >= 2.0)
            warnings.Add($"Transverse contact ratio of the virtual cylindrical gear (eps_valpha = {r.epsVAlpha:0.###}) is >= 2 - outside the validity range of ISO 10300-2/-3 (scope: eps_valpha < 2).");

        // ---- A.2.4: facewidth (general form; collapses to bv,eff = b2,eff and bv = b when a = 0) ----
        double b2eff = In.EffectiveFacewidthFraction * In.b;
        double thetaMp = Atan(Sin(In.Delta2) * Tan(In.Zeta_m));
        double gammaPrime = thetaMp - In.Zeta_mp / 2.0;
        T.R("theta_mp", "Eq(A.21)", "Auxiliary angle for virtual facewidth", thetaMp, "deg");
        T.R("gamma'", "Eq(A.20)", "Projected auxiliary angle for length of contact line", gammaPrime, "deg");
        r.bvEff = T.R("bv_eff", "Eq(A.19)", "Effective facewidth of virtual cylindrical gear",
            (b2eff / Cos(In.Zeta_mp / 2.0) - r.gvAlpha * Cos(r.alphaVet) * Tan(In.Zeta_mp / 2.0)) /
            (1.0 - Tan(gammaPrime) * Tan(In.Zeta_mp / 2.0)), "mm");
        r.bv = T.R("bv", "Eq(A.22)", "Facewidth of virtual cylindrical gear", In.b * (r.bvEff / b2eff), "mm");
        r.gamma = T.R("gamma", "Eq(A.35)", "Auxiliary angle for length of contact line", ToDeg(Math.Atan(Math.Tan(ToRad(gammaPrime)) / Math.Cos(ToRad(r.alphaVet)))), "deg");

        // ---- A.2.6: face and virtual contact ratio ----
        r.epsVBeta = T.R("eps_vbeta", "Eq(A.24)", "Face contact ratio", r.bvEff * Sin(r.betav) / (Math.PI * In.mmn), "");
        r.epsVGamma = T.R("eps_vgamma", "Eq(A.25)", "Virtual contact ratio", r.epsVAlpha + r.epsVBeta, "");

        // ---- A.2.7: length of contact lines (tip/middle/root), Table A.2 ----
        double tanGamma = Tan(r.gamma);
        double fmaxB = 0.5 * (r.gvAlpha + r.bvEff * (tanGamma + Tan(r.betaVb))) * Cos(r.betaVb);
        double fmax0 = 0.5 * (r.gvAlpha - r.bvEff * (tanGamma + Tan(r.betaVb))) * Cos(r.betaVb);
        r.fmax = T.R("fmax", "Eq(A.33/34)", "Maximum distance to middle contact line", Math.Max(fmaxB, fmax0), "mm");
        if (hypoid && fmaxB > 1e-9 && fmax0 < -1e-9 && Math.Abs(fmax0) / fmaxB > 0.2)
            warnings.Add($"Pronounced hypoid offset asymmetry detected (fmaxB={fmaxB:0.##}mm vs fmax0={fmax0:0.##}mm): the contact-line clipping factor Clb (10300-1, Eq A.36) is validated against ISO/TR 10300-30 Sample 2 only at the middle contact line (f=0); the tip/root lines showed a ~10% high bias in ZLS/YLS for that sample's similarly asymmetric geometry - verify ZLS/YLS independently for this design (see ISO10300_Implementation_Findings.docx).");
        if (r.epsVBeta >= 1.0)
        {
            // Table A.2, eps_vbeta >= 1 branch.
            r.fm = T.R("fm", "T:A.2", "Distance of the middle contact line (eps_vbeta>=1 branch)", 0.0, "mm");
            r.ft = T.R("ft", "T:A.2", "Distance of the tip contact line (eps_vbeta>=1 branch)", r.pvet * Cos(r.betaVb), "mm");
            r.fr = T.R("fr", "T:A.2", "Distance of the root contact line (eps_vbeta>=1 branch)", -r.pvet * Cos(r.betaVb), "mm");
        }
        else
        {
            // Table A.2, eps_vbeta < 1 branch (covers eps_vbeta = 0 as the X*1 special case too -
            // validated against ISO/TR 10300-30 Sample 3, eps_vbeta = 0.860, continuous with the
            // eps_vbeta >= 1 branch above at eps_vbeta = 1 exactly, where X -> 0).
            double X = (r.pvet - 0.5 * r.pvet * r.epsVAlpha) * Cos(r.betaVb) * (1.0 - r.epsVBeta);
            r.fm = T.R("fm", "T:A.2", "Distance of the middle contact line (eps_vbeta<1 branch)", -X, "mm");
            r.ft = T.R("ft", "T:A.2", "Distance of the tip contact line (eps_vbeta<1 branch)", -X + r.pvet * Cos(r.betaVb), "mm");
            r.fr = T.R("fr", "T:A.2", "Distance of the root contact line (eps_vbeta<1 branch)", -X - r.pvet * Cos(r.betaVb), "mm");
        }
        r.lbm = T.R("lbm", "Eq(A.26)", "Length of the middle contact line", ContactLineLength(r.fm, r.betaVb, r.gamma, r.gvAlpha, r.bvEff, r.bv, r.fmax), "mm");

        // ---- A.2.8: radius of relative curvature (drive side, Eq A.39) ----
        double Rm1 = In.dm1 / (2.0 * Sin(In.Delta1));
        double Rm2 = In.dm2 / (2.0 * Sin(In.Delta2));
        T.R("Rm1", "", "Mean cone distance, pinion (derived)", Rm1, "mm");
        T.R("Rm2", "", "Mean cone distance, wheel (derived)", Rm2, "mm");
        r.betaB = T.R("betaB", "Eq(A.38)", "Inclination angle of contact line", Atan(Tan(r.betav) * Sin(In.Alpha_eD)), "deg");
        double denom = (1.0 / (Cos(In.Alpha_nD) * (Tan(In.Alpha_nD) - Tan(In.Alpha_lim)) + Tan(In.Zeta_mp) * Tan(r.betaB)))
                       * (Cos(In.Beta_m1) * Cos(In.Beta_m2) / Cos(In.Zeta_mp))
                       * (1.0 / (Rm2 * Tan(In.Delta2)) + 1.0 / (Rm1 * Tan(In.Delta1)));
        r.rhoT = T.R("rho_t", "Eq(A.39)", "Radius of relative curvature in normal section at mean point (drive side)", 1.0 / denom, "mm");
        r.rhoRel = T.R("rho_rel", "Eq(A.37)", "Local equivalent radius of curvature vertical to the contact line", Math.Abs(r.rhoT) * Cos(r.betaB) * Cos(r.betaB), "mm");

        // ---- A.3: virtual cylindrical gear in normal section ----
        double cosBvb2 = Cos(r.betaVb) * Cos(r.betaVb);
        r.zvn1 = T.R("zvn1", "Eq(A.41)", "Number of teeth of virtual gear in normal section, pinion", r.zv1 / (cosBvb2 * Cos(r.betav)), "");
        r.zvn2 = T.R("zvn2", "Eq(A.42)", "Number of teeth of virtual gear in normal section, wheel", r.uv * r.zvn1, "");
        r.dvn1 = T.R("dvn1", "Eq(A.43)", "Reference diameter of virtual gear in normal section, pinion", r.zvn1 * In.mmn, "mm");
        r.dvn2 = T.R("dvn2", "Eq(A.43)", "Reference diameter of virtual gear in normal section, wheel", r.zvn2 * In.mmn, "mm");
        r.dvan1 = T.R("dvan1", "Eq(A.44)", "Tip diameter of virtual gear in normal section, pinion", r.dvn1 + 2.0 * In.h_am1, "mm");
        r.dvan2 = T.R("dvan2", "Eq(A.44)", "Tip diameter of virtual gear in normal section, wheel", r.dvn2 + 2.0 * In.h_am2, "mm");
        r.dvbn1 = T.R("dvbn1", "Eq(A.45)", "Base diameter of virtual gear in normal section, pinion", r.dvn1 * Cos(In.Alpha_eD), "mm");
        r.dvbn2 = T.R("dvbn2", "Eq(A.45)", "Base diameter of virtual gear in normal section, wheel", r.dvn2 * Cos(In.Alpha_eD), "mm");
        r.epsVAlphaN = T.R("eps_valphan", "Eq(A.46)", "Profile contact ratio (normal section)", r.epsVAlpha / cosBvb2, "");

        if (!hypoid && (Math.Abs(In.Zeta_mp) > 1e-9 || Math.Abs(In.Zeta_m) > 1e-9 || Math.Abs(In.Zeta_R) > 1e-9))
            warnings.Add("Hypoid offset a = 0 but a nonzero offset angle (zeta_mp/zeta_m/zeta_R) was supplied - these should normally all be zero together for a non-offset bevel gear.");

        return r;
    }
}
