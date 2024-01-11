using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Linux.Bluetooth;
using Linux.Bluetooth.Extensions;
using Prism.Mvvm;

namespace BleTester.ViewModels;

public class BleDevice : BindableBase, IDisposable
{
  private string _name = string.Empty;
  private Device? _nativeDevice = null;
  private string _rssi = string.Empty;
  private int _txPower = 0;
  private string _uuid = string.Empty;

  public BleDevice()
  {
    Properties = new();
  }

  /// <summary>Link to known BLE device.</summary>
  /// <param name="nativeDevice">BlueZ device</param>
  /// <param name="isConnected">Only set if you know the status of the connection.</param>
  public BleDevice(Device nativeDevice, bool isConnected = false)
  {
    NativeDevice = nativeDevice;
    IsConnected = isConnected;
    Properties = new()
    {
      IsConnected = isConnected,
    };

    NativeDevice.Connected += Device_OnConnectAsync;
    NativeDevice.Disconnected += Device_OnDisconnectAsync;
    NativeDevice.ServicesResolved += Device_OnServicesResolvedAsync;
  }

  public BleDevice(Device nativeDevice, DeviceProperties props)
  {
    NativeDevice = nativeDevice;
    IsConnected = props.IsConnected;
    Properties = props;

    // TODO: Move all handlers under here
    NativeDevice.Connected += Device_OnConnectAsync;
    NativeDevice.Disconnected += Device_OnDisconnectAsync;
    NativeDevice.ServicesResolved += Device_OnServicesResolvedAsync;
  }

  public event Action<Device, BlueZEventArgs> OnConnected;

  public event Action<Device, BlueZEventArgs> OnDisconnected;

  public event Action<Device, BlueZEventArgs> OnServicesResolved;

  /// <summary>Is device connected.</summary>
  public bool IsConnected { get; set; }

  /// <summary>Gets or sets the name of the device.</summary>
  public string Name
  {
    get => _name;
    set => SetProperty(ref _name, value);
  }

  public DeviceProperties Properties { get; private set; }

  /// <summary>Gets or sets the signal strength.</summary>
  public string Rssi { get => _rssi; set => SetProperty(ref _rssi, value); }

  public ObservableCollection<string> ServiceUuids { get; set; }

  public int TxPower { get => _txPower; set => SetProperty(ref _txPower, value); }

  /// <summary>Gets or sets the device's Universally Unique Id.</summary>
  public string Uuid
  {
    get => _uuid; set => SetProperty(ref _uuid, value);
  }

  /// <summary>Gets or sets the BLE Device.</summary>
  private Device NativeDevice { get => _nativeDevice; set => SetProperty(ref _nativeDevice, value); }

  public async Task<bool> ConnectAsync()
  {
    try
    {
      if (IsConnected)
        return true;

      NativeDevice.Connected -= Device_OnConnectAsync;
      NativeDevice.Disconnected -= Device_OnDisconnectAsync;
      NativeDevice.ServicesResolved -= Device_OnServicesResolvedAsync;

      NativeDevice.Connected += Device_OnConnectAsync;
      NativeDevice.Disconnected += Device_OnDisconnectAsync;
      NativeDevice.ServicesResolved += Device_OnServicesResolvedAsync;

      await NativeDevice.ConnectAsync();

      IsConnected = true;

      return true;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error connecting to device:{Environment.NewLine}{ex}");
      return false;
    }
  }

  public async Task<bool> DisconnectAsync()
  {
    try
    {
      if (NativeDevice is null)
        return true;

      await NativeDevice.DisconnectAsync();
      IsConnected = false;

      NativeDevice.Connected -= Device_OnConnectAsync;
      NativeDevice.Disconnected -= Device_OnDisconnectAsync;
      NativeDevice.ServicesResolved -= Device_OnServicesResolvedAsync;

      return true;
    }
    catch (Exception ex)
    {
      Debug.WriteLine($"Error disconnecting from device:{Environment.NewLine}{ex}");
      return false;
    }
  }

  public void Dispose()
  {
    if (NativeDevice is null)
      return;

    NativeDevice.Connected -= Device_OnConnectAsync;
    NativeDevice.Disconnected -= Device_OnDisconnectAsync;
    NativeDevice.ServicesResolved -= Device_OnServicesResolvedAsync;
  }

  /// <summary>Read string from GATT Characteristic.</summary>
  /// <param name="serviceUuid">Service UUID.</param>
  /// <param name="characteristicUuid">Characteristic UUID.</param>
  /// <returns>String data from Characteristic.</returns>
  public async Task<string> ReadAsync(string serviceUuid, string characteristicUuid)
  {
    byte[] bytes = await ReadBytesAsync(serviceUuid, characteristicUuid);
    return Helper.StringFromBytes(bytes);
  }

  /// <summary>Read Characteristic using default Service and Characteristic UUIDs.</summary>
  /// <returns>Data returned from device.</returns>
  public async Task<string> ReadAsync()
  {
    return await ReadAsync(Constants.BasicServiceUuid, Constants.BasicCharacteristicRxUuid);
  }

  /// <summary>Read bytes from GATT Characteristic.</summary>
  /// <param name="serviceUuid">Service UUID.</param>
  /// <param name="characteristicUuid">Characteristic UUID.</param>
  /// <returns>Byte array from Characteristic.</returns>
  public async Task<byte[]> ReadBytesAsync(string serviceUuid, string characteristicUuid)
  {
    // TODO: Check if we're connected
    ////if (!IsConnected)
    ////    return new byte[] { };

    byte[] data = new byte[] { };

    try
    {
      var gattChar = await GetCharacteristicAsync(serviceUuid, characteristicUuid);

      if (gattChar == null)
        return new byte[] { };

      byte[] value = await gattChar.ReadValueAsync(new Dictionary<string, object>());
      data = value;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error reading Handshake{ex.Message}");
    }

    return data;
  }

  /// <summary>Send byte array to GATT Characteristic.</summary>
  /// <param name="serviceUuid">Service UUID.</param>
  /// <param name="characteristicUuid">Characteristic UUID.</param>
  /// <param name="data">Byte array.</param>
  /// <returns>True on success.</returns>
  public async Task<bool> WriteAsync(string serviceUuid, string characteristicUuid, byte[] data)
  {
    // TODO: Check if we're connected
    var svcChar = await GetCharacteristicAsync(serviceUuid, characteristicUuid);
    if (svcChar == null)
      return false;

    var success = true;

    try
    {
      await svcChar.WriteValueAsync(data, new Dictionary<string, object>());
      Console.WriteLine($"Command Sent: '{data}'");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error sending command:{Environment.NewLine}{ex}");
      success = false;
    }

    return success;
  }

  /// <summary>Write string to GATT Characteristic.</summary>
  /// <param name="serviceUuid">Service UUID.</param>
  /// <param name="characteristicUuid">Characteristic UUID.</param>
  /// <param name="data">String data to send.</param>
  /// <returns>True on success.</returns>
  public async Task<bool> WriteAsync(string serviceUuid, string characteristicUuid, string data)
  {
    var bytes = Helper.StringToBytes(data);
    return await WriteAsync(serviceUuid, characteristicUuid, bytes);
  }

  /// <summary>Write to default Service Characteristic.</summary>
  /// <param name="data">String to write.</param>
  /// <returns>Task.</returns>
  public async Task<bool> WriteAsync(string data)
  {
    // TODO: Check if we're connected
    // TODO: Allow custom Service/Characteristic UUIDs
    return await WriteAsync(Constants.BasicServiceUuid, Constants.BasicCharacteristicTxUuid, data);
  }

  private async Task Device_OnConnectAsync(Device device, BlueZEventArgs eventArgs)
  {
    // TODO: Moved device actions from BleService to here
    IsConnected = true;
  }

  /// <summary>Device disconnected event.</summary>
  /// <param name="device">Device.</param>
  /// <param name="eventArgs">BlueZ Event Args.</param>
  /// <returns>Task.</returns>
  private async Task Device_OnDisconnectAsync(Device device, BlueZEventArgs eventArgs)
  {
    // TODO: Moved device actions from BleService to here
    IsConnected = false;
  }

  /// <summary>Device service found event.</summary>
  /// <param name="device">Device.</param>
  /// <param name="eventArgs">BlueZ Event Args.</param>
  /// <returns>Task.</returns>
  private async Task Device_OnServicesResolvedAsync(Device device, BlueZEventArgs eventArgs)
  {
    // TODO: Moved device actions from BleService to here
    return;
  }

  /// <summary>Attempt to get Characteristic object from specified service./summary>
  /// <param name="serviceUuid">Service UUID.</param>
  /// <param name="characteristicUuid">Characteristic UUID.</param>
  /// <returns>GATT Characteristic.</returns>
  private async Task<GattCharacteristic?> GetCharacteristicAsync(string serviceUuid, string characteristicUuid)
  {
    // TODO: Don't hard-code the Service here. Allo it to be passed in for flexibility
    GattCharacteristic? svcChar = null;

    try
    {
      var service = await NativeDevice.GetServiceAsync(serviceUuid);
      if (service == null)
      {
        Console.WriteLine($"Service UUID '{serviceUuid}' was not found; check connection.");
        return null;
      }

      svcChar = await service.GetCharacteristicAsync(characteristicUuid);
      if (svcChar == null)
      {
        Console.WriteLine($"Service CharacteristicUuid not found ('{characteristicUuid}')");
        return null;
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error getting service characteristic '{serviceUuid}' - '{characteristicUuid}'{Environment.NewLine}{ex.Message}");
      return null;
    }

    return svcChar;
  }

  /// <summary>Gets the device's properties and description.</summary>
  /// <param name="device">Device.</param>
  /// <returns>Device true on successful retrieval.</returns>
  private async Task<bool> RefreshPropertiesAsync()
  {
    var success = false;

    try
    {
      var p = await NativeDevice.GetAllAsync();
      Properties = new DeviceProperties
      {
        Address = p.Address,
        AddressType = p.AddressType,
        Alias = p.Alias,
        Appearance = p.Appearance,
        Blocked = p.Blocked,
        Class = p.Class,
        IsConnected = p.Connected,
        Icon = p.Icon,
        LegacyPairing = p.LegacyPairing,
        ManufacturerData = p.ManufacturerData,
        Modalias = p.Modalias,
        Name = p.Name,
        Paired = p.Paired,
        Rssi = p.RSSI,
        ServiceData = p.ServiceData,
        ServicesResolved = p.ServicesResolved,
        Trusted = p.Trusted,
        TxPower = p.TxPower,
        UUIDs = p.UUIDs,
      };

      success = true;

      Console.WriteLine($"BLE - Device Properties:{Environment.NewLine}{Properties}");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"BLE - Could not obtain Device Properties:{Environment.NewLine}{ex.Message}");
    }

    return success;
  }
}
