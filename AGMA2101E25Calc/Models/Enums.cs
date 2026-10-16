// NOTE: block namespace / C# 7.3 style so these enums can be ported unchanged into IGDv4.0.
namespace AGMA2101E25Calc.Models
{
    /// <summary>Gear material and heat treatment, clause 15 (Tables 4 to 7, Figures 7 to 10).</summary>
    public enum GearMaterial
    {
        SteelThroughHardened,
        SteelFlameOrInductionHardenedTypeA,
        SteelFlameOrInductionHardenedTypeB,
        SteelCarburizedAndHardened,
        SteelNitridedThroughHardenedAlloy,
        SteelNitrided2p5Chrome,
        SteelNitridedNitralloy135M,
        SteelNitridedNitralloyN,
        GrayCastIronClass20,
        GrayCastIronClass30,
        GrayCastIronClass40,
        DuctileIron60_40_18,
        DuctileIron80_55_06,
        DuctileIron100_70_03,
        DuctileIron120_90_02,
        BronzeSandCast,
        BronzeAlloy954HeatTreated
    }

    /// <summary>Metallurgical quality grade for steel gears, clause 15 (Tables 8 to 11).</summary>
    public enum MaterialGrade
    {
        Grade1 = 1,
        Grade2 = 2,
        Grade3 = 3
    }

    /// <summary>Type of generating tool, AGMA 908-B89 clause 5.5.</summary>
    public enum GeneratingToolType
    {
        /// <summary>Hob, rack cutter or generating grinding wheel (modelled with nc = 10 000).</summary>
        RackTypeTool,
        /// <summary>Pinion-type shaper cutter with the tooth number given below.</summary>
        ShaperCutter
    }

    /// <summary>How the transmitted load is specified (clause 7.5).</summary>
    public enum LoadSpecification
    {
        TransmittedPower,
        PinionTorque
    }

    /// <summary>Source of the geometry factors Z_I and Y_J (clause 10).</summary>
    public enum GeometryFactorSource
    {
        /// <summary>Analytical method of AGMA 908-B89 from the gear and tool geometry.</summary>
        CalculatedPerAgma908,
        /// <summary>Values entered directly (e.g. from AGMA 908 tables or another analysis).</summary>
        UserSpecified
    }

    /// <summary>Determination of the dynamic factor K_V (clause 11).</summary>
    public enum DynamicFactorMethod
    {
        /// <summary>Method B, Eq 21/22, with the flank tolerance class number A_v entered directly.</summary>
        MethodBFromAv,
        /// <summary>Method B with A_v estimated from the single pitch deviations f_pt, Eq 24.</summary>
        MethodBFromPitchDeviation,
        /// <summary>Method A (measurement / analysis) or "very accurate gearing" (11.2.1): K_V entered directly.</summary>
        UserValue
    }

    /// <summary>Determination of the load distribution factor K_H (clause 14).</summary>
    public enum LoadDistributionMethod
    {
        /// <summary>Method B, empirical, Eq 31 - 35.</summary>
        MethodBEmpirical,
        /// <summary>Method A (e.g. AGMA 927) or other analysis: K_H entered directly.</summary>
        UserValue
    }

    /// <summary>Lead correction factor K_Hmc (clause 14.2).</summary>
    public enum LeadModification
    {
        /// <summary>Unmodified leads, K_Hmc = 1.0.</summary>
        Unmodified,
        /// <summary>Leads properly modified by crowning or lead correction, K_Hmc = 0.8.</summary>
        CrownedOrLeadCorrected
    }

    /// <summary>Mesh alignment factor curves, Figure 6 / Table 3.</summary>
    public enum MeshAlignmentCurve
    {
        Curve1OpenGearing,
        Curve2CommercialEnclosed,
        Curve3PrecisionEnclosed,
        Curve4ExtraPrecisionEnclosed
    }

    /// <summary>Source of the elastic coefficient Z_E (clause 12).</summary>
    public enum ElasticCoefficientSource
    {
        /// <summary>Table 2 value for the material pair; falls back to Eq 26 for pairs Table 2 does not list.</summary>
        Table2,
        /// <summary>Eq 26 from E and Poisson's ratio, rounded to three significant figures.</summary>
        Equation26,
        UserValue
    }

    /// <summary>Region of the stress cycle factor curves beyond the knee (Tables 12 and 13).</summary>
    public enum StressCycleCurve
    {
        /// <summary>Upper curves, general applications.</summary>
        GeneralApplication,
        /// <summary>Lower curves, critical service (clause 7.2).</summary>
        CriticalService
    }

    /// <summary>Reliability requirement, Table 14.</summary>
    public enum ReliabilityRequirement
    {
        FewerThanOneFailureIn10000,
        FewerThanOneFailureIn1000,
        FewerThanOneFailureIn100,
        FewerThanOneFailureIn10,
        FewerThanOneFailureIn2,
        UserValue
    }
}
