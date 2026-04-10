// This example demonstrates how to use a Bluetooth agent to manage incoming device connections.
// When another device initiates a connection, the current device handles the pairing process
// and automatically marks the device as trusted.

using Linux.Bluetooth;
using Linux.Bluetooth.Extensions;
using Tmds.DBus;

public class AgentExample
{
  public static async Task Main(string[] args)
  {
    Console.WriteLine("Linux.Bluetooth Agent Example");

    using var connection = new Connection(Address.System);
    await connection.ConnectAsync();

    //var capability = "DisplayOnly";
    //var capability = "DisplayYesNo";
    //var capability = "KeyboardOnly";
    var capability = "NoInputNoOutput";
    //var capability = "KeyboardDisplay";

    using var agent = await BlueZManager.CreateAgentAsync(connection, capability);

    Console.WriteLine($"Agent created with capability: {agent.Capability}");
    Console.WriteLine($"Agent object path: {agent.ObjectPath}");

    agent.SetDefaultPinCode("1234");
    agent.SetDefaultPasskey(123456);

    agent.PinCodeRequested += (sender, eventArgs) =>
    {
      Console.WriteLine($"PIN code requested for device: {eventArgs.Device}");
      Console.Write("Enter your PIN code: ");
      var pin = Console.ReadLine()!;
      return Task.FromResult(pin);
    };

    agent.PasskeyRequested += (sender, eventArgs) =>
    {
      Console.WriteLine($"Passkey requested for device: {eventArgs.Device}");
      Console.Write("Enter your passkey: ");
      var passkey = uint.Parse(Console.ReadLine()!);
      return Task.FromResult(passkey);
    };

    agent.ConfirmationRequested += (sender, eventArgs) =>
    {
      Console.WriteLine($"Confirmation requested for device: {eventArgs.Device}, passkey: {eventArgs.Passkey}");
      return Task.CompletedTask;
    };

    agent.AuthorizationRequested += (sender, eventArgs) =>
    {
      Console.WriteLine($"Authorization requested for device: {eventArgs.Device}");
      return Task.CompletedTask;
    };

    agent.ServiceAuthorizationRequested += (sender, eventArgs) =>
    {
      Console.WriteLine($"Service authorization requested for device: {eventArgs.Device}, service: {eventArgs.ServiceUuid}");
      return Task.CompletedTask;
    };

    agent.PinCodeDisplayed += (sender, eventArgs) =>
    {
      Console.WriteLine($"Display PIN code for device {eventArgs.Device}: {eventArgs.PinCode}");
      return Task.CompletedTask;
    };

    agent.PasskeyDisplayed += (sender, eventArgs) =>
    {
      Console.WriteLine($"Display passkey for device {eventArgs.Device}: {eventArgs.Passkey} (entered: {eventArgs.Entered})");
      return Task.CompletedTask;
    };

    agent.OperationCancelled += (sender, eventArgs) =>
    {
      Console.WriteLine("Operation cancelled");
      return Task.CompletedTask;
    };

    var agentManager = await BlueZManager.GetAgentManagerAsync(connection);
    Console.WriteLine($"AgentManager object path: {agentManager.ObjectPath}");

    await agentManager.RegisterAgentAsync(agent.ObjectPath, agent.Capability);
    await agentManager.RequestDefaultAgentAsync(agent.ObjectPath);

    var adapters = await BlueZManager.GetAdaptersAsync();
    var adapter = adapters[0];
    Console.WriteLine($"Selected adapter {adapter.Name} ({adapter.ObjectPath})");

    if (!await adapter.GetPoweredAsync())
    {
      await adapter.SetPoweredAsync(true);
    }

    await adapter.SetPairableAsync(true);
    await adapter.SetDiscoverableAsync(true);

    Console.WriteLine("Now try to connect to this device and do some actions.");

    while (true)
    {
      var devices = await adapter.GetDevicesAsync();

      foreach (var device in devices)
      {
        var connected = await device.GetConnectedAsync();
        Console.WriteLine($"Connected: {connected}; Device: {device.ObjectPath}");

        var trusted = await device.GetTrustedAsync();
        Console.WriteLine($"Trusted: {trusted}; Device: {device.ObjectPath}");

        if (connected)
        {
          if (!trusted)
          {
            Console.WriteLine($"Pairing device {device.ObjectPath}");
            await device.PairAsync();
            Console.WriteLine($"Paired device {device.ObjectPath}");

            Console.WriteLine($"Trusting device {device.ObjectPath}");
            await device.SetTrustedAsync(true);
            Console.WriteLine($"Trusted device {device.ObjectPath}");
          }
        }
      }

      await Task.Delay(1000);
    }
  }
}
