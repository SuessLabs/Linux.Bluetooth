using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Linux.Bluetooth
{
  /// <summary>
  /// Device Properties.
  /// Note, in the 6.0 release, most of the following properties will all be marked as, Read-Only.
  /// As per the BlueZ 5.53 documentation https://git.kernel.org/pub/scm/bluetooth/bluez.git/tree/doc/device-api.txt?h=5.53
  /// </summary>
  public class DeviceProperties
  {
    /// <summary>Readonly. Gets or sets the Bluetooth device address of the remote device.</summary>
    public string? Address { get; set; }

    /// <summary>
    /// Readonly.
    /// The Bluetooth device Address Type. For dual-mode and BR/EDR only devices this defaults to "public". Single
    /// mode LE devices may have either value. If remote device uses privacy than before pairing this represents address
    /// type used for connection and Identity Address after pairing.
    ///
    /// Possible values:
    ///   "public" - Public address
    ///   "random" - Random address
    /// </summary>
    public string? AddressType { get; set; }

    /// <summary>
    ///   The name alias for the remote device.The alias can be used to have a different friendly name for the remote device.
    ///
    ///   In case no alias is set, it will return the remote device name.Setting an empty string as alias will
    ///   convert it back to the remote device name.
    ///
    ///   When resetting the alias with an empty string, the
    ///   property will default back to the remote name.
    /// </summary>
    public string? Alias { get; set; }

    /// <summary>Optional, readonly. External appearance of device, as found on GAP service.</summary>
    public ushort Appearance { get; set; }

    /// <summary>
    /// If set to true any incoming connections from the device will be immediately rejected.Any device drivers will also be removed and no new ones will be probed as long as the device is blocked.
    /// </summary>
    public bool Blocked { get; set; }

    /// <summary>Optional, readonly. The Bluetooth class of device of the remote device.</summary>
    public uint Class { get; set; }

    /// <summary>Readonly. Indicates if the remote device is currently connected. A PropertiesChanged signal indicate changes to this status.</summary>
    [Obsolete("Use the preferred, IsConnected, property.")]
    public bool Connected { get; set; }

    /// <summary>Readonly. Indicates if the remote device is currently connected. A PropertiesChanged signal indicate changes to this status.</summary>
    public bool IsConnected { get; set; }

    /// <summary>Optional, readonly. Proposed icon name according to the freedesktop.org icon naming specification.</summary>
    public string? Icon { get; set; }

    /// <summary>
    ///   Readonly.
    ///   Set to true if the device only supports the pre-2.1
    ///   pairing mechanism. This property is useful during
    ///   device discovery to anticipate whether legacy or
    ///   simple pairing will occur if pairing is initiated.
    ///
    ///   Note that this property can exhibit false-positives
    ///   in the case of Bluetooth 2.1 (or newer) devices that
    ///   have disabled Extended Inquiry Response support.
    /// </summary>
    public bool LegacyPairing { get; set; }

    /// <summary>
    /// Optional, readonly. Manufacturer specific advertisement data.Keys are 16 bits Manufacturer ID followed by its byte array value.
    /// </summary>
    public IDictionary<ushort, object>? ManufacturerData { get; set; }

    /// <summary>Optional, readonly. Remote Device ID information in modalias format used by the kernel and udev</summary>
    public string? Modalias { get; set; }

    /// <summary>
    /// Optional, readonly. The Bluetooth remote name.
    /// This value can not be changed. Use the Alias property instead.
    ///
    /// This value is only present for completeness. It is better to always use the Alias property when
    /// displaying the devices name.
    ///
    /// If the Alias property is unset, it will reflect this value which makes it more convenient.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>Readonly. Indicates if the remote device is paired.</summary>
    public bool Paired { get; set; }

    /// <summary>Optional, readonly. Received Signal Strength Indicator of the remote device(inquiry or advertising).</summary>
    [Obsolete("Use the preferred, Rssi, property")]
    public short RSSI { get; set; }

    /// <summary>Optional, readonly. Received Signal Strength Indicator of the remote device(inquiry or advertising).</summary>
    public short Rssi { get; set; }

    /////// <summary>
    /////// Readonly. The object path of the adapter the device belongs to.
    /////// </summary>
    //// public ObjectPath Adapter { get; set; }

    /// <summary>
    /// Optional, readonly. Service advertisement data.Keys are the UUIDs in string format followed by its byte array value.
    /// </summary>
    public IDictionary<string, object>? ServiceData { get; set; }

    /// <summary>Optional, readonly. Indicate whether or not service discovery has been resolved.</summary>
    public bool ServicesResolved { get; set; }

    /// <summary>Indicates if the remote is seen as trusted. This setting can be changed by the application.</summary>
    public bool Trusted { get; set; }

    /// <summary>Optional, readonly. Advertised transmitted power level (inquiry or advertising).</summary>
    public short TxPower { get; set; }

    /// <summary>Optional, readonly. List of 128-bit UUIDs that represents the available remote services.</summary>
    public string[]? UUIDs { get; set; }

    public override string ToString()
    {
      var desc = string.Empty;

      try
      {
        desc = $"'{Name}' - {Address} (Alias: {Alias}; RSSI: {Rssi}; IsPaired: {Paired})";
      }
      catch (Exception)
      {
      }

      return desc;
    }
  }
}
