using System;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Linux.Bluetooth
{
  public delegate Task DeviceEventHandlerAsync(Device sender, BlueZEventArgs eventArgs);

  /// <summary>
  /// Adds events to IDevice1.
  /// </summary>
  /// <remarks>
  ///   Reference: https://github.com/bluez/bluez/blob/master/doc/device-api.txt
  /// </remarks>
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

    /// <summary>
    ///   This method can be used to cancel a pairing operation initiated by the Pair method.
    /// </summary>
    /// <remarks>
    ///   Possible errors:
    ///   - org.bluez.Error.DoesNotExist
    ///   - org.bluez.Error.Failed
    /// </remarks>
    /// <returns>Task.</returns>
    public Task CancelPairingAsync()
    {
      return _proxy.CancelPairingAsync();
    }

    /// <summary>
    ///   This is a generic method to connect any profiles the remote device supports that can be connected
    ///   to and have been flagged as auto-connectable on our side. If only subset of profiles is already
    ///   connected it will try to connect currently disconnecte ones.
    /// </summary>
    /// <returns>Task.</returns>
    public Task ConnectAsync()
    {
      return _proxy.ConnectAsync();
    }

    /// <summary>
    ///   This method connects a specific profile of this
    ///   device.The UUID provided is the remote service
    ///   UUID for the profile.
    /// </summary>
    /// <remarks>
    ///   Possible errors:
    ///   - org.bluez.Error.Failed
    ///   - org.bluez.Error.InProgress
    ///   - org.bluez.Error.InvalidArguments
    ///   - org.bluez.Error.NotAvailable
    ///   - org.bluez.Error.NotReady
    /// </remarks>
    /// <param name="uuid">Remote profile UUID.</param>
    /// <returns></returns>
    public Task ConnectProfileAsync(string uuid)
    {
      return _proxy.ConnectProfileAsync(uuid);
    }

    public Task DisconnectAsync()
    {
      return _proxy.DisconnectAsync();
    }

    /// <summary>
    ///   This method disconnects a specific profile of this device.The profile needs to be registered client profile.
    ///   There is no connection tracking for a profile, so as long as the profile is registered this will always succeed.
    /// </summary>
    /// <remarks>
    ///   Possible errors:
    ///   - org.bluez.Error.Failed
    ///   - org.bluez.Error.InProgress
    ///   - org.bluez.Error.InvalidArguments
    ///   - org.bluez.Error.NotSupported
    /// </remarks>
    /// <param name="uuid">Profile UUID.</param>
    /// <returns>Task.</returns>
    public Task DisconnectProfileAsync(string uuid)
    {
      return _proxy.DisconnectProfileAsync(uuid);
    }

    /// <summary>Gets all properties for connected device.</summary>
    /// <returns>BlueZ <seealso cref="Device1Properties"/>.</returns>
    public Task<Device1Properties> GetAllAsync()
    {
      return _proxy.GetAllAsync();
    }

    public Task<T> GetAsync<T>(string prop)
    {
      return _proxy.GetAsync<T>(prop);
    }

    /// <summary>Gets all properties for device.</summary>
    /// <returns><seealso cref="DeviceProperties"/> object.</returns>
    public async Task<DeviceProperties> GetPropertiesAsync()
    {
      var p = await _proxy.GetAllAsync();

      var props = new DeviceProperties
      {
        Address = p.Address,
        AddressType = p.AddressType,
        Alias = p.Alias,
        Appearance = p.Appearance,
        Blocked = p.Blocked,
        Class = p.Class,
        Connected = p.Connected, // Connected is marked for deprecation (2024-01-11)
        IsConnected = p.Connected,
        Icon = p.Icon,
        LegacyPairing = p.LegacyPairing,
        ManufacturerData = p.ManufacturerData,
        Modalias = p.Modalias,
        Name = p.Name,
        Paired = p.Paired,
        RSSI = p.RSSI,    // RSSI is marked for deprecation (2024-01-11)
        Rssi = p.RSSI,
        ServiceData = p.ServiceData,
        ServicesResolved = p.ServicesResolved,
        Trusted = p.Trusted,
        TxPower = p.TxPower,
        UUIDs = p.UUIDs,
      };

      return props;
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
