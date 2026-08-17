using System.Threading.Tasks;
using System;
using Tmds.DBus;
using System.Collections.Generic;

namespace Linux.Bluetooth
{
  /// <summary>
  /// Advertisement Monitor class.
  /// Requires 'Experimental = true' and 'KernelExperimental = true' in BlueZ main.conf
  /// </summary>
  public class AdvertisementMonitor : IAdvertisementMonitor1, IObjectManager, IDisposable
  {
    public ObjectPath ObjectPath { get; }

    public event EventHandler<Device>? DeviceFoundEvent;
    public event EventHandler<Device>? DeviceLostEvent;

    /// <summary>
    ///   Raised when the D-Bus connection backing this monitor drops.
    /// </summary>
    /// <remarks>
    ///   The connection is created by this monitor and never reconnects, so BlueZ loses the monitor
    ///   registration for good: no further DeviceFoundEvent or DeviceLostEvent is ever raised. Dispose this
    ///   monitor and create a new one to resume monitoring.
    /// </remarks>
    public event EventHandler? ConnectionLost;

    private readonly Connection _conn;
    private readonly AdvertisementMonitor1Properties _properties;
    private readonly IAdvertisementMonitorManager1 _manager;

    // No-op IDisposable for the “Watch…” methods
    struct NoOpDisposable : IDisposable { public void Dispose() { } }

    public AdvertisementMonitor(Adapter adapter, AdvertisementMonitor1Properties properties)
    {
      _conn = new Connection(Address.System);
      _properties = properties;

      ObjectPath = new ObjectPath($"{adapter.Name}/advmon0");

      _manager = _conn.CreateProxy<IAdvertisementMonitorManager1>(
        BluezConstants.DbusService,
        adapter.ObjectPath);
    }

    public async Task StartAsync()
    {
      await _conn.ConnectAsync();
      await _conn.RegisterObjectAsync(this);
      await _manager.RegisterMonitorAsync(ObjectPath);

      _conn.StateChanged += OnConnectionStateChanged;
    }

    public async Task StopAsync()
    {
      _conn.StateChanged -= OnConnectionStateChanged;

      await _manager.UnregisterMonitorAsync(ObjectPath);
      _conn.UnregisterObject(this);
    }

    private void OnConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
    {
      if (e.State == ConnectionState.Disconnected)
      {
        ConnectionLost?.Invoke(this, EventArgs.Empty);
      }
    }

    /// <summary>
    /// Closes the D-Bus connection opened by the constructor. Each monitor owns its own connection, so a
    /// monitor dropped without disposing leaks its socket until finalization.
    /// </summary>
    public void Dispose()
    {
      _conn.StateChanged -= OnConnectionStateChanged;
      _conn.Dispose();
      GC.SuppressFinalize(this);
    }

    public Task ActivateAsync()
    {
      Console.WriteLine("Advertisement monitoring activated");
      return Task.CompletedTask;
    }

    public Task ReleaseAsync()
    {
      Console.WriteLine("Advertisement monitoring released");
      return Task.CompletedTask;
    }

    public async Task DeviceFoundAsync(ObjectPath devicePath)
    {
      IDevice1 proxy = _conn.CreateProxy<IDevice1>(BluezConstants.DbusService, devicePath);
      Device device = await Device.CreateAsync(proxy);
      DeviceFoundEvent?.Invoke(this, device);
    }

    public async Task DeviceLostAsync(ObjectPath devicePath)
    {
      IDevice1 proxy = _conn.CreateProxy<IDevice1>(BluezConstants.DbusService, devicePath);
      Device device = await Device.CreateAsync(proxy);
      DeviceLostEvent?.Invoke(this, device);
    }

    public Task<object> GetAsync(string prop)
    {
      var value = _properties.GetType().GetProperty(prop).GetValue(_properties);
      return Task.FromResult(value);
    }

    public Task<AdvertisementMonitor1Properties> GetAllAsync()
    {
      return Task.FromResult(_properties);
    }

    public Task SetAsync(string prop, object val)
    {
      _properties.GetType().GetProperty(prop).SetValue(_properties, val);
      return Task.CompletedTask;
    }

    public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
    {
      return SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);
    }

    public event Action<PropertyChanges>? OnPropertiesChanged;

    public Task<IDictionary<ObjectPath, IDictionary<string, IDictionary<string, object>>>>
      GetManagedObjectsAsync()
    {
      Dictionary<string, object> ifaceProps = new()
      {
        { "Type",               _properties.Type              },
        { "RSSILowThreshold",   _properties.RSSILowThreshold  },
        { "RSSIHighThreshold",  _properties.RSSIHighThreshold },
        { "RSSILowTimeout",     _properties.RSSILowTimeout    },
        { "RSSIHighTimeout",    _properties.RSSIHighTimeout   },
        { "RSSISamplingPeriod", _properties.RSSISamplingPeriod},
        { "Patterns",           _properties.Patterns          }
      };

      IDictionary<string, IDictionary<string, object>> interfaces = new Dictionary<string, IDictionary<string, object>>()
      {
        { "org.bluez.AdvertisementMonitor1", ifaceProps }
      };

      IDictionary<ObjectPath, IDictionary<string, IDictionary<string, object>>> result =
          new Dictionary<ObjectPath, IDictionary<string, IDictionary<string, object>>>()
      {
        { ObjectPath, interfaces }
      };

      return Task.FromResult(result);
    }

    public Task<IDisposable> WatchInterfacesAddedAsync(
      Action<(ObjectPath @object, IDictionary<string, IDictionary<string, object>> interfaces)> handler,
      Action<Exception>? onError = null)
    {
      return Task.FromResult<IDisposable>(new NoOpDisposable());
    }

    public Task<IDisposable> WatchInterfacesRemovedAsync(
      Action<(ObjectPath @object, string[] interfaces)> handler,
      Action<Exception>? onError = null)
    {
      return Task.FromResult<IDisposable>(new NoOpDisposable());
    }
  }

}
