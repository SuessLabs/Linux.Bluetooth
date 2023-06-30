using Avalonia;
using Avalonia.Controls;
using BleClientTester.Services;
using Prism.Ioc;

namespace BleClientTester.Views;

public partial class MainView : UserControl
{
  public MainView()
  {
    InitializeComponent();
  }

  protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
  {
    base.OnAttachedToVisualTree(e);

    // Initialize the WindowNotificationManager with the "TopLevel". Previously (v0.10), MainWindow
    var window = TopLevel.GetTopLevel(this);
    if (window is null)
      return;

    var notifyService = ContainerLocator.Current.Resolve<INotificationService>();
    notifyService.SetHostWindow(window);
  }
}
