using System.Threading.Tasks;
using Tmds.DBus;

namespace Plugin.BlueZ.Tests.Services
{
  [DBusInterface("tmds.myservice")]
  public interface IMyService : IDBusObject
  {
    Task<string> SendCommandAsync(string message);
  }
}
