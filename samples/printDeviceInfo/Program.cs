﻿// This program is the equivalent of the sample code posted to https://stackoverflow.com/questions/53933345/utilizing-bluetooth-le-on-raspberry-pi-using-net-core/56623587#56623587
//
// Use the `bluetoothctl` command-line tool or the Bluetooth Manager GUI to scan for devices and possibly pair.
// Then you can use this program to connect and print "Device Information" GATT service values.
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Linux.Bluetooth;
using Linux.Bluetooth.Constants;
using Linux.Bluetooth.Extensions;

class Program
{
  static TimeSpan timeout = TimeSpan.FromSeconds(15);

  static async Task Main(string[] args)
  {
    if (args.Length < 1)
    {
      Console.WriteLine("Usage: PrintDeviceInfo <deviceAddress> [adapterName]");
      Console.WriteLine("Example: PrintDeviceInfo AA:BB:CC:11:22:33 hci0");
      Console.WriteLine("");
      Console.WriteLine("Available Adapters:");

      var tmp = await BlueZManager.GetAdaptersAsync();
      foreach (var a in tmp)
      {
        Console.WriteLine($"- {a.Name}");
      }

      return;
    }

    var deviceAddress = args[0];

    // Get available Bluetooth adapters
    IAdapter1 adapter;
    if (args.Length > 1)
    {
      // FALSE: 'hci0', TRUE: '/org/bluez/hci0'
      var fullName = args[1].Contains("/org/bluez/");
      adapter = await BlueZManager.GetAdapterAsync(args[1], fullName);
    }
    else
    {
      var adapters = await BlueZManager.GetAdaptersAsync();
      if (adapters.Count == 0)
      {
        throw new Exception("No Bluetooth adapters found.");
      }
      else
      {
        foreach (var a in adapters)
        {
          var name = a.GetNameAsync();
          Console.WriteLine($"  - Adapter: '{name}' - '{a.ObjectPath.ToString()}'");
        }
      }

      adapter = adapters.First();
    }

    // Select Bluetooth Adapter
    var adapterPath = adapter.ObjectPath.ToString();
    var adapterName = adapterPath.Substring(adapterPath.LastIndexOf("/") + 1);
    Console.WriteLine($"Using Bluetooth adapter {adapterName}");

    // Find the Bluetooth to connect to
    var device = await adapter.GetDeviceAsync(deviceAddress);
    if (device == null)
    {
      Console.WriteLine($"Bluetooth peripheral with address '{deviceAddress}' not found. Use `bluetoothctl` or Bluetooth Manager to scan and possibly pair first.");
      return;
    }

    // Connect and wait
    Console.WriteLine("Connecting...");
    await device.ConnectAsync();
    await device.WaitForPropertyValueAsync("Connected", value: true, timeout);
    var deviceObjPath = device.ObjectPath.ToString();
    Console.WriteLine($"Connected ({deviceObjPath}).");

    // Wait for services to be resolved
    Console.WriteLine("Waiting for services to resolve...");
    await device.WaitForPropertyValueAsync("ServicesResolved", value: true, timeout);

    // Output the available services
    var servicesUUID = await device.GetUUIDsAsync();
    Console.WriteLine($"Device offers {servicesUUID.Length} service(s).");
    if (servicesUUID is not null)
    {
      foreach (var svc in servicesUUID)
      {
        Console.WriteLine($"- Uuid: {svc}");
      }
    }

    var deviceInfoServiceFound = servicesUUID.Any(uuid => String.Equals(uuid, GattConstants.DeviceInformationServiceUUID, StringComparison.OrdinalIgnoreCase));
    if (!deviceInfoServiceFound)
    {
      Console.WriteLine("Device doesn't have the Device Information Service. Try pairing first?");
      return;
    }

    // Console.WriteLine("Retrieving Device Information service...");
    var service = await device.GetServiceAsync(GattConstants.DeviceInformationServiceUUID);
    var modelNameCharacteristic = await service.GetCharacteristicAsync(GattConstants.ModelNameCharacteristicUUID);
    var manufacturerCharacteristic = await service.GetCharacteristicAsync(GattConstants.ManufacturerNameCharacteristicUUID);

    int characteristicsFound = 0;
    if (modelNameCharacteristic != null)
    {
      characteristicsFound++;
      Console.WriteLine("Reading model name characteristic...");
      var modelNameBytes = await modelNameCharacteristic.ReadValueAsync(timeout);
      Console.WriteLine($"Model name: {Encoding.UTF8.GetString(modelNameBytes)}");
    }

    if (manufacturerCharacteristic != null)
    {
      characteristicsFound++;
      Console.WriteLine("Reading manufacturer characteristic...");
      var manufacturerBytes = await manufacturerCharacteristic.ReadValueAsync(timeout);
      Console.WriteLine($"Manufacturer: {Encoding.UTF8.GetString(manufacturerBytes)}");
    }

    if (characteristicsFound == 0)
    {
      Console.WriteLine("Model name and manufacturer characteristics not found.");
    }

    await device.DisconnectAsync();
    Console.WriteLine("Disconnected.");
  }
}
