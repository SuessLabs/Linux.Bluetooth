using System;
using System.Collections.Generic;
using Tmds.DBus;

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

  public class GattCharacteristicServerValueEventArgs(object devicePath, byte[] value) : EventArgs
  {
    public object DevicePath { get; } = devicePath;

    public byte[] Value { get; } = value;
  }

  public class GattDescriptorValueEventArgs(object devicePath, byte[] value) : EventArgs
  {
    public object DevicePath { get; } = devicePath;

    public byte[] Value { get; } = value;
  }

}
