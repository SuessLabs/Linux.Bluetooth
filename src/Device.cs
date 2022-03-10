using System;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Plugin.DotNetBlueZ
{
  public delegate Task DeviceEventHandlerAsync(Device sender, BlueZEventArgs eventArgs);

  /// <summary>
  /// Adds events to IDevice1.
  /// </summary>
  public class Device : IDevice1, IDisposable
  {
    private const string DeviceConnected = "Connected";
    private const string DeviceServicesResolved = "ServicesResolved";

    private IDevice1 _proxy;
    private IDisposable _propertyWatcher;

    private event DeviceEventHandlerAsync OnConnected;

    private event DeviceEventHandlerAsync OnResolved;

    ~Device()
    {
      Dispose();
    }

    internal static async Task<Device> CreateAsync(IDevice1 proxy)
    {
      var device = new Device
      {
        _proxy = proxy,
      };
      device._propertyWatcher = await proxy.WatchPropertiesAsync(device.OnPropertyChanges);

      return device;
    }

    public void Dispose()
    {
      _propertyWatcher?.Dispose();
      _propertyWatcher = null;

      GC.SuppressFinalize(this);
    }

    public event DeviceEventHandlerAsync Connected
    {
      add
      {
        OnConnected += value;
        FireEventIfPropertyAlreadyTrueAsync(OnConnected, "Connected");
      }
      remove
      {
        OnConnected -= value;
      }
    }

    public event DeviceEventHandlerAsync Disconnected;

    public event DeviceEventHandlerAsync ServicesResolved
    {
      add
      {
        OnResolved += value;
        FireEventIfPropertyAlreadyTrueAsync(OnResolved, "ServicesResolved");
      }
      remove
      {
        OnResolved -= value;
      }
    }

    public ObjectPath ObjectPath => _proxy.ObjectPath;

    public Task CancelPairingAsync()
    {
      return _proxy.CancelPairingAsync();
    }

    public Task ConnectAsync()
    {
      return _proxy.ConnectAsync();
    }

    public Task ConnectProfileAsync(string UUID)
    {
      return _proxy.ConnectProfileAsync(UUID);
    }

    public Task DisconnectAsync()
    {
      return _proxy.DisconnectAsync();
    }

    public Task DisconnectProfileAsync(string UUID)
    {
      return _proxy.DisconnectProfileAsync(UUID);
    }

    public Task<Device1Properties> GetAllAsync()
    {
      return _proxy.GetAllAsync();
    }

    public Task<T> GetAsync<T>(string prop)
    {
      return _proxy.GetAsync<T>(prop);
    }

    public Task PairAsync()
    {
      return _proxy.PairAsync();
    }

    public Task SetAsync(string prop, object val)
    {
      return _proxy.SetAsync(prop, val);
    }

    public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
    {
      return _proxy.WatchPropertiesAsync(handler);
    }

    private async void FireEventIfPropertyAlreadyTrueAsync(DeviceEventHandlerAsync handler, string prop)
    {
      try
      {
        var value = await _proxy.GetAsync<bool>(prop);
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
          case DeviceConnected:
            if (true.Equals(pair.Value))
              OnConnected?.Invoke(this, new BlueZEventArgs());
            else
              Disconnected?.Invoke(this, new BlueZEventArgs());

            break;

          case DeviceServicesResolved:
            if (true.Equals(pair.Value))
              OnResolved?.Invoke(this, new BlueZEventArgs());

            break;
        }
      }
    }
  }
}