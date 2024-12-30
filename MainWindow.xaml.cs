using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using Microsoft.Win32;
using xml_cars.Models;

namespace xml_cars
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private static ObservableCollection<Car>? LoadCarsFromXml(string filePath)
        {
            if (!File.Exists(filePath)) return null;

            ObservableCollection<Car> carCollection = [];
            XDocument importedXml = XDocument.Load(filePath);

            if (importedXml.Root?.Elements("Car") == null) return null;

            foreach (XElement carElement in importedXml.Root.Elements("Car"))
            {
                var modelName = carElement.Element("ModelName")?.Value;
                var saleDate = carElement.Element("SaleDate")?.Value;
                var price = carElement.Element("Price")?.Value;
                var tax = carElement.Element("Tax")?.Value;

                if (modelName != null && DateTime.TryParse(saleDate, out DateTime parsedDate) &&
                    double.TryParse(price, out double parsedPrice) &&
                    double.TryParse(tax, out double parsedTax))
                {
                    carCollection.Add(new Car
                    {
                        ModelName = modelName,
                        SaleDate = parsedDate,
                        Price = parsedPrice,
                        Tax = parsedTax
                    });
                }
                else
                {
                    MessageBox.Show(
                        $"Neplatné data pro záznam: \nModelName={modelName} \nSaleDate={saleDate} \nPrice={price} \nTax={tax}",
                        "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            return carCollection;
        }

        private void LoadXMLButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new() { Filter = "XML soubor (*.xml)|*.xml" };

            if (fileDialog.ShowDialog() == true)
            {
                try
                {
                    ObservableCollection<Car>? carCollection = LoadCarsFromXml(fileDialog.FileName);

                    if (carCollection != null && carCollection.Count > 0)
                    {
                        WeekendSalesGrid.IsEnabled = true;
                        CarsDataGrid.ItemsSource = carCollection;
                    }
                    else
                    {
                        MessageBox.Show("Soubor XML je prázdný a nebo obsahuje nevalidní data.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Chyba při načítání XML souboru: \n{ex.Message}", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }

        private void CalculateWeekendSalesButton_Click(object sender, RoutedEventArgs e)
        {
            if (CarsDataGrid.ItemsSource is not ObservableCollection<Car> carCollection || carCollection.Count == 0)
            {
                MessageBox.Show("Soubor XML neobsahuje žádné validní data.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var weekendSales = carCollection
                    .Where(car => car.SaleDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    .GroupBy(car => car.ModelName)
                    .Select(group => new
                    {
                        ModelName = group.Key,
                        TotalPrice = group.Sum(car => car.Price),
                        TotalPriceWithTax = group.Sum(car => car.Price * (1 + car.Tax / 100))
                    });
                WeekendSalesDataGrid.ItemsSource = weekendSales;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Vyskytla se chyba při provádění operace: \n{ex.Message}", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}