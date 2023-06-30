using System;

namespace BleClientTester.Services;

public interface ILogService
{
  void Debug(string msg);

  void Error(string msg);

  void Status(string msg);
}

public class LogService : ILogService
{
  public LogService()
  {
  }

  public void Debug(string msg)
  {
    Log($"DEBUG: {msg}");
  }

  public void Error(string msg)
  {
    Log($"ERROR: {msg}");
  }

  public void Status(string msg)
  {
    // Use Prism's EventAggregator to bubble up the message
    Log(msg);
  }

  private void Log(string msg)
  {
    System.Diagnostics.Debug.WriteLine(msg);
    Console.WriteLine(msg);
  }
}
