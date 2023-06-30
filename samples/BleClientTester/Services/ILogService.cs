namespace BleClientTester.Services;

public interface ILogService
{
  void Debug(string msg);

  void Error(string msg);

  void Status(string msg);
}
