using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BleClientTester.ViewModels;
using Linux.Bluetooth;
using Linux.Bluetooth.Constants;
using Linux.Bluetooth.Extensions;

namespace BleClientTester.Services;

public class BluetoothLeService : IBluetoothLeService
{
  private readonly ILogService _log;
  private Adapter _adapter;

  public BluetoothLeService(ILogService log)
  {
    _log = log;

    IsInitialized = false;

    var isLinux = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
        System.Runtime.InteropServices.OSPlatform.Linux);

    var isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
        System.Runtime.InteropServices.OSPlatform.Windows);

    Console.WriteLine($"IsLinux: {isLinux}, IsWindows: {isWindows}");
  }

  /// <summary>Gets or sets the BluetoothLE Adapter.</summary>
  public Adapter Adapter { get => _adapter; set => _adapter = value; }

  /// <summary>
  ///     Gets or sets a value indicating whether to auto-attach to discovered BLE adapter.
  ///     Set to TRUE if you know you only have one or none, and FALSE for manual configuration.
  /// </summary>
  public bool AutoConnectAdapter { get; set; } = true;

  /// <summary>Gets or sets a value indicating whether to auto-connect to adapter when turned on (default = false).</summary>
  public bool AutoConnectDevice { get; set; } = false;

  /// <summary>Gets or sets a value indicating whether to automatically scan for new devices on init or when adapter is turned on (default = false).</summary>
  public bool AutoScanDevices { get; set; } = false;

  [Obsolete("Get devices from Scan and perform actions against `BleDevice`.")]
  public BleDevice CurrentDevice { get; private set; } = new();

  /// <summary>Gets or sets the filter for discovered devices name during scanning. Default = "" (no filter).</summary>
  public string DeviceScanFilter { get; set; } = string.Empty;

  public bool IsDeviceConnected { get; private set; }

  public bool IsInitialized { get; private set; }

  /// <summary>Gets a value indicating whether the host OS support the BluetoothLeService.</summary>
  /// <remarks>
  ///   For multi-platform support, use:
  ///   <c>OperatingSystem.IsLinux() || OperatingSystem.IsWindows();</c>.
  /// </remarks>
  public bool IsOsSupported => OperatingSystem.IsLinux();

  /// <summary>Gets a value indicating whether if we're scanning for nearby devices.</summary>
  public bool IsScanning { get; private set; }

  /// <summary>Last reported error.</summary>
  public string LastError { get; private set; }

  /// TODO: Replace with -->> public event Action<BleDevice> OnAdapterDiscoveredDevice;
  public event Action<Device, DeviceProperties> OnAdapterDiscoveredDevice;

  public event Action<Adapter> OnAdapterPoweredOff;

  public event Action<Adapter> OnAdapterPoweredOn;

  public event Action<Device, BlueZEventArgs> OnDeviceConnected;

  public event Action<Device, BlueZEventArgs> OnDeviceDisconnected;

  [Obsolete("This feature is not in use.")]
  public event Action<string> OnDeviceNotification;

  public event Action<Device, BlueZEventArgs> OnDeviceServicesResolved;

  // TODO: Create a generic IGattService
  public event Action<IGattService1> OnDeviceServiceResolved;

  /// <summary>Initialize BLE Service and return a list of available adapters.</summary>
  /// <returns>Tuple of successful initialization and list of BLE Adapters.</returns>
  public async Task<(bool Success, IReadOnlyList<Adapter> Adapters)> AdapterInitializeAsync()
  {
    var adapters = await GetAdaptersAsync();

    if (adapters.Count == 0)
    {
      _log.Debug("BLE - No Bluetooth Adapters Found.");
      return (false, adapters);
    }

    if (IsInitialized)
    {
      _log.Debug("BLE - Already initialized");
      return (true, adapters);
    }

    // Auto-select adapter
    Adapter = adapters.First();

    // if (AutoSetAdapter)
    //     SetAdapter(adapters.First);
    IsInitialized = true;
    Adapter.PoweredOn += Adapter_OnPoweredOnAsync;
    Adapter.PoweredOff += Adapter_OnPoweredOffAsync;
    Adapter.DeviceFound += Adapter_OnDeviceFoundAsync;

    return (true, adapters);
  }

  /// <summary>Turn the BLE Adapter on or off.</summary>
  /// <param name="poweredOn">Turn the adapter on/off.</param>
  /// <returns>Success when true.</returns>
  public async Task<bool> AdapterPowerAsync(bool poweredOn)
  {
    var success = false;
    try
    {
      _log.Debug($"BLE - Set Adapter Power to {poweredOn}");
      await Adapter.SetPoweredAsync(poweredOn);
      success = true;
    }
    catch (Exception ex)
    {
      var msg = $"Error setting Power to, {poweredOn}. {ex.Message}";

      LastError = msg;
      _log.Debug(msg);
    }

    return success;
  }

  /// <summary>Scan for devices.</summary>
  /// <returns>Task.</returns>
  public async Task<bool> AdapterScanForDevicesAsync()
  {
    if (_adapter == null)
    {
      _log.Debug("BLE Scan - Must initialize adapter before scanning.");
      return false;
    }

    if (IsScanning)
    {
      _log.Debug("BLE Scan - Already in progress.");
      return true;
    }

    IsScanning = true;

    try
    {
      await _adapter.StartDiscoveryAsync();
    }
    catch (Tmds.DBus.DBusException dbusEx)
    {
      // "Tmds.DBus.DBusException" - 'org.bluez.Error.InProgress: Operation already in progress'
      LastError = "Error attempting to scan for devices.";
      _log.Error($"{LastError} {dbusEx.ErrorName} - {dbusEx.ErrorMessage}");
      return false;
    }
    catch (Exception ex)
    {
      LastError = "Unknown error while attempting to scan for devices. {ex.Message}";
      _log.Error($"{LastError} {ex.Message}");
      return false;
    }

    // Report know list of devices
    IReadOnlyList<Device> knownDevices;
    try
    {
      knownDevices = await _adapter.GetDevicesAsync();
    }
    catch (Exception ex)
    {
      // just auto-succeed
      LastError = "Error getting cached devices for scanning.";
      _log.Error($"{LastError} {ex.Message}");

      return false;
    }

    // Get known devices
    // TODO: MOVE TO ITS OWN METHOD and CALL BEFORE SCAN
    foreach (var dev in knownDevices)
    {
      try
      {
        // Device Name filter
        if (!string.IsNullOrEmpty(DeviceScanFilter))
        {
          var devName = await dev.GetNameAsync();
          if (!devName.Contains(DeviceScanFilter))
            continue;
        }

        // TODO: Use the Client-Side `BleDevice` object
        var props = await DevicePropertiesAsync(dev);
        OnAdapterDiscoveredDevice?.Invoke(dev, props);

        // TODO: Cross-platform methodology
        ////BleDevice gattDevice = new BleDevice(dev, props);
        ////OnAdapterDiscoveredDevice?.Invoke(gattDevice);
      }
      catch (Exception ex)
      {
        LastError = "Error getting cached device's properties.";
        _log.Error($"{LastError} {ex.Message}");
      }
    }

    return true;
  }

  public async Task<bool> AdapterStopScanForDevicesAsync()
  {
    if (_adapter == null)
      return false;

    IsScanning = false;

    try
    {
      await _adapter.StopDiscoveryAsync();
    }
    catch (Tmds.DBus.DBusException dbusEx)
    {
      // TODO: Create custom Linux.Bluetooth.NotStartedException
      // Tmds.DBus.DBusException: 'org.bluez.Error.Failed: No discovery started'
      if (dbusEx.ErrorMessage.Contains("No discovery started"))
        return true;
      LastError = "Exception stopping device discovery";
      _log.Error($"BLE - {LastError}. {dbusEx.ErrorName} - {dbusEx.ErrorMessage}");
      return false;
    }
    catch (Exception ex)
    {
      _log.Error($"BLE - Generic exception stopping device discovery.{Environment.NewLine}{ex.Message}");
      return false;
    }

    return true;
  }

  /// <summary>Shutdown the Bluetooth Service; disconnecting from event handlers.</summary>
  public void AdapterUnitialize()
  {
    if (!IsInitialized)
      return;

    Adapter.PoweredOn -= Adapter_OnPoweredOnAsync;
    Adapter.PoweredOff -= Adapter_OnPoweredOffAsync;
    Adapter.DeviceFound -= Adapter_OnDeviceFoundAsync;

    IsInitialized = false;
  }

  /// <summary>
  /// COMING SOON!! - Use this for Cross-platform Q3
  /// </summary>
  /// <param name="device"></param>
  /// <returns></returns>
  public async Task<bool> DeviceConnect2Async(BleDevice device)
  {
    device.OnConnected += (device, bzArgs) =>
    {
    };

    device.OnDisconnected += async (device, bzArgs) =>
    {
      // Remove from adapter.
      await _adapter.RemoveDeviceAsync(device.ObjectPath);
    };

    device.OnServicesResolved += (device, bzArgs) =>
    {
    };

    return await device.ConnectAsync();
  }

  /// <summary>Connect to specified device.</summary>
  /// <param name="device">Device to connect to.</param>
  /// <returns>True if connection is successful.</returns>
  [Obsolete("Use cross-platform, `BleDevice` instead.", error: false)]
  public async Task<bool> DeviceConnectAsync(Device device)
  {
    if (device is null)
      return false;

    try
    {
      // IsConnected
      if (await device.GetConnectedAsync())
      {
        // TODO: Ensure CurrentDevice is this device
        return true;
      }
    }
    catch (Exception ex)
    {
      _log.Error($"BluetoothLeService.Linux - Error validating active connectivity to device.\n{ex.Message}");
    }

    // TODO: (DS: 2022-05-25) - Set it here, not the OnConnect
    //// CurrentDevice = new BleDevice(device);

    // TODO: detach from events
    device.Connected -= Device_OnConnectAsync;
    device.Disconnected -= Device_OnDisconnectAsync;
    device.ServicesResolved -= Device_OnServicesResolvedAsync;

    device.Connected += Device_OnConnectAsync;
    device.Disconnected += Device_OnDisconnectAsync;
    device.ServicesResolved += Device_OnServicesResolvedAsync;

    // TODO: Add try/catch for when device is NULL
    string addr = string.Empty;

    try
    {
      addr = await device.GetAddressAsync();
      _log.Debug($"BLE - Connected to '{addr}'");
    }
    catch (Exception ex)
    {
      LastError = "Error getting device address for connecting.";
      _log.Error(ex.ToString());
      return false;
    }

    try
    {
      await device.ConnectAsync();
      return true;
    }
    catch (Exception ex)
    {
      LastError = $"Error connecting to the device ({addr})";
      _log.Error($"BLE - {LastError}. {ex.Message}");
      return false;
    }
  }

  /// <summary>Disconnect from BLE Device.</summary>
  /// <returns>True on success.</returns>
  [Obsolete("Use cross-platform, `BleDevice` instead.")]
  public async Task<bool> DeviceDisconnectAsync()
  {
    try
    {
      if (CurrentDevice is null)
        return false;

      var result = await CurrentDevice.DisconnectAsync();
      return result;
    }
    catch (Exception ex)
    {
      LastError = $"Failed to disconnect. {ex.Message}";
      _log.Error(LastError);
      return false;
    }
  }

  public async Task<IReadOnlyList<Adapter>> GetAdaptersAsync()
  {
    IReadOnlyList<Adapter> adapters = new List<Adapter>();

    try
    {
      adapters = await BlueZManager.GetAdaptersAsync();

      // TODO: Which is better? Event or Action?
      //// await OnAdaptersFound?.Invoke(adapters);
      //// OnAdapterFound?.Invoke(adapters);
    }
    catch (Tmds.DBus.ConnectException conEx)
    {
      LastError = $"Failed to get adapters; DBus Connection unavailable.{Environment.NewLine}{conEx}";
      _log.Error(LastError);
      //// throw;
    }
    catch (Tmds.DBus.DBusException dbusEx)
    {
      LastError = $"Failed to get adapters. Potential missing or unavailable BLE Adapter .{Environment.NewLine}{dbusEx}";
      _log.Error(LastError);
      //// throw;
    }
    catch (Exception ex)
    {
      LastError = $"Failed to get adapters.{Environment.NewLine}{ex}";
      _log.Error(LastError);
      //// throw;
    }

    return adapters;
  }

  private async Task Adapter_OnDeviceFoundAsync(Adapter sender, DeviceFoundEventArgs deviceArgs)
  {
    var device = deviceArgs.Device;

    try
    {
      // Filter Scan results
      if (!string.IsNullOrEmpty(DeviceScanFilter))
      {
        var devName = await device.GetNameAsync();
        if (!devName.Contains(DeviceScanFilter))
          return;
      }
    }
    catch (Tmds.DBus.DBusException dbusEx)
    {
      // org.freedesktop.DBus.Error.InvalidArgs: No such property 'Name'
      _log.Error($"BLE - Adapter_OnDeviceFoundAsync - Exception reading device name.{Environment.NewLine}{dbusEx}");
      return;
    }
    catch (Exception ex)
    {
      // Could not validate device name, leave.
      _log.Error($"BLE - Adapter_OnDeviceFoundAsync - Unknown Exception reading device name.{Environment.NewLine}{ex}");
      return;
    }

    // OLD:
    var properties = await DevicePropertiesAsync(device);
    var isNew = deviceArgs.IsStateChange;
    OnAdapterDiscoveredDevice?.Invoke(device, properties);

    // TODO: Use Cross-Platform Classes
    ////var bleDevice = new BleDevice(device, properties);
    ////OnAdapterDiscoveredDevice?.Invoke(device);

    _log.Debug($"BLE - Adapter Found Device. IsNew: {isNew}; Details: '{properties}'");

    var isOurDevice = false;

    // Connect to device
    if (AutoConnectDevice && isOurDevice && device is not null)
      await DeviceConnectAsync(device);
  }

  private async Task Adapter_OnPoweredOffAsync(Adapter adapter, BlueZEventArgs e)
  {
    try
    {
      if (e.IsStateChange)
        _log.Status("BLE - Adapter turned OFF");
      else
        _log.Status("BLE - Adapter is (already) off.");

      _adapter = adapter;

      OnAdapterPoweredOff?.Invoke(adapter);
      await Task.Yield();
    }
    catch (Exception ex)
    {
      _log.Error($"BLE - Adapter PoweredOff Exception:{Environment.NewLine}{ex}");
    }
  }

  private async Task Adapter_OnPoweredOnAsync(Adapter adapter, BlueZEventArgs e)
  {
    try
    {
      if (e.IsStateChange)
        _log.Status("BLE - Adapter turned ON");
      else
        _log.Status("BLE - Adapter is (already) on.");

      OnAdapterPoweredOn?.Invoke(adapter);

      if (AutoConnectAdapter)
        _adapter = adapter;

      if (AutoScanDevices)
        await AdapterScanForDevicesAsync();
    }
    catch (Exception ex)
    {
      _log.Error($"BLE - Adapter PoweredOn Exception:{Environment.NewLine}{ex}");
    }
  }

  [Obsolete("Use cross-platform, `BleDevice` instead.")]
  private async Task Device_OnConnectAsync(Device device, BlueZEventArgs eventArgs)
  {
    try
    {
      // TEMP DISABLED
      ////// Check if we're already connected
      ////var props = await DevicePropertiesAsync(device);
      ////if (props.Address == CurrentDevice.Properties.Address)
      ////{
      ////    _log.Debug($"Already connected to BLE Device {props.Address}");
      ////    return;
      ////}
      ////
      ////CurrentDevice = new(device, props);

      _log.Debug($"BLE - Device {(eventArgs.IsStateChange ? "Connected" : "Updated")}");
      OnDeviceConnected?.Invoke(device, eventArgs);

      // Don't update current device. TRUE means new connection, FALSE is RSSI update
      if (!eventArgs.IsStateChange)
        return;

      IsDeviceConnected = true;
      CurrentDevice = new(device, true);
    }
    catch (Exception ex)
    {
      _log.Error(ex.ToString());
    }
  }

  /// <summary>Device disconnected event.</summary>
  /// <param name="device">Device.</param>
  /// <param name="eventArgs">BlueZ Event Args.</param>
  /// <returns>Task.</returns>
  [Obsolete("Use cross-platform, `BleDevice` instead.")]
  private async Task Device_OnDisconnectAsync(Device device, BlueZEventArgs eventArgs)
  {
    OnDeviceDisconnected?.Invoke(device, eventArgs);

    // Remove `CurrentDevice` from cache
    IsDeviceConnected = false;
    CurrentDevice.IsConnected = false;
    CurrentDevice.Dispose();
    CurrentDevice = new();

    var addr = await device.GetAddressAsync();
    _log.Debug($"BLE - Disconnected from device address, {addr}");

    ////// Remove `CurrentDevice` from cache
    ////var cached = CachedDevices.FirstOrDefault(cache => cache.NativeDevice == device);
    ////CachedDevices.Remove(cached);

    _log.Debug($"BLE - Removing device from adapter");
    await _adapter.RemoveDeviceAsync(device.ObjectPath);
  }

  [Obsolete("Use cross-platform, `BleDevice` instead.")]
  private async Task Device_OnNotificationAsync(GattCharacteristic characteristic, GattCharacteristicValueEventArgs eventArgs)
  {
    try
    {
      var uuid = await characteristic.GetUUIDAsync();
      var bytes = eventArgs.Value;

      if (uuid.Equals(GattConstants.ANCSNotificationSourceUUID))
      {
        // Convert byte to string
        var msg = PrintAncsDescription(bytes);
        OnDeviceNotification?.Invoke(msg);
      }
      else if (uuid.Equals(GattConstants.CurrentTimeCharacteristicUUID))
      {
        // Current time is...
        var dttm = ReadCurrentTime(bytes);
        OnDeviceNotification?.Invoke(dttm.ToString());
      }
      else
      {
        var strValue = Helper.StringFromBytes(bytes);

        OnDeviceNotification?.Invoke(strValue);
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Device Notification error: {ex.Message}");
    }
  }

  /// <summary>Device service found event.</summary>
  /// <param name="device">Device.</param>
  /// <param name="eventArgs">BlueZ Event Args.</param>
  /// <returns>Task.</returns>
  [Obsolete("Use cross-platform, `BleDevice` instead.")]
  private async Task Device_OnServicesResolvedAsync(Device device, BlueZEventArgs eventArgs)
  {
    try
    {
      OnDeviceServicesResolved?.Invoke(device, eventArgs);

      _log.Debug($"BLE - Device Services {(eventArgs.IsStateChange ? "Found" : "Updated")}");

      var serviceUuids = await device.GetUUIDsAsync();
      _log.Debug($"BLE - Found '{serviceUuids.Length}' service(s).");

      // Notify of all services
      var services = await device.GetServicesAsync();

      if (services is not null && services.Count > 0)
      {
        foreach (var s in services)
          OnDeviceServiceResolved?.Invoke(s);
      }
    }
    catch (Exception ex)
    {
      _log.Error($"BLE - Error{Environment.NewLine}{ex}");
    }
  }

  /// <summary>Gets the device's properties and description.</summary>
  /// <param name="device">Device.</param>
  /// <returns>Device properties.</returns>
  private async Task<DeviceProperties> DevicePropertiesAsync(IDevice1 device)
  {
    var props = new DeviceProperties();

    try
    {
      var p = await device.GetAllAsync();
      props = new DeviceProperties
      {
        Address = p.Address,
        AddressType = p.AddressType,
        Alias = p.Alias,
        Appearance = p.Appearance,
        Blocked = p.Blocked,
        Class = p.Class,
        Icon = p.Icon,
        IsConnected = p.Connected,
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
    }
    catch (Exception ex)
    {
      _log.Debug($"BLE - Exception getting device properties.{Environment.NewLine}{ex.Message}");
    }

    _log.Debug($"BLE - Found Device: {props}");
    return props;
  }

  private string PrintAncsDescription(byte[] value)
  {
    if (value.Length < 8)
      return "Invalid ANCS: 8 bytes are required for ANCS notifications.";

    var eventIds = new string[] { "added", "modified", "removed" };
    var categoryIds = new string[] { "Other", "IncomingCall", "MissedCall", "Voicemail", "Social", "Schedule", "Email", "News", "Health & Fitness", "Business & Finance", "Location", "Entertainment" };

    byte[] notificationUid = new byte[4];
    Array.Copy(value, 4, notificationUid, 0, 4);

    return $"{categoryIds[value[2]]} notification {eventIds[value[0]]} (Count: {value[3]}) (UID: {BitConverter.ToString(notificationUid)})";
  }

  private DateTime ReadCurrentTime(byte[] value)
  {
    if (value.Length < 7)
      throw new ArgumentException("7+ bytes are required for the current date time.");

    // https://github.com/sputnikdev/bluetooth-gatt-parser/blob/master/src/main/resources/gatt/characteristic/org.bluetooth.characteristic.date_time.xml
    var year = value[0] + 256 * value[1];
    var month = value[2];
    var day = value[3];
    var hour = value[4];
    var minute = value[5];
    var second = value[6];

    return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
  }

  private void SetAdapter(Adapter adapter)
  {
    if (Adapter is not null)
    {
      // Consider: Disconnecting from any active connections.
      Adapter.PoweredOn -= Adapter_OnPoweredOnAsync;
      Adapter.PoweredOff -= Adapter_OnPoweredOffAsync;
      Adapter.DeviceFound -= Adapter_OnDeviceFoundAsync;
    }

    Adapter = adapter;

    Adapter.PoweredOn += Adapter_OnPoweredOnAsync;
    Adapter.PoweredOff += Adapter_OnPoweredOffAsync;
    Adapter.DeviceFound += Adapter_OnDeviceFoundAsync;
  }
}
