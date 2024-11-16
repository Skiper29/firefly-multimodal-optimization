using firefly_multimodal_optimization.Models;

namespace firefly_multimodal_optimization.Algorithm;

public class FireflyAlgorithm
{
    private readonly List<Alternative> _alternatives;
    private readonly List<Criterion> _criteria;
    private readonly int _maxIter;
    private double _alpha;
    private readonly double _beta0;
    private readonly double _gamma;

    public FireflyAlgorithm(List<Alternative> alternatives, List<Criterion> criteria, int maxIter, double alpha,
        double beta0, double gamma)
    {
        _alternatives = alternatives;
        _criteria = criteria;
        _maxIter = maxIter;
        _alpha = alpha;
        _beta0 = beta0;
        _gamma = gamma;
    }

    public Alternative Run()
    {
        double startAlpha = _alpha;
        foreach (var alternative in _alternatives)
            alternative.FitnessValue = alternative.CalculateFitness(_criteria);

        for (int iter = 0; iter < _maxIter; iter++)
        {
            for (int i = 0; i < _alternatives.Count; i++)
            {
                for (int j = 0; j < _alternatives.Count; j++)
                {
                    if (_alternatives[i].FitnessValue < _alternatives[j].FitnessValue)
                    {
                        double rij = Math.Abs(_alternatives[i].FitnessValue - _alternatives[j].FitnessValue);
                        double beta = _beta0 * Math.Exp(-_gamma * rij * rij);
                        _alternatives[i].FitnessValue +=
                            beta * (_alternatives[j].FitnessValue - _alternatives[i].FitnessValue)
                            + _alpha * (new Random().NextDouble() - 0.5);
                    }
                }

                // _alpha = startAlpha * Math.Pow(0.9, iter + 1);
            }
        }

        _alternatives.Sort((a, b) => b.FitnessValue.CompareTo(a.FitnessValue));
        return _alternatives[0];
    }

    public List<Alternative> GetAlternatives()
    {
        return _alternatives;
    }
}