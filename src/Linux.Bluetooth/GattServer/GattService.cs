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
    private static int _serviceCounter = 1;

    public ObjectPath ObjectPath { get; } = $"{basePath}/service{_serviceCounter++:0000}";

    public List<GattCharacteristicServer> Characteristics { get; } = new List<GattCharacteristicServer>();

    public GattCharacteristicServer AddCharacteristic(GattCharacteristic1Properties characteristicProperties,
                                  List<GattDescriptor1Properties>? descriptorProperties = null)
    {
      GattCharacteristicServer characteristic = new(ObjectPath, characteristicProperties);
      if (descriptorProperties is not null)
      {
        foreach (GattDescriptor1Properties properties in descriptorProperties)
        {
          characteristic.AddDescriptor(new(characteristic.ObjectPath, properties));
        }
      }

      Characteristics.Add(characteristic);
      return characteristic;
    }

    public Task<GattService1Properties> GetAllAsync()
    {
      return Task.FromResult(_gattServiceProperties);
    }

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
