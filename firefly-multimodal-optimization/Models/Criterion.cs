namespace firefly_multimodal_optimization.Models;

public class Criterion
{
    public string Name { get; set; }
    public bool IsMaximization { get; set; }
    public double Weight { get; set; }
    
    public Criterion(string name, bool isMaximization, double weight)
    {
        Name = name;
        IsMaximization = isMaximization;
        Weight = weight;
    }
}