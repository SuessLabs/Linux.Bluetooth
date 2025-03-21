namespace Linux.Bluetooth.GattServer
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Tmds.DBus;

  public class GattDescriptor : IGattDescriptor1
  {
    private GattDescriptor1Properties _gattDescriptorProperties;

    public ObjectPath ObjectPath { get; }

    public GattDescriptor(ObjectPath characteristicPath, GattDescriptor1Properties gattDescriptorProperties)
    {
      _gattDescriptorProperties = gattDescriptorProperties;
      ObjectPath = $"{characteristicPath}/{gattDescriptorProperties.UUID.Substring(0, 8)}";
    }

    public IDictionary<string, IDictionary<string, object>> GetProperties()
    {
      return new Dictionary<string, IDictionary<string, object>>()
            {
                {
                    BluezConstants.GattDescriptorInterface,
                    new Dictionary<string, object>
                    {
                        { "Characteristic", _gattDescriptorProperties.Characteristic },
                        { "UUID", _gattDescriptorProperties.UUID },
                    }
                }
            };
    }

    public Task<byte[]> ReadValueAsync(IDictionary<string, object> Options)
    {
      throw new NotImplementedException();
    }

    public Task WriteValueAsync(byte[] Value, IDictionary<string, object> Options)
    {
      throw new NotImplementedException();
    }

    public Task<object> GetAsync(string prop)
    {
      var value = _gattDescriptorProperties.GetType().GetProperty(prop).GetValue(_gattDescriptorProperties);
      return Task.FromResult(value);
    }

    public Task<GattDescriptor1Properties> GetAllAsync()
    {
      return Task.FromResult(_gattDescriptorProperties);
    }

    public Task SetAsync(string prop, object val)
    {
      _gattDescriptorProperties.GetType().GetProperty(prop).SetValue(_gattDescriptorProperties, val);
      return Task.CompletedTask;
    }

    public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
    {
      return SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);
    }

    public event Action<PropertyChanges>? OnPropertiesChanged;
  }
}
