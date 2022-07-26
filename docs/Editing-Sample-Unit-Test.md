# Overview

> **WARNING:** Coming soon!

## Sample Generic DBus Unit Test

```cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tmds.DBus;

namespace Xeno.MyProject.Tests.DBus
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
    [Ignore("The test passes successfully by itself but not when ran as a group.")]
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

```
