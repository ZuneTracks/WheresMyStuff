using System.Diagnostics;
using WheresMyStuff.Models;

namespace WheresMyStuff.Utilities
{
    public static class NotificationHelper
    {
        public static void SendPackageStatusNotification(Package package)
        {
            Debug.WriteLine($"Status changed for {package?.TrackingNumber}");
        }

        public static void SendLocationUpdateNotification(Package package)
        {
            Debug.WriteLine($"Location updated for {package?.TrackingNumber}");
        }

        public static void SendDeliveredNotification(Package package)
        {
            Debug.WriteLine($"Package delivered: {package?.TrackingNumber}");
        }

        public static void SendDeliveryFailureNotification(Package package)
        {
            Debug.WriteLine($"Delivery failure: {package?.TrackingNumber}");
        }
    }
}
