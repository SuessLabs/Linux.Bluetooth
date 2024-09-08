using Linux.Bluetooth;
using Tmds.DBus;

Console.WriteLine("Linux.Bluetooth Server Example");

using (var bluez = new DBusServer())
{
  // Connect to DBus
  await bluez.InitializeAsync();

  // Get BLE Adapter
  var adapter = await GetDefaultAdapterAsync();
  if (adapter is null)
  {
    Console.WriteLine("No BLE Adapters found.");
    return;
  }

  // Turn on BLE Adapter
  await adapter.SetPoweredAsync(true);

  await CreateAdvertisementAsync();

  Console.WriteLine("Press any key to quit");
  Console.ReadLine();
}

static async Task<Adapter?> GetDefaultAdapterAsync()
{
  var adapters = await BlueZManager.GetAdaptersAsync();
  if (adapters.Count == 0)
    return null;

  return adapters[0];
}

static async Task CreateAdvertisementAsync()
{
  var bluezAdv = new LEAdvertisement1Properties
  {
    Type = "",
    ServiceUUIDs = ["3515A516-A069-41EF-9222-1D0343124680"],
    LocalName = "My Linux.Bluetooth Device!",
    Appearance = 0x80,
    Discoverable = true,  // False for Broadcast
    IncludeTxPower = true,
  };

  // TODO: Register with AdvertizeManager
  return;
}

/// <summary>BlueZ DBus Server class.</summary>
/// <remarks>Rename to BleServer and move methods into here.</remarks>
class DBusServer : IDisposable
{
  public DBusServer() => Connection = new Connection(Address.System);

  public async Task InitializeAsync() => await Connection.ConnectAsync();

  public Connection Connection { get; }

  public void Dispose() => Connection.Dispose();
}

