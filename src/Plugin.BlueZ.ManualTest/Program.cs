using System;
using System.Threading.Tasks;

namespace Plugin.BlueZ.ManualTest
{
  internal class Program
  {
    private const string DefaultAdapterName = "hci0";
    private static TimeSpan _timeout = TimeSpan.FromSeconds(15);

    /// <summary>
    ///   GATT Client - Scan for devices
    ///
    /// Usage: BlueZTest1 <ScanForSeconds> [adapterName]
    ///
    /// </summary>
    /// <param name="args">ScanForSeconds, (optional) AdapterName</param>
    /// <returns>Task.</returns>
    private static async Task Main(string[] args)
    {
      int scanSeconds;
      string adapterName;

      if (args.Length == 0)
      {
        scanSeconds = 10;
        adapterName = DefaultAdapterName;
      }
      else if (args.Length < 1 || args.Length > 2 ||
        !int.TryParse(args[0], out scanSeconds))
      {
        Console.WriteLine("Usage: BlueZTest1 <SecondsToScan> [adapterName]");
        Console.WriteLine("Example: BlueZTest1 15 hci0");
        return;
      }
      else
      {
        adapterName = args.Length > 1 ? args[1] : DefaultAdapterName;
      }

      var client = new GattClient();
      await client.ScanForDevicesAsync(adapterName, scanSeconds);
    }
  }
}
