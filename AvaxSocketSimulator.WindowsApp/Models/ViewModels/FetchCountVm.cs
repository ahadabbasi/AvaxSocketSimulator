namespace AvaxSocketSimulator.WindowsApp.Models.ViewModels
{
    public class FetchCountVm
    {
        public string Imei { get; set; }

        public int Count { get; set; }

        public override string ToString() => 
            $"{nameof(Imei)}: {Imei}\n{nameof(Count)}: {Count}";
    }
}