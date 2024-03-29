﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BleTester.ViewModels;
using Linux.Bluetooth;

namespace BleTester.Services;

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

  /// <summary>Gets a value indicating whether if we're scanning for nearby devices.</summary>
  public bool IsScanning { get; }

  /// <summary>Gets a value indicating whether the host OS support the BluetoothLeService.</summary>
  bool IsOsSupported { get; }

  string LastError { get; }

  //// TODO: Add this feature (i.e. Helps maintain the limit of paired devices)
  //// bool UnpairOnDisconnect { get; set; }

  /// <summary>Initialize BLE Service and return a list of available adapters.</summary>
  /// <returns>Tuple of successful initialization and list of BLE Adapters.</returns>
  Task<(bool Success, IReadOnlyList<Adapter> Adapters)> AdapterInitializeAsync();

  Task<bool> AdapterScanForDevicesAsync();

  Task<bool> AdapterStopScanForDevicesAsync();

  void AdapterUnitialize();

  /// <summary>Connect to sepecified device.</summary>
  /// <param name="device">Device to connect to.</param>
  /// <returns>True if connection is successful.</returns>
  Task<bool> DeviceConnectAsync(Device device);

  Task<bool> DeviceDisconnectAsync();

  Task<IReadOnlyList<Adapter>> GetAdaptersAsync();
}
