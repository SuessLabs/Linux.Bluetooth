namespace Linux.Bluetooth.GattServer
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Tmds.DBus;

  public class GattApplication(ObjectPath? objectPath = null) : IObjectManager
  {
    public ObjectPath ObjectPath { get; } = objectPath ?? $"/{Guid.NewGuid().ToString().Substring(0, 8)}";

    public List<GattService> Services { get; } = new List<GattService>();

    public void AddService(GattService service)
    {
      Services.Add(service);
    }

    public Task<IDictionary<ObjectPath, IDictionary<string, IDictionary<string, object>>>> GetManagedObjectsAsync()
    {
      IDictionary<ObjectPath, IDictionary<string, IDictionary<string, object>>> result =
                new Dictionary<ObjectPath, IDictionary<string, IDictionary<string, object>>>();
      foreach (var service in Services)
      {
        result[service.ObjectPath] = service.GetProperties();
        foreach (var characteristic in service.Characteristics)
        {
          result[characteristic.ObjectPath] = characteristic.GetProperties();
          foreach (var descriptor in characteristic.Descriptors)
          {
            result[descriptor.ObjectPath] = descriptor.GetProperties();
          }
        }
      }

      return Task.FromResult(result);
    }

    public async Task<IDisposable> WatchInterfacesAddedAsync(Action<(ObjectPath @object, IDictionary<string, IDictionary<string, object>> interfaces)> handler, Action<Exception> onError = null)
    {
      await Task.Yield();
      return Task.CompletedTask;
    }

    public async Task<IDisposable> WatchInterfacesRemovedAsync(Action<(ObjectPath @object, string[] interfaces)> handler, Action<Exception> onError = null)
    {
      await Task.Yield();
      return Task.CompletedTask;
    }

  }
}
