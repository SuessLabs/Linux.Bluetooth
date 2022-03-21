using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Plugin.BlueZ.Tests
{
  [DBusInterface("tmds.myservice")]
  public interface IMyService : IDBusObject
  {
    Task<string> SendCommandAsync(string message);
  }

  public class MyService : IMyService
  {
    public static readonly ObjectPath Path = new ObjectPath("/tmds/myservice");

    public ObjectPath ObjectPath { get => Path; }

    public Task<string> SendCommandAsync(string command)
    {
      return Task.FromResult($"Hello {command}!");
    }
  }

  [TestClass]
  public class DbusSimulatorTests
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
