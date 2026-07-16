using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using WheresMyStuff.Models;

namespace WheresMyStuff.Services
{
    public class StorageService
    {
        private const string PackagesStorageKey = "TrackedPackages";

        public Task<List<Package>> GetAllPackagesAsync()
        {
            return Task.FromResult(ReadPackages());
        }

        public Task AddPackageAsync(Package package)
        {
            var packages = ReadPackages();
            packages.Add(package);
            SavePackages(packages);
            return Task.CompletedTask;
        }

        public Task RemovePackageAsync(string packageId)
        {
            var packages = ReadPackages();
            var package = packages.FirstOrDefault(p => p.Id == packageId);
            if (package != null)
            {
                packages.Remove(package);
                SavePackages(packages);
            }

            return Task.CompletedTask;
        }

        public Task UpdatePackageAsync(Package updatedPackage)
        {
            var packages = ReadPackages();
            var index = packages.FindIndex(p => p.Id == updatedPackage.Id);
            if (index >= 0)
            {
                packages[index] = updatedPackage;
            }
            else
            {
                packages.Add(updatedPackage);
            }

            SavePackages(packages);
            return Task.CompletedTask;
        }

        private static List<Package> ReadPackages()
        {
            object rawValue;
            if (!ApplicationData.Current.LocalSettings.Values.TryGetValue(PackagesStorageKey, out rawValue))
            {
                return new List<Package>();
            }

            string json = rawValue as string;
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Package>();
            }

            try
            {
                var packages = new List<Package>();
                JsonArray array = JsonArray.Parse(json);
                foreach (IJsonValue item in array)
                {
                    JsonObject obj = item.GetObject();
                    var package = new Package
                    {
                        Id = GetString(obj, "id"),
                        TrackingNumber = GetString(obj, "trackingNumber"),
                        CarrierName = GetString(obj, "carrierName"),
                        Status = GetString(obj, "status"),
                        CurrentLocation = GetString(obj, "currentLocation"),
                        Sender = GetString(obj, "sender"),
                        Recipient = GetString(obj, "recipient"),
                        Notes = GetString(obj, "notes"),
                        LastUpdated = GetDateTime(obj, "lastUpdated")
                    };

                    if (string.IsNullOrWhiteSpace(package.Id))
                    {
                        package.Id = System.Guid.NewGuid().ToString();
                    }

                    packages.Add(package);
                }

                return packages;
            }
            catch
            {
                return new List<Package>();
            }
        }

        private static void SavePackages(List<Package> packages)
        {
            var array = new JsonArray();
            foreach (var package in packages)
            {
                var obj = new JsonObject
                {
                    { "id", JsonValue.CreateStringValue(package.Id ?? string.Empty) },
                    { "trackingNumber", JsonValue.CreateStringValue(package.TrackingNumber ?? string.Empty) },
                    { "carrierName", JsonValue.CreateStringValue(package.CarrierName ?? string.Empty) },
                    { "status", JsonValue.CreateStringValue(package.Status ?? string.Empty) },
                    { "currentLocation", JsonValue.CreateStringValue(package.CurrentLocation ?? string.Empty) },
                    { "lastUpdated", JsonValue.CreateStringValue(package.LastUpdated.ToString("o")) },
                    { "sender", JsonValue.CreateStringValue(package.Sender ?? string.Empty) },
                    { "recipient", JsonValue.CreateStringValue(package.Recipient ?? string.Empty) },
                    { "notes", JsonValue.CreateStringValue(package.Notes ?? string.Empty) }
                };
                array.Add(obj);
            }

            ApplicationData.Current.LocalSettings.Values[PackagesStorageKey] = array.Stringify();
        }

        private static string GetString(JsonObject obj, string key)
        {
            IJsonValue value;
            if (obj.TryGetValue(key, out value) && value.ValueType == JsonValueType.String)
            {
                return value.GetString();
            }

            return string.Empty;
        }

        private static System.DateTime GetDateTime(JsonObject obj, string key)
        {
            System.DateTime parsedValue;
            string value = GetString(obj, key);
            if (!string.IsNullOrWhiteSpace(value) && System.DateTime.TryParse(value, out parsedValue))
            {
                return parsedValue;
            }

            return System.DateTime.Now;
        }
    }
}
