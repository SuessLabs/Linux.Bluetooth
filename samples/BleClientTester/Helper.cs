using System;
using System.Text;
using System.Threading.Tasks;

namespace BleClientTester;

public static class Helper
{
  /// <summary>
  ///   Wait until condition is met or until cancellation is triggered.
  /// </summary>
  /// <param name="condition">Boolean condition to meet.</param>
  /// <param name="refreshRate">Refresh rate in milliseconds.</param>
  /// <param name="timeout">Maximum runtime timeout.</param>
  /// <returns>True on success.</returns>
  /// <example>
  /// <code>
  ///   var cancelToken = new new System.Threading.CancellationToken();
  ///   var success = await this.WaitUntilAsync(
  ///     () => (_countCircuitscompleted > 0),
  ///     refreshRate: 25,
  ///     timeout: 5000);
  /// </code></example>
  public static async Task<bool> WaitUntilAsync(Func<bool> condition, int refreshRate = 25, int timeout = -1)
  {
    var waitTask = Task.Run(async () =>
    {
      while (!condition())
        await Task.Delay(refreshRate);
    });

    return waitTask == await Task.WhenAny(waitTask, Task.Delay(timeout));
  }

  public static string StringFromBytes(byte[] data)
  {
    return Encoding.UTF8.GetString(data);
  }

  public static byte[] StringToBytes(string data)
  {
    return Encoding.UTF8.GetBytes(data);
  }
}
