using System;
using System.Threading.Tasks;
using BleClientTester.ViewModels;
using Linux.Bluetooth;

namespace BleClientTester.Services;

public class BluetoothLeService : IBluetoothLeService
{
  public Adapter Adapter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
  public bool AutoConnectDevice { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
  public bool AutoScanDevices { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

  public BleDevice CurrentDevice => throw new NotImplementedException();

  public string DeviceScanFilter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

  public bool IsInitialized => throw new NotImplementedException();

  public bool IsOsSupported => throw new NotImplementedException();

  public string LastError => throw new NotImplementedException();

  public event Action<Device, DeviceProperties> OnAdapterDiscoveredDevice;
  public event Action<Adapter> OnAdapterPoweredOff;
  public event Action<Adapter> OnAdapterPoweredOn;
  public event Action<Device, BlueZEventArgs> OnDeviceConnected;
  public event Action<Device, BlueZEventArgs> OnDeviceDisconnected;
  public event Action<string> OnDeviceNotification;
  public event Action<Device, BlueZEventArgs> OnDeviceServicesResolved;
  public event Action<IGattService1> OnDeviceServiceResolved;

  public Task<bool> AdapterPowerAsync(bool poweredOn)
  {
    throw new NotImplementedException();
  }
}
