using Neutrivox.Models;

namespace Neutrivox.Services;

public enum CommercialFeature
{
    BasicProjects,
    BasicEquipment,
    BasicIo,
    BasicLogic,
    BasicSimulation,
    ProjectSaveLoad,
    DeviceDiscovery,
    AdvancedSimulation,
    DeploymentPreparation,
    PhysicalDeployment,
    AdvancedDiagnostics,
    BatchDeployment,
    ExtendedDocumentation
}

public sealed class CommercialFeatureGateService
{
    public bool IsAllowed(ProductEdition edition, CommercialFeature feature) => edition switch
    {
        ProductEdition.Owner => true,
        ProductEdition.Professional => feature != CommercialFeature.PhysicalDeployment || true,
        ProductEdition.Standard => feature is CommercialFeature.BasicProjects
            or CommercialFeature.BasicEquipment
            or CommercialFeature.BasicIo
            or CommercialFeature.BasicLogic
            or CommercialFeature.BasicSimulation
            or CommercialFeature.ProjectSaveLoad
            or CommercialFeature.DeviceDiscovery
            or CommercialFeature.AdvancedSimulation
            or CommercialFeature.DeploymentPreparation
            or CommercialFeature.AdvancedDiagnostics
            or CommercialFeature.ExtendedDocumentation,
        _ => feature is CommercialFeature.BasicProjects
            or CommercialFeature.BasicEquipment
            or CommercialFeature.BasicIo
            or CommercialFeature.BasicLogic
            or CommercialFeature.BasicSimulation
            or CommercialFeature.ProjectSaveLoad
    };
}
