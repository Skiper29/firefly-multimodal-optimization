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
public partial class MainWindow
{
    private readonly List<Criterion> _criteria;
    private readonly List<Alternative> _alternatives;

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
        
        AddPredefinedAlternatives();
        UpdateAlternativesDataGridColumns();
        _criteria = CriteriaList.ToList();
        _alternatives = AlternativesList.ToList();
        
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
        
        NormalizeCriteriaValues();

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

            ResultBox.Text =
                $"Найкраща альтернатива: {resultAlternative.Name}, Придатність: {resultAlternative.FitnessValue:F4}\n\n";

            foreach (var alternative in firefly.GetAlternatives())
            {
                ResultBox.Text += $"{alternative.Name}: {alternative.FitnessValue}\n";
            }
            
        }
    }

    private void BackToCriteriaScreen(object sender, RoutedEventArgs e)
    {
        AlternativesScreen.Visibility = Visibility.Collapsed;
        CriteriaScreen.Visibility = Visibility.Visible;
    }
    
    private void NormalizeCriteriaValues()
    {
        var criteriaNames = CriteriaList.Select(c => c.Name).ToList();

        foreach (var criterionName in criteriaNames)
        {
            // Отримуємо всі значення для даного критерію
            var values = AlternativesList.Select(a => a.CriteriaValues[criterionName]).ToList();
            double minValue = values.Min();
            double maxValue = values.Max();

            foreach (var alternative in AlternativesList)
            {
                double originalValue = alternative.CriteriaValues[criterionName];

                // Якщо критерій потрібно максимізувати
                if (CriteriaList.First(c => c.Name == criterionName).IsMaximization)
                {
                    alternative.CriteriaValues[criterionName] = (originalValue - minValue) / (maxValue - minValue);
                }
                // Якщо критерій потрібно мінімізувати
                else
                {
                    alternative.CriteriaValues[criterionName] = (maxValue - originalValue) / (maxValue - minValue);
                }
            }
        }
        AlternativesDataGrid.Items.Refresh();
    }


    // Метод для додавання 20 альтернатив
    private void AddPredefinedAlternatives()
    {
        const string yield = "Врожайність (ц/га)";
        const string diseaseResistance = "Стійкість до хвороб (бали 1-10)";
        const string irrigationRequirements = "Вимоги до зрошення (л/га)";
        const string climateResistance = "Стійкість до клімату (бали 1-10)";
        const string seedCost = "Вартість насіння (грн/кг)";
        
        // Додаємо критерії (Назва критерію, вага критерію)
        CriteriaList.Add(new Criterion(yield, true, 0.3)); // Максимізація
        CriteriaList.Add(new Criterion(diseaseResistance, true, 0.2)); // Максимізація
        CriteriaList.Add(new Criterion(irrigationRequirements, false, 0.2)); // Мінімізація
        CriteriaList.Add(new Criterion(climateResistance, true, 0.2)); // Максимізація
        CriteriaList.Add(new Criterion(seedCost, false, 0.1)); // Мінімізація


        // Додаємо альтернативи (Сорти пшениці)
        var alternatives = new List<Alternative>
        {
            new("Шестопалівка")
            {
                CriteriaValues =
                {
                    [yield] = 60, [diseaseResistance] = 8,
                    [irrigationRequirements] = 4800, [climateResistance] = 9,
                    [seedCost] = 35
                }
            },
            new("Смуглянка")
            {
                CriteriaValues =
                {
                    [yield] = 55, [diseaseResistance] = 9,
                    [irrigationRequirements] = 5200, [climateResistance] = 8,
                    [seedCost] = 32
                }
            },
            new("Фаворитка")
            {
                CriteriaValues =
                {
                    [yield] = 50, [diseaseResistance] = 7,
                    [irrigationRequirements] = 5000, [climateResistance] = 7,
                    [seedCost] = 30
                }
            },
            new("Подолянка")
            {
                CriteriaValues =
                {
                    [yield] = 58, [diseaseResistance] = 8,
                    [irrigationRequirements] = 4700, [climateResistance] = 8,
                    [seedCost] = 33
                }
            },
            new("Миронівська 61")
            {
                CriteriaValues =
                {
                    [yield] = 52, [diseaseResistance] = 6,
                    [irrigationRequirements] = 4900, [climateResistance] = 7,
                    [seedCost] = 29
                }
            },
            new("Досконала")
            {
                CriteriaValues =
                {
                    [yield] = 54, [diseaseResistance] = 8,
                    [irrigationRequirements] = 5100, [climateResistance] = 7,
                    [seedCost] = 31
                }
            },
            new("Куяльник")
            {
                CriteriaValues =
                {
                    [yield] = 61, [diseaseResistance] = 8,
                    [irrigationRequirements] = 5000, [climateResistance] = 9,
                    [seedCost] = 36
                }
            },
            new("Колосок")
            {
                CriteriaValues =
                {
                    [yield] = 56, [diseaseResistance] = 7,
                    [irrigationRequirements] = 4600, [climateResistance] = 8,
                    [seedCost] = 32
                }
            },
            new("Галичанка")
            {
                CriteriaValues =
                {
                    [yield] = 57, [diseaseResistance] = 8,
                    [irrigationRequirements] = 4700, [climateResistance] = 8,
                    [seedCost] = 34
                }
            },
            new("Одеська 267")
            {
                CriteriaValues =
                {
                    [yield] = 53, [diseaseResistance] = 6,
                    [irrigationRequirements] = 4800, [climateResistance] = 7,
                    [seedCost] = 28
                }
            },
            new("Фортуна")
            {
                CriteriaValues =
                {
                    [yield] = 60, [diseaseResistance] = 9,
                    [irrigationRequirements] = 5200, [climateResistance] = 9,
                    [seedCost] = 37
                }
            },
            new("Бунчук")
            {
                CriteriaValues =
                {
                    [yield] = 55, [diseaseResistance] = 8,
                    [irrigationRequirements] = 4900, [climateResistance] = 8,
                    [seedCost] = 30
                }
            },
            new("Мрія")
            {
                CriteriaValues =
                {
                    [yield] = 62, [diseaseResistance] = 9,
                    [irrigationRequirements] = 5400, [climateResistance] = 9,
                    [seedCost] = 38
                }
            },
            new("Придніпровська")
            {
                CriteriaValues =
                {
                    [yield] = 54, [diseaseResistance] = 7,
                    [irrigationRequirements] = 4800, [climateResistance] = 8,
                    [seedCost] = 29
                }
            },
            new("Альянс")
            {
                CriteriaValues =
                {
                    [yield] = 59, [diseaseResistance] = 8,
                    [irrigationRequirements] = 5000, [climateResistance] = 8,
                    [seedCost] = 35
                }
            },
            new("Степовий")
            {
                CriteriaValues =
                {
                    [yield] = 60, [diseaseResistance] = 8,
                    [irrigationRequirements] = 5300, [climateResistance] = 7,
                    [seedCost] = 34
                }
            },
            new("Злагода")
            {
                CriteriaValues =
                {
                    [yield] = 53, [diseaseResistance] = 6,
                    [irrigationRequirements] = 4700, [climateResistance] = 7,
                    [seedCost] = 28
                }
            },
            new("Лебідь")
            {
                CriteriaValues =
                {
                    [yield] = 57, [diseaseResistance] = 8,
                    [irrigationRequirements] = 4900, [climateResistance] = 9,
                    [seedCost] = 33
                }
            },
            new("Добродій")
            {
                CriteriaValues =
                {
                    [yield] = 61, [diseaseResistance] = 9,
                    [irrigationRequirements] = 5200, [climateResistance] = 8,
                    [seedCost] = 36
                }
            }
        };

        foreach (var alternative in alternatives)
        {
            AlternativesList.Add(alternative);
        }
    }
}