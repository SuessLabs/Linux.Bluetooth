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
    private IGattCharacteristic1? _proxy;
    private IDisposable? _propertyWatcher;

    private event GattCharacteristicEventHandlerAsync? _onValue;

    private IGattCharacteristic1 Proxy => 
      _proxy ?? throw new InvalidOperationException("GATT characteristic has not been initialized.");

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

    public ObjectPath ObjectPath => Proxy.ObjectPath;

    public Task<byte[]> ReadValueAsync(IDictionary<string, object> Options)
    {
      return Proxy.ReadValueAsync(Options);
    }

    public Task WriteValueAsync(byte[] Value, IDictionary<string, object> Options)
    {
      return Proxy.WriteValueAsync(Value, Options);
    }

    public Task<(CloseSafeHandle fd, ushort mtu)> AcquireWriteAsync(IDictionary<string, object> Options)
    {
      return Proxy.AcquireWriteAsync(Options);
    }

    public Task<(CloseSafeHandle fd, ushort mtu)> AcquireNotifyAsync(IDictionary<string, object> Options)
    {
      return Proxy.AcquireNotifyAsync(Options);
    }

    public Task StartNotifyAsync()
    {
      return Proxy.StartNotifyAsync();
    }

    public Task StopNotifyAsync()
    {
      return Proxy.StopNotifyAsync();
    }

    public Task<object> GetAsync(string prop)
    {
      return Proxy.GetAsync(prop);
    }

    public Task<GattCharacteristic1Properties> GetAllAsync()
    {
      return Proxy.GetAllAsync();
    }

    public Task SetAsync(string prop, object val)
    {
      return Proxy.SetAsync(prop, val);
    }

    public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
    {
      return Proxy.WatchPropertiesAsync(handler);
    }

    private async void Subscribe()
    {
      try
      {
        await Proxy.StartNotifyAsync();

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
