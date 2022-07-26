using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linux.Bluetooth.Tests.Services;
using Tmds.DBus;
using Linux.Bluetooth;

namespace Linux.Bluetooth.Tests
{
  [TestClass]
  public class AdapterTests
  {
    ////[TestMethod]
    ////public async Task CanInitializeAdapterTest()
    ////{
    ////  // Test Server
    ////  var server = new ServerConnectionOptions();
    ////  using (var connection = new Connection(server))
    ////  {
    ////    await connection.RegisterObjectAsync(new Adapter());
    ////    var boundAddress = await server.StartAsync("tcp:host=localhost");
    ////
    ////    Console.WriteLine($"Server is listening at, {boundAddress}");
    ////
    ////    // Multi-client connections:
    ////    //// for (int cli = 0; i < 2; i++) { .. create clients .. };
    ////
    ////    // 1. Create fake adapter for testing
    ////    // 2. Connect to fake adapter
    ////    //    var adapter = await BlueZManager.GetAdapterAsync("hci99");
    ////    // 3. Get properties from FakeAdapterService
    ////    // 4. Dispose objects.
    ////            
    ////
    ////    // Previous Client Example
    ////    ////using (var client = new Connection(boundAddress))
    ////    ////{
    ////    ////  await client.ConnectAsync();
    ////    ////  Console.WriteLine("Client Connected!");
    ////    ////
    ////    ////  var proxy = client.CreateProxy<IMyService>("Test.BleAdapter", MyService.Path);
    ////    ////  var greeting = await proxy.SendCommandAsync("BLE");
    ////    ////
    ////    ////  Assert.AreEqual(greeting, "Hello BLE!");
    ////    ////}
    ////  }
    ////}
  }
}
