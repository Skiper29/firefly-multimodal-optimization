namespace firefly_multimodal_optimization.Models;

public class Alternative
{
    public string Name { get; set; }
    public Dictionary<string,double> CriteriaValues { get; set; }
    public double FitnessValue { get; set; }

    public Alternative(string name)
    {
        Name = name;
        CriteriaValues = new Dictionary<string, double>();
        FitnessValue = 0;
    }
    
    public double CalculateFitness(List<Criterion> criteria)
    {
        FitnessValue = 0;
        foreach (var criterion in criteria)
        {
            double value = CriteriaValues[criterion.Name];
            // value = criterion.IsMaximization ? value : 1.0 - value;
            FitnessValue += criterion.Weight * value;
        }

        return FitnessValue;
    }
}