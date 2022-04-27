using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.BlueZ.Extensions;
using Tmds.DBus;

namespace Plugin.BlueZ
{
  public delegate Task DeviceChangeEventHandlerAsync(Adapter sender, DeviceFoundEventArgs eventArgs);

  public delegate Task AdapterEventHandlerAsync(Adapter sender, BlueZEventArgs eventArgs);

  /// <summary>
  /// Add events to IAdapter1.
  /// </summary>
  public class Adapter : IAdapter1, IDisposable
  {
    private IAdapter1 m_proxy;
    private IDisposable m_interfacesWatcher;
    private IDisposable m_propertyWatcher;
    private DeviceChangeEventHandlerAsync m_deviceFound;
    private AdapterEventHandlerAsync m_poweredOn;

    ~Adapter()
    {
      Dispose();
    }

    internal static async Task<Adapter> CreateAsync(IAdapter1 proxy)
    {
      var adapter = new Adapter
      {
        m_proxy = proxy,
      };

      var objectManager = Connection.System.CreateProxy<IObjectManager>(BluezConstants.DbusService, "/");
      adapter.m_interfacesWatcher = await objectManager.WatchInterfacesAddedAsync(adapter.OnDeviceAddedAsync);
      adapter.m_propertyWatcher = await proxy.WatchPropertiesAsync(adapter.OnPropertyChanges);

      return adapter;
    }

    public void Dispose()
    {
      m_interfacesWatcher?.Dispose();
      m_interfacesWatcher = null;

      GC.SuppressFinalize(this);
    }

    public event DeviceChangeEventHandlerAsync DeviceFound
    {
      add
      {
        m_deviceFound += value;
        FireEventForExistingDevicesAsync();
      }
      remove
      {
        m_deviceFound -= value;
      }
    }

    public event AdapterEventHandlerAsync PoweredOn
    {
      add
      {
        m_poweredOn += value;
        FireEventIfPropertyAlreadyTrueAsync(m_poweredOn, "Powered");
      }
      remove
      {
        m_poweredOn -= value;
      }
    }

    public event AdapterEventHandlerAsync PoweredOff;

    public ObjectPath ObjectPath => m_proxy.ObjectPath;

    public Task<Adapter1Properties> GetAllAsync()
    {
      return m_proxy.GetAllAsync();
    }

    /// <summary>Name of Adapter (i.e. "/org/bluez/hci0").</summary>
    public string Name => ObjectPath.ToString();

    public Task<T> GetAsync<T>(string prop)
    {
      return m_proxy.GetAsync<T>(prop);
    }

    /// <summary>Return available filters that can be given to SetDiscoveryFilter.</summary>
    /// <returns>String of filters.</returns>
    public Task<string[]> GetDiscoveryFiltersAsync()
    {
      return m_proxy.GetDiscoveryFiltersAsync();
    }

    public Task RemoveDeviceAsync(ObjectPath Device)
    {
      return m_proxy.RemoveDeviceAsync(Device);
    }

    /// <summary>Set Property Value Async.</summary>
    /// <param name="prop"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    public Task SetAsync(string prop, object val)
    {
      return m_proxy.SetAsync(prop, val);
    }

    /// <summary>
    /// This method sets the device discovery filter for the
    /// caller. When this method is called with no filter
    /// parameter, filter is removed.
    /// </summary>
    /// <param name="Properties">Filter parameters. Ref: <see cref="https://git.kernel.org/pub/scm/bluetooth/bluez.git/tree/doc/adapter-api.txt"/>.</param>
    /// <returns></returns>
    public Task SetDiscoveryFilterAsync(IDictionary<string, object> Properties)
    {
      return m_proxy.SetDiscoveryFilterAsync(Properties);
    }

    /// <summary>Scan for devices nearby.</summary>
    /// <returns>Task.</returns>
    public Task StartDiscoveryAsync()
    {
      return m_proxy.StartDiscoveryAsync();
    }

    /// <summary>Stop scanning for devices nearby.</summary>
    /// <returns>Task.</returns>
    public Task StopDiscoveryAsync()
    {
      return m_proxy.StopDiscoveryAsync();
    }

    /// <summary>Watch for property updates.</summary>
    /// <param name="handler">Handler with argument of <seealso cref="PropertyChanges"/>.</param>
    /// <returns>Disposable task.</returns>
    public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
    {
      return m_proxy.WatchPropertiesAsync(handler);
    }

    private async void FireEventForExistingDevicesAsync()
    {
      var devices = await this.GetDevicesAsync();
      foreach (var device in devices)
      {
        m_deviceFound?.Invoke(this, new DeviceFoundEventArgs(device, isStateChange: false));
      }
    }

    private async void OnDeviceAddedAsync((ObjectPath objectPath, IDictionary<string, IDictionary<string, object>> interfaces) args)
    {
      if (BlueZManager.IsMatch(BluezConstants.DeviceInterface, args.objectPath, args.interfaces, this))
      {
        var device = Connection.System.CreateProxy<IDevice1>(BluezConstants.DbusService, args.objectPath);

        var dev = await Device.CreateAsync(device);
        m_deviceFound?.Invoke(this, new DeviceFoundEventArgs(dev));
      }
    }

    private async void FireEventIfPropertyAlreadyTrueAsync(AdapterEventHandlerAsync handler, string prop)
    {
      try
      {
        var value = await m_proxy.GetAsync<bool>(prop);
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
              m_poweredOn?.Invoke(this, new BlueZEventArgs());
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
