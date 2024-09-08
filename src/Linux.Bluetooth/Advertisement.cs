using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Linux.Bluetooth
{
  public class Advertisement : ILEAdvertisement1, IDisposable
  {
    public ObjectPath ObjectPath => throw new NotImplementedException();

    public void Dispose()
    {
      throw new NotImplementedException();
    }

    public Task<LEAdvertisement1Properties> GetAllAsync()
    {
      throw new NotImplementedException();
    }

    public Task<object> GetAsync(string prop)
    {
      throw new NotImplementedException();
    }

    public Task ReleaseAsync()
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
