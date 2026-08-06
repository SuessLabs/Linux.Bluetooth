namespace Linux.Bluetooth.GattServer
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Threading.Tasks;
  using Tmds.DBus;

  /// <summary>
  /// BlueZ D-Bus GATT Server class.
  /// </summary>
  /// <remarks>Move  methods into here.</remarks>
  public class GattServer : IDisposable
  {
    private readonly Adapter _adapter;
    private Advertisement? _advertisement;
    private readonly ILEAdvertisingManager1 _advManager;
    private readonly IGattManager1 _gattManager;
    private GattApplication? _gattApplication;

    public Connection Connection { get; }

    public GattServer(Adapter adapter)
    {
      Connection = new Connection(Address.System);
      _adapter = adapter;
      _advManager = Connection.CreateProxy<ILEAdvertisingManager1>(BluezConstants.DbusService, adapter.ObjectPath);
      _gattManager = Connection.CreateProxy<IGattManager1>(BluezConstants.DbusService, adapter.ObjectPath);
    }

    public void Dispose()
    {
      Task.Run(async () =>
      {
        await UnregisterAdvertisement();
        await UnregisterGattApplication();
      }).Wait();

      Console.Error.WriteLine("Disposed Gatt server.");
      Connection.Dispose();
    }

    public async Task InitializeAsync()
    {
      await Connection.ConnectAsync();
    }

    public void CreateGattApplication(ObjectPath? applicationPath=null)
    {
      _gattApplication ??= new GattApplication(applicationPath);
    }

    public GattService CreateService(GattService1Properties serviceProperties)
    {
      _gattApplication ??= new GattApplication();
      return new GattService(_gattApplication.ObjectPath, serviceProperties);
    }

    public async Task RegisterGattApplication(List<GattService> GattServices, Dictionary<string, object>? Options = null)
    {
      if (_gattApplication is not null)
      {
        // Add all services to the application
        foreach (GattService service in GattServices)
        {
          _gattApplication.AddService(service);
        }

        // Register the Application, each Service with its Characteristic and Descriptors objects
        await Connection.RegisterObjectAsync(_gattApplication);
        foreach (GattService service in _gattApplication.Services)
        {
          await Connection.RegisterObjectAsync(service);
          Debug.WriteLine($"Registered service {service.ObjectPath}");

          foreach (GattCharacteristicServer characteristic in service.Characteristics)
          {
            await Connection.RegisterObjectAsync(characteristic);
            Debug.WriteLine($"Registered characterisitc {characteristic.ObjectPath}");

            foreach (GattDescriptor descriptor in characteristic.Descriptors)
            {
              await Connection.RegisterObjectAsync(descriptor);
              Debug.WriteLine($"Registered descriptor {descriptor.ObjectPath}");
            }
          }
        }

        // Register the Application
        Options ??= new Dictionary<string, object>();
        await _gattManager.RegisterApplicationAsync(_gattApplication.ObjectPath, Options);
        Debug.WriteLine($"Registered application {_gattApplication.ObjectPath}");
      }
      else
      {
        throw new NullReferenceException("Gatt Application must be created before calling this method.");
      }
    }

    public async Task UnregisterGattApplication()
    {
      if (_gattApplication is not null)
      {
        // unregister the application before unregister objects
        try
        {
          await _gattManager.UnregisterApplicationAsync(_gattApplication.ObjectPath);
        }
        catch (DBusException ex) when (ex.ErrorName == "org.bluez.Error.DoesNotExist")
        {
          // BlueZ already dropped it (e.g. bluetoothd restarted): that is the wanted end state.
        }


        foreach (GattService service in _gattApplication.Services)
        {
          foreach (GattCharacteristicServer characteristic in service.Characteristics)
          {
            Connection.UnregisterObjects(characteristic.Descriptors);
          }

          Connection.UnregisterObjects(service.Characteristics);
        }

        Connection.UnregisterObjects(_gattApplication.Services);

        _gattApplication = null;
      }
    }

    public Advertisement CreateAdvertisement(LEAdvertisement1Properties advProperties)
    {
      return new Advertisement(_adapter.Name + "/advertisement0", advProperties);
    }

    public async Task RegisterAdvertisement(Advertisement advertisement, Dictionary<string, object>? Options = null)
    {
      var advertisementExists = await _advManager.GetAsync<byte>("ActiveInstances");
      if (advertisementExists == 0)
      {
        await Connection.RegisterObjectAsync(advertisement);

        Options ??= new Dictionary<string, object>();
        await _advManager.RegisterAdvertisementAsync(advertisement.ObjectPath, Options);
        _advertisement = advertisement;
        Debug.WriteLine($"Registered advertisement {advertisement.ObjectPath}");
      }
    }

    public async Task UnregisterAdvertisement()
    {
      if (_advertisement is not null)
      {
        try
        {
          await _advManager.UnregisterAdvertisementAsync(_advertisement.ObjectPath);
        }
        catch (DBusException ex) when (ex.ErrorName == "org.bluez.Error.DoesNotExist")
        {
          // BlueZ already dropped it (e.g. bluetoothd restarted): that is the wanted end state.
        }

        Connection.UnregisterObject(_advertisement);
        Debug.WriteLine($"Advertisement {_advertisement.ObjectPath} unregistered");
        _advertisement = null;
      }
    }

  }
}
