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
    public static async Task<IBattery1> GetBatteryAsync(this IDevice1 device)
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
    public static async Task<IGattService1> GetServiceAsync(this IDevice1 device, string serviceUuid)
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

    /// <summary>Wait for Device's Property and specified value to resolve.</summary>
    /// <typeparam name="T">Type of value.</typeparam>
    /// <param name="obj">Device.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="value">Value to wait for.</param>
    /// <param name="timeout">TimeSpan to wait for.</param>
    /// <returns>Task or exception.</returns>
    /// <exception cref="TimeoutException">On timeout a <seealso cref="TimeoutException"/> is thrown.</exception>
    public static async Task WaitForPropertyValueAsync<T>(this IDevice1 obj, string propertyName, T value, TimeSpan timeout)
    {
      // TODO: Make this available to other generated interfaces too, not just IDevice1.
      // TODO: Change to Task<bool> versus throwing an error.
      var (watchTask, watcher) = WaitForPropertyValueInternal<T>(obj, propertyName, value);
      var currentValue = await obj.GetAsync<T>(propertyName);

      // https://stackoverflow.com/questions/390900/cant-operator-be-applied-to-generic-types-in-c
      if (EqualityComparer<T>.Default.Equals(currentValue, value))
      {
        watcher.Dispose();
        return;
      }

      await Task.WhenAny(new Task[] { watchTask, Task.Delay(timeout) });
      if (!watchTask.IsCompleted)
      {
        throw new TimeoutException($"Timed out waiting for '{propertyName}' to change to '{value}'.");
      }

      // propogate any exceptions.
      await watchTask;
    }

    /// <summary>Wait for property and value.</summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="obj">Device object.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="value">Value to wait for.</param>
    /// <returns><see cref="ValueTuple"/> containing the Task ans Watcher.</returns>
    private static (Task, IDisposable) WaitForPropertyValueInternal<T>(IDevice1 obj, string propertyName, T value)
    {
      var taskSource = new TaskCompletionSource<bool>();

      IDisposable watcher = null;
      watcher = obj.WatchPropertiesAsync(propertyChanges =>
      {
        try
        {
          if (propertyChanges.Changed.Any(kvp => kvp.Key == propertyName))
          {
            var pair = propertyChanges.Changed.Single(kvp => kvp.Key == propertyName);
            if (pair.Value.Equals(value))
            {
              // Console.WriteLine($"[CHG] {propertyName}: {pair.Value}.");
              taskSource.SetResult(true);
              watcher.Dispose();
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Exception: {ex}");
          taskSource.SetException(ex);
          watcher.Dispose();
        }
      });

      return (taskSource.Task, watcher);
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
