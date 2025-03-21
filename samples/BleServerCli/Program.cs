using System;
using Linux.Bluetooth;
using Linux.Bluetooth.GattServer;

public class Program
{
  private static async Task Main(string[] args)
  {
    Console.WriteLine("Linux.Bluetooth Server Example");

    // Get BLE Adapter
    var adapter = await GetDefaultAdapterAsync();
    if (adapter is null)
    {
      Console.WriteLine("No BLE Adapters found.");
      return;
    }

    // Turn on BLE Adapter
    await adapter.SetPoweredAsync(true);

    using var bleServer = new GattServer(adapter);
    // Connect to DBus
    await bleServer.InitializeAsync();

    // Create custom Advertisement object
    ushort companyId = 0x0000;
    byte[] advertisementData = [1, 2, 3, 4];
    LEAdvertisement1Properties oAdvProperties = new()
    {
      Type = "peripheral",
      LocalName = "My Linux.Bluetooth Device!",
      ManufacturerData = { { companyId, advertisementData } },
      Appearance = 0x80,
      Discoverable = true,  // False for Broadcast
      IncludeTxPower = true,
    };
    Advertisement advertisement = bleServer.CreateAdvertisement(oAdvProperties);

    // Start advertising
    await bleServer.RegisterAdvertisement(advertisement);

    // Create Gatt Application
    bleServer.CreateGattApplication(applicationPath: null);         // can use a custom application path

    GattService1Properties serviceProperties = new()                // create service
    {
      UUID = "00000001-0000-0000-0000-008000000000",                // mandatory
      Primary = true,                                               // mandatory
    };
    GattService gattService = bleServer.CreateService(serviceProperties);

    GattCharacteristic1Properties characteristicProperties = new()  // create characteristic
    {
      UUID = "00000002-0000-0000-0000-008000000000",                // mandatory
      Flags = [CharacteristicFlags.Write],
    };

    GattDescriptor1Properties descriptorProperties = new()          // create descriptor
    {
      UUID = "00000003-0000-0000-0000-008000000000",                // mandatory
      Value = [5,6,7,8],
      Flags = [DescriptorFlags.Read],
    };

    // Add the charcteristic with its descriptor(s) to the service
    gattService.AddCharacteristic(characteristicProperties, [descriptorProperties]);

    // Start the Gatt Apllication, containing the provided service(s)
    await bleServer.RegisterGattApplication([gattService]);

    // Wait for user input
    Console.WriteLine("Press any key to quit");
    Console.ReadLine();

    // Stop Advertising
    await bleServer.UnregisterAdvertisement();

    // Stop Gatt Application
    await bleServer.UnregisterGattApplication();

  }

  private static async Task<Adapter?> GetDefaultAdapterAsync()
  {
    var adapters = await BlueZManager.GetAdaptersAsync();
    if (adapters.Count == 0)
      return null;

    return adapters[0];
  }

}
