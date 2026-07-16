using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WheresMyStuff.ViewModels;

namespace WheresMyStuff
{
    /// <summary>
    /// Main page for package tracking
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MainPageViewModel _viewModel;

        public MainPage()
        {
            this.InitializeComponent();
            _viewModel = new MainPageViewModel(App.GetShippoApiKey());
            this.DataContext = _viewModel;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await _viewModel.InitializeAsync();
        }

        /// <summary>
        /// Handles adding a new package
        /// </summary>
        private async void AddPackageButton_Click(object sender, RoutedEventArgs e)
        {
            string trackingNumber = TrackingNumberTextBox.Text?.Trim();
            string carrier = (CarrierComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "fedex";
            string senderText = SenderTextBox.Text?.Trim() ?? "";
            string recipientText = RecipientTextBox.Text?.Trim() ?? "";
            string notesText = NotesTextBox.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(trackingNumber))
            {
                ShowErrorMessage("Please enter a tracking number");
                return;
            }

            await _viewModel.AddPackageAsync(trackingNumber, carrier, senderText, recipientText, notesText);

            // Clear input fields
            TrackingNumberTextBox.Text = "";
            SenderTextBox.Text = "";
            RecipientTextBox.Text = "";
            NotesTextBox.Text = "";
            CarrierComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Handles refreshing all packages
        /// </summary>
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.RefreshAllPackagesAsync();
        }

        /// <summary>
        /// Handles package selection
        /// </summary>
        private async void PackageListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var package = e.ClickedItem as Models.Package;
            if (package != null)
            {
                await _viewModel.RefreshPackageAsync(package);
            }
        }

        /// <summary>
        /// Handles deleting a package
        /// </summary>
        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var package = button?.DataContext as Models.Package;

            if (package != null)
            {
                // Show confirmation dialog
                ContentDialog deleteDialog = new ContentDialog
                {
                    Title = "Delete Package",
                    Content = $"Are you sure you want to stop tracking {package.TrackingNumber}?",
                    PrimaryButtonText = "Delete",
                    SecondaryButtonText = "Cancel",
                    PrimaryButtonBackground = Resources["ErrorBrush"] as Windows.UI.Xaml.Media.SolidColorBrush
                };

                ContentDialogResult result = await deleteDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    await _viewModel.RemovePackageAsync(package);
                }
            }
        }

        /// <summary>
        /// Shows an error message
        /// </summary>
        private async void ShowErrorMessage(string message)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK"
            };

            await errorDialog.ShowAsync();
        }
    }
}
