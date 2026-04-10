using System;
using Tmds.DBus;

namespace Linux.Bluetooth
{
  public class BlueZEventArgs(bool isStateChange = true) : EventArgs
  {
    public bool IsStateChange { get; } = isStateChange;
  }

  public class DeviceFoundEventArgs(Device device, bool isStateChange = true) : BlueZEventArgs(isStateChange)
  {
    public Device Device { get; } = device;
  }

  public class GattCharacteristicValueEventArgs(byte[] value) : EventArgs
  {
    public byte[] Value { get; } = value;
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

  public class AgentPinCodeEventArgs(ObjectPath device) : EventArgs
  {
    public ObjectPath Device { get; } = device;
  }

  public class AgentPasskeyEventArgs(ObjectPath device) : EventArgs
  {
    public ObjectPath Device { get; } = device;
  }

  public class AgentConfirmationEventArgs(ObjectPath device, uint passkey) : EventArgs
  {
    public ObjectPath Device { get; } = device;
    public uint Passkey { get; } = passkey;
  }

  public class AgentAuthorizationEventArgs(ObjectPath device) : EventArgs
  {
    public ObjectPath Device { get; } = device;
  }

  public class AgentServiceAuthorizationEventArgs(ObjectPath device, string uuid) : EventArgs
  {
    public ObjectPath Device { get; } = device;
    public string ServiceUuid { get; } = uuid;
  }

  public class AgentDisplayPinCodeEventArgs(ObjectPath device, string pincode) : EventArgs
  {
    public ObjectPath Device { get; } = device;
    public string PinCode { get; } = pincode;
  }

  public class AgentDisplayPasskeyEventArgs(ObjectPath device, uint passkey, ushort entered) : EventArgs
  {
    public ObjectPath Device { get; } = device;
    public uint Passkey { get; } = passkey;
    public ushort Entered { get; } = entered;
  }

}
