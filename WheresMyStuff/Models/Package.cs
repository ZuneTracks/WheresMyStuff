using System;

namespace WheresMyStuff.Models
{
    public class Package
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string TrackingNumber { get; set; } = string.Empty;
        public string CarrierName { get; set; } = string.Empty;
        public string Status { get; set; } = "unknown";
        public string CurrentLocation { get; set; } = "Unknown";
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public string Sender { get; set; } = string.Empty;
        public string Recipient { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        public string GetStatusDescription
        {
            get
            {
                switch ((Status ?? string.Empty).Trim().ToLowerInvariant())
                {
                    case "pre_transit":
                        return "Pre-transit";
                    case "in_transit":
                        return "In transit";
                    case "out_for_delivery":
                        return "Out for delivery";
                    case "delivered":
                        return "Delivered";
                    case "failure":
                        return "Delivery failure";
                    default:
                        return "Unknown";
                }
            }
        }
    }
}
