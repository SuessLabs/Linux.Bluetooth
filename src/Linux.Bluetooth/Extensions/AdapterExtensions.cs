using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Linux.Bluetooth.Extensions
{
  public static class AdapterExtensions
  {
    /// <summary>Get available devices.</summary>
    /// <param name="adapter">Adapter object.</param>
    /// <returns>Collection of <seealso cref="Device"/>s.</returns>
    public static async Task<IReadOnlyList<Device>> GetDevicesAsync(this IAdapter1 adapter)
    {
      var devices = await BlueZManager.GetProxiesAsync<IDevice1>(BluezConstants.DeviceInterface, adapter);

      return await Task.WhenAll(devices.Select(Device.CreateAsync));
    }

    /// <summary>Get <seealso cref="Device"/> object from the specifed BLE address.</summary>
    /// <param name="adapter">Adapter object.</param>
    /// <param name="deviceAddress">BLE Device Address.</param>
    /// <returns><seealso cref="Device"/> object or NULL if not found.</returns>
    /// <exception cref="Exception"><see cref="Exception"/> thrown if an duplicate address are found.</exception>
    public static async Task<Device> GetDeviceAsync(this IAdapter1 adapter, string deviceAddress)
    {
      var devices = await BlueZManager.GetProxiesAsync<IDevice1>(BluezConstants.DeviceInterface, adapter);

      var matches = new List<IDevice1>();
      foreach (var device in devices)
      {
        if (String.Equals(await device.GetAddressAsync(), deviceAddress, StringComparison.OrdinalIgnoreCase))
        {
          matches.Add(device);
        }
      }

      // BlueZ can get in a weird state, probably due to random public BLE addresses.
      if (matches.Count > 1)
      {
        throw new Exception($"{matches.Count} devices found with the address {deviceAddress}!");
      }

      var dev = matches.FirstOrDefault();
      if (dev != null)
      {
        return await Device.CreateAsync(dev);
      }

      return null;
    }

    /// <summary>Disposable object which waits for discovered devices.</summary>
    /// <param name="adapter">Adapter object.</param>
    /// <param name="handler">Action delegate with <seealso cref="Device"/> as a parameter.</param>
    /// <returns>Disposable object.</returns>
    public static Task<IDisposable> WatchDevicesAddedAsync(this IAdapter1 adapter, Action<Device> handler)
    {
      async void OnDeviceAdded((ObjectPath objectPath, IDictionary<string, IDictionary<string, object>> interfaces) args)
      {
        if (BlueZManager.IsMatch(BluezConstants.DeviceInterface, args.objectPath, args.interfaces, adapter))
        {
          var device = Connection.System.CreateProxy<IDevice1>(BluezConstants.DbusService, args.objectPath);

          var dev = await Device.CreateAsync(device);
          handler(dev);
        }
      }

      var objectManager = Connection.System.CreateProxy<IObjectManager>(BluezConstants.DbusService, "/");
      return objectManager.WatchInterfacesAddedAsync(OnDeviceAdded);
    }
  }
}
