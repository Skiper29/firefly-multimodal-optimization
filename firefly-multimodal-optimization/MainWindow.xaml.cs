using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using firefly_multimodal_optimization.Algorithm;
using firefly_multimodal_optimization.Models;

namespace firefly_multimodal_optimization;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private List<Criterion> _criteria = new List<Criterion>();
    private List<Alternative> _alternatives = new List<Alternative>();
    
    public ObservableCollection<Criterion> CriteriaList { get; set; }
    public ObservableCollection<Alternative> AlternativesList { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this; // Налаштовуємо DataContext на поточний об'єкт вікна
        
        // Ініціалізація колекцій
        CriteriaList = new ObservableCollection<Criterion>();
        AlternativesList = new ObservableCollection<Alternative>();

        // Прив'язка до DataGrid
        AlternativesDataGrid.ItemsSource = AlternativesList;
    }

    // Властивості для прив'язки


    private void AddCriterion(object sender, RoutedEventArgs e)
    {
        // Перевірка правильності введення
        if (string.IsNullOrWhiteSpace(CriterionName.Text))
        {
            MessageBox.Show("Введіть назву критерію");
            return;
        }

        if (!double.TryParse(Weight.Text, out double weight) || weight <= 0 || weight > 1)
        {
            MessageBox.Show("Вага повинна бути числом від 0 до 1");
            return;
        }
        
        

        bool isMax = IsMaximization.SelectedIndex == 0;
        _criteria.Add(new Criterion(CriterionName.Text, isMax, weight));
        CriteriaList.Add(new Criterion(CriterionName.Text, isMax, weight));
        
        UpdateAlternativesDataGridColumns();
        
        CriterionName.Clear();
        Weight.Clear();

        // Оновлюємо DataGrid за допомогою DataBinding
        CriteriaDataGrid.Items.Refresh();
    }

    private void UpdateAlternativesDataGridColumns()
    {
        AlternativesDataGrid.Columns.Clear(); // Очищаємо попередні стовпці

        // Додавання стовпця для імені альтернативи
        var nameColumn = new DataGridTextColumn
        {
            Header = "Альтернатива",
            Binding = new Binding("Name")
        };
        AlternativesDataGrid.Columns.Add(nameColumn);

        // Додавання стовпців для кожного критерію
        foreach (var criterion in CriteriaList)
        {
            var column = new DataGridTextColumn
            {
                Header = criterion.Name,
                Binding = new Binding($"CriteriaValues[{criterion.Name}]")
            };
            AlternativesDataGrid.Columns.Add(column);
        }
    }

    private void GoToAlternativesScreen(object sender, RoutedEventArgs e)
    {
        if (_criteria.Count == 0)
        {
            MessageBox.Show("Додайте хоча б один критерій");
            return;
        }

        CriteriaScreen.Visibility = Visibility.Collapsed;
        AlternativesScreen.Visibility = Visibility.Visible;
    }

    private void AddAlternative(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(AlternativeName.Text))
        {
            MessageBox.Show("Введіть назву альтернативи");
            return;
        }

        Alternative alternative = new Alternative(AlternativeName.Text);

        foreach (var criterion in _criteria)
        {
            double value;
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                $"Введіть значення критерію {criterion.Name} для альтернативи {AlternativeName.Text}",
                "Значення критерію");

            if (!double.TryParse(input, out value))
            {
                MessageBox.Show("Введіть коректне числове значення для критерію");
                return;
            }

            alternative.CriteriaValues[criterion.Name] = value;
        }

        _alternatives.Add(alternative);
        AlternativesList.Add(alternative);
        AlternativeName.Clear();

        // Оновлюємо DataGrid за допомогою DataBinding
        AlternativesDataGrid.Items.Refresh();
    }

    private void RunAlgorithm(object sender, RoutedEventArgs e)
    {
        if (_alternatives.Count == 0)
        {
            MessageBox.Show("Додайте хоча б одну альтернативу");
            return;
        }

        // Створення та відкриття вікна для введення параметрів алгоритму
        var parametersWindow = new AlgorithmParametersWindow();
        bool? result = parametersWindow.ShowDialog();

        if (result == true)
        {
            // Отримуємо значення з вікна
            int iterations = parametersWindow.Iterations;
            double alpha = parametersWindow.Alpha;
            double beta0 = parametersWindow.Beta0;
            double gamma = parametersWindow.Gamma;

            // Створюємо і запускаємо алгоритм
            FireflyAlgorithm firefly = new FireflyAlgorithm(_alternatives, _criteria, iterations, alpha, beta0, gamma);
            Alternative resultAlternative = firefly.Run();

            ResultBox.Text = $"Найкраща альтернатива: {resultAlternative.Name}, Придатність: {resultAlternative.FitnessValue:F2}";
        }
    }

    private void BackToCriteriaScreen(object sender, RoutedEventArgs e)
    {
        AlternativesScreen.Visibility = Visibility.Collapsed;
        CriteriaScreen.Visibility = Visibility.Visible;
    }
}