using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.BlueZ
{
  public class DeviceProperties
  {
    public string Address { get; set; }

    public string AddressType { get; set; }

    public string Alias { get; set; }

    public ushort Appearance { get; set; }

    public bool Blocked { get; set; }

    public uint Class { get; set; }

    public bool Connected { get; set; }

    public string Icon { get; set; }

    public bool LegacyPairing { get; set; }

    public IDictionary<ushort, object> ManufacturerData { get; set; }

    public string Modalias { get; set; }

    public string Name { get; set; }

    public bool Paired { get; set; }

    public short RSSI { get; set; }

    //// public ObjectPath Adapter { get; set; }

    public IDictionary<string, object> ServiceData { get; set; }

    public bool ServicesResolved { get; set; }

    public bool Trusted { get; set; }

    public short TxPower { get; set; }

    public string[] UUIDs { get; set; }

    public override string ToString()
    {
      var desc = string.Empty;

      try
      {
        desc = $"'{Name}' - {Address} (Alias: {Alias}; RSSI: {RSSI}; IsPaired: {Paired})";
      }
      catch (Exception)
      {
      }

      return desc;
    }
  }
}
