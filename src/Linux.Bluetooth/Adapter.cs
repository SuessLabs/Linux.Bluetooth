using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Linux.Bluetooth.Extensions;
using Tmds.DBus;

namespace Linux.Bluetooth
{
  public delegate Task DeviceChangeEventHandlerAsync(Adapter sender, DeviceFoundEventArgs eventArgs);

  public delegate Task AdapterEventHandlerAsync(Adapter sender, BlueZEventArgs eventArgs);

  /// <summary>Add events to IAdapter1.</summary>
  /// <remarks>
  ///   Reference: https://github.com/bluez/bluez/blob/master/doc/org.bluez.Adapter.rst
  /// </remarks>
  public class Adapter : IAdapter1, IDisposable
  {
    private IAdapter1? _proxy;
    private IDisposable? _interfacesWatcher;
    private IDisposable? _propertyWatcher;
    private DeviceChangeEventHandlerAsync? _deviceFound;
    private AdapterEventHandlerAsync? _poweredOn;
    private IObjectManager? _objectManager;

    private IAdapter1 Proxy => _proxy ?? throw new InvalidOperationException("Adapter has not been initialized.");

    private IObjectManager ObjectManager => 
      _objectManager ?? throw new InvalidOperationException("Adapter object manager has not been initialized.");

    ~Adapter()
    {
      Dispose();
    }

    internal static async Task<Adapter> CreateAsync(IAdapter1 proxy)
    {
      var adapter = new Adapter
      {
        _proxy = proxy,
      };

      var objectManager = Connection.System.CreateProxy<IObjectManager>(BluezConstants.DbusService, "/");
      adapter._objectManager = objectManager;
      adapter._interfacesWatcher = await objectManager.WatchInterfacesAddedAsync(adapter.OnDeviceAddedAsync);
      adapter._propertyWatcher = await proxy.WatchPropertiesAsync(adapter.OnPropertyChanges);

      return adapter;
    }

    public void Dispose()
    {
      _interfacesWatcher?.Dispose();
      _interfacesWatcher = null;
      _propertyWatcher?.Dispose();
      _propertyWatcher = null;

      GC.SuppressFinalize(this);
    }

    public event DeviceChangeEventHandlerAsync DeviceFound
    {
      add
      {
        _deviceFound += value;
        FireEventForExistingDevicesAsync();
      }
      remove
      {
        _deviceFound -= value;
      }
    }

    public event AdapterEventHandlerAsync PoweredOn
    {
      add
      {
        _poweredOn += value;
        FireEventIfPropertyAlreadyTrueAsync(_poweredOn, "Powered");
      }
      remove
      {
        _poweredOn -= value;
      }
    }

    public event AdapterEventHandlerAsync? PoweredOff;

    /// <summary>See also, Name, property.</summary>
    public ObjectPath ObjectPath => Proxy.ObjectPath;

    public Task<Adapter1Properties> GetAllAsync()
    {
      return Proxy.GetAllAsync();
    }

    public async Task<AdapterProperties> GetPropertiesAsync()
    {
      var p = await Proxy.GetAllAsync();

      return new AdapterProperties
      {
        Address = p.Address,
        AddressType = p.AddressType,
        Name = p.Name,
        Alias = p.Alias,
        Class = p.Class,
        Powered = p.Powered,
        Discoverable = p.Discoverable,
        DiscoverableTimeout = p.DiscoverableTimeout,
        Discovering = p.Discovering,
        Pairable = p.Pairable,
        PairableTimeout = p.PairableTimeout,
        UUIDs = p.UUIDs,
        Modalias = p.Modalias,
      };
    }

    /// <summary>Name of Adapter (i.e. "/org/bluez/hci0").</summary>
    /// <remarks>This is a custom property to easily translate the name.</remarks>
    public string Name => ObjectPath.ToString();

    /// <summary>Gets a property of the BlueZ Adapter object.</summary>
    /// <remarks>See, <seealso cref="Adapter1Extensions"/> for references.</remarks>
    /// <typeparam name="T">Type of property.</typeparam>
    /// <param name="prop">Name of the property.</param>
    /// <returns></returns>
    public Task<T> GetAsync<T>(string prop)
    {
      return Proxy.GetAsync<T>(prop);
    }

    /// <summary>
    ///   Return available filters that can be given to SetDiscoveryFilter.
    ///
    ///   Possible errors: None.
    /// </summary>
    /// <returns>String of filters.</returns>
    public Task<string[]> GetDiscoveryFiltersAsync()
    {
      return Proxy.GetDiscoveryFiltersAsync();
    }

    public Task RemoveDeviceAsync(ObjectPath Device)
    {
      return Proxy.RemoveDeviceAsync(Device);
    }

    /// <summary>Set Property Value Async.</summary>
    /// <param name="prop"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    public Task SetAsync(string prop, object val)
    {
      return Proxy.SetAsync(prop, val);
    }

    /// <summary>
    /// This method sets the device discovery filter for the
    /// caller. When this method is called with no filter
    /// parameter, filter is removed.
    /// </summary>
    /// <param name="properties">Filter parameters. Ref: <see cref="https://git.kernel.org/pub/scm/bluetooth/bluez.git/tree/doc/adapter-api.txt"/>.</param>
    /// <returns></returns>
    public Task SetDiscoveryFilterAsync(IDictionary<string, object> properties)
    {
      return Proxy.SetDiscoveryFilterAsync(properties);
    }

    /// <summary>Scan for devices nearby.</summary>
    /// <returns>Task.</returns>
    public Task StartDiscoveryAsync()
    {
      return Proxy.StartDiscoveryAsync();
    }

    /// <summary>Stop scanning for devices nearby.</summary>
    /// <returns>Task.</returns>
    public Task StopDiscoveryAsync()
    {
      return Proxy.StopDiscoveryAsync();
    }

    public async Task<List<ObjectPath>> GetDevicesPathsAsync()
    {
      List<ObjectPath> result = new List<ObjectPath>();
      var objects = await ObjectManager.GetManagedObjectsAsync();
      foreach (var path in objects.Keys)
      {
        var interfaces = objects[path];
        foreach (var intf in interfaces.Keys)
        {
          if (intf == BluezConstants.DeviceInterface)
          {
            result.Add(path);
          }
        }
      }

      return result;
    }

    /// <summary>Watch for property updates.</summary>
    /// <param name="handler">Handler with argument of <seealso cref="PropertyChanges"/>.</param>
    /// <returns>Disposable task.</returns>
    public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
    {
      return Proxy.WatchPropertiesAsync(handler);
    }

    private async void FireEventForExistingDevicesAsync()
    {
      var devices = await this.GetDevicesAsync();
      foreach (var device in devices)
      {
        _deviceFound?.Invoke(this, new DeviceFoundEventArgs(device, isStateChange: false));
      }
    }

    private async void OnDeviceAddedAsync((ObjectPath objectPath, IDictionary<string, IDictionary<string, object>> interfaces) args)
    {
      if (BlueZManager.IsMatch(BluezConstants.DeviceInterface, args.objectPath, args.interfaces, this))
      {
        var device = Connection.System.CreateProxy<IDevice1>(BluezConstants.DbusService, args.objectPath);

        var dev = await Device.CreateAsync(device);
        _deviceFound?.Invoke(this, new DeviceFoundEventArgs(dev));
      }
    }

    private async void FireEventIfPropertyAlreadyTrueAsync(AdapterEventHandlerAsync handler, string prop)
    {
      try
      {
        var value = await Proxy.GetAsync<bool>(prop);
        if (value)
        {
          // TODO: Suppress duplicate event from OnPropertyChanges.
          handler?.Invoke(this, new BlueZEventArgs(isStateChange: false));
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error checking if '{prop}' is already true: {ex}");
      }
    }

    private void OnPropertyChanges(PropertyChanges changes)
    {
      foreach (var pair in changes.Changed)
      {
        switch (pair.Key)
        {
          case "Powered":
            if (true.Equals(pair.Value))
            {
              _poweredOn?.Invoke(this, new BlueZEventArgs());
            }
            else
            {
              PoweredOff?.Invoke(this, new BlueZEventArgs());
            }

            break;
        }
      }
    }
  }
}
