namespace Linux.Bluetooth.GattServer
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Tmds.DBus;

  public class GattService(ObjectPath basePath, GattService1Properties gattServiceProperties) : IGattService1
  {
    private readonly GattService1Properties _gattServiceProperties = gattServiceProperties;

    public ObjectPath ObjectPath { get; } = $"{basePath}/{gattServiceProperties.UUID.Substring(0,8)}";

    public List<GattCharacteristicServer> Characteristics { get; } = new List<GattCharacteristicServer>();

    public void AddCharacteristic(GattCharacteristicServer characteristic)
    {
      Characteristics.Add(characteristic);
    }

    public Task<GattService1Properties> GetAllAsync()
    {
      return Task.FromResult(_gattServiceProperties);
    }

    //public Task<T> GetAsync<T>(string prop)
    //{
    //  var value = _gattServiceProperties.GetType().GetProperty(prop).GetValue(_gattServiceProperties);
    //  return Task.FromResult((T)value);
    //}

    public Task<object> GetAsync(string prop)
    {
      var value = _gattServiceProperties.GetType().GetProperty(prop).GetValue(_gattServiceProperties);
      return Task.FromResult(value);
    }

    public Task SetAsync(string prop, object val)
    {
      _gattServiceProperties.GetType().GetProperty(prop).SetValue(_gattServiceProperties, val);
      return Task.CompletedTask;
    }

    public IDictionary<string, IDictionary<string, object>> GetProperties()
    {
      return new Dictionary<string, IDictionary<string, object>>
        {
            {
                BluezConstants.GattServiceInterface,
                new Dictionary<string, object>
                {
                    { "UUID", _gattServiceProperties.UUID },
                    { "Primary", _gattServiceProperties.Primary },
                    { "Characteristics", Characteristics.Select(c => c.ObjectPath).ToArray() }
                }
            }
        };
    }

    public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
    {
      return SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);
    }

    public event Action<PropertyChanges>? OnPropertiesChanged;
  }
}
