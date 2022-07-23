using System.Threading.Tasks;
using Tmds.DBus;

namespace Plugin.BlueZ.Tests.Services
{
  public class MyService : IMyService
  {
    public static readonly ObjectPath Path = new ObjectPath("/tmds/myservice");

    public ObjectPath ObjectPath { get => Path; }

    public Task<string> SendCommandAsync(string command)
    {
      return Task.FromResult($"Hello {command}!");
    }
  }
}
