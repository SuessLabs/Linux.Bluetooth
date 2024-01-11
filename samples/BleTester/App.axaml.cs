using Avalonia;
using Avalonia.Markup.Xaml;
using BleTester.Services;
using BleTester.ViewModels;
using BleTester.Views;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Regions;

namespace BleTester;

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
    containerRegistry.RegisterSingleton<INotificationService, NotificationService>();

    // Views
    containerRegistry.Register<MainWindow>();
    containerRegistry.RegisterForNavigation<MainView, MainViewModel>();

    // Dialogs
    // containerRegistry.RegisterDialog<MessageBoxView, MessageBoxViewModel>();
  }

  protected override void OnInitialized()
  {
    base.OnInitialized();
    var regionManager = Container.Resolve<IRegionManager>();
    regionManager.RegisterViewWithRegion(Constants.ContentRegion, typeof(MainView));
  }
}
