using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfAppXMLBioPlanet.Models
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

        [ObservableProperty]
        private bool nowyProdukt = false;

        [ObservableProperty]
        private bool wycofany = false;

        [ObservableProperty]
        private string status = string.Empty;

        [ObservableProperty]
        private string nazwaBazowa = string.Empty;

        public string KluczGrupy => $"{NazwaBazowa} | {Kraj}";
        public int StanMagazynowy { get; set; } = 0;
        public bool NaStanie { get; set; } = false;


    }
}
