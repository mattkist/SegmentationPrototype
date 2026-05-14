namespace Segmentation.Domain.Entities;

public class FarmerClusterFarmer
{
    public Guid ClusterId { get; set; }
    public FarmerCluster Cluster { get; set; } = null!;

    public Guid FarmerId { get; set; }
    public Farmer Farmer { get; set; } = null!;
}
