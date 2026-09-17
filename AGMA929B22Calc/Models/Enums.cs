namespace AGMA929B22Calc.Models;

/// <summary>Manufacturing (cutting) method — governs which spiral-angle and cone-distance
/// formulas apply (AGMA 929-B22, 4.2 and 4.3).</summary>
public enum CuttingMethod
{
    FaceMilling,
    FaceHobbing
}

/// <summary>Whether the wheel (gear) member is generated or non-generated (Formate).
/// Drives Eq (59)/(60), (122)-(127), (171)/(172), (179)/(180), (181)/(182), (210)/(211).</summary>
public enum WheelGeneration
{
    Generated,
    NonGenerated
}

/// <summary>Manufacturing process for a member — completing (one setup) vs. separate
/// rough and finish operations. Drives 4.5.5 and 4.6.2 branching.</summary>
public enum MemberProcess
{
    Completing,
    RoughAndFinish
}

/// <summary>Selects which formula (Eq 65-71) defines the wheel rotation factor k_E,
/// based on the cutting method / machine type used (3.2, 4.5.2).</summary>
public enum KeMethod
{
    [KeInfo("Face hobbing – Gleason or Klingelnberg (k_E = 1.0)")]
    FaceHobbingGleasonOrKlingelnberg,
    [KeInfo("Face hobbing – Oerlikon, not using roughing blade (k_E = 1.0)")]
    FaceHobbingOerlikonNoRoughingBlade,
    [KeInfo("Face hobbing – Oerlikon, using a roughing blade (k_E = 1.3)")]
    FaceHobbingOerlikonWithRoughingBlade,
    [KeInfo("Face milling – Spread blade (k_E = 0.0)")]
    FaceMillingSpreadBlade,
    [KeInfo("Face milling – Unitool and single cycle")]
    FaceMillingUnitoolSingleCycle,
    [KeInfo("Face milling – Versacut and standard single side")]
    FaceMillingVersacutStandardSingleSide,
    [KeInfo("Face milling – Straight bevel and planing generator")]
    FaceMillingStraightBevelPlaningGenerator
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class KeInfoAttribute : Attribute
{
    public string Description { get; }
    public KeInfoAttribute(string description) => Description = description;
}
