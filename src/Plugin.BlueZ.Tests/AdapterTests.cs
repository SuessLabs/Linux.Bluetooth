using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Plugin.BlueZ.Tests.Services;
using Tmds.DBus;
using Plugin.BlueZ;

namespace Plugin.BlueZ.Tests
{
  [TestClass]
  public class AdapterTests
  {
    [TestMethod]
    public async Task BasicTest()
    {
      // Test Server
      var server = new ServerConnectionOptions();
      using (var connection = new Connection(server))
      {
        await connection.RegisterObjectAsync(new Adapter());
        var boundAddress = await server.StartAsync("tcp:host=localhost");

        Console.WriteLine($"Server is listening at, {boundAddress}");

        using (var client = new Connection(boundAddress))
        {
          await client.Cre
        }


        // Client
        ////using (var client = new Connection(boundAddress))
        ////{
        ////  await client.ConnectAsync();
        ////  Console.WriteLine("Client Connected!");
        ////
        ////  var proxy = client.CreateProxy<IMyService>("Test.BleAdapter", MyService.Path);
        ////  var greeting = await proxy.SendCommandAsync("BLE");
        ////
        ////  Assert.AreEqual(greeting, "Hello BLE!");
        ////}
      }
    }
  }
}
