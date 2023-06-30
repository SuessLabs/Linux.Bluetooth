using System;
using System.Collections.Generic;

namespace Linux.Bluetooth
{
  public class DeviceProperties
  {
    public string Address { get; set; } = string.Empty;

    public string AddressType { get; set; } = string.Empty;

    public string Alias { get; set; } = string.Empty;

    public ushort Appearance { get; set; }

    public bool Blocked { get; set; }

    public uint Class { get; set; }

    public bool IsConnected { get; set; }

    public string Icon { get; set; } = string.Empty;

    public bool LegacyPairing { get; set; }

    public IDictionary<ushort, object>? ManufacturerData { get; set; }

    public string Modalias { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool Paired { get; set; }

    public short Rssi { get; set; }

    //// public ObjectPath Adapter { get; set; }

    public IDictionary<string, object>? ServiceData { get; set; }

    public bool ServicesResolved { get; set; }

    public bool Trusted { get; set; }

    public short TxPower { get; set; }

    public string[] UUIDs { get; set; } = Array.Empty<string>();

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
