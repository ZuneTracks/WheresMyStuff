using System.Diagnostics;
using WheresMyStuff.Models;

namespace WheresMyStuff.Utilities
{
    public static class TileHelper
    {
        public static void ClearTile()
        {
            Debug.WriteLine("Tile cleared");
        }

        public static void UpdateAppTile(Package package)
        {
            Debug.WriteLine($"Tile updated for {package?.TrackingNumber}");
        }

        public static void UpdateTileWithSummary(int packageCount, string nextDelivery)
        {
            Debug.WriteLine($"Tile summary updated. Count: {packageCount}, Next: {nextDelivery}");
        }
    }
}
