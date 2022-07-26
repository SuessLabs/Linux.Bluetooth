Welcome to the Plugin.BlueZ wiki!

## Usage Samples

### Get a Bluetooth adapter

```C#
using Plugin.BlueZ;
...

// Get first adapter (useful for single BLE adapter system)
IAdapter1 adapter = (await BlueZManager.GetAdaptersAsync()).FirstOrDefault();

// Get specific adapter
IAdapter1 adapter = await BlueZManager.GetAdapterAsync(adapterName: "hci0");
```

### Scan for Bluetooth devices

```C#
adapter.DeviceFound += adapter_DeviceFoundAsync;

await adapter.StartDiscoveryAsync();
...
await adapter.StopDiscoveryAsync();
```
