namespace api_sample.Controllers.DTO;

public class PlaceDto
{
    public string placeId { get; set;} = string.Empty;
    public double[] coordinates { get; set; } = new double[] {};

    public string[] availableAssetIds { get; set;} = new string[] {};
}