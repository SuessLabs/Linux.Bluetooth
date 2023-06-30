using Avalonia;
using Avalonia.Markup.Xaml;
using BleClientTester.Services;
using BleClientTester.ViewModels;
using BleClientTester.Views;
using Prism.DryIoc;
using Prism.Ioc;

namespace BleClientTester;

public partial class App : PrismApplication
{
  public override void Initialize()
  {
    AvaloniaXamlLoader.Load(this);
    base.Initialize();
  }

  protected override AvaloniaObject CreateShell()
  {
    return Container.Resolve<MainWindow>();
  }

  protected override void RegisterTypes(IContainerRegistry containerRegistry)
  {
    // Services
    containerRegistry.RegisterSingleton<ILogService, LogService>();
    containerRegistry.RegisterSingleton<IBluetoothLeService, BluetoothLeService>();

    // Views
    containerRegistry.Register<MainWindow>();
    containerRegistry.RegisterForNavigation<MainView, MainViewModel>();

    // Dialogs
    // containerRegistry.RegisterDialog<MessageBoxView, MessageBoxViewModel>();
  }
}
