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
            var package = new Package
            {
                TrackingNumber = trackingNumber,
                CarrierName = string.IsNullOrWhiteSpace(carrier) ? "unknown" : carrier,
                Status = string.IsNullOrWhiteSpace(_apiKey) ? "unknown" : "in_transit",
                CurrentLocation = "Tracking update pending",
                LastUpdated = DateTime.Now
            };

            return Task.FromResult(package);
        }
    }
}
