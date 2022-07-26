using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Linux.Bluetooth.Extensions
{
  public static class GattExtensions
  {
    /// <summary>Get the Characteristic object based on the supplied UUID address.</summary>
    /// <param name="service">GATT Service.</param>
    /// <param name="characteristicUuid">GATT Characteristic UUID.</param>
    /// <returns><seealso cref="GattCharacteristic"/> object or null.</returns>
    public static async Task<GattCharacteristic> GetCharacteristicAsync(this IGattService1 service, string characteristicUuid)
    {
      var characteristics = await BlueZManager.GetProxiesAsync<IGattCharacteristic1>(BluezConstants.GattCharacteristicInterface, service);

      foreach (var characteristic in characteristics)
      {
        var uuid = await characteristic.GetUUIDAsync();
        if (String.Equals(uuid, characteristicUuid, StringComparison.OrdinalIgnoreCase))
        {
          var ch = await GattCharacteristic.CreateAsync(characteristic);
          return ch;
        }
      }

      return null;
    }

    /// <summary>Get characteristics of the Service.</summary>
    /// <param name="service">Service object.</param>
    /// <returns>Collection of GATT Characteristics.</returns>
    public static Task<IReadOnlyList<IGattCharacteristic1>> GetCharacteristicsAsync(this IGattService1 service)
    {
        return BlueZManager.GetProxiesAsync<IGattCharacteristic1>(BluezConstants.GattCharacteristicInterface, service);
    }

    /// <summary>Get the value of the Characteristic. Otherwise throws a <seealso cref="TimeoutException"/> if the max time has elapsed.</summary>
    /// <param name="characteristic">Characteristic.</param>
    /// <param name="timeout">TimeSpan to wait for the value.</param>
    /// <returns>Byte array of the value.</returns>
    /// <exception cref="TimeoutException">Timeout exception.</exception>
    public static async Task<byte[]> ReadValueAsync(this IGattCharacteristic1 characteristic, TimeSpan timeout)
    {
      var options = new Dictionary<string, object>();
      var readTask = characteristic.ReadValueAsync(options);
      var timeoutTask = Task.Delay(timeout);

      await Task.WhenAny(new Task[] { readTask, timeoutTask });
      if (!readTask.IsCompleted)
      {
        throw new TimeoutException("Timed out waiting to read characteristic value.");
      }

      return await readTask;
    }
  }
}
