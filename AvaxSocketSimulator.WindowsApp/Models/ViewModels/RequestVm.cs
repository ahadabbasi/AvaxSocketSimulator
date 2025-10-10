using System.Collections.Generic;
using System.Linq;

namespace AvaxSocketSimulator.WindowsApp.Models.ViewModels
{
    public class RequestVm
    {
        public RequestVm(FetchVm entry)
        {
            Imei = entry.Imei;

            Data = entry.Data
                .Split(' ')
                .Where(item => byte.TryParse(item, out byte _))
                .Select(byte.Parse);
        }

        public string Imei { get; }

        public IEnumerable<byte> Data { get; }
    }
}
