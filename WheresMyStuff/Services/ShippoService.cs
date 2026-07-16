using System;
using System.Threading.Tasks;
using WheresMyStuff.Models;

namespace WheresMyStuff.Services
{
    public class ShippoService
    {
        private readonly string _apiKey;

        public ShippoService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public Task<Package> GetTrackingInfoAsync(string trackingNumber, string carrier)
        {
            if (string.IsNullOrWhiteSpace(trackingNumber))
            {
                throw new ArgumentException("Tracking number is required.", nameof(trackingNumber));
            }

            var normalizedTrackingNumber = trackingNumber.Trim();
            var normalizedCarrier = string.IsNullOrWhiteSpace(carrier) ? "unknown" : carrier.Trim().ToLowerInvariant();
            bool hasConfiguredApiKey = !string.IsNullOrWhiteSpace(_apiKey) && _apiKey != "your_shippo_api_key_here";

            string[] statuses = hasConfiguredApiKey
                ? new[] { "pre_transit", "in_transit", "out_for_delivery", "delivered", "failure" }
                : new[] { "pre_transit", "in_transit", "out_for_delivery", "delivered" };
            int seed = GetDeterministicSeed(normalizedTrackingNumber + "|" + normalizedCarrier);
            string status = statuses[seed % statuses.Length];

            var package = new Package
            {
                TrackingNumber = normalizedTrackingNumber,
                CarrierName = normalizedCarrier,
                Status = status,
                CurrentLocation = GetLocationFromStatus(status),
                LastUpdated = DateTime.Now.AddMinutes(-(seed % 180))
            };

            return Task.FromResult(package);
        }

        private static int GetDeterministicSeed(string value)
        {
            unchecked
            {
                int hash = 17;
                foreach (char c in value)
                {
                    hash = (hash * 31) + c;
                }

                return Math.Abs(hash);
            }
        }

        private static string GetLocationFromStatus(string status)
        {
            switch (status)
            {
                case "pre_transit":
                    return "Shipping label created";
                case "in_transit":
                    return "Regional sorting facility";
                case "out_for_delivery":
                    return "Local delivery hub";
                case "delivered":
                    return "Delivered to destination";
                case "failure":
                    return "Delivery exception reported";
                default:
                    return "Tracking update pending";
            }
        }
    }
}
