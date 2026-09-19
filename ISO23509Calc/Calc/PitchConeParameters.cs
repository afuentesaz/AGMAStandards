using ISO23509Calc.Models;
using static ISO23509Calc.Calc.MathUtil;

namespace ISO23509Calc.Calc;

/// <summary>Results of ISO 23509:2016, Clause 6 ("Pitch cone parameters"): the seven quantities
/// (Rm1, Rm2, delta1, delta2, beta_m1, beta_m2, c_be2) that Annex A.2 identifies as the common
/// output of all four methods, plus dm1/dm2 (needed directly by several Clause 7 formulas).</summary>
public class PitchConeResult
{
    public double u, delta1, delta2, Rm1, Rm2, beta_m1, beta_m2, c_be2, dm1, dm2;
}

/// <summary>
/// Implements ISO 23509:2016, 6.2.1-6.2.4 (Methods 0-3). Equation numbers in comments refer to
/// the main body's own sequential numbering, Formula (1) to Formula (109).
/// </summary>
public static class PitchConeParameters
{
    public static PitchConeResult Calculate(Iso23509Inputs In, CalcTrace T, List<string> warnings)
    {
        return In.Method switch
        {
            PitchConeMethod.Method0 => Method0(In, T),
            PitchConeMethod.Method1 => Method1(In, T, warnings),
            PitchConeMethod.Method2 => Method2(In, T, warnings),
            PitchConeMethod.Method3 => Method3(In, T, warnings),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    // ================================================================= Method 0 (Formulae 1-7)
    private static PitchConeResult Method0(Iso23509Inputs In, CalcTrace T)
    {
        T.Section("Clause 6.2.1: Pitch cone parameters, Method 0 (bevel gear, a = 0)");
        var r = new PitchConeResult();
        r.u = T.R("u", "Eq(1)", "Gear ratio", (double)In.z2 / In.z1, "");
        r.delta1 = T.R("delta1", "Eq(2)", "Pinion pitch angle", Atan(Sin(In.Sigma) / (Cos(In.Sigma) + r.u)), "deg");
        r.delta2 = T.R("delta2", "Eq(3)", "Wheel pitch angle", In.Sigma - r.delta1, "deg");
        double Re = T.R("Re", "Eq(4)", "Outer cone distance", In.de2 / (2.0 * Sin(r.delta2)), "mm");
        r.Rm1 = r.Rm2 = T.R("Rm", "Eq(5)", "Mean cone distance (pinion = wheel)", Re - In.b2 / 2.0, "mm");
        r.beta_m1 = r.beta_m2 = T.R("beta_m1=beta_m2", "Eq(6)", "Spiral angle (pinion = wheel)", In.beta_m2, "deg");
        r.c_be2 = T.R("c_be2", "Eq(7)", "Face width factor", 0.5, "");
        r.dm1 = 2.0 * r.Rm1 * Sin(r.delta1);
        r.dm2 = 2.0 * r.Rm2 * Sin(r.delta2);
        return r;
    }

    // ================================================================= Method 1 (Formulae 8-41)
    private static PitchConeResult Method1(Iso23509Inputs In, CalcTrace T, List<string> warnings)
    {
        T.Section("Clause 6.2.2: Pitch cone parameters, Method 1 (Gleason)");
        double u = T.R("u", "Eq(8)", "Gear ratio", (double)In.z2 / In.z1, "");
        double betaDelta1 = T.R("beta_Delta1", "Eq(9)", "Desired pinion spiral angle", In.beta_m1_desired, "deg");
        double DSigma = T.R("DeltaSigma", "Eq(10)", "Shaft angle departure from 90 deg", In.Sigma - 90.0, "deg");
        double deltaInt2 = T.R("delta_int2", "Eq(11)", "Approximate wheel pitch angle", Atan(u * Cos(DSigma) / (1.2 * (1.0 - u * Sin(DSigma)))), "deg");
        double rMpt2 = T.R("r_mpt2", "Eq(12)", "Wheel mean pitch radius", (In.de2 - In.b2 * Sin(deltaInt2)) / 2.0, "mm");
        double epsI = T.R("eps_i'", "Eq(13)", "Approximate pinion offset angle in pitch plane", Asin(In.a * Sin(deltaInt2) / rMpt2), "deg");
        double K1 = T.R("K1", "Eq(14)", "Approximate hypoid dimension factor", Tan(betaDelta1) * Sin(epsI) + Cos(epsI), "");
        double rMn1 = T.R("r_mn1", "Eq(15)", "Approximate pinion mean radius", rMpt2 * K1 / u, "mm");

        double eta0 = Atan(In.a / (rMpt2 * (Tan(deltaInt2) * Cos(DSigma) - Sin(DSigma)) + rMn1));
        T.R("eta (seed)", "Eq(16)", "Wheel offset angle in axial plane (starting value)", eta0, "deg");

        (double residual, double rhoMBeta, double rhoLim, double eps1p, double delta1, double delta2,
            double Rm1, double Rm2, double betaM1, double betaM2) Evaluate(double eta, CalcTrace? trace)
        {
            double eps2 = Asin((In.a - rMn1 * Sin(eta)) / rMpt2);
            trace?.R("eps2", "Eq(17)", "Intermediate pinion offset angle in axial plane", eps2, "deg");
            double deltaInt1 = Atan(Sin(eta) / (Tan(eps2) * Cos(DSigma)) + Tan(DSigma) * Cos(eta));
            trace?.R("delta_int1", "Eq(18)", "Intermediate pinion pitch angle", deltaInt1, "deg");
            double eps2p = Asin(Sin(eps2) * Cos(DSigma) / Cos(deltaInt1));
            trace?.R("eps2'", "Eq(19)", "Intermediate pinion offset angle in pitch plane", eps2p, "deg");
            double betaMint1 = Atan((K1 - Cos(eps2p)) / Sin(eps2p));
            trace?.R("beta_m,int1", "Eq(20)", "Intermediate pinion mean spiral angle", betaMint1, "deg");
            double deltaK = Sin(eps2p) * (Tan(betaDelta1) - Tan(betaMint1));
            trace?.R("DeltaK", "Eq(21)", "Increment in hypoid dimension factor", deltaK, "");
            double deltaRmpt1 = rMpt2 * deltaK / u;
            trace?.R("Delta r_mpt1", "Eq(22)", "Pinion mean radius increment", deltaRmpt1, "mm");
            double eps1 = Asin(Sin(eps2) - (deltaRmpt1 / rMpt2) * Sin(eta));
            trace?.R("eps1", "Eq(23)", "Pinion offset angle in axial plane", eps1, "deg");
            double delta1 = Atan(Sin(eta) / (Tan(eps1) * Cos(DSigma)) + Tan(DSigma) * Cos(eta));
            trace?.R("delta1", "Eq(24)", "Pinion pitch angle", delta1, "deg");
            double eps1p = Asin(Sin(eps1) * Cos(DSigma) / Cos(delta1));
            trace?.R("eps1'", "Eq(25)", "Pinion offset angle in pitch plane", eps1p, "deg");
            double betaM1 = Atan((K1 + deltaK - Cos(eps1p)) / Sin(eps1p));
            trace?.R("beta_m1", "Eq(26)", "Spiral angle, pinion", betaM1, "deg");
            double betaM2 = betaM1 - eps1p;
            trace?.R("beta_m2", "Eq(27)", "Spiral angle, wheel", betaM2, "deg");
            double delta2 = Atan(Sin(eps1) / (Tan(eta) * Cos(DSigma)) + Cos(eps1) * Tan(DSigma));
            trace?.R("delta2", "Eq(28)", "Wheel pitch angle", delta2, "deg");
            double Rm1 = (rMn1 + deltaRmpt1) / Sin(delta1);
            trace?.R("Rm1", "Eq(29)", "Mean cone distance, pinion", Rm1, "mm");
            double Rm2 = rMpt2 / Sin(delta2);
            trace?.R("Rm2", "Eq(30)", "Mean cone distance, wheel", Rm2, "mm");
            trace?.R("r_mpt1", "Eq(31)", "Mean pinion radius", Rm1 * Sin(delta1), "mm");
            double alphaLim = Atan(-Tan(delta1) * Tan(delta2) / Cos(eps1p) *
                ((Rm1 * Sin(betaM1) - Rm2 * Sin(betaM2)) / (Rm1 * Tan(delta1) + Rm2 * Tan(delta2))));
            trace?.R("alpha_lim", "Eq(32)", "Limit pressure angle", alphaLim, "deg");
            double rhoLim = (1.0 / Cos(alphaLim)) * (Tan(betaM1) - Tan(betaM2)) /
                (-Tan(alphaLim) * (Tan(betaM1) / (Rm1 * Tan(delta1)) + Tan(betaM2) / (Rm2 * Tan(delta2)))
                 + 1.0 / (Rm1 * Cos(betaM1)) - 1.0 / (Rm2 * Cos(betaM2)));
            trace?.R("rho_lim", "Eq(33)", "Limit radius of curvature", rhoLim, "mm");

            double rhoMBeta;
            if (In.Process == ManufacturingProcess.FaceHobbing)
            {
                double zp = T.R("zp", "Eq(34)", "Number of crown gear teeth", In.z2 / Sin(delta2), "");
                double nu = trace?.R("nu", "Eq(35)", "Lead angle of cutter", Asin((Rm2 * In.z0 / (In.rc0 * zp)) * Cos(betaM2)), "deg") ?? Asin((Rm2 * In.z0 / (In.rc0 * zp)) * Cos(betaM2));
                double lambda = trace?.R("lambda", "Eq(36)", "First auxiliary angle", 90.0 - betaM2 + nu, "deg") ?? (90.0 - betaM2 + nu);
                double rhoP0 = trace?.R("rho_P0", "Eq(37)", "Crown gear to cutter centre distance", Math.Sqrt(Rm2 * Rm2 + In.rc0 * In.rc0 - 2.0 * Rm2 * In.rc0 * Cos(lambda)), "mm")
                    ?? Math.Sqrt(Rm2 * Rm2 + In.rc0 * In.rc0 - 2.0 * Rm2 * In.rc0 * Cos(lambda));
                double eta1 = trace?.R("eta1", "Eq(38)", "Second auxiliary angle", Acos((Rm2 * Cos(betaM2) / (rhoP0 * zp)) * (zp + In.z0)), "deg")
                    ?? Acos((Rm2 * Cos(betaM2) / (rhoP0 * zp)) * (zp + In.z0));
                rhoMBeta = Rm2 * Cos(betaM2) * (Tan(betaM2) + Tan(eta1) / (1.0 + Tan(nu) * (Tan(betaM2) + Tan(eta1))));
                trace?.R("rho_mBeta", "Eq(39)", "Lengthwise tooth mean radius of curvature (face hobbing)", rhoMBeta, "mm");
            }
            else
            {
                rhoMBeta = In.rc0;
                trace?.R("rho_mBeta", "Eq(40)", "Lengthwise tooth mean radius of curvature (face milling)", rhoMBeta, "mm");
            }

            return (rhoMBeta - rhoLim, rhoMBeta, rhoLim, eps1p, delta1, delta2, Rm1, Rm2, betaM1, betaM2);
        }

        double etaFinal = Bisect(eta => Evaluate(eta, null).residual, eta0, 8.0, tol: 1e-8);
        var final = Evaluate(etaFinal, T);
        T.R("eta (converged)", "", "Wheel offset angle in axial plane, converged value", etaFinal, "deg");
        double checkRatio = final.rhoMBeta / final.rhoLim - 1.0;
        T.Note(Math.Abs(checkRatio) <= 0.01
            ? $"Iteration converged: |rho_mBeta/rho_lim - 1| = {checkRatio:0.00000} <= 0.01."
            : $"WARNING: iteration did not converge to the standard's 0.01 tolerance (|rho_mBeta/rho_lim - 1| = {checkRatio:0.00000}).");
        if (Math.Abs(checkRatio) > 0.01)
            warnings.Add("Method 1 pitch-cone iteration did not converge to the standard's 0.01 tolerance on rho_mBeta/rho_lim - results may be inaccurate.");

        double cbe2 = T.R("c_be2", "Eq(41)", "Face width factor", (In.de2 / (2.0 * Sin(final.delta2)) - final.Rm2) / In.b2, "");

        return new PitchConeResult
        {
            u = u,
            delta1 = final.delta1,
            delta2 = final.delta2,
            Rm1 = final.Rm1,
            Rm2 = final.Rm2,
            beta_m1 = final.betaM1,
            beta_m2 = final.betaM2,
            c_be2 = cbe2,
            dm1 = 2.0 * final.Rm1 * Sin(final.delta1),
            dm2 = 2.0 * final.Rm2 * Sin(final.delta2)
        };
    }

    // ================================================================= Method 2 (Formulae 42-83)
    private static PitchConeResult Method2(Iso23509Inputs In, CalcTrace T, List<string> warnings)
    {
        T.Section("Clause 6.2.3: Pitch cone parameters, Method 2 (Oerlikon)");
        double nu = T.R("nu", "Eq(42)", "Lead angle of cutter", Asin(In.z0 * In.dm2 * Cos(In.beta_m2) / (2.0 * In.z2 * In.rc0)), "deg");
        double lambda = T.R("lambda", "Eq(43)", "First auxiliary angle", 90.0 - In.beta_m2 + nu, "deg");
        double u = T.R("u", "Eq(44)", "Gear ratio", (double)In.z2 / In.z1, "");
        double delta1app = T.R("delta1app", "Eq(45)", "First approximate pinion pitch angle", Atan(Sin(In.Sigma) / (u + Cos(In.Sigma))), "deg");
        double delta2app = T.R("delta2app", "Eq(46)", "First approximate wheel pitch angle", In.Sigma - delta1app, "deg");
        double zetaMapp = T.R("zeta_mapp", "Eq(47)", "First approximate pinion offset angle in axial plane",
            Asin((2.0 * In.a / In.dm2) / (1.0 + Cos(delta2app) / (u * Cos(delta1app)))), "deg");
        double Fapp = T.R("Fapp", "Eq(48)", "Approximate hypoid dimension factor", Cos(In.beta_m2) / Cos(In.beta_m2 + zetaMapp), "");
        double dm1app = T.R("dm1app", "Eq(49)", "Approximate pinion mean pitch diameter", Fapp * In.dm2 / u, "mm");
        double phi2 = T.R("phi2", "Eq(50)", "Intermediate angle", Atan((u * Cos(zetaMapp)) / (u / Tan(delta2app) + (Fapp - 1.0) * Sin(In.Sigma))), "deg");
        double Rmapp = T.R("Rmapp", "Eq(51)", "Approximate mean radius of crown gear", In.dm2 / (2.0 * Sin(phi2)), "mm");
        double eta1 = T.R("eta1", "Eq(52)", "Second auxiliary angle", Atan((In.rc0 * Cos(nu) - Rmapp * Sin(In.beta_m2)) / (In.rc0 * Sin(nu) + Rmapp * Cos(In.beta_m2))), "deg");
        double phi3 = T.R("phi3", "Eq(53)", "Intermediate angle", Atan(Tan(In.beta_m2 + eta1) / Sin(phi2)), "deg");
        double delta1pp = T.R("delta1''", "Eq(54)", "Second approximate pinion pitch angle",
            Atan(dm1app * Sin(In.Sigma) / (In.dm2 * Cos(zetaMapp) + dm1app * Cos(In.Sigma) - 2.0 * In.a / Tan(phi3 + zetaMapp))), "deg");
        double delta2pp = T.R("delta2''", "Eq(55)", "Approximate wheel pitch angle (projected)", In.Sigma - delta1pp, "deg");
        double delta2imp0 = Atan(Tan(delta2pp) * Cos(zetaMapp));
        T.R("delta2imp (seed)", "Eq(56)", "Improved wheel pitch angle (starting value)", delta2imp0, "deg");

        (double residual, double Rm1, double Rm2, double delta1, double delta2, double F, double dm1, double betaM1) Evaluate(double delta2imp, CalcTrace? trace)
        {
            double etaP = Atan(Sin(zetaMapp) * Cos(delta2imp) / Cos(In.Sigma - delta2imp));
            trace?.R("eta_p", "Eq(57)", "Auxiliary angle", etaP, "deg");
            double etaApp = Atan(2.0 * In.a / (In.dm2 * Tan(delta2imp) + dm1app * Cos(etaP) * Sin(In.beta_m2 + eta1) / Cos(In.Sigma - delta2imp)));
            trace?.R("eta_app", "Eq(58)", "Approximate wheel offset angle", etaApp, "deg");
            double zetaMimp = Asin(2.0 * In.a / In.dm2 - Fapp * Tan(etaApp) * Sin(delta2imp) * Cos(etaP) / (u * Cos(In.Sigma - delta2imp)));
            trace?.R("zeta_m,imp", "Eq(59)", "Improved pinion offset angle in axial plane", zetaMimp, "deg");
            double zetaMpImp = Atan(Tan(zetaMimp) * Sin(In.Sigma) / Cos(In.Sigma - delta2imp));
            trace?.R("zeta_mp,imp", "Eq(60)", "Improved pinion offset angle in pitch plane", zetaMpImp, "deg");
            double F = Cos(In.beta_m2) / Cos(In.beta_m2 + zetaMpImp);
            trace?.R("F", "Eq(61)", "Hypoid dimension factor", F, "");
            double dm1 = F * In.dm2 / u;
            trace?.R("dm1", "Eq(62)", "Pinion mean pitch diameter", dm1, "mm");
            double phi4 = Atan(Sin(lambda) * Sin(In.Sigma) / (In.dm2 / (2.0 * In.rc0) - Cos(lambda) * Sin(delta2imp)));
            trace?.R("phi4", "Eq(63)", "Intermediate angle", phi4, "deg");
            double delta1ppImp = Atan(dm1 * Sin(In.Sigma) / (In.dm2 * Cos(zetaMimp) + dm1 * Cos(In.Sigma) * Cos(etaP) - 2.0 * In.a / Tan(phi4 + zetaMimp)));
            trace?.R("delta1''_imp", "Eq(64)", "Improved pinion pitch angle", delta1ppImp, "deg");
            double delta2ppImp = In.Sigma - delta1ppImp;
            trace?.R("delta2''_imp", "Eq(65)", "Improved wheel pitch angle (projected)", delta2ppImp, "deg");
            double delta2 = Atan(Tan(delta2ppImp) * Cos(zetaMpImp));
            trace?.R("delta2", "Eq(66)", "Wheel pitch angle", delta2, "deg");
            double phi5 = Atan(Tan(delta2) / Cos(zetaMimp));
            trace?.R("phi5", "Eq(67)", "Intermediate angle", phi5, "deg");
            double etaPimp = Atan(Tan(etaApp) * Sin(phi5) / Cos(In.Sigma - phi5));
            trace?.R("eta_pimp", "Eq(68)", "Improved auxiliary angle", etaPimp, "deg");
            double eta = Atan(2.0 * In.a / (In.dm2 * Tan(delta2) + dm1 * Cos(etaPimp) * Sin(phi5) / Cos(In.Sigma - phi5)));
            trace?.R("eta", "Eq(69)", "Wheel offset angle in axial plane", eta, "deg");
            double zetaM = Asin(Tan(delta2) * Tan(eta));
            trace?.R("zeta_m", "Eq(70)", "Pinion offset angle in axial plane", zetaM, "deg");
            double zetaMp = Atan(Tan(zetaM) * Sin(In.Sigma) / Cos(In.Sigma - delta2));
            trace?.R("zeta_mp", "Eq(71)", "Pinion offset angle in pitch plane", zetaMp, "deg");
            double betaM1 = In.beta_m2 + zetaMp;
            trace?.R("beta_m1", "Eq(72)", "Spiral angle, pinion", betaM1, "deg");
            dm1 = In.dm2 * Cos(In.beta_m2) / (u * Cos(betaM1));
            trace?.R("dm1", "Eq(73)", "Pinion mean pitch diameter", dm1, "mm");
            double xi = Math.Abs(In.Sigma - 90.0) > 1e-9
                ? Atan(Tan(In.Sigma) * Cos(zetaM)) - delta2
                : 90.0 - delta2;
            trace?.R("xi", Math.Abs(In.Sigma - 90.0) > 1e-9 ? "Eq(74)" : "Eq(75)", "Auxiliary angle", xi, "deg");
            double delta1 = Atan(Tan(xi) * Cos(zetaMp));
            trace?.R("delta1", "Eq(76)", "Pinion pitch angle", delta1, "deg");
            double Rm1 = dm1 / (2.0 * Sin(delta1));
            trace?.R("Rm1", "Eq(77)", "Mean cone distance, pinion", Rm1, "mm");
            double Rm2 = In.dm2 / (2.0 * Sin(delta2));
            trace?.R("Rm2", "Eq(78)", "Mean cone distance, wheel", Rm2, "mm");
            double rhoP0 = Math.Sqrt(In.rc0 * In.rc0 + Rm2 * Rm2 - 2.0 * In.rc0 * Rm2 * Cos(lambda));
            trace?.R("rho_P0", "Eq(79)", "Crown gear to cutter centre distance", rhoP0, "mm");
            double phi6 = Asin(In.rc0 * Sin(lambda) / rhoP0);
            trace?.R("phi6", "Eq(80)", "Intermediate angle", phi6, "deg");
            double phiComp = 180.0 - zetaMp - phi6;
            trace?.R("phi_comp", "Eq(81)", "Complementary angle", phiComp, "deg");
            double Rmcheck = Rm2 * Sin(phi6) / Sin(phiComp);
            trace?.R("Rmcheck", "Eq(82)", "Checking variable", Rmcheck, "mm");

            return (Rm1 - Rmcheck, Rm1, Rm2, delta1, delta2, F, dm1, betaM1);
        }

        // The approximation chain (Formulae 42-55) already lands within a fraction of a degree
        // of the converged value (often within the standard's own 1% tolerance on the first
        // trial) - a narrow initial bracket avoids the residual function's other, unwanted
        // roots further away from the physically sensible solution.
        double delta2impFinal = Bisect(x => Evaluate(x, null).residual, delta2imp0, 0.5, tol: 1e-9, maxExpand: 10);
        var final = Evaluate(delta2impFinal, T);
        T.R("delta2imp (converged)", "", "Improved wheel pitch angle, converged value", delta2impFinal, "deg");
        double relResidual = final.residual / final.Rm1;
        T.Note(Math.Abs(relResidual) <= 0.01
            ? $"Iteration converged: |Rm1/Rmcheck - 1| = {relResidual:0.00000} <= 0.01."
            : $"WARNING: iteration did not converge to the standard's 0.01 tolerance (|Rm1/Rmcheck - 1| = {relResidual:0.00000}).");
        if (Math.Abs(relResidual) > 0.01)
            warnings.Add("Method 2 pitch-cone iteration did not converge to the standard's 0.01 tolerance on Rm1/Rmcheck - results may be inaccurate.");

        double cbe2 = T.R("c_be2", "Eq(83)", "Face width factor", 0.5, "");

        return new PitchConeResult
        {
            u = u,
            delta1 = final.delta1,
            delta2 = final.delta2,
            Rm1 = final.Rm1,
            Rm2 = final.Rm2,
            beta_m1 = final.betaM1,
            beta_m2 = In.beta_m2,
            c_be2 = cbe2,
            dm1 = final.dm1,
            dm2 = In.dm2
        };
    }

    // ================================================================= Method 3 (Formulae 84-109)
    private static PitchConeResult Method3(Iso23509Inputs In, CalcTrace T, List<string> warnings)
    {
        T.Section("Clause 6.2.4: Pitch cone parameters, Method 3 (Klingelnberg)");
        double u = T.R("u", "Eq(84)", "Gear ratio", (double)In.z2 / In.z1, "");
        double F = T.R("F (seed)", "Eq(85)", "Hypoid dimension factor (starting value)", 1.0, "");
        double delta2 = T.R("delta2 (seed)", "Eq(86)", "Wheel pitch angle (starting value)", Atan(Sin(In.Sigma) / (F / u + Cos(In.Sigma))), "deg");
        double delta1 = T.R("delta1 (seed)", "Eq(87)", "Pinion pitch angle (starting value)", In.Sigma - delta2, "deg");

        const int maxIter = 60;
        // Each pass re-evaluates Formulae (88)-(108) in sequence from the current (delta1,
        // delta2, F) state, matching the standard's own "repeat calculation from Formula (88)
        // to Formula (108)" fixed-point recipe; called silently until convergence, then once
        // more with tracing enabled to reproduce the "Last trial" style of Annex F.
        (double residualRm, double dm2, double zetaM, double zetaMp, double mMn, double betaM1, double dm1, double Rm1, double Rm2, double Rmint, double delta1, double delta2, double F)
            Pass(double delta1In, double delta2In, double Fin, CalcTrace? trace)
        {
            double dm2p = In.de2 - In.b2 * Sin(delta2In);
            trace?.R("dm2", "Eq(88)", "Wheel mean pitch diameter", dm2p, "mm");
            double zetaMp2 = Asin(2.0 * In.a / (dm2p * (1.0 + Fin * Cos(delta2In) / (u * Cos(delta1In)))));
            trace?.R("zeta_m", "Eq(89)", "Pinion offset angle in axial plane", zetaMp2, "deg");
            double delta1New = Asin(Cos(zetaMp2) * Sin(In.Sigma) * Cos(delta2In) - Cos(In.Sigma) * Sin(delta2In));
            trace?.R("delta1", "Eq(90)", "Pinion pitch angle", delta1New, "deg");
            double zetaMpAngle = Asin(Sin(zetaMp2) * Sin(In.Sigma) / Cos(delta1New));
            trace?.R("zeta_mp", "Eq(91)", "Pinion offset angle in pitch plane", zetaMpAngle, "deg");
            double mMnVal = Cos(In.beta_m2) * dm2p / In.z2;
            trace?.R("m_mn", "Eq(92)", "Mean normal module", mMnVal, "mm");
            double betaM1Val = In.beta_m2 + zetaMpAngle;
            trace?.R("beta_m1", "Eq(93)", "Spiral angle, pinion", betaM1Val, "deg");
            double Fnew = Cos(In.beta_m2) / Cos(betaM1Val);
            trace?.R("F", "Eq(94)", "Hypoid dimension factor", Fnew, "");
            double dm1Val = dm2p / u * Fnew;
            trace?.R("dm1", "Eq(95)", "Pinion mean pitch diameter", dm1Val, "mm");
            double Rm1Val = dm1Val / (2.0 * Sin(delta1New));
            trace?.R("Rm1", "Eq(96)", "Mean cone distance, pinion", Rm1Val, "mm");
            double Rm2Val = dm2p / (2.0 * Sin(delta2In));
            trace?.R("Rm2", "Eq(97)", "Mean cone distance, wheel", Rm2Val, "mm");
            double nu = Asin(In.z0 * mMnVal / (2.0 * In.rc0));
            trace?.R("nu", "Eq(98)", "Lead angle of cutter", nu, "deg");
            double thetaM = Atan(Sin(delta2In) * Tan(zetaMp2));
            trace?.R("theta_m", "Eq(99)", "Auxiliary angle", thetaM, "deg");
            double A3 = In.rc0 * Cos(In.beta_m2 - nu) * Cos(In.beta_m2 - nu);
            trace?.R("A3", "Eq(100)", "Intermediate variable", A3, "mm");
            double A4 = Rm2Val * Cos(In.beta_m2 + thetaM) * Cos(In.beta_m2);
            trace?.R("A4", "Eq(101)", "Intermediate variable", A4, "mm");
            double A5 = Sin(zetaMpAngle) * Cos(thetaM) * Cos(nu);
            trace?.R("A5", "Eq(102)", "Intermediate variable", A5, "");
            double A6 = Rm2Val * Cos(In.beta_m2) + In.rc0 * Sin(nu);
            trace?.R("A6", "Eq(103)", "Intermediate variable", A6, "mm");
            double A7 = Cos(betaM1Val) * Cos(In.beta_m2 + thetaM) -
                Sin(In.beta_m2 + thetaM - nu) * Sin(zetaMpAngle) / Cos(In.beta_m2 - nu);
            trace?.R("A7", "Eq(104)", "Intermediate variable", A7, "");
            double RmintVal = A3 * A4 / (A5 * A6 + A3 * A7);
            trace?.R("Rmint", "Eq(105)", "Intermediate variable", RmintVal, "mm");
            double residual = RmintVal - Rm1Val;
            trace?.R("|Rmint-Rm1|", "Eq(106)", "Check", Math.Abs(residual), "mm");

            double delta1Final = Asin(dm1Val / (2.0 * RmintVal));
            trace?.R("delta1'", "Eq(107)", "Pinion pitch angle (improved)", delta1Final, "deg");
            double delta2Final = Acos((Sin(delta1Final) * Cos(zetaMp2) * Sin(In.Sigma) + Cos(delta1Final) * Cos(zetaMpAngle) * Cos(In.Sigma))
                / (1.0 - Sin(In.Sigma) * Sin(In.Sigma) * Sin(zetaMp2) * Sin(zetaMp2)));
            trace?.R("delta2'", "Eq(108)", "Wheel pitch angle (improved)", delta2Final, "deg");

            return (residual, dm2p, zetaMp2, zetaMpAngle, mMnVal, betaM1Val, dm1Val, Rm1Val, Rm2Val, RmintVal, delta1Final, delta2Final, Fnew);
        }

        // F (like delta1/delta2) carries forward from the Formula (94) of one pass into the
        // Formula (89) of the next: only Formulae (88)-(108) are repeated, so F's seed value of
        // 1 (Formula 85, outside that range) is used only on the very first pass.
        double d1 = delta1, d2 = delta2, fCur = F;
        var pass = Pass(d1, d2, fCur, null);
        bool converged = Math.Abs(pass.residualRm) <= 0.0001 * pass.Rm1;
        int passes = 1;
        while (!converged && passes < maxIter)
        {
            d1 = pass.delta1; d2 = pass.delta2; fCur = pass.F;
            pass = Pass(d1, d2, fCur, null);
            converged = Math.Abs(pass.residualRm) <= 0.0001 * pass.Rm1;
            passes++;
        }
        // Re-run the converged pass once more with full tracing, using the state that produced it.
        var final = Pass(d1, d2, fCur, T);
        T.Note(converged
            ? $"Iteration converged after {passes} pass(es): |Rmint-Rm1| = {Math.Abs(final.residualRm):0.00000} mm <= {0.0001 * final.Rm1:0.00000} mm."
            : $"WARNING: iteration did not converge within {maxIter} passes.");
        if (!converged)
            warnings.Add("Method 3 pitch-cone iteration did not converge to the standard's 0.0001*Rm1 tolerance - results may be inaccurate.");

        double cbe2 = T.R("c_be2", "Eq(109)", "Face width factor", 0.5, "");

        return new PitchConeResult
        {
            u = u,
            delta1 = final.delta1,
            delta2 = final.delta2,
            Rm1 = final.Rm1,
            Rm2 = final.Rm2,
            beta_m1 = final.betaM1,
            beta_m2 = In.beta_m2,
            c_be2 = cbe2,
            dm1 = final.dm1,
            dm2 = final.dm2
        };
    }
}
