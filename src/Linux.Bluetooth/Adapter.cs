using System;
using System.Collections.Generic;
using System.Threading;
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
    private IDisposable? _interfacesRemovedWatcher;
    private IDisposable? _propertyWatcher;
    private DeviceChangeEventHandlerAsync? _deviceFound;
    private DeviceChangeEventHandlerAsync? _deviceConnected;
    private DeviceChangeEventHandlerAsync? _deviceDisconnected;
    private AdapterEventHandlerAsync? _poweredOn;
    private IObjectManager? _objectManager;

    // Devices whose Connected/Disconnected we relay to the adapter-level events.
    private readonly object _connTrackLock = new();
    private readonly Dictionary<ObjectPath, Device> _connTrackedDevices = new();

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
      adapter._interfacesRemovedWatcher = await objectManager.WatchInterfacesRemovedAsync(adapter.OnDeviceRemoved);
      adapter._propertyWatcher = await proxy.WatchPropertiesAsync(adapter.OnPropertyChanges);

      return adapter;
    }

    public void Dispose()
    {
      _interfacesWatcher?.Dispose();
      _interfacesWatcher = null;
      _interfacesRemovedWatcher?.Dispose();
      _interfacesRemovedWatcher = null;
      _propertyWatcher?.Dispose();
      _propertyWatcher = null;

      lock (_connTrackLock)
      {
        foreach (var device in _connTrackedDevices.Values)
        {
          // Null while TrackDeviceForConnection holds a reserved slot.
          device?.Dispose();
        }

        _connTrackedDevices.Clear();
      }

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

    /// <summary>
    ///   Raised when any device transitions to Connected — the connection-side twin of
    ///   <see cref="DeviceFound"/>. Covers both newly-added and pre-existing device objects
    ///   (BlueZ reuses an existing object for an inbound connection, so InterfacesAdded does
    ///   not re-fire). Subscribing replays devices that are already connected.
    /// </summary>
    public event DeviceChangeEventHandlerAsync DeviceConnected
    {
      add
      {
        _deviceConnected += value;
        TrackExistingDevicesForConnectionAsync();
      }
      remove
      {
        _deviceConnected -= value;
      }
    }

    /// <summary>
    ///   Raised when any device transitions to Disconnected. See <see cref="DeviceConnected"/>.
    /// </summary>
    public event DeviceChangeEventHandlerAsync DeviceDisconnected
    {
      add
      {
        _deviceDisconnected += value;
        TrackExistingDevicesForConnectionAsync();
      }
      remove
      {
        _deviceDisconnected -= value;
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

        // Relay this device's connection state if anyone is listening to the adapter-level events.
        TrackDeviceForConnection(args.objectPath);
      }
    }

    private void OnDeviceRemoved((ObjectPath objectPath, string[] interfaces) args)
    {
      // Runs on the DBus receive loop: consumer throws must not escape.
      try
      {
        lock (_connTrackLock)
        {
          // netstandard2.0: no Remove(key, out value) overload.
          if (_connTrackedDevices.TryGetValue(args.objectPath, out var device))
          {
            _connTrackedDevices.Remove(args.objectPath);
            device?.Dispose();
          }
        }
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine($"InterfacesRemoved handler threw: {ex.Message}");
      }
    }

    /// <summary>
    ///   Relays a device's Connected/Disconnected to the adapter-level events. The Device's
    ///   add-accessors replay immediately if the device is already connected, so a pre-existing
    ///   connection is not missed.
    /// </summary>
    private async void TrackDeviceForConnection(ObjectPath objectPath)
    {
      if (_deviceConnected is null && _deviceDisconnected is null)
      {
        return;
      }

      lock (_connTrackLock)
      {
        if (_connTrackedDevices.ContainsKey(objectPath))
        {
          return;
        }

        // reserve the slot against a concurrent call
        _connTrackedDevices[objectPath] = null!;
      }

      try
      {
        var proxy = Connection.System.CreateProxy<IDevice1>(BluezConstants.DbusService, objectPath);
        var device = await Device.CreateAsync(proxy);

        // Device.Connected can fire twice (add-accessor replay + live change), so relay only
        // on an actual transition.
        var connected = 0;

        device.Connected += (sender, e) =>
        {
          if (Interlocked.Exchange(ref connected, 1) == 0)
          {
            _deviceConnected?.Invoke(this, new DeviceFoundEventArgs(sender, e.IsStateChange));
          }

          return Task.CompletedTask;
        };
        device.Disconnected += (sender, e) =>
        {
          if (Interlocked.Exchange(ref connected, 0) == 1)
          {
            _deviceDisconnected?.Invoke(this, new DeviceFoundEventArgs(sender, e.IsStateChange));
          }

          return Task.CompletedTask;
        };

        lock (_connTrackLock)
        {
          _connTrackedDevices[objectPath] = device;
        }
      }
      catch (Exception ex)
      {
        lock (_connTrackLock)
        {
          _connTrackedDevices.Remove(objectPath);
        }

        Console.WriteLine($"Error tracking device '{objectPath}' for connection: {ex.Message}");
      }
    }

    private async void TrackExistingDevicesForConnectionAsync()
    {
      // async void: exceptions must not escape.
      try
      {
        var paths = await GetDevicesPathsAsync();
        foreach (var path in paths)
        {
          TrackDeviceForConnection(path);
        }
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine($"Connection-tracking replay threw: {ex.Message}");
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
