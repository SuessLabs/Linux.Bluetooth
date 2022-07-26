using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linux.Bluetooth.Tests.Services;
using Tmds.DBus;

namespace Linux.Bluetooth.Tests
{
  [TestClass]
  public class DbusLocalServerTests
  {
    [TestMethod]
    public async Task SimulateDbusServerTestAsync()
    {
      // Test Server
      var server = new ServerConnectionOptions();
      using (var connection = new Connection(server))
      {
        await connection.RegisterObjectAsync(new MyService());
        var boundAddress = await server.StartAsync("tcp:host=localhost");

        Console.WriteLine($"Server is listening at, {boundAddress}");

        // Client
        using (var client = new Connection(boundAddress))
        {
          await client.ConnectAsync();
          Console.WriteLine("Client Connected!");

          var proxy = client.CreateProxy<IMyService>("any.service", MyService.Path);
          var greeting = await proxy.SendCommandAsync("BLE");

          Assert.AreEqual(greeting, "Hello BLE!");
        }
      }
    }
  }
}
