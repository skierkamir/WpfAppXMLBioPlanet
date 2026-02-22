using System.Windows;
using WpfAppXMLBioPlanet.Services;
using WpfAppXMLBioPlanet.ViewModels;

namespace WpfAppXMLBioPlanet.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Ustawienie ViewModelu
            this.DataContext = new WpfAppXMLBioPlanet.ViewModels.MainViewModel();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                // Sprawdź, czy brak ustawień API lub brak magazynowego API
                if (string.IsNullOrWhiteSpace(vm.ApiAddress) ||
                    string.IsNullOrWhiteSpace(vm.ApiKey) ||
                    string.IsNullOrWhiteSpace(vm.ApiAddress2)) // 🔹 trzeci warunek
                {
                    var settingsWindow = new ApiSettingsWindow(vm.ApiAddress, vm.ApiKey, vm.ApiAddress2)
                    {
                        Owner = this
                    };

                    if (settingsWindow.ShowDialog() == true)
                    {
                        vm.ApiAddress = settingsWindow.ApiAddress;
                        vm.ApiKey = settingsWindow.ApiKey;
                        vm.ApiAddress2 = settingsWindow.ApiAddress2; // 🔹 zapisujemy trzeci adres

                        ApiSettingsService.Save(new ApiSettings
                        {
                            ApiAddress = vm.ApiAddress,
                            ApiKey = vm.ApiKey,
                            ApiAddress2 = vm.ApiAddress2 // 🔹 zapisujemy trzeci adres do pliku
                        });
                    }
                }
            }
        }

    }
}
