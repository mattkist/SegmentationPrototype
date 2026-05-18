namespace Segmentation.Domain.Entities;

/// <summary>
/// Tobacco culture type (Virginia, Burley, Comum).
/// PK is <see cref="Code"/> (short string, e.g. "FCV", "BLY", "CO").
/// </summary>
public class CultureType
{
    public required string Code { get; set; }

    public required string Name { get; set; }
}
