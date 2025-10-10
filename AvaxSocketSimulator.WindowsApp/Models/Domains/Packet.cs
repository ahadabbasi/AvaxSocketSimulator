using System;

namespace AvaxSocketSimulator.WindowsApp.Models.Domains
{
    public sealed class Packet
    {
        public Guid Id { get; set; }

        public string Imei { get; set; }

        public string Type { get; set; }

        public string Data { get; set; }

        public DateTime Inserted { get; set; }

        public DateTime? Modified { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
