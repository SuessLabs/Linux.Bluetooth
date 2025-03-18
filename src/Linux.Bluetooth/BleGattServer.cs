namespace Linux.Bluetooth
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Tmds.DBus;

  /// <summary>
  /// BlueZ D-Bus GATT Server class.
  /// </summary>
  /// <remarks>Move  methods into here.</remarks>
  public class BleGattServer : IDisposable
  {
    private const string Peripheral = "peripheral";
    private readonly Adapter _adapter;
    private ILEAdvertisingManager1 _advManager;
    private Advertisement _advertisement;

    public event EventHandler<AdvertisementReceivedEventArgs>? AdvertisementReceived;

    public Connection Connection { get; }

    public BleGattServer(Adapter adapter)
    {
      Connection = new Connection(Address.System);
      _adapter = adapter;
      _advManager = Connection.CreateProxy<ILEAdvertisingManager1>(BluezConstants.DbusService, adapter.ObjectPath);
    }

    ~BleGattServer()
    {
      Dispose();
    }

    public void Dispose()
    {
      Connection.Dispose();
      GC.SuppressFinalize(this);
    }

    public async Task InitializeAsync()
    {
      await Connection.ConnectAsync();
    }

    public Advertisement CreateAdvertisement(string name, string[] uuids)
    {
      var advProperties = new LEAdvertisement1Properties
      {
        Type = Peripheral,
        ServiceUUIDs = uuids,
        LocalName = name,
        Appearance = 0x80,
        Discoverable = true,  // False for Broadcast
        IncludeTxPower = true,
      };

      return new Advertisement(_adapter.Name + "/advertisement0", advProperties);
    }

    public async Task<GattService> CreateService(string uuid)
    {
      var gattService = new GattService(uuid);

      return gattService;
    }

    /// <summary>Begin advertising our BLE Server.</summary>
    /// <param name="advertisement"></param>
    /// <returns></returns>
    public async Task RegisterAdvertisement(Advertisement advertisement)
    {
      var advertisementExists = await _advManager.GetAsync<byte>("ActiveInstances");
      if (advertisementExists == 0)
      {
        // Subscribe to the AdvertisementReceived event with the provided action
        AdvertisementReceived += OnAdvertisementReceived;
        await Connection.RegisterObjectAsync(advertisement);
        Console.WriteLine($"Advertisement object {advertisement.ObjectPath} created");

        await _advManager.RegisterAdvertisementAsync(
            ((IDBusObject)advertisement).ObjectPath,
            new Dictionary<string, object>()
        );
        _advertisement = advertisement;
        Console.WriteLine($"Advertisement {advertisement.ObjectPath} registered in BlueZ advertising manager");
      }
    }

    public async Task UnregisterAdvertisement()
    {
      AdvertisementReceived -= OnAdvertisementReceived;
      await _advManager.UnregisterAdvertisementAsync(_advertisement.ObjectPath);
      Connection.UnregisterObject(_advertisement);
      Console.WriteLine($"Advertisement {_advertisement.ObjectPath} unregistered");
    }

    public void OnAdvertisementReceived(object? sender, AdvertisementReceivedEventArgs e)
    {
      AdvertisementReceived?.Invoke(this, e);
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
}
