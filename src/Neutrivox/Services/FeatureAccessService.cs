using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record FeatureAccessResult(
    bool Allowed,
    ProductEdition Edition,
    CommercialFeature Feature,
    string MessageRu,
    string MessageEn);

public sealed class FeatureAccessService
{
    private readonly CommercialFeatureMatrixService _matrix = new();
    private readonly CommercialFeatureCatalogService _catalog = new();

    public FeatureAccessResult Check(ProductEdition edition, CommercialFeature feature)
    {
        var definition = _catalog.Find(feature);
        if (definition is null)
            return new(false, edition, feature, "Функция не зарегистрирована.", "Feature is not registered.");

        var allowed = _matrix.IsAtLeast(edition, definition.MinimumEdition);
        return allowed
            ? new(true, edition, feature, "Функция доступна в текущей редакции.", "Feature is available in the current edition.")
            : new(false, edition, feature,
                $"Функция доступна начиная с редакции {definition.MinimumEdition}.",
                $"This feature is available starting with the {definition.MinimumEdition} edition.");
    }
}
