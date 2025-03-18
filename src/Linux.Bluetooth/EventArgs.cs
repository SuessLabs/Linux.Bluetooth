using System;
using System.Collections.Generic;

namespace Linux.Bluetooth
{
  public class BlueZEventArgs : EventArgs
  {
    public BlueZEventArgs(bool isStateChange = true)
    {
      IsStateChange = isStateChange;
    }

    public bool IsStateChange { get; }
  }

  public class DeviceFoundEventArgs : BlueZEventArgs
  {
    public DeviceFoundEventArgs(Device device, bool isStateChange = true)
      : base(isStateChange)
    {
      Device = device;
    }

    public Device Device { get; }
  }

  public class GattCharacteristicValueEventArgs : EventArgs
  {
    public GattCharacteristicValueEventArgs(byte[] value)
    {
      Value = value;
    }

    public byte[] Value { get; }
  }

  public class AdvertisementReceivedEventArgs(string deviceAddress, IDictionary<string, object> advertisementData) : EventArgs
  {
    public string DeviceAddress { get; } = deviceAddress;
    public IDictionary<string, object> AdvertisementData { get; } = advertisementData;
  }

}
