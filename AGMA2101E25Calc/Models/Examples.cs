using AGMA2101E25Calc.Calc;

namespace AGMA2101E25Calc.Models;

/// <summary>Built-in example input sets, used by the example selector and the self-test.</summary>
public static class Examples
{
    public static readonly string[] Names =
    {
        "Annex B - Illustrative example (Z_I, Y_J given)",
        "Helical 21/35, 30 deg - geometry of AGMA 908 table p.49",
        "Spur 21/35, HPSTC - geometry of AGMA 908 table p.41",
        "Helical 21/35 - pinion torque for S_H = 1.3, S_F = 1.6 at 1800 rpm",
    };

    public static AgmaInputs Create(int index) => index switch
    {
        0 => AnnexB(),
        1 => Helical908Table(),
        2 => Spur908Table(),
        3 => HelicalRatedTorque(),
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>
    /// The helical pair of <see cref="Helical908Table"/> rated for the required safety factors S_H = 1.3, S_F = 1.6:
    /// the pinion torque (and the power) it can transmit at 1800 rpm for 20 000 h.
    /// </summary>
    public static AgmaInputs HelicalRatedTorque()
    {
        AgmaInputs inputs = Helical908Table();
        inputs.Goal = RatingGoal.RatedLoadForRequiredSafetyFactors;
        inputs.LoadInput = LoadSpecification.PinionTorque;
        inputs.MinSafetyFactorPitting = 1.3;
        inputs.MinSafetyFactorBending = 1.6;
        return inputs;
    }

    /// <summary>
    /// ANSI/AGMA 2101-E25 Annex D.2 (fatigue life by Miner's rule): the Annex B gear set (d_w1 = 70.338462 mm, K_H =
    /// 1.17938, K_V by method B with Av = 6: B = 0.25, C = 92) under the six bins of the D.2 load spectrum, with the
    /// desired safety factors S_H = S_F = 1. Not one of <see cref="Names"/>: used by the self-test.
    /// </summary>
    public static AgmaInputs AnnexD2()
    {
        AgmaInputs inputs = AnnexB();
        inputs.Goal = RatingGoal.FatigueLifeForLoadSpectrum;
        inputs.LoadSpectrum = "700 3052 1000; 800 2670 2000; 900 2374 4000; 1000 2136 1500; 1200 1780 1000; 1500 1424 500";
        return inputs;
    }

    /// <summary>ANSI/AGMA 2101-E25 Annex B input data (B.1). Z_I, Y_J and Z_E are given by the standard.</summary>
    public static AgmaInputs AnnexB() => new()
    {
        NormalModule = 4.5,
        NormalPressureAngle = 20.0,
        HelixAngle = 11.0,
        z1 = 15,
        z2 = 63,
        FaceWidthPinion = 76.2,
        FaceWidthWheel = 76.2,
        OperatingCenterDistance = 182.88,
        PinionSpeed = 1450.0,
        LoadInput = LoadSpecification.PinionTorque,
        PinionTorque = 1494.0,
        LifeHours = 10000.0,
        CycleCurve = StressCycleCurve.GeneralApplication,
        OverloadFactor = 1.0,
        MeshLoadFactor = 1.0,
        TemperatureFactor = 1.0,
        Reliability = ReliabilityRequirement.FewerThanOneFailureIn100,
        MinSafetyFactorPitting = 1.0,
        MinSafetyFactorBending = 1.0,
        KvMethod = DynamicFactorMethod.MethodBFromAv,
        Av = 6,
        KhMethod = LoadDistributionMethod.MethodBEmpirical,
        Lead = LeadModification.CrownedOrLeadCorrected,         // K_Hmc = 0.8
        AlignmentCurve = MeshAlignmentCurve.Curve3PrecisionEnclosed,
        AdjustedOrLapped = false,                               // K_He = 1
        BearingSpan = 331.72,
        PinionOffset = 67.56,
        GeometryFactors = GeometryFactorSource.UserSpecified,
        ZIUser = 0.231,
        YJ1User = 0.505,
        YJ2User = 0.521,
        ZESource = ElasticCoefficientSource.Table2,             // steel/steel: 190
        ZWUser = 1.0,
        MaterialPinion = GearMaterial.SteelCarburizedAndHardened,
        GradePinion = MaterialGrade.Grade2,                     // sigma_HP = 1550, sigma_FP = 450
        HardnessHRCPinion = 58.0,
        MaterialWheel = GearMaterial.SteelCarburizedAndHardened,
        GradeWheel = MaterialGrade.Grade2,
        HardnessHRCWheel = 58.0,
        ContactsPerRevPinion = 1.0,
        ContactsPerRevWheel = 1.0,
    };

    /// <summary>
    /// Helical pair with the geometry of the AGMA 908-B89 table on page 49 (20 deg normal pressure
    /// angle, 30 deg helix, 2.25 whole depth factor, 0.25 tool edge radius, x = +/-0.25, 0.024 mn
    /// thinning, m_F = 2.0, standard center distance) at mn = 5 mm. The table lists I = 0.174,
    /// J = 0.49 / 0.44 for 21/35 teeth. Material and operating data are illustrative.
    /// </summary>
    public static AgmaInputs Helical908Table()
    {
        const double mn = 5.0;
        double b = 2.0 * Math.PI * mn / Math.Sin(30.0 * Math.PI / 180.0);    // m_F = 2.0
        return new AgmaInputs
        {
            NormalModule = mn,
            NormalPressureAngle = 20.0,
            HelixAngle = 30.0,
            z1 = 21,
            z2 = 35,
            FaceWidthPinion = b,
            FaceWidthWheel = b,
            OperatingCenterDistance = 0.0,
            AddendumCoefficient = 1.0,
            x1 = 0.25,
            x2 = -0.25,
            ThinningPinion = 0.024 * mn,
            ThinningWheel = 0.024 * mn,
            PinionSpeed = 1800.0,
            LoadInput = LoadSpecification.TransmittedPower,
            TransmittedPower = 110.0,
            LifeHours = 20000.0,
            KvMethod = DynamicFactorMethod.MethodBFromAv,
            Av = 8,
            KhMethod = LoadDistributionMethod.MethodBEmpirical,
            Lead = LeadModification.Unmodified,
            AlignmentCurve = MeshAlignmentCurve.Curve2CommercialEnclosed,
            BearingSpan = 250.0,
            PinionOffset = 0.0,
            GeometryFactors = GeometryFactorSource.CalculatedPerAgma908,
            ZESource = ElasticCoefficientSource.Table2,
            ZWUser = 0.0,
            ToolAddendumPinion = 1.25,
            ToolTipRadiusPinion = 0.25,
            ToolAddendumWheel = 1.25,
            ToolTipRadiusWheel = 0.25,
            MaterialPinion = GearMaterial.SteelThroughHardened,
            GradePinion = MaterialGrade.Grade1,
            HardnessHBPinion = 300.0,
            MaterialWheel = GearMaterial.SteelThroughHardened,
            GradeWheel = MaterialGrade.Grade1,
            HardnessHBWheel = 250.0,
        };
    }

    /// <summary>
    /// Spur pair with the geometry of the AGMA 908-B89 table on page 41 (20 deg, 2.25 whole depth
    /// factor, 0.25 tool edge radius, x = 0, 0.024 mn thinning, loaded at the HPSTC, standard center
    /// distance) at mn = 4 mm. The table lists I = 0.091, J = 0.34 / 0.37 for 21/35 teeth.
    /// Material and operating data are illustrative.
    /// </summary>
    public static AgmaInputs Spur908Table()
    {
        const double mn = 4.0;
        return new AgmaInputs
        {
            NormalModule = mn,
            NormalPressureAngle = 20.0,
            HelixAngle = 0.0,
            z1 = 21,
            z2 = 35,
            FaceWidthPinion = 40.0,
            FaceWidthWheel = 40.0,
            OperatingCenterDistance = 0.0,
            AddendumCoefficient = 1.0,
            x1 = 0.0,
            x2 = 0.0,
            ThinningPinion = 0.024 * mn,
            ThinningWheel = 0.024 * mn,
            PinionSpeed = 3000.0,
            LoadInput = LoadSpecification.PinionTorque,
            PinionTorque = 200.0,
            LifeHours = 10000.0,
            KvMethod = DynamicFactorMethod.MethodBFromAv,
            Av = 7,
            KhMethod = LoadDistributionMethod.MethodBEmpirical,
            Lead = LeadModification.CrownedOrLeadCorrected,
            AlignmentCurve = MeshAlignmentCurve.Curve3PrecisionEnclosed,
            BearingSpan = 150.0,
            PinionOffset = 15.0,
            GeometryFactors = GeometryFactorSource.CalculatedPerAgma908,
            SpurLoadPoint = SpurLoadPoint.HPSTC,
            ZESource = ElasticCoefficientSource.Table2,
            ZWUser = 0.0,
            ToolAddendumPinion = 1.25,
            ToolTipRadiusPinion = 0.25,
            ToolAddendumWheel = 1.25,
            ToolTipRadiusWheel = 0.25,
            MaterialPinion = GearMaterial.SteelCarburizedAndHardened,
            GradePinion = MaterialGrade.Grade2,
            HardnessHRCPinion = 60.0,
            MaterialWheel = GearMaterial.SteelCarburizedAndHardened,
            GradeWheel = MaterialGrade.Grade2,
            HardnessHRCWheel = 60.0,
        };
    }
}
