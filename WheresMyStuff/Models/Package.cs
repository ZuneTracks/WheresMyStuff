using System;

namespace WheresMyStuff.Models
{
    public class Package
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string TrackingNumber { get; set; }
        public string CarrierName { get; set; }
        public string Status { get; set; }
        public string CurrentLocation { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public string Sender { get; set; } = string.Empty;
        public string Recipient { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string GetStatusDescription => Status ?? "unknown";
    }
}
