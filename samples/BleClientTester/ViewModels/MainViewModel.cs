using Prism.Commands;

namespace BleClientTester.ViewModels;

public class MainViewModel : ViewModelBase
{
  public MainViewModel()
  {
  }

  public DelegateCommand CmdScan => new(() =>
  {
  });
}
