namespace AGMA2003D19Calc.Models;

/// <summary>Basic bevel gear tooth form (clause 1.1). Governs the lengthwise curvature
/// factor K_x (Y_beta), clause 14, and which C(M) branch (straight/zerol vs spiral) applies
/// to the face-contact-ratio and load-sharing equations in Annex C(M).</summary>
public enum GearType
{
    StraightBevel,
    ZerolBevel,
    SpiralBevel
}

/// <summary>Which flank of a spiral bevel tooth is being analyzed (Annex C(M).2.2/.3.4.3):
/// normally the pinion concave / gear convex flanks are the loaded (driving) surfaces.</summary>
public enum ToothSide
{
    Concave,
    Convex
}

/// <summary>Source of the pitting/bending geometry factors, I (Z_I) and J (Y_J), clause 15.
/// Annex C(M) gives the exact analytical method; the standard's own Annex E worked example
/// instead reads I and J off an Annex D chart because its proportions happen to match one
/// exactly - both paths are offered here.</summary>
public enum GeometryFactorMode
{
    ComputedAnnexC,
    ManualEntry
}

/// <summary>Source of the dynamic factor K_v (clause 10): computed from an AGMA transmission
/// accuracy number, Q_v (10.6), or a directly supplied value (10.7/10.8, e.g. unity per 10.8
/// or a value from a full dynamic/vibration analysis per 10.7).</summary>
public enum DynamicFactorMode
{
    FromTransmissionAccuracy,
    Manual
}

/// <summary>Mounting condition for the load distribution factor K_m (K_Hbeta), clause 12 /
/// Figure 4: governs K_mb.</summary>
public enum MountingCondition
{
    BothMembersStraddleMounted,
    OneMemberStraddleMounted,
    NeitherMemberStraddleMounted
}

/// <summary>Material/heat-treatment selection for the allowable stress numbers, s_ac
/// (sigma_H lim) and s_at (sigma_F lim), clause 21 / Tables 3-6. ManualOverride lets the
/// user supply s_ac and s_at directly (e.g. from other qualified test data).</summary>
public enum MaterialSelection
{
    SteelThroughHardened,
    SteelFlameOrInductionHardened,
    SteelCarburizedCaseHardened,
    SteelNitridedAisi4140,
    SteelNitridedNitralloy135M,
    CastIronClass30,
    CastIronClass40,
    DuctileIronGrade80_55_06,
    DuctileIronGrade120_90_02,
    ManualOverride
}

/// <summary>Material quality grade, Table 3/5/8-11. Not every grade is available for every
/// material selection (e.g. only Grade 2 is tabulated for nitrided AISI 4140 / Nitralloy).</summary>
public enum MaterialGrade
{
    Grade1,
    Grade2,
    Grade3
}

/// <summary>For through/flame/induction hardened steel bending stress numbers (Table 5),
/// whether the root fillet itself is hardened.</summary>
public enum RootHardening
{
    UnhardenedRoots,
    HardenedRoots
}

/// <summary>Reliability requirement of the application, Table 2 - selects C_R (Z_Z) and
/// K_R (Y_Z).</summary>
public enum ReliabilityLevel
{
    OneFailureIn10000,
    OneFailureIn1000,
    OneFailureIn100,
    OneFailureIn10,
    OneFailureIn2
}

/// <summary>Hardness-ratio factor mode, clause 17: equal-hardness materials (C_H = Z_W = 1.0,
/// 17.3), a through-hardened pair where the pinion is harder (17.1, Figure 7 / Eq 27-28), or
/// a surface-hardened pinion run against a through-hardened gear (17.2, Figure 8 / Eq 29-30).</summary>
public enum HardnessRatioMode
{
    EqualHardness,
    ThroughHardenedHarderPinion,
    SurfaceHardenedPinion
}
