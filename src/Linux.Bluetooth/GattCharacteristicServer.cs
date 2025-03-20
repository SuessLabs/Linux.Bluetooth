namespace Linux.Bluetooth
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Tmds.DBus;

  public class GattCharacteristicServer : IGattCharacteristic1
  {
    private readonly GattCharacteristic1Properties _gattCharacteristicProperties;

    public ObjectPath ObjectPath { get; }

    public List<GattDescriptor> Descriptors { get; } = new List<GattDescriptor>();

    public GattCharacteristicServer(ObjectPath servicePath, GattCharacteristic1Properties gattCharacteristicProperties)
    {
      _gattCharacteristicProperties = gattCharacteristicProperties;
      ObjectPath = $"{servicePath}/{gattCharacteristicProperties.UUID.Substring(0, 8)}";
      _gattCharacteristicProperties.Service = servicePath;
    }

    public void AddDescriptor(GattDescriptor descriptor)
    {
      Descriptors.Add(descriptor);
    }

    public Task<(CloseSafeHandle fd, ushort mtu)> AcquireNotifyAsync(IDictionary<string, object> Options)
    {
      throw new NotImplementedException();
    }

    public Task<(CloseSafeHandle fd, ushort mtu)> AcquireWriteAsync(IDictionary<string, object> Options)
    {
      throw new NotImplementedException();
    }

    public Task<GattCharacteristic1Properties> GetAllAsync()
    {
      return Task.FromResult(_gattCharacteristicProperties);
    }

    public Task<object> GetAsync(string prop)
    {
      var value = _gattCharacteristicProperties.GetType().GetProperty(prop).GetValue(_gattCharacteristicProperties);
      return Task.FromResult(value);
    }

    public Task<byte[]> ReadValueAsync(IDictionary<string, object> Options)
    {
      throw new NotImplementedException();
    }

    public Task SetAsync(string prop, object val)
    {
      _gattCharacteristicProperties.GetType().GetProperty(prop).SetValue(_gattCharacteristicProperties, val);
      return Task.CompletedTask;
    }

    public Task StartNotifyAsync()
    {
      throw new NotImplementedException();
    }

    public Task StopNotifyAsync()
    {
      throw new NotImplementedException();
    }

    public IDictionary<string, IDictionary<string, object>> GetProperties()
    {
      return new Dictionary<string, IDictionary<string, object>>
      {
          {
              BluezConstants.GattCharacteristicInterface,
              new Dictionary<string, object>
              {
                  { "Service", _gattCharacteristicProperties.Service },
                  { "UUID", _gattCharacteristicProperties.UUID },
                  { "Flags", _gattCharacteristicProperties.Flags },
                  { "Descriptors", Descriptors.Select(d => d.ObjectPath).ToArray() }
              }
          }
      };
    }

    public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
    {
      return SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);
    }

    public Task WriteValueAsync(byte[] Value, IDictionary<string, object> Options)
    {
      throw new NotImplementedException();
    }

    public event Action<PropertyChanges>? OnPropertiesChanged;
  }
}
