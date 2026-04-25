using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Linux.Bluetooth.Extensions
{
  public static class WatchableExtensions
  {
    /// <summary>Wait for Adapter's Property and specified value to resolve.</summary>
    /// <typeparam name="T">Type of value.</typeparam>
    /// <param name="obj">Adapter.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="value">Value to wait for.</param>
    /// <param name="timeout">TimeSpan to wait for.</param>
    /// <returns>Task or exception.</returns>
    /// <exception cref="TimeoutException">On timeout a <seealso cref="TimeoutException"/> is thrown.</exception>
    public static Task WaitForPropertyValueAsync<T>(this IAdapter1 obj, string propertyName, T value, TimeSpan timeout)
      => WaitForPropertyValueInternalAsync(obj.GetAsync<T>, obj.WatchPropertiesAsync, propertyName, value, timeout);

    /// <summary>Wait for AdvertisingManager's Property and specified value to resolve.</summary>
    /// <typeparam name="T">Type of value.</typeparam>
    /// <param name="obj">AdvertisingManager.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="value">Value to wait for.</param>
    /// <param name="timeout">TimeSpan to wait for.</param>
    /// <returns>Task or exception.</returns>
    /// <exception cref="TimeoutException">On timeout a <seealso cref="TimeoutException"/> is thrown.</exception>
    public static Task WaitForPropertyValueAsync<T>(this ILEAdvertisingManager1 obj, string propertyName, T value, TimeSpan timeout)
      => WaitForPropertyValueInternalAsync(obj.GetAsync<T>, obj.WatchPropertiesAsync, propertyName, value, timeout);

    /// <summary>Wait for Device's Property and specified value to resolve.</summary>
    /// <typeparam name="T">Type of value.</typeparam>
    /// <param name="obj">Device.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="value">Value to wait for.</param>
    /// <param name="timeout">TimeSpan to wait for.</param>
    /// <returns>Task or exception.</returns>
    /// <exception cref="TimeoutException">On timeout a <seealso cref="TimeoutException"/> is thrown.</exception>
    public static Task WaitForPropertyValueAsync<T>(this IDevice1 obj, string propertyName, T value, TimeSpan timeout)
      => WaitForPropertyValueInternalAsync(obj.GetAsync<T>, obj.WatchPropertiesAsync, propertyName, value, timeout);

    /// <summary>Wait for Battery's Property and specified value to resolve.</summary>
    /// <typeparam name="T">Type of value.</typeparam>
    /// <param name="obj">Battery.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="value">Value to wait for.</param>
    /// <param name="timeout">TimeSpan to wait for.</param>
    /// <returns>Task or exception.</returns>
    /// <exception cref="TimeoutException">On timeout a <seealso cref="TimeoutException"/> is thrown.</exception>
    public static Task WaitForPropertyValueAsync<T>(this IBattery1 obj, string propertyName, T value, TimeSpan timeout)
      => WaitForPropertyValueInternalAsync(obj.GetAsync<T>, obj.WatchPropertiesAsync, propertyName, value, timeout);

    /*
    /// <summary>Wait for GattService's Property and specified value to resolve.</summary>
    /// <typeparam name="T">Type of value.</typeparam>
    /// <param name="obj">GattService.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="value">Value to wait for.</param>
    /// <param name="timeout">TimeSpan to wait for.</param>
    /// <returns>Task or exception.</returns>
    /// <exception cref="TimeoutException">On timeout a <seealso cref="TimeoutException"/> is thrown.</exception>
    public static Task WaitForPropertyValueAsync<T>(this IGattService1 obj, string propertyName, T value, TimeSpan timeout)
      => WaitForPropertyValueInternalAsync(obj.GetAsync<T>, obj.WatchPropertiesAsync, propertyName, value, timeout);

    /// <summary>Wait for GattCharacteristic's Property and specified value to resolve.</summary>
    /// <typeparam name="T">Type of value.</typeparam>
    /// <param name="obj">GattCharacteristic.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="value">Value to wait for.</param>
    /// <param name="timeout">TimeSpan to wait for.</param>
    /// <returns>Task or exception.</returns>
    /// <exception cref="TimeoutException">On timeout a <seealso cref="TimeoutException"/> is thrown.</exception>
    public static Task WaitForPropertyValueAsync<T>(this IGattCharacteristic1 obj, string propertyName, T value, TimeSpan timeout)
      => WaitForPropertyValueInternalAsync(obj.GetAsync<T>, obj.WatchPropertiesAsync, propertyName, value, timeout);

    /// <summary>Wait for GattDescriptor's Property and specified value to resolve.</summary>
    /// <typeparam name="T">Type of value.</typeparam>
    /// <param name="obj">GattDescriptor.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="value">Value to wait for.</param>
    /// <param name="timeout">TimeSpan to wait for.</param>
    /// <returns>Task or exception.</returns>
    /// <exception cref="TimeoutException">On timeout a <seealso cref="TimeoutException"/> is thrown.</exception>
    public static Task WaitForPropertyValueAsync<T>(this IGattDescriptor1 obj, string propertyName, T value, TimeSpan timeout)
      => WaitForPropertyValueInternalAsync(obj.GetAsync<T>, obj.WatchPropertiesAsync, propertyName, value, timeout);
    */

    /// <summary>Wait for MediaControl's Property and specified value to resolve.</summary>
    /// <typeparam name="T">Type of value.</typeparam>
    /// <param name="obj">MediaControl.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="value">Value to wait for.</param>
    /// <param name="timeout">TimeSpan to wait for.</param>
    /// <returns>Task or exception.</returns>
    /// <exception cref="TimeoutException">On timeout a <seealso cref="TimeoutException"/> is thrown.</exception>
    public static Task WaitForPropertyValueAsync<T>(this IMediaControl1 obj, string propertyName, T value, TimeSpan timeout)
      => WaitForPropertyValueInternalAsync(obj.GetAsync<T>, obj.WatchPropertiesAsync, propertyName, value, timeout);

    /// <summary>
    /// Wait for watchable objects property and specified value to resolve.
    /// The <paramref name="getAsync"/> and <paramref name="watchPropertiesAsync"/> functions should be of the same object.
    /// </summary>
    /// <typeparam name="T">Type of value.</typeparam>
    /// <param name="getAsync">GetAsync method of the watchable object.</param>
    /// <param name="watchPropertiesAsync">WatchPropertiesAsync function of the watchable object.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="value">Value to wait for.</param>
    /// <param name="timeout">TimeSpan to wait for.</param>
    /// <returns>Task or exception.</returns>
    /// <exception cref="TimeoutException">On timeout a <seealso cref="TimeoutException"/> is thrown.</exception>
    private static async Task WaitForPropertyValueInternalAsync<T>(
      Func<string, Task<T>> getAsync,
      Func<Action<PropertyChanges>, Task<IDisposable>> watchPropertiesAsync,
      string propertyName, T value, TimeSpan timeout)
    {
      // TODO: Change to Task<bool> versus throwing an error.
      var (watchTask, watcher) = WaitForPropertyValueInternal(watchPropertiesAsync, propertyName, value);
      var currentValue = await getAsync(propertyName);

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
    private static (Task, IDisposable) WaitForPropertyValueInternal<T>(Func<Action<PropertyChanges>, Task<IDisposable>> watchPropertiesAsync, string propertyName, T value)
    {
      var taskSource = new TaskCompletionSource<bool>();

      IDisposable watcher = null;
      watcher = watchPropertiesAsync(propertyChanges =>
      {
        try
        {
          if (propertyChanges.Changed.Any(kvp => kvp.Key == propertyName))
          {
            var pair = propertyChanges.Changed.Single(kvp => kvp.Key == propertyName);
            if (pair.Value.Equals(value))
            {
              // Console.WriteLine($"[CHG] {propertyName}: {pair.Value}.");
              taskSource.TrySetResult(true);
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
  }
}
