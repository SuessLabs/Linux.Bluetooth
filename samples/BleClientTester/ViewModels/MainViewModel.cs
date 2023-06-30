using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BleClientTester.Services;
using Linux.Bluetooth;
using Linux.Bluetooth.Extensions;
using Prism.Commands;
using Prism.Regions;

namespace BleClientTester.ViewModels;

public class MainViewModel : ViewModelBase
{
  private readonly IBluetoothLeService _ble;
  private readonly INotificationService _notify;
  private readonly ILogService _log;

  private ObservableCollection<string> _bleAdapters = new();
  private string _bleAdapterSelected = string.Empty;
  private Dictionary<Device, DeviceProperties> _bleCachedDevices = new();
  private bool _bleDeviceIsConnected = false;
  private Linux.Bluetooth.Device? _bleDeviceNative = null;
  private string _bleDeviceScanFilter = string.Empty;
  private int _bleDeviceSelectedIndex;
  private int _bleDeviceServiceMessageIndex;
  private ObservableCollection<string> _bleDeviceServiceMessages = new();
  private string _bleDeviceServiceMessageText = string.Empty;
  private ObservableCollection<string> _bleDevicesFound = new();
  private byte[]? _bleLastNotification = null;
  private IGattService1? _bleService = null;
  private string _bleTextMessage = string.Empty;
  private bool _isHost = false;
  private IRegionNavigationJournal? _journal;
  private bool _bleReadCharacteristicTx = false;
  private bool _bleAdapterIsScanning;

  public MainViewModel(IRegionManager region, ILogService log, IBluetoothLeService ble, INotificationService notify)
  {
    _log = log;
    _ble = ble;
    _notify = notify;

    Title = "BLE Connection Tester";

    _isHost = false;

    _ble.AutoScanDevices = false;
    _ble.AutoConnectDevice = false;

    // SET Device Scan Filter
    BleDeviceScanFilter = "";
    BleDeviceIsConnected = false;
  }

  public ObservableCollection<string> BleAdapters { get => _bleAdapters; set => SetProperty(ref _bleAdapters, value); }

  public string BleAdapterSelected { get => _bleAdapterSelected; set => SetProperty(ref _bleAdapterSelected, value); }

  public bool BleDeviceIsConnected { get => _bleDeviceIsConnected; set => SetProperty(ref _bleDeviceIsConnected, value); }

  /// <summary>Is scanning for devices.</summary>
  public bool BleAdapterIsScanning { get => _bleAdapterIsScanning; set => SetProperty(ref _bleAdapterIsScanning, value); }

  /// <summary>BLE Scan device name filter.</summary>
  public string BleDeviceScanFilter
  {
    get => _bleDeviceScanFilter;
    set
    {
      SetProperty(ref _bleDeviceScanFilter, value);
      _ble.DeviceScanFilter = value;
    }
  }

  public string BleDeviceSelectedAddress
  {
    get
    {
      if (BleDeviceSelectedIndex == -1)
        return string.Empty;

      try
      {
        var selectedDevice = BleDevicesFound[BleDeviceSelectedIndex];

        var props = selectedDevice.Split(';');
        if (props.Length == 0)
          return string.Empty;

        // 2nd segment is "Addr=XXXXX;"
        var fullAddr = props[1];
        return fullAddr.Split('=')[1];
      }
      catch (Exception ex)
      {
        _log.Error($"Issue parsing selected Ble Device Addess. {ex.Message}");
        return string.Empty;
      }
    }
  }

  public int BleDeviceSelectedIndex { get => _bleDeviceSelectedIndex; set => SetProperty(ref _bleDeviceSelectedIndex, value); }

  /// <summary>Device Service Characteristic Messages.</summary>
  public ObservableCollection<string> BleDeviceServiceMessages { get => _bleDeviceServiceMessages; set => SetProperty(ref _bleDeviceServiceMessages, value); }

  /// <summary>Selected Service Characteristic Message Index.</summary>
  public int BleDeviceServiceMessageSelected
  {
    get => _bleDeviceServiceMessageIndex;
    set
    {
      SetProperty(ref _bleDeviceServiceMessageIndex, value);

      if (_bleDeviceServiceMessageIndex == -1)
        return;

      // TODO: Add fixtures to display selected data from service
      // TODO: Create a structure for messages (deviceUuid;serviceUuid;message)
      var eventItem = BleDeviceServiceMessages[value];
      if (string.IsNullOrEmpty(eventItem))
        return;

      BleDeviceServiceMessageText = eventItem;
    }
  }

  /// <summary>Selected Service Characteristic Message Text.</summary>
  public string BleDeviceServiceMessageText { get => _bleDeviceServiceMessageText; set => SetProperty(ref _bleDeviceServiceMessageText, value); }

  public ObservableCollection<string> BleDevicesFound { get => _bleDevicesFound; set => SetProperty(ref _bleDevicesFound, value); }

  ////public string BleTextBrightness { get => _bleTextBrightness; set => SetProperty(ref _bleTextBrightness, value); }

  public string BleTextMessage { get => _bleTextMessage; set => SetProperty(ref _bleTextMessage, value); }

  public bool BleReadCharacteristicTx { get => _bleReadCharacteristicTx; set => SetProperty(ref _bleReadCharacteristicTx, value); }

  public DelegateCommand CmdAdapterInit => new(async () =>
  {
    // Find BLE Adapters
    _log.Status("Initializing BLE Adapter(s)");

    // TODO: Allow usage of selected adapter.
    // Wire-up event handlers and get list of available adapters.
    var ret = await _ble.AdapterInitializeAsync();

    if (ret.Success)
    {
      BleAdapters.Clear();

      // Sample: "/org/bluez/hci0"
      foreach (var adapter in ret.Adapters)
      {
        var name = adapter.ObjectPath.ToString();
        AddAdapterItem(name);
      }

      _notify.Show("BLE Adapter", "Refreshed");
    }
    else
    {
      _notify.Show("BLE Adapter", "Refresh failed");
    }
  });

  public DelegateCommand CmdAdapterRefresh => new(RefreshAdaptersAsync);

  public DelegateCommand CmdAdapterOn => new(async () =>
  {
    await _ble.AdapterPowerAsync(true);
  });

  public DelegateCommand CmdAdapterOff => new(async () =>
  {
    await _ble.AdapterPowerAsync(false);
  });

  public DelegateCommand CmdAdapterDeviceScanOn => new(async () =>
  {
    if (!_ble.IsInitialized)
    {
      _log.Status("Cannot scan; initialize BLE Adapter first");
      return;
    }

    BleDevicesFound.Clear();
    _bleCachedDevices.Clear();

    // Scan for devices
    BleAdapterIsScanning = await _ble.AdapterScanForDevicesAsync();
  });

  public DelegateCommand CmdAdapterDeviceScanOff => new(async () =>
  {
    if (!_ble.IsInitialized)
    {
      _log.Status("Cannot scan; initialize BLE Adapter first");
      return;
    }

    // Cancel Scan for devices
    await _ble.AdapterStopScanForDevicesAsync();
    BleAdapterIsScanning = false;
  });

  public DelegateCommand CmdClearCache => new(() =>
  {
    BleDeviceServiceMessages.Clear();
    BleDeviceServiceMessageSelected = -1;
    BleDeviceServiceMessageText = string.Empty;
  });

  public DelegateCommand CmdDeviceConnect => new(async () =>
  {
    // Scan for devices
    if (!_ble.IsInitialized)
    {
      _log.Status("Cannot connect. Need to initialize BLE Adapter first");
      return;
    }

    var deviceUuid = string.Empty;

    if (BleDeviceSelectedIndex == -1)
    {
      _log.Status("Please select a device first.");
    }
    else if (string.IsNullOrEmpty(BleDeviceSelectedAddress))
    {
      _log.Status("Invalid device selected");
    }
    else
    {
      _log.Status($"Connecting to '{BleDeviceSelectedAddress}'...");
      await DeviceConnectAsync(BleDeviceSelectedAddress);
    }
  });

  public DelegateCommand CmdDeviceDisconnect => new(async () => await DeviceDisconnectAsync());

  public DelegateCommand CmdDeviceReadRx => new(async () => await GattReadAsync(Constants.BasicCharacteristicRxUuid));

  public DelegateCommand CmdDeviceReadTx => new(async () => await GattReadAsync(Constants.BasicCharacteristicTxUuid));

  public DelegateCommand CmdDeviceWriteJunk => new(async () =>
  {
    var junk = Helper.StringToBytes("junk");
    await GattWriteAsync(junk);
  });

  /// <summary>Write to specified Device Characteristic.</summary>
  public DelegateCommand CmdDeviceWriteInfo => new(async () => await GattWriteAsync("Some info"));

  public DelegateCommand CmdDeviceGetInfo => new(async () => await GattWriteAsync("GetInfo"));

  public DelegateCommand CmdDeviceUnlock => new(async () => await GattWriteAsync("Unlock"));

  public DelegateCommand CmdForceNav => new(() =>
  {
    ////var fakeDevice = _bleDeviceNative;
    ////
    ////var navParams = new NavigationParameters();
    ////navParams.Add(NavigationKey.BleDevice, fakeDevice);
    ////
    ////_regionManager.RequestNavigate(
    ////    RegionNames.ContentRegion,
    ////    nameof(BleServicesView),
    ////    r =>
    ////    {
    ////      _log.Status($"Navigation success: {r.Result}");
    ////    },
    ////    navParams);
  });

  private string FormattedTime => $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}.{DateTime.Now.Millisecond:00}";

  ////public void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
  ////{
  ////    bool result = true;
  ////
  ////    ////if (MessageBox.Show("Do you to navigate?", "Navigate?", MessageBoxButton.YesNo) == MessageBoxResult.No)
  ////    ////    result = false;
  ////
  ////    continuationCallback(result);
  ////}
  ////
  /////// <summary>Called when the implementer is being navigated away from.</summary>
  /////// <param name="navigationContext">The navigation context.</param>
  ////public override void OnNavigatedFrom(NavigationContext navigationContext)
  ////{
  ////  // Turn off event handlers
  ////  BleRegisterEventHandlers(false);
  ////}
  ////
  ////public override void OnNavigatedTo(NavigationContext navigationContext)
  ////{
  ////  _journal = navigationContext.NavigationService.Journal;
  ////  BleRegisterEventHandlers(true);
  ////}

  /// <summary>Add adapter name to list.</summary>
  /// <param name="adapterName">Adapter Name (i.e. `/org/bluez/hci0`).</param>
  private void AddAdapterItem(string adapterName)
  {
    var found = false;
    foreach (var item in BleAdapters)
    {
      if (item == adapterName)
      {
        found = true;
        break;
      }
    }

    if (!found)
      BleAdapters.Add(adapterName);
  }

  private void Ble_OnAdapterPoweredOff(Linux.Bluetooth.Adapter adapter)
  {
    var name = adapter.ObjectPath.ToString();
    _log.Status($"Adapter powered off ('{name}'");
  }

  private void Ble_OnAdapterPoweredOn(Linux.Bluetooth.Adapter adapter)
  {
    var name = adapter.ObjectPath.ToString();
    _log.Status($"Adapter powered on ('{name}')");
  }

  private void Ble_OnDeviceConnected(Device bleDevice, BlueZEventArgs args)
  {
    BleDeviceIsConnected = true;
  }

  private void Ble_OnDeviceDisconnected(Device device, BlueZEventArgs eventArgs)
  {
    BleDeviceIsConnected = false;

    _log.Status($"Disconnected from device!");
    _bleService = null;
    _bleDeviceNative = null;
  }

  private void Ble_OnDeviceFound(Linux.Bluetooth.Device device, DeviceProperties deviceProperties)
  {
    _bleCachedDevices.SafeAdd(device, deviceProperties);

    // Add human-friendly debug name to ListView (w/o duplicates)
    bool found = false;
    var deviceDisplayName = deviceProperties.ToString();

    for (int ndx = 0; ndx < BleDevicesFound.Count; ndx++)
    {
      if (BleDevicesFound[ndx].Contains($";Addr={deviceProperties.Address};"))
      {
        // Update the RSSI
        found = true;
        BleDevicesFound[ndx] = deviceDisplayName;
        break;
      }
    }

    ////foreach (var dev in BleDevicesFound)
    ////{
    ////    if (dev.Contains(deviceProperties.Address))
    ////    {
    ////        // TODO: Update RSSI
    ////        found = true;
    ////        break;
    ////    }
    ////}

    if (!found)
      BleDevicesFound.Add(deviceDisplayName);
  }

  private async Task Ble_OnDeviceNotificationAsync(GattCharacteristic gattChar, GattCharacteristicValueEventArgs eventArgs)
  {
    await Task.Yield();

    try
    {
      var bytes = eventArgs.Value;
      _bleLastNotification = bytes;
      CacheLogMessage("Nx", bytes);
    }
    catch (Exception ex)
    {
      _log.Status($"NOTIFY Exception: {ex.Message}");
    }
  }

  private async void Ble_OnDeviceServiceResolvedAsync(IGattService1 gattService)
  {
    // Check if is connected
    if (!_bleDeviceIsConnected)
      return;

    var uuid = await gattService.GetUUIDAsync();

    _log.Status($"Service Found: {uuid}");
    var props = await gattService.GetAllAsync();
    var propIncludes = props.Includes;
    var propIsPrimary = props.Primary;

    if (uuid == Constants.BasicServiceUuid)
    {
      _bleService = gattService;

      var charNotify = await _bleService.GetCharacteristicAsync(Constants.BasicCharacteristicNotifyUuid);
      if (charNotify is null)
      {
        _log.Status("Could not resolve Notify Characteristic.");
      }
      else
      {
        _log.Status("Notifications Registered for Char!");
        charNotify.Value -= Ble_OnDeviceNotificationAsync;
        charNotify.Value += Ble_OnDeviceNotificationAsync;
      }
    }
  }

  private void Ble_OnDeviceServicesResolved(Device arg1, BlueZEventArgs arg2)
  {
    _log.Status("Device Services Resolved");
  }

  private void BleRegisterEventHandlers(bool registerForEvents)
  {
    // Turn off event handlers
    _ble.OnAdapterPoweredOn -= Ble_OnAdapterPoweredOn;
    _ble.OnAdapterPoweredOff -= Ble_OnAdapterPoweredOff;
    _ble.OnAdapterDiscoveredDevice -= Ble_OnDeviceFound;
    _ble.OnDeviceConnected -= Ble_OnDeviceConnected;
    _ble.OnDeviceDisconnected -= Ble_OnDeviceDisconnected;
    _ble.OnDeviceServicesResolved -= Ble_OnDeviceServicesResolved;
    _ble.OnDeviceServiceResolved -= Ble_OnDeviceServiceResolvedAsync;
    //// _ble.OnDeviceNotification -= Ble_OnDeviceNotification;

    if (registerForEvents)
    {
      // BleAdapters.Clear();
      BleDevicesFound.Clear();
      _bleCachedDevices.Clear();

      // Turn on event handlers
      _ble.OnAdapterPoweredOn += Ble_OnAdapterPoweredOn;
      _ble.OnAdapterPoweredOff += Ble_OnAdapterPoweredOff;
      _ble.OnAdapterDiscoveredDevice += Ble_OnDeviceFound;
      _ble.OnDeviceConnected += Ble_OnDeviceConnected;
      _ble.OnDeviceDisconnected += Ble_OnDeviceDisconnected;
      _ble.OnDeviceServicesResolved += Ble_OnDeviceServicesResolved;
      _ble.OnDeviceServiceResolved += Ble_OnDeviceServiceResolvedAsync;
      //// _ble.OnDeviceNotification += Ble_OnDeviceNotification;
    }
  }

  /// <summary>Connect to selected Device UUID.</summary>
  /// <param name="deviceUuid">Device UUID.</param>
  /// <returns>Task.</returns>
  private async Task<bool> DeviceConnectAsync(string deviceUuid)
  {
    Device? device = null;

    foreach (var cache in _bleCachedDevices)
    {
      if (deviceUuid == cache.Value.Address)
      {
        device = cache.Key as Device;
        break;
      }
    }

    if (device is null)
    {
      _log.Status("Device UUID not found in cache.");
      return false;
    }

    _bleDeviceNative = device;

    var result = await _ble.DeviceConnectAsync(device);
    if (result)
    {
      _log.Status($"Connected to device!");
    }
    else
    {
      _log.Status($"Failed to connect to the device.");
      _log.Status($"Last Error: {_ble.LastError}");
    }

    var navToServices = false;

    if (result && !navToServices)
    {
      _log.Status($"Fetching services");

      // This breaks shit!
      ////var services = await _bleDeviceNative.GetServiceDataAsync();
    }
    else if (result && navToServices)
    {
      ////var navParams = new NavigationParameters();
      ////navParams.Add(NavigationKey.BleDevice, _bleDeviceNative);
      ////
      ////_regionManager.RequestNavigate(
      ////    RegionNames.ContentRegion,
      ////    nameof(BleServicesView),
      ////    r =>
      ////    {
      ////      _log.Status($"Navigation success: {r.Result}");
      ////    },
      ////    navParams);
    }

    return result;
  }

  private async Task DeviceDisconnectAsync()
  {
    if (!_ble.IsInitialized)
    {
      _log.Status("Cannot disconnect. Need to initialize BLE Adapter first");
      return;
    }
    else if (_ble.CurrentDevice is not null && _ble.CurrentDevice.IsConnected)
    {
      await _ble.DeviceDisconnectAsync();
    }
    else
    {
      // Disconnect via address
      var addr = BleDeviceSelectedAddress;

      try
      {
        foreach (var cachedDevice in _bleCachedDevices)
        {
          if (addr == cachedDevice.Value.Address)
          {
            var device = cachedDevice.Key as Device;
            await device.DisconnectAsync();
            break;
          }
        }
      }
      catch (Exception ex)
      {
        _log.Error($"Error disconnecting cached device via selected address. {ex.Message}");
      }
    }
  }

  private async void RefreshAdaptersAsync()
  {
    // Find BLE Adapters
    _log.Status("Refreshing BLE Adapter List");

    BleAdapters.Clear();
    var adapters = await _ble.GetAdaptersAsync();

    if (adapters.Count == 0)
    {
      _log.Status($"No adapters found.{Environment.NewLine}{_ble.LastError}");
      return;
    }

    // Sample: "/org/bluez/hci0"
    foreach (var a in adapters)
    {
      var name = a.ObjectPath.ToString();
      AddAdapterItem(name);
    }
  }

  private bool ValidateConnection()
  {
    if (!BleDeviceIsConnected)
    {
      _log.Status("Not connected to Ble");
      return false;
    }
    else
    {
      return true;
    }
  }

  private async Task<bool> SendCommandAsync(string commandText)
  {
    var cmdBytes = Helper.StringToBytes(commandText);
    var success = await GattWriteAsync(cmdBytes);

    return success;
  }

  /// <summary>Read response from .</summary>
  /// <param name="forcedCharacteristicUuid">Optional GATT Characteristic.</param>
  /// <returns>Task.</returns>
  private async Task<string?> GattReadAsync(string forcedCharacteristicUuid = "")
  {
    if (_bleService is null)
    {
      _log.Status("BLE GATT Service Unavailable for Reading.");
      return null;
    }

    string? rx = null;
    string characteristicUuid;

    if (string.IsNullOrEmpty(forcedCharacteristicUuid))
    {
      characteristicUuid = BleReadCharacteristicTx
          ? Constants.BasicCharacteristicTxUuid
          : Constants.BasicCharacteristicRxUuid;
    }
    else
    {
      characteristicUuid = forcedCharacteristicUuid;
    }

    ////// PERFERRED METHODOLOGY:
    ////var rx = await _ble.CurrentDevice.ReadAsync(
    ////    Constants.BasicServiceUuid,
    ////    characteristicUuid);

    // ------ ALT METHOD (2022-06-21) ------ \\
    var gattChar = await _bleService.GetCharacteristicAsync(characteristicUuid);

    if (gattChar is null)
    {
      _log.Status("Error: Could not get GATT Characteristic for Reading");
      return null;
    }

    try
    {
      // TEST TOMORROW: 2022-06-21
      ////var tryThis = await gattChar.GetValueAsync();
      ////rx = Encoding.UTF8.GetString(tryThis);
      ////CacheLogMessage("TEST", rx);

      var bytes = await gattChar.ReadValueAsync(new TimeSpan(0, 0, 30));
      rx = Helper.StringFromBytes(bytes);
    }
    catch (Exception ex)
    {
      _log.Status($"Error reading characteristic. {ex.Message}");
    }

    // ------ END ALT METHOD ------ \\
    CacheLogMessage("Rx", rx);

    return rx;
  }

  private async Task<bool> GattWriteAsync(string data)
  {
    return await GattWriteAsync(Helper.StringToBytes(data));
  }

  private async Task<bool> GattWriteAsync(byte[] data)
  {
    if (_bleService is null)
    {
      _log.Status("BLE GATT Service Unavailable for Writing.");
      return false;
    }

    // TODO: Check if we're connected
    ////var svcChar = await GetCharacteristicAsync(serviceUuid, characteristicUuid);
    ////if (svcChar == null)
    ////    return false;

    var gattChar = await _bleService.GetCharacteristicAsync(Constants.BasicCharacteristicTxUuid);
    if (gattChar is null)
    {
      _log.Status("Error: Could not get GATT Characteristic for Writing");
      return false;
    }

    try
    {
      CacheLogMessage("Tx", data);
      await gattChar.WriteValueAsync(data, new Dictionary<string, object>());
      Console.WriteLine($"Sent: '{data}'");
      //// _log.Status($"Tx", $"'{data}'");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error sending command:{Environment.NewLine}{ex}");
      _log.Status($"Error sending command:{Environment.NewLine}{ex.Message}");
      _log.Error($"Error sending command:{Environment.NewLine}{ex}");

      return false;
    }

    return true;
  }

  private async Task<bool> GattWriteAndWaitAsync(byte[] data, ushort waitForDeviceAction, int msTimeout = 500)
  {
    _bleLastNotification = null;
    if (await GattWriteAsync(data))
    {
      var success = await WaitForNotificationAsync(waitForDeviceAction, msTimeout);

      if (!success)
        _log.Status($"Tx: Timeout waiting for DeviceAction {waitForDeviceAction}");

      return success;
    }
    else
    {
      return false;
    }
  }

  private async Task<bool> WaitForNotificationAsync(ushort deviceAction, int msTimeout = 500)
  {
    var success = await Helper.WaitUntilAsync(
        condition: () => (_bleLastNotification is not null),
        timeout: msTimeout);

    return success;
  }

  /// <summary>Store BLE transmission in cache.</summary>
  /// <param name="typeRxTx">Source from 'Rx' or 'Tx'.</param>
  /// <param name="message">Message to send.</param>
  private void CacheLogMessage(string typeRxTx, byte[]? message)
  {
    if (message is null)
    {
      _log.Status($"{typeRxTx}: NULL");
    }
    else
    {
      // Convert to HEX for quick debugging
      var hex = BitConverter.ToString(message);
      _log.Status($"{typeRxTx}: '{hex}'");

      var cache = $"[{typeRxTx}] {FormattedTime}: '{hex}'";
      BleDeviceServiceMessages.Add(cache);
    }
  }

  /// <summary>Cache message transmission.</summary>
  /// <param name="typeRxTx">Specify, 'Rx' or 'Tx'.</param>
  /// <param name="message">Message transaction.</param>
  private void CacheLogMessage(string typeRxTx, string? message)
  {
    if (message is null)
      _log.Status($"{typeRxTx}: NULL");
    else
      CacheLogMessage(typeRxTx, Helper.StringToBytes(message));
  }
}
