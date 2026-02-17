using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfAppXMLBioPlanet
{
    public partial class ProduktModel : ObservableObject
    {
        [ObservableProperty]
        private string ean = string.Empty;

        [ObservableProperty]
        private string nazwa = string.Empty;

        [ObservableProperty]
        private string kraj = string.Empty;

        [ObservableProperty]
        private string zmiana = string.Empty;
    }
}
