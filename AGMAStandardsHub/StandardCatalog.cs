namespace AGMAStandardsHub;

/// <summary>
/// One entry in the standard selector: the display text plus a factory that creates
/// a fresh instance of the calculator's main form when the user opens it.
/// </summary>
internal sealed record StandardEntry(
    string Code,
    string Title,
    string Description,
    Color AccentColor,
    Func<Form> CreateForm);

/// <summary>
/// The catalog shown on the hub's selector screen. To add a newly implemented standard,
/// reference its project from AGMAStandardsHub.csproj and add one entry here.
/// </summary>
internal static class StandardCatalog
{
    public static IReadOnlyList<StandardEntry> All { get; } = new List<StandardEntry>
    {
        new(
            Code: "AGMA 929-B22",
            Title: "Bevel Gear Top Land, Slot Width and Cutter Edge Radius Calculator",
            Description: "Computes top land thickness, slot width, and minimum cutter edge " +
                "radius limits for straight and spiral bevel gears, with Annex C-E examples.",
            AccentColor: Color.FromArgb(0, 122, 153),
            CreateForm: () => new AGMA929B22Calc.MainForm()),

        new(
            Code: "ANSI/AGMA 2003-D19",
            Title: "Bevel Gear Pitting Resistance and Bending Strength Calculator",
            Description: "Rates bevel and hypoid gear sets for surface durability (pitting) and " +
                "root bending strength, validated against the standard's Annex E example.",
            AccentColor: Color.FromArgb(196, 93, 16),
            CreateForm: () => new AGMA2003D19Calc.MainForm()),

        new(
            Code: "ISO 10300:2023",
            Title: "Bevel Gear Surface Durability and Tooth Root Strength Calculator",
            Description: "Calculates pitting and bending safety factors for straight, spiral and " +
                "hypoid bevel gears, validated against ISO/TR 10300-30 worked samples.",
            AccentColor: Color.FromArgb(53, 122, 66),
            CreateForm: () => new ISO10300Calc.MainForm()),
    };
}
