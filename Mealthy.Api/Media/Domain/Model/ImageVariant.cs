namespace Mealthy.Api.Media.Domain.Model;

/// <summary>
/// Tamaños que se generan de cada imagen (lado mayor en píxeles, sin agrandar nunca la original).
/// El frontend elige la variante según dónde la muestra: sm en tarjetas, md en detalle, lg en pantallas grandes.
/// </summary>
public sealed record ImageVariant(string Name, int MaxSize)
{
    public static readonly ImageVariant Small = new("sm", 320);
    public static readonly ImageVariant Medium = new("md", 800);
    public static readonly ImageVariant Large = new("lg", 1600);

    public static readonly IReadOnlyList<ImageVariant> All = [Small, Medium, Large];

    public static ImageVariant? FromName(string? name)
        => All.FirstOrDefault(v => string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase));
}
