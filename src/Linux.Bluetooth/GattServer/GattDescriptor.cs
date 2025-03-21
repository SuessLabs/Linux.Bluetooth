namespace Linux.Bluetooth.GattServer
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Tmds.DBus;

  public delegate Task GattDescriptorValueEventHandlerAsync(GattDescriptor sender, GattDescriptorValueEventArgs eventArgs);

  public class GattDescriptor : IGattDescriptor1
  {
    private static int _descriptorCounter = 1;
    private GattDescriptor1Properties _gattDescriptorProperties;

    public event GattDescriptorValueEventHandlerAsync? WriteValueEvent;
    public event Action<PropertyChanges>? OnPropertiesChanged;

    public ObjectPath ObjectPath { get; }

    public GattDescriptor(ObjectPath characteristicPath, GattDescriptor1Properties gattDescriptorProperties)
    {
      ObjectPath = $"{characteristicPath}/descriptor{_descriptorCounter++:0000}";

      _gattDescriptorProperties = gattDescriptorProperties;
      _gattDescriptorProperties.Characteristic = characteristicPath;
      _gattDescriptorProperties.Value ??= new byte[] { };
      _gattDescriptorProperties.Flags ??= new string[] { };
    }

    public IDictionary<string, IDictionary<string, object>> GetProperties()
    {
      return new Dictionary<string, IDictionary<string, object>>()
            {
                {
                    BluezConstants.GattDescriptorInterface,
                    new Dictionary<string, object>
                    {
                        { "UUID", _gattDescriptorProperties.UUID },
                        { "Characteristic", _gattDescriptorProperties.Characteristic },
                        { "Value", _gattDescriptorProperties.Value },
                        { "Flags", _gattDescriptorProperties.Flags },
                    }
                }
            };
    }

    public void SetValue(byte[] value)
    {
      _gattDescriptorProperties.Value = value;
    }

    public Task<byte[]> ReadValueAsync(IDictionary<string, object> Options)
    {
      // todo: handle Options
      return Task.FromResult(_gattDescriptorProperties.Value);
    }

    public Task WriteValueAsync(byte[] Value, IDictionary<string, object> Options)
    {
      WriteValueEvent?.Invoke(this, new GattDescriptorValueEventArgs(Options["device"], Value));
      return Task.CompletedTask;
    }

    public Task<object> GetAsync(string prop)
    {
      var value = _gattDescriptorProperties.GetType().GetProperty(prop).GetValue(_gattDescriptorProperties);
      return Task.FromResult(value);
    }

    public Task<GattDescriptor1Properties> GetAllAsync()
    {
      return Task.FromResult(_gattDescriptorProperties);
    }

    public Task SetAsync(string prop, object val)
    {
      _gattDescriptorProperties.GetType().GetProperty(prop).SetValue(_gattDescriptorProperties, val);
      return Task.CompletedTask;
    }

    public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
    {
      return SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);
    }

  }
  public static class DescriptorFlags
  {
    public const string EncryptAuthRead = "encrypt-read";
    public const string EncryptAuthWrite = "encrypt-write";
    public const string EncryptRead = "encrypt-authenticated-read";
    public const string EncryptWrite = "encrypt-authenticated-write";
    public const string Read = "read";
    public const string SecureRead = "secure-read";
    public const string SecureWrite = "secure-write";
    public const string Write = "write";
  }
}
