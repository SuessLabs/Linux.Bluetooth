using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Linux.Bluetooth.Extensions
{
  public static class DeviceExtensions
  {
    /// <summary>Get battery information for the </summary>
    /// <param name="device">Device object.</param>
    /// <example>
    ///   var battery = await device.GetBatteryAsync();
    ///   var percentage = await battery.GetPercentageAsync();
    /// </example>
    /// <returns>Battery or null if unavailable.</returns>
    public static async Task<IBattery1?> GetBatteryAsync(this IDevice1 device)
    {
      try
      {
        // TODO: Create Battery class with OnPropertyChanges for event notification subscriptions
        return await GetBatteryInternalAsync(BluezConstants.BatteryInterface, device);
      }
      catch
      {
        return null;
      }
    }

    /// <summary>Get a GATT Service with the supplied Service UUID.</summary>
    /// <param name="device">Device object.</param>
    /// <param name="serviceUuid">UUID of the Service.</param>
    /// <returns><seealso cref="IGattService1"/> object or null.</returns>
    public static async Task<IGattService1?> GetServiceAsync(this IDevice1 device, string serviceUuid)
    {
      var services = await BlueZManager.GetProxiesAsync<IGattService1>(BluezConstants.GattServiceInterface, device);

      foreach (var service in services)
      {
        var uuid = await service.GetUUIDAsync();
        if (String.Equals(uuid, serviceUuid, StringComparison.OrdinalIgnoreCase))
        {
          return service;
        }
      }

      return null;
    }

    /// <summary>Get Device's collection of GATT Services.</summary>
    /// <param name="device">Device.</param>
    /// <returns>Collection of Gatt Services.</returns>
    public static Task<IReadOnlyList<IGattService1>> GetServicesAsync(this IDevice1 device)
    {
      return BlueZManager.GetProxiesAsync<IGattService1>(BluezConstants.GattServiceInterface, device);
    }

    private static async Task<IBattery1> GetBatteryInternalAsync(string batteryInterface, IDevice1 device)
    {
      var battery = await Task.Run(() =>
        Connection.System.CreateProxy<IBattery1>(BluezConstants.DbusService, device.ObjectPath)
      );

      return battery;
    }
  }
}
