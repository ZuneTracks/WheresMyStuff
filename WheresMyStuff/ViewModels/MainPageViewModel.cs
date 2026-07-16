using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WheresMyStuff.Models;
using WheresMyStuff.Services;
using WheresMyStuff.Utilities;

namespace WheresMyStuff.ViewModels
{
    /// <summary>
    /// ViewModel for the main page
    /// </summary>
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private ShippoService _shippoService;
        private StorageService _storageService;
        private bool _isLoading;
        private string _statusMessage;

        public ObservableCollection<Package> Packages { get; }
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainPageViewModel(string shippoApiKey)
        {
            Packages = new ObservableCollection<Package>();
            _shippoService = new ShippoService(shippoApiKey);
            _storageService = new StorageService();
        }

        /// <summary>
        /// Initializes the view model by loading stored packages
        /// </summary>
        public async Task InitializeAsync()
        {
            IsLoading = true;
            StatusMessage = "Loading packages...";

            try
            {
                var packages = await _storageService.GetAllPackagesAsync();
                foreach (var package in packages)
                {
                    Packages.Add(package);
                }

                if (Packages.Count > 0)
                {
                    UpdateLiveTile();
                }

                StatusMessage = $"Loaded {Packages.Count} packages";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading packages: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Adds a new package to track
        /// </summary>
        public async Task AddPackageAsync(string trackingNumber, string carrier, string sender = "", string recipient = "", string notes = "")
        {
            IsLoading = true;
            StatusMessage = "Adding package...";

            try
            {
                var package = await _shippoService.GetTrackingInfoAsync(trackingNumber, carrier);
                package.Sender = sender;
                package.Recipient = recipient;
                package.Notes = notes;

                await _storageService.AddPackageAsync(package);
                Packages.Add(package);

                StatusMessage = $"Package added: {trackingNumber}";
                UpdateLiveTile();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding package: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Removes a package from tracking
        /// </summary>
        public async Task RemovePackageAsync(Package package)
        {
            IsLoading = true;
            StatusMessage = "Removing package...";

            try
            {
                await _storageService.RemovePackageAsync(package.Id);
                Packages.Remove(package);

                StatusMessage = "Package removed";
                UpdateLiveTile();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error removing package: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Refreshes tracking information for all packages
        /// </summary>
        public async Task RefreshAllPackagesAsync()
        {
            IsLoading = true;
            StatusMessage = "Refreshing packages...";

            try
            {
                int updatedCount = 0;

                foreach (var package in Packages)
                {
                    try
                    {
                        var updatedPackage = await _shippoService.GetTrackingInfoAsync(package.TrackingNumber, package.CarrierName);
                        
                        // Check if status changed
                        if (updatedPackage.Status != package.Status)
                        {
                            NotificationHelper.SendPackageStatusNotification(updatedPackage);
                        }

                        // Check if location changed
                        if (updatedPackage.CurrentLocation != package.CurrentLocation)
                        {
                            NotificationHelper.SendLocationUpdateNotification(updatedPackage);
                        }

                        // Check if delivered
                        if (updatedPackage.Status == "delivered" && package.Status != "delivered")
                        {
                            NotificationHelper.SendDeliveredNotification(updatedPackage);
                        }

                        // Check if failed
                        if (updatedPackage.Status == "failure" && package.Status != "failure")
                        {
                            NotificationHelper.SendDeliveryFailureNotification(updatedPackage);
                        }

                        // Update the package
                        updatedPackage.Id = package.Id;
                        updatedPackage.Sender = package.Sender;
                        updatedPackage.Recipient = package.Recipient;
                        updatedPackage.Notes = package.Notes;

                        await _storageService.UpdatePackageAsync(updatedPackage);

                        int index = Packages.IndexOf(package);
                        if (index >= 0)
                        {
                            Packages[index] = updatedPackage;
                        }

                        updatedCount++;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error refreshing package {package.TrackingNumber}: {ex.Message}");
                    }
                }

                StatusMessage = $"Refreshed {updatedCount} packages";
                UpdateLiveTile();
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Refreshes a specific package
        /// </summary>
        public async Task RefreshPackageAsync(Package package)
        {
            try
            {
                var updatedPackage = await _shippoService.GetTrackingInfoAsync(package.TrackingNumber, package.CarrierName);
                updatedPackage.Id = package.Id;
                updatedPackage.Sender = package.Sender;
                updatedPackage.Recipient = package.Recipient;
                updatedPackage.Notes = package.Notes;

                await _storageService.UpdatePackageAsync(updatedPackage);

                int index = Packages.IndexOf(package);
                if (index >= 0)
                {
                    Packages[index] = updatedPackage;
                }

                StatusMessage = $"Updated: {package.TrackingNumber}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating package: {ex.Message}";
            }
        }

        private void UpdateLiveTile()
        {
            if (Packages.Count == 0)
            {
                TileHelper.ClearTile();
            }
            else if (Packages.Count == 1)
            {
                TileHelper.UpdateAppTile(Packages[0]);
            }
            else
            {
                string nextDelivery = "Multiple packages";
                TileHelper.UpdateTileWithSummary(Packages.Count, nextDelivery);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
