using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Linux.Bluetooth
{
  public class Advertisement : ILEAdvertisement1, IDisposable
  {
    private LEAdvertisement1Properties _properties;

    public event Action<PropertyChanges>? OnPropertiesChanged;

    public Advertisement(ObjectPath objectPath, LEAdvertisement1Properties properties)
    {
      _properties = properties;
      ObjectPath = objectPath;
    }

    public ObjectPath ObjectPath { get; }

    public void Dispose()
    {
      // Anything to dispose ?
      GC.SuppressFinalize(this);
    }

    public Task<LEAdvertisement1Properties> GetAllAsync()
    {
      return Task.FromResult(_properties);
    }

    public Task<object> GetAsync(string prop)
    {
      return Task.FromResult(_properties.GetType().GetProperty(prop).GetValue(_properties));
    }

    public Task ReleaseAsync()
    {
      throw new NotImplementedException();
    }

    public Task SetAsync(string prop, object val)
    {
      OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty(prop, val));
      _properties.GetType().GetProperty(prop).SetValue(_properties, val);
      return Task.CompletedTask;
    }

    public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
    {
      return SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);
    }
  }
}
