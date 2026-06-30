namespace KidGuard.Api.Entities;

public class Mode
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<Device> Devices { get; set; } = new List<Device>();
}
