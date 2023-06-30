using BleClientTester.Services;
using Prism.Commands;

namespace BleClientTester.ViewModels;

public class MainViewModel : ViewModelBase
{
  private readonly IBluetoothLeService _ble;

  public MainViewModel(IBluetoothLeService ble)
  {
    _ble = ble;
  }

  public DelegateCommand CmdScan => new(() =>
  {
  });
}
