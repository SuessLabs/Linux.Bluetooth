using System.Text;

namespace BleClientTester;

public static class Converter
{
  public static string StringFromBytes(byte[] data)
  {
    return Encoding.UTF8.GetString(data);
  }

  public static byte[] StringToBytes(string data)
  {
    return Encoding.UTF8.GetBytes(data);
  }
}
