using System;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Linux.Bluetooth
{
  // Manually defined interface according to BlueZ API specifications:
  // https://github.com/bluez/bluez/blob/master/doc/org.bluez.Agent.rst
  [DBusInterface("org.bluez.Agent1")]
  public interface IAgent1 : IDBusObject
  {
    Task ReleaseAsync();
    Task<string> RequestPinCodeAsync(ObjectPath device);
    Task DisplayPinCodeAsync(ObjectPath device, string pincode);
    Task<uint> RequestPasskeyAsync(ObjectPath device);
    Task DisplayPasskeyAsync(ObjectPath device, uint passkey, ushort entered);
    Task RequestConfirmationAsync(ObjectPath device, uint passkey);
    Task RequestAuthorizationAsync(ObjectPath device);
    Task AuthorizeServiceAsync(ObjectPath device, string uuid);
    Task CancelAsync();
  }

  public delegate Task<string> AgentPinCodeEventHandlerAsync(Agent sender, AgentPinCodeEventArgs eventArgs);

  public delegate Task<uint> AgentPasskeyEventHandlerAsync(Agent sender, AgentPasskeyEventArgs eventArgs);

  public delegate Task AgentConfirmationEventHandlerAsync(Agent sender, AgentConfirmationEventArgs eventArgs);

  public delegate Task AgentAuthorizationEventHandlerAsync(Agent sender, AgentAuthorizationEventArgs eventArgs);

  public delegate Task AgentServiceAuthorizationEventHandlerAsync(Agent sender, AgentServiceAuthorizationEventArgs eventArgs);

  public delegate Task AgentDisplayPinCodeEventHandlerAsync(Agent sender, AgentDisplayPinCodeEventArgs eventArgs);

  public delegate Task AgentDisplayPasskeyEventHandlerAsync(Agent sender, AgentDisplayPasskeyEventArgs eventArgs);

  public delegate Task AgentOperationCancelledEventHandlerAsync(Agent sender, EventArgs eventArgs);

  /// <summary>
  /// BlueZ D-Bus Agent.
  /// </summary>
  /// <remarks>
  ///   Reference: https://github.com/bluez/bluez/blob/master/doc/org.bluez.Agent.rst
  /// </remarks>
  public class Agent : IAgent1, IDisposable
  {
    private const string DefaultCapability = "NoInputNoOutput";

    private readonly string _capability;
    private readonly ObjectPath _objectPath;
    private readonly Connection _connection;

    private string _defaultPinCode = "0000";
    private uint _defaultPasskey = 000000;

    private event AgentPinCodeEventHandlerAsync? _pinCodeRequested;
    private event AgentPasskeyEventHandlerAsync? _passkeyRequested;
    private event AgentConfirmationEventHandlerAsync? _confirmationRequested;
    private event AgentAuthorizationEventHandlerAsync? _authorizationRequested;
    private event AgentServiceAuthorizationEventHandlerAsync? _serviceAuthorizationRequested;
    private event AgentDisplayPinCodeEventHandlerAsync? _pinCodeDisplayed;
    private event AgentDisplayPasskeyEventHandlerAsync? _passkeyDisplayed;
    private event AgentOperationCancelledEventHandlerAsync? _operationCancelled;

    ~Agent()
    {
      Dispose();
    }

    private Agent(Connection connection, string capability = DefaultCapability, ObjectPath? objectPath = null)
    {
      _connection = connection;
      _capability = capability;
      _objectPath = objectPath ?? new ObjectPath($"/linux/bluetooth/agent/{Guid.NewGuid().ToString().Replace("-", string.Empty)}");
    }

    /// <summary>
    /// Creates and registers a new BlueZ Agent on the D-Bus system.
    /// </summary>
    /// <param name="connection">
    /// The D-Bus system connection instance for agent registration.
    /// <para>This connection must be the same instance used for Agent and AgentManager.</para>
    /// </param>
    /// <param name="capability">The agent capability level. Default is "NoInputNoOutput". 
    /// Possible values: "DisplayOnly", "DisplayYesNo", "KeyboardOnly", "KeyboardDisplay".</param>
    /// <param name="objectPath">Optional custom D-Bus object path. If null, a unique path will be generated.</param>
    /// <remarks>
    /// <para>The agent is automatically registered on the D-Bus system and ready to handle pairing requests from BlueZ daemon.</para>
    /// Always pass the same Connection instance to both Agent.CreateAsync() and AgentManager.CreateAsync().
    /// </remarks>
    /// <returns>A task that completes with a new Agent instance.</returns>
    internal static async Task<Agent> CreateAsync(Connection connection, string capability = DefaultCapability, ObjectPath? objectPath = null)
    {
      var agent = new Agent(connection, capability, objectPath);
      await agent.RegisterObjectAsync();
      return agent;
    }

    public void Dispose()
    {
      UnregisterObject();
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gets the D-Bus object path of this agent.
    /// </summary>
    /// <remarks>
    /// This path is used to identify the agent in BlueZ operations.
    /// </remarks>
    public ObjectPath ObjectPath => _objectPath;

    /// <summary>
    /// Gets the capability level of this agent.
    /// </summary>
    /// <remarks>
    /// Possible values: "NoInputNoOutput", "DisplayOnly", "DisplayYesNo", "KeyboardOnly", "KeyboardDisplay".
    /// </remarks>
    public string Capability => _capability;

    /// <summary>
    /// Sets the default PIN code to be used for device pairing.
    /// </summary>
    /// <param name="pinCode">The PIN code (1-16 alphanumeric characters).</param>
    /// <exception cref="ArgumentNullException">Thrown if pinCode is null.</exception>
    /// <exception cref="ArgumentException">Thrown if pinCode length is invalid or contains non-alphanumeric characters.</exception>
    public void SetDefaultPinCode(string pinCode)
    {
      if (pinCode is null)
      {
        throw new ArgumentNullException(nameof(pinCode));
      }

      if (pinCode.Length < 1 || pinCode.Length > 16)
      {
        throw new ArgumentException("PIN code must be between 1 and 16 characters.", nameof(pinCode));
      }

      foreach (var c in pinCode)
      {
        if (!char.IsLetterOrDigit(c))
        {
          throw new ArgumentException("PIN code must be alphanumeric.", nameof(pinCode));
        }
      }

      _defaultPinCode = pinCode;
    }

    /// <summary>
    /// Sets the default passkey to be used for Bluetooth 2.1 Secure Simple Pairing (SSP).
    /// </summary>
    /// <param name="passkey">The passkey (0-999999).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if passkey is greater than 999999.</exception>
    public void SetDefaultPasskey(uint passkey)
    {
      if (passkey > 999999)
      {
        throw new ArgumentOutOfRangeException(nameof(passkey), "Passkey must be between 0 and 999999.");
      }

      _defaultPasskey = passkey;
    }

    private void UnregisterObject()
    {
      _connection.UnregisterObject(_objectPath);
    }

    private async Task RegisterObjectAsync()
    {
      await _connection.RegisterObjectAsync(this);
    }

    /// <summary>
    /// Occurs when bluetoothd(8) requests a PIN code for pairing.
    /// </summary>
    public event AgentPinCodeEventHandlerAsync PinCodeRequested
    {
      add { _pinCodeRequested += value; }
      remove { _pinCodeRequested -= value; }
    }

    /// <summary>
    /// Occurs when bluetoothd(8) requests a passkey for Secure Simple Pairing (SSP).
    /// </summary>
    public event AgentPasskeyEventHandlerAsync PasskeyRequested
    {
      add { _passkeyRequested += value; }
      remove { _passkeyRequested -= value; }
    }

    /// <summary>
    /// Occurs when bluetoothd(8) requests confirmation of a passkey.
    /// </summary>
    public event AgentConfirmationEventHandlerAsync ConfirmationRequested
    {
      add { _confirmationRequested += value; }
      remove { _confirmationRequested -= value; }
    }

    /// <summary>
    /// Occurs when bluetoothd(8) requests authorization for an incoming pairing.
    /// </summary>
    public event AgentAuthorizationEventHandlerAsync AuthorizationRequested
    {
      add { _authorizationRequested += value; }
      remove { _authorizationRequested -= value; }
    }

    /// <summary>
    /// Occurs when bluetoothd(8) requests authorization for service access.
    /// </summary>
    public event AgentServiceAuthorizationEventHandlerAsync ServiceAuthorizationRequested
    {
      add { _serviceAuthorizationRequested += value; }
      remove { _serviceAuthorizationRequested -= value; }
    }

    /// <summary>
    /// Occurs when bluetoothd(8) needs to display a PIN code for pairing.
    /// </summary>
    public event AgentDisplayPinCodeEventHandlerAsync PinCodeDisplayed
    {
      add { _pinCodeDisplayed += value; }
      remove { _pinCodeDisplayed -= value; }
    }

    /// <summary>
    /// Occurs when bluetoothd(8) needs to display a passkey for SSP pairing.
    /// </summary>
    public event AgentDisplayPasskeyEventHandlerAsync PasskeyDisplayed
    {
      add { _passkeyDisplayed += value; }
      remove { _passkeyDisplayed -= value; }
    }

    /// <summary>
    /// Occurs when a pairing request is cancelled by bluetoothd(8).
    /// </summary>
    public event AgentOperationCancelledEventHandlerAsync OperationCancelled
    {
      add { _operationCancelled += value; }
      remove { _operationCancelled -= value; }
    }

    /// <summary>
    /// This method gets called when bluetoothd(8) unregisters the agent.
    /// </summary>
    /// <remarks>
    /// An agent can use it to do cleanup tasks. There is no need to unregister the agent,
    /// because when this method gets called it has already been unregistered.
    /// </remarks>
    /// <returns>A task representing the asynchronous cleanup operation.</returns>
    public Task ReleaseAsync()
    {
      return Task.CompletedTask;
    }

    /// <summary>
    /// This method gets called when bluetoothd(8) needs to get the passkey for an authentication.
    /// The return value should be a string of 1-16 characters length. The string can be alphanumeric.
    /// </summary>
    /// <param name="device">The object path of the device requesting authentication.</param>
    /// <remarks>
    /// Possible errors:
    /// <list type="bullet">
    ///   <item>org.bluez.Error.Rejected</item>
    ///   <item>org.bluez.Error.Canceled</item>
    /// </list>
    /// </remarks>
    /// <returns>A task that completes with a <c>string</c> representing the PIN code for authentication.</returns>
    public Task<string> RequestPinCodeAsync(ObjectPath device)
    {
      if (_pinCodeRequested is not null)
      {
        return _pinCodeRequested(this, new AgentPinCodeEventArgs(device));
      }

      return Task.FromResult(_defaultPinCode);
    }

    /// <summary>
    /// This method gets called when bluetoothd(8) needs to display a pincode for an authentication.
    /// An empty reply should be returned.
    /// <para>When the pincode needs no longer to be displayed, the Cancel method of the agent will be called.</para>
    /// </summary>
    /// <param name="device">The object path of the device requesting authentication.</param>
    /// <param name="pincode">The PIN code to display (typically 6 digits, zero-padded).</param>
    /// <remarks>
    /// This is used during the pairing process of keyboards that don't support Bluetooth 2.1 Secure Simple Pairing,
    /// in contrast to DisplayPasskey which is used for those that do.
    /// <para>This method will only ever be called once since older keyboards do not support typing notification.</para>
    /// <para>Note that the PIN will always be a 6-digit number, zero-padded to 6 digits.
    /// This is for harmony with the later specification.</para>
    /// Possible errors:
    /// <list type="bullet">
    ///   <item>org.bluez.Error.Rejected</item>
    ///   <item>org.bluez.Error.Canceled</item>
    /// </list>
    /// </remarks>
    /// <returns>A task representing the asynchronous PIN code display operation to the subscriber.</returns>
    public Task DisplayPinCodeAsync(ObjectPath device, string pincode)
    {
      return _pinCodeDisplayed?.Invoke(this, new AgentDisplayPinCodeEventArgs(device, pincode)) ?? Task.CompletedTask;
    }

    /// <summary>
    /// This method gets called when bluetoothd(8) needs to get the passkey for an authentication.
    /// The return value should be a numeric value between 0-999999.
    /// </summary>
    /// <param name="device">The object path of the device requesting authentication.</param>
    /// <remarks>
    /// Possible errors:
    /// <list type="bullet">
    ///   <item>org.bluez.Error.Rejected</item>
    ///   <item>org.bluez.Error.Canceled</item>
    /// </list>
    /// </remarks>
    /// <returns>A task that completes with a <c>uint</c> representing the passkey for authentication.</returns>
    public Task<uint> RequestPasskeyAsync(ObjectPath device)
    {
      if (_passkeyRequested is not null)
      {
        return _passkeyRequested(this, new AgentPasskeyEventArgs(device));
      }

      return Task.FromResult(_defaultPasskey);
    }

    /// <summary>
    /// This method gets called when bluetoothd(8) needs to display a passkey for an authentication.
    /// <para>The entered parameter indicates the number of already typed keys on the remote side.</para>
    /// An empty reply should be returned.When the passkey needs no longer to be displayed, the Cancel method of the agent will be called.
    /// </summary>
    /// <param name="device">The object path of the device requesting authentication.</param>
    /// <param name="passkey">The passkey to display (0-999999, typically 6 digits).</param>
    /// <param name="entered">The number of digits already entered by the user on the remote device.</param>
    /// <remarks>
    /// During the pairing process this method might be called multiple times to update the entered value.
    /// <para>Note that the passkey will always be a 6 - digit number,
    /// so the display should be zero-padded at the start if the value contains less than 6 digits.</para>
    /// </remarks>
    /// <returns>A task representing the asynchronous passkey display operation to the subscriber.</returns>
    public Task DisplayPasskeyAsync(ObjectPath device, uint passkey, ushort entered)
    {
      return _passkeyDisplayed?.Invoke(this, new AgentDisplayPasskeyEventArgs(device, passkey, entered)) ?? Task.CompletedTask;
    }

    /// <summary>
    /// This method gets called when bluetoothd(8) needs to confirm a passkey for an authentication.
    /// <para>To confirm the value it should return an empty reply or an error in case the passkey is invalid.</para>
    /// </summary>
    /// <param name="device">The object path of the device requesting authentication.</param>
    /// <param name="passkey">The passkey to confirm (0-999999, typically 6 digits).</param>
    /// <remarks>
    /// <para>Note that the passkey will always be a 6-digit number,
    /// so the display should be zero-padded at the start if the value contains less than 6 digits.</para>
    /// Possible errors:
    /// <list type="bullet">
    ///   <item>org.bluez.Error.Rejected</item>
    ///   <item>org.bluez.Error.Canceled</item>
    /// </list>
    /// </remarks>
    /// <returns>A task representing the asynchronous passkey confirmation request to the subscriber.</returns>
    public Task RequestConfirmationAsync(ObjectPath device, uint passkey)
    {
      return _confirmationRequested?.Invoke(this, new AgentConfirmationEventArgs(device, passkey)) ?? Task.CompletedTask;
    }

    /// <summary>
    /// This method gets called to request the user to authorize an incoming pairing attempt
    /// which would in other circumstances trigger the just-works model,
    /// or when the user plugged in a device that implements cable pairing.
    /// In the latter case, the device would not be connected to the adapter via Bluetooth yet.
    /// </summary>
    /// <param name="device">The object path of the device requesting authorization.</param>
    /// <remarks>
    /// Possible errors:
    /// <list type="bullet">
    ///   <item>org.bluez.Error.Rejected</item>
    ///   <item>org.bluez.Error.Canceled</item>
    /// </list>
    /// </remarks>
    /// <returns>A task representing the asynchronous authorization request to the subscriber.</returns>
    public Task RequestAuthorizationAsync(ObjectPath device)
    {
      return _authorizationRequested?.Invoke(this, new AgentAuthorizationEventArgs(device)) ?? Task.CompletedTask;
    }

    /// <summary>
    /// This method gets called when the service daemon needs to authorize a connection/service request.
    /// </summary>
    /// <param name="device">The object path of the device requesting service access.</param>
    /// <param name="uuid">The UUID of the service being authorized.</param>
    /// <remarks>
    /// Possible errors:
    /// <list type="bullet">
    ///   <item>org.bluez.Error.Rejected</item>
    ///   <item>org.bluez.Error.Canceled</item>
    /// </list>
    /// </remarks>
    /// <returns>A task representing the asynchronous service authorization request to the subscriber.</returns>
    public Task AuthorizeServiceAsync(ObjectPath device, string uuid)
    {
      return _serviceAuthorizationRequested?.Invoke(this, new AgentServiceAuthorizationEventArgs(device, uuid)) ?? Task.CompletedTask;
    }

    /// <summary>
    /// This method gets called to indicate that the agent request failed before a reply was returned.
    /// </summary>
    /// <returns>A task representing the asynchronous cancellation notification to the subscriber.</returns>
    public Task CancelAsync()
    {
      return _operationCancelled?.Invoke(this, EventArgs.Empty) ?? Task.CompletedTask;
    }
  }
}
