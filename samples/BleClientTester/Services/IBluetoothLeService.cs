using System;
using System.Threading.Tasks;
using Avalonia.Input;
using Linux.Bluetooth;

namespace BleClientTester.Services;

public interface IBluetoothLeService
{
  event Action<Device, DeviceProperties> OnAdapterDiscoveredDevice;

  event Action<Adapter> OnAdapterPoweredOff;

  event Action<Adapter> OnAdapterPoweredOn;

  event Action<Device, BlueZEventArgs> OnDeviceConnected;

  event Action<Device, BlueZEventArgs> OnDeviceDisconnected;

  event Action<string> OnDeviceNotification;

  event Action<Device, BlueZEventArgs> OnDeviceServicesResolved;

  event Action<IGattService1> OnDeviceServiceResolved;

  Adapter Adapter { get; set; }

  /// <summary>Sets the BLE Adapter power to on or off.</summary>
  /// <param name="poweredOn">True to turn on, false for off.</param>
  /// <returns>True on success.</returns>
  Task<bool> AdapterPowerAsync(bool poweredOn);

  /// <summary>Gets or sets a value indicating whether to auto-connect to device when adapter is turned on.</summary>
  bool AutoConnectDevice { get; set; }

  /// <summary>Gets or sets a value indicating whether to automatically scan for new devices on init or when adapter is turned on.</summary>
  public bool AutoScanDevices { get; set; }

  BleDevice CurrentDevice { get; }

  /// <summary>Gets or sets the filter for discovered devices name during scanning. Default = "" (no filter).</summary>
  string DeviceScanFilter { get; set; }

  /// <summary>Gets a value indicating whether the adapter is initialized.</summary>
  bool IsInitialized { get; }

  /// <summary>Gets a value indicating whether the host OS support the BluetoothLeService.</summary>
  bool IsOsSupported { get; }

  string LastError { get; }

  //// TODO: Add this feature
  //// bool UnpairOnDisconnect { get; set; }

}
