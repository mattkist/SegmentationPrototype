namespace Segmentation.Domain.Entities;

public class FarmerCluster
{
    public Guid Id { get; set; }

    public required string Code { get; set; }

    public required string Name { get; set; }

    public ICollection<FarmerClusterFarmer> Farmers { get; set; } = new List<FarmerClusterFarmer>();
}
