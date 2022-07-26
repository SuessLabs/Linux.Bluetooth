using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Linux.Bluetooth
{
  public delegate Task GattCharacteristicEventHandlerAsync(GattCharacteristic sender, GattCharacteristicValueEventArgs eventArgs);

  /// <summary>
  /// Adds events to IGattCharacteristic1.
  /// </summary>
  public class GattCharacteristic : IGattCharacteristic1, IDisposable
  {
    private IGattCharacteristic1 _proxy;
    private IDisposable _propertyWatcher;

    private event GattCharacteristicEventHandlerAsync _onValue;

    ~GattCharacteristic()
    {
      Dispose();
    }

    internal static async Task<GattCharacteristic> CreateAsync(IGattCharacteristic1 proxy)
    {
      var characteristic = new GattCharacteristic
      {
        _proxy = proxy,
      };

      characteristic._propertyWatcher = await proxy.WatchPropertiesAsync(characteristic.OnPropertyChanges);

      return characteristic;
    }

    public void Dispose()
    {
      _propertyWatcher?.Dispose();
      _propertyWatcher = null;

      GC.SuppressFinalize(this);
    }

    public event GattCharacteristicEventHandlerAsync Value
    {
      add
      {
        _onValue += value;

        // Subscribe here instead of CreateAsync, because not all GATT characteristics are notifable.
        Subscribe();
      }
      remove
      {
        _onValue -= value;
      }
    }

    public ObjectPath ObjectPath => _proxy.ObjectPath;

    public Task<byte[]> ReadValueAsync(IDictionary<string, object> Options)
    {
      return _proxy.ReadValueAsync(Options);
    }

    public Task WriteValueAsync(byte[] Value, IDictionary<string, object> Options)
    {
      return _proxy.WriteValueAsync(Value, Options);
    }

    public Task<(CloseSafeHandle fd, ushort mtu)> AcquireWriteAsync(IDictionary<string, object> Options)
    {
      return _proxy.AcquireWriteAsync(Options);
    }

    public Task<(CloseSafeHandle fd, ushort mtu)> AcquireNotifyAsync(IDictionary<string, object> Options)
    {
      return _proxy.AcquireNotifyAsync(Options);
    }

    public Task StartNotifyAsync()
    {
      return _proxy.StartNotifyAsync();
    }

    public Task StopNotifyAsync()
    {
      return _proxy.StopNotifyAsync();
    }

    public Task<T> GetAsync<T>(string prop)
    {
      return _proxy.GetAsync<T>(prop);
    }

    public Task<GattCharacteristic1Properties> GetAllAsync()
    {
      return _proxy.GetAllAsync();
    }

    public Task SetAsync(string prop, object val)
    {
      return _proxy.SetAsync(prop, val);
    }

    public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
    {
      return _proxy.WatchPropertiesAsync(handler);
    }

    private async void Subscribe()
    {
      try
      {
        await _proxy.StartNotifyAsync();

        // Is there a way to check if a characteristic supports Read?
        // // Reading the current value will trigger OnPropertyChanges.
        // var options = new Dictionary<string, object>();
        // var value = await m_proxy.ReadValueAsync(options);
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine($"Error subscribing to characteristic value: {ex}");
      }
    }

    private void OnPropertyChanges(PropertyChanges changes)
    {
      // Console.WriteLine("OnPropertyChanges called.");
      foreach (var pair in changes.Changed)
      {
        switch (pair.Key)
        {
          case "Value":
            _onValue?.Invoke(this, new GattCharacteristicValueEventArgs((byte[])pair.Value));
            break;
        }
      }
    }
  }
}
