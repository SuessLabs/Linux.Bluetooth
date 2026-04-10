using System;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Linux.Bluetooth;

/// <summary>
/// BlueZ D-Bus Agent Manager.
/// </summary>
/// <remarks>
///   Reference: https://github.com/bluez/bluez/blob/master/doc/org.bluez.AgentManager.rst
/// </remarks>
public class AgentManager : IAgentManager1, IDisposable
{
  private readonly IAgentManager1 _proxy;

  ~AgentManager()
  {
    Dispose();
  }

  private AgentManager(IAgentManager1 proxy)
  {
    _proxy = proxy;
  }

  /// <summary>
  /// Creates a new AgentManager instance connected to the BlueZ service.
  /// </summary>
  /// <param name="connection">
  /// The D-Bus system connection instance to use for agent management operations.
  /// <para>This connection must be the same instance used for Agent and AgentManager.</para>
  /// </param>
  /// <remarks>
  /// <para>The AgentManager is used to register, unregister, and set default pairing agents for the system.</para>
  /// Always pass the same Connection instance to both Agent.CreateAsync() and AgentManager.CreateAsync().
  /// </remarks>
  /// <returns>A task that completes with a new AgentManager instance.</returns>
  internal static Task<AgentManager> CreateAsync(Connection connection)
  {
    var proxy = connection.CreateProxy<IAgentManager1>(BluezConstants.DbusService, "/org/bluez");
    return Task.FromResult(new AgentManager(proxy));
  }

  /// <summary>
  /// Releases resources used by the AgentManager.
  /// </summary>
  public void Dispose()
  {
    GC.SuppressFinalize(this);
  }

  /// <summary>
  /// Gets the D-Bus object path of the BlueZ Agent Manager.
  /// </summary>
  public ObjectPath ObjectPath => _proxy?.ObjectPath ?? new ObjectPath("/org/bluez");

  /// <summary>
  /// Registers pairing agent.
  /// <para>The object path defines the path of the agent that will be called
  /// when user input is needed and must implement org.bluez.Agent(5) interface.</para>
  /// </summary>
  /// <param name="Agent">The object path of the agent implementation.</param>
  /// <param name="Capability">
  /// Possible capability values:
  /// <list type="bullet">
  ///   <item>"" - Fallback to "KeyboardDisplay"</item>
  ///   <item>"DisplayOnly"</item>
  ///   <item>"DisplayYesNo"</item>
  ///   <item>"KeyboardOnly"</item>
  ///   <item>"NoInputNoOutput"</item>
  ///   <item>"KeyboardDisplay"</item>
  /// </list>
  /// </param>
  /// <remarks>
  /// Every application can register its own agent and for all actions triggered by that application its agent is used.
  /// <para>It is not required by an application to register an agent.
  /// If an application does chooses to not register an agent, the default agent is used. This is on most cases a good idea.
  /// Only application like a pairing wizard should register their own agent.</para>
  /// An application can only register one agent.Multiple agents per application is not supported.
  /// Possible errors:
  /// <list type="bullet">
  ///   <item>org.bluez.Error.InvalidArguments</item>
  ///   <item>org.bluez.Error.AlreadyExists</item>
  /// </list>
  /// </remarks>
  /// <returns>A task representing the asynchronous agent registration operation.</returns>
  public Task RegisterAgentAsync(ObjectPath Agent, string Capability)
  {
    return _proxy!.RegisterAgentAsync(Agent, Capability);
  }

  /// <summary>
  /// Unregisters an agent that has been previously registered using RegisterAgent.
  /// The object path parameter must match the same value that has been used on registration.
  /// </summary>
  /// <param name="Agent">The object path of the agent implementation to unregister.</param>
  /// <remarks>
  /// Possible errors:
  /// <list type="bullet">
  ///   <item>org.bluez.Error.DoesNotExist</item>
  /// </list>
  /// </remarks>
  /// <returns>A task representing the asynchronous agent unregistration operation.</returns>
  public Task UnregisterAgentAsync(ObjectPath Agent)
  {
    return _proxy!.UnregisterAgentAsync(Agent);
  }

  /// <summary>
  /// Requests to make the application agent the default agent. The application is required to register an agent.
  /// <para>Special permission might be required to become the default agent.</para>
  /// </summary>
  /// <param name="Agent">The object path of the agent to set as default.</param>
  /// <remarks>
  /// Possible errors:
  /// <list type="bullet">
  ///   <item>org.bluez.Error.DoesNotExist</item>
  /// </list>
  /// </remarks>
  /// <returns>A task representing the asynchronous operation to set the default agent.</returns>
  public Task RequestDefaultAgentAsync(ObjectPath Agent)
  {
    return _proxy!.RequestDefaultAgentAsync(Agent);
  }
}
