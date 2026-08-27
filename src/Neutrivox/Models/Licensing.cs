namespace Neutrivox.Models;

public enum ProductEdition { Free, Standard, Professional, Business, Owner }
public enum LicenseState { Unknown, Trial, Active, Expired, Invalid }

public sealed record LicenseSnapshot(
    ProductEdition Edition,
    LicenseState State,
    DateTimeOffset? ExpiresAtUtc,
    string DisplayName)
{
    public bool HasProfessionalFeatures => Edition is ProductEdition.Professional or ProductEdition.Business or ProductEdition.Owner;
    public bool IsUsable => State is LicenseState.Trial or LicenseState.Active;
}

public static class EditionFeatures
{
    public static readonly IReadOnlyDictionary<ProductEdition, string[]> Features = new Dictionary<ProductEdition, string[]>
    {
        [ProductEdition.Free] = ["Basic projects", "Basic equipment catalog", "Project viewing"],
        [ProductEdition.Standard] = ["Extended projects", "Additional tools"],
        [ProductEdition.Professional] = ["Professional tools", "Advanced validation", "Extended configuration"],
        [ProductEdition.Business] = ["Professional features", "Team and business capabilities"],
        [ProductEdition.Owner] = ["All available features"]
    };
}
