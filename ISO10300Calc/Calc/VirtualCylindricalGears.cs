using ISO10300Calc.Models;
using static ISO10300Calc.Calc.MathUtil;

namespace ISO10300Calc.Calc;

/// <summary>
/// Results of ISO 10300-1:2023, Annex A ("Calculation of virtual cylindrical gears - Method
/// B1"), restricted to bevel gears without hypoid offset (a = 0). Only the loaded (drive)
/// flank is evaluated, matching the worked example in ISO/TR 10300-30:2024, Annex A, which
/// likewise rates the drive flank only.
/// </summary>
public class VirtualGearResult
{
    public double u, dv1, dv2, av, betav, alphaVet, mvt, zv1, zv2, uv;
    public double betaVb, dva1, dva2, dvf1, dvf2, dvb1, dvb2, pvet, gvAlpha;
    public double epsVAlpha, bvEff, bv, epsVBeta, epsVGamma;
    public double rhoT, betaB, rhoRel;
    public double zvn1, zvn2, dvn1, dvn2, dvan1, dvan2, dvbn1, dvbn2, epsVAlphaN;
    public double fmax, lbm, ft, fm, fr;
}

public static class VirtualCylindricalGears
{
    /// <summary>Length of the contact line at a signed distance f from the centre of the zone
    /// of action (ISO 10300-1:2023, A.2.7, Formulae A.26 to A.36), for a = 0 (so the auxiliary
    /// angle gamma = gamma' = 0 identically - 10300-1, A.2.4/A.2.7). Also returns fmax so
    /// callers computing several contact lines only need to evaluate it once.</summary>
    public static double ContactLineLength(double f, double betaVbDeg, double gvAlpha, double bvEff, double bv, double fmax)
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
            double bracket = f * sinBvb + bvEff / 2.0;
            x1 = (f * cosBvb + tanBvb * bracket + 0.5 * gvAlpha) / tanBvb;
            x2 = (f * cosBvb + tanBvb * bracket - 0.5 * gvAlpha) / tanBvb;
            y1 = -x1 * tanBvb + f * cosBvb + tanBvb * bracket;
            y2 = -x2 * tanBvb + f * cosBvb + tanBvb * bracket;
        }

        double lb0 = Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        double fRatio2 = fmax > 0 ? (f / fmax) * (f / fmax) : 0.0;
        double k = 1.0 - Math.Sqrt(bvEff / bv);
        double clb = Math.Sqrt(fRatio2 + k * k * (1.0 - fRatio2));
        return lb0 * (1.0 - clb);
    }

    public static VirtualGearResult Calculate(Iso10300Inputs In, CalcTrace T, List<string> warnings)
    {
        var r = new VirtualGearResult();
        double u = (double)In.z2 / In.z1;
        r.u = T.R("u", "", "Gear ratio", u, "");

        // ---- A.2.2: diameters (a = 0, Sigma = 90 deg special case, Formulae A.3/A.4) ----
        r.dv1 = T.R("dv1", "Eq(A.3)", "Reference diameter of virtual cylindrical gear, pinion", In.dm1 * Math.Sqrt(u * u + 1.0) / u, "mm");
        r.dv2 = T.R("dv2", "Eq(A.4)", "Reference diameter of virtual cylindrical gear, wheel", u * u * r.dv1, "mm");
        r.av = T.R("av", "Eq(A.5)", "Centre distance of virtual cylindrical gear pair", (r.dv1 + r.dv2) / 2.0, "mm");

        // ---- A.2.3: helix angle, base diameter, module, teeth, base pitch, path of contact ----
        r.betav = T.R("betav", "Eq(A.8)", "Helix angle of virtual cylindrical gear", (In.Beta_m1 + In.Beta_m2) / 2.0, "deg");
        r.alphaVet = T.R("alphavet", "Eq(A.10)", "Transverse pressure angle of virtual cylindrical gear (drive flank)", Atan(Tan(In.Alpha_eD) / Cos(r.betav)), "deg");
        r.dvb1 = T.R("dvb1", "Eq(A.9)", "Base diameter, pinion", r.dv1 * Cos(r.alphaVet), "mm");
        r.dvb2 = T.R("dvb2", "Eq(A.9)", "Base diameter, wheel", r.dv2 * Cos(r.alphaVet), "mm");
        r.mvt = T.R("mvt", "Eq(A.13)", "Transverse module", In.mmn / Cos(r.betav), "mm");
        r.zv1 = T.R("zv1", "Eq(A.14)", "Number of teeth, virtual pinion", In.z1 * Math.Sqrt(u * u + 1.0) / u, "");
        r.zv2 = T.R("zv2", "Eq(A.15)", "Number of teeth, virtual wheel", In.z2 * Math.Sqrt(u * u + 1.0), "");
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

        // ---- A.2.4: facewidth (a = 0 -> bv,eff = b2,eff and bv = b exactly) ----
        double b2eff = In.EffectiveFacewidthFraction * In.b;
        r.bvEff = T.R("bv_eff", "Eq(A.19)", "Effective facewidth of virtual cylindrical gear (a=0: equals b2,eff)", b2eff, "mm");
        r.bv = T.R("bv", "Eq(A.22)", "Facewidth of virtual cylindrical gear (a=0: equals b)", In.b, "mm");

        // ---- A.2.6: face and virtual contact ratio ----
        r.epsVBeta = T.R("eps_vbeta", "Eq(A.24)", "Face contact ratio", r.bvEff * Sin(r.betav) / (Math.PI * In.mmn), "");
        r.epsVGamma = T.R("eps_vgamma", "Eq(A.25)", "Virtual contact ratio", r.epsVAlpha + r.epsVBeta, "");

        // ---- A.2.7: length of contact lines (tip/middle/root), Table A.2 (eps_vbeta >= 1 branch) ----
        r.fmax = T.R("fmax", "Eq(A.33/34)", "Maximum distance to middle contact line",
            Math.Max(0.5 * (r.gvAlpha + r.bvEff * Tan(r.betaVb)) * Cos(r.betaVb),
                     0.5 * (r.gvAlpha - r.bvEff * Tan(r.betaVb)) * Cos(r.betaVb)), "mm");
        if (r.epsVBeta < 1.0)
            warnings.Add($"Face contact ratio eps_vbeta = {r.epsVBeta:0.###} < 1: this tool only implements the ISO 10300-1 Table A.2 tip/mid/root contact-line positions for eps_vbeta >= 1 (validated against ISO/TR 10300-30 Sample 1); the eps_vbeta < 1 branch is applied as an approximation - verify ZLS/YLS independently for this design.");
        r.fm = T.R("fm", "T:A.2", "Distance of the middle contact line (eps_vbeta>=1 branch)", 0.0, "mm");
        r.ft = T.R("ft", "T:A.2", "Distance of the tip contact line (eps_vbeta>=1 branch)", r.pvet * Cos(r.betaVb), "mm");
        r.fr = T.R("fr", "T:A.2", "Distance of the root contact line (eps_vbeta>=1 branch)", -r.pvet * Cos(r.betaVb), "mm");
        r.lbm = T.R("lbm", "Eq(A.26)", "Length of the middle contact line", ContactLineLength(r.fm, r.betaVb, r.gvAlpha, r.bvEff, r.bv, r.fmax), "mm");

        // ---- A.2.8: radius of relative curvature (drive side, Eq A.39) ----
        double Rm1 = In.dm1 / (2.0 * Sin(In.Delta1));
        double Rm2 = In.dm2 / (2.0 * Sin(In.Delta2));
        T.R("Rm1", "", "Mean cone distance, pinion (derived)", Rm1, "mm");
        T.R("Rm2", "", "Mean cone distance, wheel (derived)", Rm2, "mm");
        double denom = (1.0 / (Cos(In.Alpha_nD) * (Tan(In.Alpha_nD) - Tan(In.Alpha_lim))))
                       * Cos(In.Beta_m1) * Cos(In.Beta_m2)
                       * (1.0 / (Rm2 * Tan(In.Delta2)) + 1.0 / (Rm1 * Tan(In.Delta1)));
        r.rhoT = T.R("rho_t", "Eq(A.39)", "Radius of relative curvature in normal section at mean point (drive side, a=0)", 1.0 / denom, "mm");
        r.betaB = T.R("betaB", "Eq(A.38)", "Inclination angle of contact line", Atan(Tan(r.betav) * Sin(In.Alpha_eD)), "deg");
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

        return r;
    }
}
