using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Linux.Bluetooth.GattServer
{
  public class GattDescriptor : IGattDescriptor1
  {
    private GattDescriptor1Properties _gattDescriptorProperties;

    public ObjectPath ObjectPath { get; }

    public GattDescriptor(ObjectPath servicePath, GattDescriptor1Properties gattDescriptorProperties)
    {
      _gattDescriptorProperties = gattDescriptorProperties;
      ObjectPath = $"{servicePath}/{gattDescriptorProperties.UUID.Substring(0, 8)}";
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
                        //{ "Flags", _gattDescriptorProperties.Flags }
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
      throw new NotImplementedException();
    }

    public Task<GattDescriptor1Properties> GetAllAsync()
    {
      throw new NotImplementedException();
    }

    public Task SetAsync(string prop, object val)
    {
      throw new NotImplementedException();
    }

    public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
    {
      throw new NotImplementedException();
    }
  }
}
