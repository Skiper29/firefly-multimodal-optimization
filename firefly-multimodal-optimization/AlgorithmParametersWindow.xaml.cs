using System.Windows;

namespace firefly_multimodal_optimization;

public partial class AlgorithmParametersWindow : Window
{
    public int Iterations { get; private set; }
    public double Alpha { get; private set; }
    public double Beta0 { get; private set; }
    public double Gamma { get; private set; }

    public AlgorithmParametersWindow()
    {
        InitializeComponent();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        // Перевірка правильності введених значень
        if (!int.TryParse(IterationsTextBox.Text, out int iterations) || iterations <= 0)
        {
            MessageBox.Show("Значення Iterations повинно бути цілим числом більше 0");
            return;
        }
        
        if (!double.TryParse(AlphaTextBox.Text, out double alpha) || alpha < 0 || alpha > 1)
        {
            MessageBox.Show("Значення Alpha повинно бути числом від 0 до 1");
            return;
        }

        if (!double.TryParse(Beta0TextBox.Text, out double beta0) || beta0 < 0 || beta0 > 1)
        {
            MessageBox.Show("Значення Beta0 повинно бути числом від 0 до 1");
            return;
        }

        if (!double.TryParse(GammaTextBox.Text, out double gamma) || gamma < 0 || gamma > 1)
        {
            MessageBox.Show("Значення Gamma повинно бути числом від 0 до 1");
            return;
        }

        // Зберігаємо введені значення
        Iterations = iterations;
        Alpha = alpha;
        Beta0 = beta0;
        Gamma = gamma;

        // Закриваємо вікно
        this.DialogResult = true;
        this.Close();
    }
}