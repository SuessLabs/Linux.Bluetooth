# Linux Bluetooth Library for .NET

> ## Announcement
>
> _The next generation of the [Plugin.BlueZ](https://github.com/SuessLabs/Plugin.BlueZ) library!_

The Linux.Bluetooth library for .NET gives developers the ability to quickly stand up and interface with Linux's BLE radio with very little effort. There's no need to recall the laborious D-Bus API calls, we handle that for you.

[![Linux.Bluetooth NuGet Badge](https://buildstats.info/nuget/Linux.Bluetooth?dWidth=70&includePreReleases=true)](https://www.nuget.org/packages/Linux.Bluetooth/)

![Debugging Image](https://github.com/SuessLabs/Linux.Bluetooth/blob/master/docs/Adapter%20-%20ObjectPath%20Contents.png)

The library uses, [Tmds.DBus](https://github.com/tmds/Tmds.DBus) to access Linux's D-Bus, the preferred interface for Bluetooth in userspace.

Check out the SuessLabs article on using [Plugin.BlueZ](https://suesslabs.com/csharp/net-and-linux-bluetooth/)

## Requirements

* Linux
* .NET 6, 7, or 8

_Sorry, older Mono (.NET Framework) versions are not supported._

This project has been validated against, BlueZ v5.50 and above. You can check which version you're using with, `bluetoothd -v`.

### Supported Distributions

Linux.Bluetooth aims to support Linux Distributions where both .NET and BlueZ is supported. Officially, this NuGet package has been tested against Ubuntu 20.04 LTS.

List of [BlueZ supported](http://www.bluez.org/about/) distros:

* Ubuntu Linux
* Raspbian (_Raspberry PI_)
* Debian GNU/Linux
* Fedora Core / Red Hat Linux
* OpenSuSE / SuSE Linux
* Mandrake Linux
* Gentoo Linux
* Chrome OS

## Installation

```bash
dotnet add package Linux.Bluetooth
```

## Usage

C# events are available for several properties. Events are useful for properly handling disconnects and reconnects.

### Get a Bluetooth adapter

```C#
using Linux.Bluetooth;
...
IAdapter1 adapter = (await BlueZManager.GetAdaptersAsync()).FirstOrDefault();
```

or get a particular adapter:

```C#
IAdapter1 adapter = await BlueZManager.GetAdapterAsync(adapterName: "hci0");
```

## Scan for Bluetooth devices

```C#
adapter.DeviceFound += adapter_DeviceFoundAsync;

await adapter.StartDiscoveryAsync();
...
await adapter.StopDiscoveryAsync();
```

### Get Devices

`adapter.DeviceFound` (above) will be called immediately for existing devices, and as new devices show up during scanning; `eventArgs.IsStateChange` can be used to distinguish between existing and new devices. Alternatively you can can use `GetDevicesAsync`:

```C#
IReadOnlyList<Device> devices = await adapter.GetDevicesAsync();
```

### Connect to a Device

```C#
device.Connected += device_ConnectedAsync;
device.Disconnected += device_DisconnectedAsync;
device.ServicesResolved += device_ServicesResolvedAsync;

await device.ConnectAsync();
```

Alternatively, you can wait for "Connected" and "ServicesResolved" to equal true:

```C#
TimeSpan timeout = TimeSpan.FromSeconds(15);

await device.ConnectAsync();
await device.WaitForPropertyValueAsync("Connected", value: true, timeout);
await device.WaitForPropertyValueAsync("ServicesResolved", value: true, timeout);
```

### Retrieve a GATT Service and Characteristic

Prerequisite: You must be connected to a device and services must be resolved. You may need to pair with the device in order to use some services.

Example using GATT Device Information Service UUIDs.

```C#
string serviceUUID = "0000180a-0000-1000-8000-00805f9b34fb";
string characteristicUUID = "00002a24-0000-1000-8000-00805f9b34fb";

IGattService1 service = await device.GetServiceAsync(serviceUUID);
IGattCharacteristic1 characteristic = await service.GetCharacteristicAsync(characteristicUUID);
```

### Read a GATT Characteristic value

```C#
byte[] value = await characteristic.ReadValueAsync(timeout);

string modelName = Encoding.UTF8.GetString(value);
```

### Subscribe to GATT Characteristic Notifications

```C#
characteristic.Value += characteristic_Value;
...

private static async Task characteristic_Value(GattCharacteristic characteristic, GattCharacteristicValueEventArgs e)
{
  try
  {
    Console.WriteLine($"Characteristic value (hex): {BitConverter.ToString(e.Value)}");

    Console.WriteLine($"Characteristic value (UTF-8): \"{Encoding.UTF8.GetString(e.Value)}\"");
  }
  catch (Exception ex)
  {
    Console.Error.WriteLine(ex);
  }
}
```

## Tips

It may be necessary to pair with a device for a GATT service to be visible or for reading GATT characteristics to work. To pair, one option is to run `bluetoothctl` (or `sudo bluetoothctl`)
and then run `default agent` and `agent on` within `bluetoothctl`. Watch `bluetoothctl` for pairing requests.

See [Ubuntu's Introduction to Pairing](https://ubuntu.com/core/docs/bluez/reference/pairing/introduction).

### BluetoothCtl Helper

From command line, use `bluetoothctl` or Bluetooth Manager to scan and retrieve device UUIDs and Services to assist with debugging.."

```bash
$ bluetoothctl

; Scan for devices
scan on

; Stop Scanning
scan off

; List known devices
devices
```

## Contributing

See [Contributing](./github/CONTRIBUTING.md).

## Coming Soon

* Deprecating `Linux.Bluetooth.Extensions`. It will now just be `Linux.Bluetooth` namespace.

## Reference

* [Prism Avalonia](https://github.com/AvaloniaCommunity/Prism.Avalonia) for Linux GUI test app
* [Doing Bluetooth Low Energy on Linux](https://elinux.org/images/3/32/Doing_Bluetooth_Low_Energy_on_Linux.pdf)
* **BlueZ API**:
  * [HEAD](https://git.kernel.org/pub/scm/bluetooth/bluez.git/tree/doc)
  * [v5.53](https://git.kernel.org/pub/scm/bluetooth/bluez.git/tree/doc?h=5.53) - _i.e. Ubuntu v20.04 LTS_
* [BlueZ Official Site](http://www.bluez.org/)
* [Install BlueZ on the Raspberry PI](https://learn.adafruit.com/install-bluez-on-the-raspberry-pi/overview)

**Sponsored by:** [Suess Labs](https://suesslabs.com) a subsidary of [Xeno Innovations, Inc](https://xenoinc.com).
