using System;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using WheresMyStuff.Models;

namespace WheresMyStuff.Services
{
    public class ShippoService
    {
        private const string ShippoApiBaseUrl = "https://api.goshippo.com/tracks/";
        private readonly string _apiKey;

        public ShippoService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<Package> GetTrackingInfoAsync(string trackingNumber, string carrier)
        {
            if (string.IsNullOrWhiteSpace(trackingNumber))
            {
                throw new ArgumentException("Tracking number is required.", nameof(trackingNumber));
            }

            string normalizedTrackingNumber = trackingNumber.Trim();
            string normalizedCarrier = string.IsNullOrWhiteSpace(carrier) ? "usps" : carrier.Trim().ToLowerInvariant();
            bool hasConfiguredApiKey = !string.IsNullOrWhiteSpace(_apiKey) && _apiKey != "your_shippo_api_key_here";

            if (hasConfiguredApiKey)
            {
                try
                {
                    return await GetTrackingInfoFromApiAsync(normalizedTrackingNumber, normalizedCarrier);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Shippo API error: {ex.Message}. Falling back to simulation.");
                }
            }

            return SimulatePackage(normalizedTrackingNumber, normalizedCarrier);
        }

        private async Task<Package> GetTrackingInfoFromApiAsync(string trackingNumber, string carrier)
        {
            string url = ShippoApiBaseUrl + carrier + "/" + trackingNumber;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new HttpCredentialsHeaderValue("ShippoToken", _apiKey);

                HttpResponseMessage response = await client.GetAsync(new Uri(url));
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                return ParseShippoResponse(json, trackingNumber, carrier);
            }
        }

        private static Package ParseShippoResponse(string json, string trackingNumber, string carrier)
        {
            JsonObject obj;
            if (!JsonObject.TryParse(json, out obj))
            {
                throw new InvalidOperationException("Invalid JSON response from Shippo API.");
            }

            string status = "unknown";
            string location = "Tracking update pending";
            DateTime lastUpdated = DateTime.Now;

            IJsonValue trackingStatusNode;
            if (obj.TryGetValue("tracking_status", out trackingStatusNode) && trackingStatusNode.ValueType == JsonValueType.Object)
            {
                JsonObject trackingStatus = trackingStatusNode.GetObject();

                IJsonValue statusValue;
                if (trackingStatus.TryGetValue("status", out statusValue) && statusValue.ValueType == JsonValueType.String)
                {
                    status = MapShippoStatus(statusValue.GetString());
                }

                IJsonValue statusDateValue;
                if (trackingStatus.TryGetValue("status_date", out statusDateValue) && statusDateValue.ValueType == JsonValueType.String)
                {
                    DateTime parsedDate;
                    if (DateTime.TryParse(statusDateValue.GetString(), out parsedDate))
                    {
                        lastUpdated = parsedDate;
                    }
                }

                IJsonValue locationNode;
                if (trackingStatus.TryGetValue("location", out locationNode) && locationNode.ValueType == JsonValueType.Object)
                {
                    location = FormatLocation(locationNode.GetObject());
                }
                else
                {
                    location = GetLocationFromStatus(status);
                }
            }

            return new Package
            {
                TrackingNumber = trackingNumber,
                CarrierName = carrier,
                Status = status,
                CurrentLocation = location,
                LastUpdated = lastUpdated
            };
        }

        private static string MapShippoStatus(string shippoStatus)
        {
            switch ((shippoStatus ?? string.Empty).Trim().ToUpperInvariant())
            {
                case "PRE_TRANSIT":
                    return "pre_transit";
                case "TRANSIT":
                    return "in_transit";
                case "OUT_FOR_DELIVERY":
                    return "out_for_delivery";
                case "DELIVERED":
                    return "delivered";
                case "RETURNED":
                case "FAILURE":
                    return "failure";
                default:
                    return "unknown";
            }
        }

        private static string FormatLocation(JsonObject locationObj)
        {
            string city = GetStringFromObj(locationObj, "city");
            string state = GetStringFromObj(locationObj, "state");
            string country = GetStringFromObj(locationObj, "country");

            if (!string.IsNullOrWhiteSpace(city) && !string.IsNullOrWhiteSpace(state))
            {
                return city + ", " + state;
            }

            if (!string.IsNullOrWhiteSpace(city) && !string.IsNullOrWhiteSpace(country))
            {
                return city + ", " + country;
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                return city;
            }

            if (!string.IsNullOrWhiteSpace(state))
            {
                return state;
            }

            return "Tracking update pending";
        }

        private static string GetStringFromObj(JsonObject obj, string key)
        {
            IJsonValue value;
            if (obj.TryGetValue(key, out value) && value.ValueType == JsonValueType.String)
            {
                return value.GetString();
            }

            return string.Empty;
        }

        private static Package SimulatePackage(string trackingNumber, string carrier)
        {
            string[] statuses = new[] { "pre_transit", "in_transit", "out_for_delivery", "delivered" };
            int seed = GetDeterministicSeed(trackingNumber + "|" + carrier);
            string status = statuses[seed % statuses.Length];

            return new Package
            {
                TrackingNumber = trackingNumber,
                CarrierName = carrier,
                Status = status,
                CurrentLocation = GetLocationFromStatus(status),
                LastUpdated = DateTime.Now.AddMinutes(-(seed % 180))
            };
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
