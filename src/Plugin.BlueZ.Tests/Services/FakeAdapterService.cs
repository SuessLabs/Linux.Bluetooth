using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Plugin.BlueZ.Tests.Services
{
  public class FakeAdapterService : IAdapter1
  {
    private string _address = "00:11:22:33:44:55";
    private string _addressType = "FakeType";
    private string _name = "hci99";
    private string _alias = "FakeAdapterAlias";
    private bool _isDiscovering = false;
    private Adapter1Properties _adapterProperties = new();
    public FakeAdapterService()
    {
      _adapterProperties = new Adapter1Properties
      {
        Address = _address,
        AddressType = _addressType,
        Name = _name,
        Alias = _alias,
        Class = 0,
        Powered = true,
        Discoverable = true,
        DiscoverableTimeout = 1000,
        Pairable = true,
        PairableTimeout = 1000,
        Discovering = false,
        UUIDs = new string[] { default(Guid).ToString(), },
        Modalias = "WTF",
      };

    }

    public ObjectPath ObjectPath => $"/org/bluez/{_name}";

    /// <summary>Custom helper from our <seealso cref="Adapter"/> class.</summary>
    public string Name => ObjectPath.ToString();

    public async Task<Adapter1Properties> GetAllAsync()
    {
      _adapterProperties.Discovering = _isDiscovering;

      return _adapterProperties;
    }

    public Task<T> GetAsync<T>(string prop)
    {


      throw new NotImplementedException();
    }

    public Task<string[]> GetDiscoveryFiltersAsync()
    {
      throw new NotImplementedException();
    }

    public Task RemoveDeviceAsync(ObjectPath Device)
    {
      throw new NotImplementedException();
    }

    public Task SetAsync(string prop, object val)
    {
      throw new NotImplementedException();
    }

    public Task SetDiscoveryFilterAsync(IDictionary<string, object> Properties)
    {
      throw new NotImplementedException();
    }

    public Task StartDiscoveryAsync()
    {
      throw new NotImplementedException();
    }

    public Task StopDiscoveryAsync()
    {
      throw new NotImplementedException();
    }

    public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
    {
      throw new NotImplementedException();
    }
  }
}
