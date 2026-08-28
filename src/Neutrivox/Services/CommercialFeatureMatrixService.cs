using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class CommercialFeatureMatrixService
{
    private readonly CommercialFeatureCatalogService _catalog = new();

    public IReadOnlyDictionary<CommercialFeature, bool> Build(ProductEdition edition)
        => _catalog.GetAll().ToDictionary(x => x.Feature, x => IsAtLeast(edition, x.MinimumEdition));

    public IReadOnlyList<CommercialFeatureDefinition> Available(ProductEdition edition)
        => _catalog.GetAll().Where(x => IsAtLeast(edition, x.MinimumEdition)).ToList();

    public bool IsAtLeast(ProductEdition actual, ProductEdition required)
    {
        static int Rank(ProductEdition value) => value switch
        {
            ProductEdition.Free => 0,
            ProductEdition.Standard => 1,
            ProductEdition.Professional => 2,
            ProductEdition.Business => 3,
            ProductEdition.Owner => 4,
            _ => 0
        };
        return Rank(actual) >= Rank(required);
    }
}
