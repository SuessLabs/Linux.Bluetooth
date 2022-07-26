using System.Threading.Tasks;
using Tmds.DBus;

namespace Linux.Bluetooth.Tests.Services
{
  [DBusInterface("tmds.myservice")]
  public interface IMyService : IDBusObject
  {
    Task<string> SendCommandAsync(string message);
  }
}
