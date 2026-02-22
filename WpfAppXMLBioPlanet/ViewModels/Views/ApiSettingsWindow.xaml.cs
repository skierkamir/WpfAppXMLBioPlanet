using System.Windows;

namespace WpfAppXMLBioPlanet.Views
{
    public partial class ApiSettingsWindow : Window
    {
        public string ApiAddress { get; private set; }
        public string ApiKey { get; private set; }
        public string ApiAddress2 { get; private set; }  // 🔹 nowa właściwość

        // 🔹 konstruktor z trzema parametrami
        public ApiSettingsWindow(string apiAddress, string apiKey, string apiAddress2)
        {
            InitializeComponent();

            ApiAddress = apiAddress;
            ApiKey = apiKey;
            ApiAddress2 = apiAddress2;

            // wypełniamy TextBoxy
            AdresBox.Text = ApiAddress;
            KluczBox.Text = ApiKey;
            AdresMagazynBox.Text = ApiAddress2;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // zapisujemy aktualne wartości z TextBoxów
            ApiAddress = AdresBox.Text;
            ApiKey = KluczBox.Text;
            ApiAddress2 = AdresMagazynBox.Text;

            DialogResult = true; // zamyka okno z OK
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // zamyka okno bez zapisu
        }
    }
}
