using System;
using System.Threading.Tasks;

namespace Plugin.BlueZ.ManualTest
{
  /// <summary>
  ///   GATT Client - Scan for devices
  ///
  /// Usage: dotnet Plugin.BlueZ.ManualTest.dll <SecondsToScan> [adapterName]
  /// Usage: dotnet Plugin.BlueZ.ManualTest.dll -h    Help menu
  ///
  /// </summary>
  /// <param name="args">(optional) SecondsToScan, (optional) AdapterName</param>
  /// <returns>Task.</returns>
  public class Program
  {
    private const string DefaultAdapterName = "hci0";
    private static TimeSpan _timeout = TimeSpan.FromSeconds(15);

    private static async Task Main(string[] args)
    {
      int scanSeconds;
      string adapterName;

      if (args.Length == 0)
      {
        scanSeconds = 10;
        adapterName = DefaultAdapterName;
      }
      else if (
        args.Length < 1 || args.Length > 2 ||
        args[0].ToLowerInvariant() == "-h" ||
        !int.TryParse(args[0], out scanSeconds))
      {
        Console.WriteLine("Usage: Plugin.BlueZ.ManualTest <SecondsToScan> [adapterName]");
        Console.WriteLine("Example: BlueZTest1 5 hci0");
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
