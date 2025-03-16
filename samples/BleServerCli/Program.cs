using System;
using Linux.Bluetooth;
using Tmds.DBus;

public class Program
{
  private static async Task Main(string[] args)
  {
    Console.WriteLine("Linux.Bluetooth Server Example");

    using var bleServer = new BleGattServer();
    // Connect to DBus
    await bleServer.InitializeAsync();

    // Get BLE Adapter
    var adapter = await GetDefaultAdapterAsync();
    if (adapter is null)
    {
      Console.WriteLine("No BLE Adapters found.");
      return;
    }

    // Turn on BLE Adapter
    await adapter.SetPoweredAsync(true);

    // Make advertisement details for our server
    var adv = bleServer.CreateAdvertisement(
      "My Linux.Bluetooth Device!",
      "3515A516-A069-41EF-9222-1D0343124680");

    await bleServer.StartAdvertisingAsync();

    Console.WriteLine("Press any key to quit");
    Console.ReadLine();
  }

  private static async Task<Adapter?> GetDefaultAdapterAsync()
  {
    var adapters = await BlueZManager.GetAdaptersAsync();
    if (adapters.Count == 0)
      return null;

    return adapters[0];
  }
}

/// <summary>BlueZ D-Bus GATT Server class.</summary>
/// <remarks>Move  methods into here.</remarks>
public class BleGattServer : IDisposable
{
  private const string Peripheral = "peripheral";

  public BleGattServer() => Connection = new Connection(Address.System);

  public async Task InitializeAsync() => await Connection.ConnectAsync();

  public Connection Connection { get; }

  public void Dispose() => Connection.Dispose();

  public LEAdvertisement1Properties CreateAdvertisement(string name, string uuid)
  {
    var bluezAdv = new LEAdvertisement1Properties
    {
      Type = Peripheral,
      ServiceUUIDs = [uuid],
      LocalName = name,
      Appearance = 0x80,
      Discoverable = true,  // False for Broadcast
      IncludeTxPower = true,
    };

    // TODO: Register with AdvertizeManager

    return bluezAdv;
  }

  public async Task<GattService> CreateService(string uuid)
  {
    var gattService = new GattService(uuid);

    return gattService;
  }

  /// <summary>Begin advertising our BLE Server.</summary>
  /// <param name="advertisement"></param>
  /// <returns></returns>
  public async Task RegisterAdvertisement(LEAdvertisement1Properties advertisement)
  {
    await new AdvertisingManager.
  }

  public class GattService
  {
    public GattService(string uuid) => Uuid = uuid;

    public string Uuid { get; private set; }
  }

  public class GattCharacteristic
  {
    public ushort Read = 1 << 0;
    public ushort Write = 1 << 1;
    public ushort Notify = 1 << 2;
    public ushort Broadcast = 1 << 3;
    public ushort Indicate = 1 << 4;
    public ushort Write_NR = 1 << 5; // Write No-Response

    public GattCharacteristic(string uuid, int properties)
    {
      Uuid = uuid;
      Properties = properties;
    }

    public string Uuid { get; private set; }

    public int Properties { get; private set; }
  }
}
