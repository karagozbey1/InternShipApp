using Core.Entity;

public class Slider
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int DisplayOrder { get; set; } = 0;

    public ICollection<SliderItem>? Items { get; set; }
}
