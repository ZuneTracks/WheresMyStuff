using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WheresMyStuff.Models;

namespace WheresMyStuff.Services
{
    public class StorageService
    {
        private static readonly List<Package> Packages = new List<Package>();

        public Task<List<Package>> GetAllPackagesAsync()
        {
            return Task.FromResult(Packages.ToList());
        }

        public Task AddPackageAsync(Package package)
        {
            Packages.Add(package);
            return Task.CompletedTask;
        }

        public Task RemovePackageAsync(string packageId)
        {
            var package = Packages.FirstOrDefault(p => p.Id == packageId);
            if (package != null)
            {
                Packages.Remove(package);
            }

            return Task.CompletedTask;
        }

        public Task UpdatePackageAsync(Package updatedPackage)
        {
            var index = Packages.FindIndex(p => p.Id == updatedPackage.Id);
            if (index >= 0)
            {
                Packages[index] = updatedPackage;
            }
            else
            {
                Packages.Add(updatedPackage);
            }

            return Task.CompletedTask;
        }
    }
}
