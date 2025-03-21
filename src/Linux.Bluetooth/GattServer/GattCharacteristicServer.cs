namespace Linux.Bluetooth.GattServer
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Threading.Tasks;
  using Tmds.DBus;

  public delegate Task GattCharacteristicEventHandlerAsync(GattCharacteristicServer sender, GattCharacteristicServerValueEventArgs eventArgs);

  public class GattCharacteristicServer : IGattCharacteristic1
  {
    private static int _characteristicCounter = 1;
    private readonly GattCharacteristic1Properties _gattCharacteristicProperties;

    public event GattCharacteristicEventHandlerAsync? WriteValueEvent;
    public event Action<PropertyChanges>? OnPropertiesChanged;

    public ObjectPath ObjectPath { get; }

    public List<GattDescriptor> Descriptors { get; } = new List<GattDescriptor>();

    public GattCharacteristicServer(ObjectPath servicePath, GattCharacteristic1Properties gattCharacteristicProperties)
    {
      ObjectPath = $"{servicePath}/characteristic{_characteristicCounter++:0000}";

      _gattCharacteristicProperties = gattCharacteristicProperties;
      _gattCharacteristicProperties.Service = servicePath;
      _gattCharacteristicProperties.Value ??= new byte[] { };
      _gattCharacteristicProperties.Flags ??= new string[] { };
    }

    public void AddDescriptor(GattDescriptor descriptor)
    {
      Descriptors.Add(descriptor);
    }

    public Task<(CloseSafeHandle fd, ushort mtu)> AcquireNotifyAsync(IDictionary<string, object> Options)
    {
      throw new NotImplementedException();
    }

    public Task<(CloseSafeHandle fd, ushort mtu)> AcquireWriteAsync(IDictionary<string, object> Options)
    {
      throw new NotImplementedException();
    }

    public Task<GattCharacteristic1Properties> GetAllAsync()
    {
      return Task.FromResult(_gattCharacteristicProperties);
    }

    public Task<object> GetAsync(string prop)
    {
      var value = _gattCharacteristicProperties.GetType().GetProperty(prop).GetValue(_gattCharacteristicProperties);
      return Task.FromResult(value);
    }

    public Task<byte[]> ReadValueAsync(IDictionary<string, object> Options)
    {
      throw new NotImplementedException();
    }

    public Task SetAsync(string prop, object val)
    {
      _gattCharacteristicProperties.GetType().GetProperty(prop).SetValue(_gattCharacteristicProperties, val);
      return Task.CompletedTask;
    }

    public Task StartNotifyAsync()
    {
      throw new NotImplementedException();
    }

    public Task StopNotifyAsync()
    {
      throw new NotImplementedException();
    }

    public IDictionary<string, IDictionary<string, object>> GetProperties()
    {
      return new Dictionary<string, IDictionary<string, object>>
      {
          {
              BluezConstants.GattCharacteristicInterface,
              new Dictionary<string, object>
              {
                  { "UUID", _gattCharacteristicProperties.UUID },
                  { "Service", _gattCharacteristicProperties.Service },
                  { "Value", _gattCharacteristicProperties.Value },
                  { "Flags", _gattCharacteristicProperties.Flags },
                  { "Descriptors", Descriptors.Select(d => d.ObjectPath).ToArray() }
              }
          }
      };
    }

    public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
    {
      return SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);
    }

    public Task WriteValueAsync(byte[] Value, IDictionary<string, object> Options)
    {
      WriteValueEvent?.Invoke(this, new GattCharacteristicServerValueEventArgs(Options["device"], Value));
      return Task.CompletedTask;
    }

  }
  public static class CharacteristicFlags
  {
    public const string Authorize = "authorize";
    public const string AuthSignedWrite = "authenticated-signed-writes";
    public const string Broadcast = "broadcast";
    public const string EncryptAuthRead = "encrypt-read";
    public const string EncryptAuthWrite = "encrypt-write";
    public const string EncryptRead = "encrypt-authenticated-read";
    public const string EncryptWrite = "encrypt-authenticated-write";
    public const string Indicate = "indicate";
    public const string Notify = "notify";
    public const string Read = "read";
    public const string ReliableWrite = "reliable-write";
    public const string SecureRead = "secure-read";
    public const string SecureWrite = "secure-write";
    public const string Write = "write";
    public const string WriteNoResponse = "write-without-response";
    public const string WritableAuxiliaries = "writable-auxiliaries";
  }
}
