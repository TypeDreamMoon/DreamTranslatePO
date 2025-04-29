using DreamTranslatePO.Helpers;

using Windows.UI.ViewManagement;
using DreamTranslatePO.Classes;

namespace DreamTranslatePO;

public sealed partial class MainWindow : WindowEx
{
    private Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;

    private UISettings settings;

    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();

        // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        settings = new UISettings();
        settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event

        switch (AppSettingsManager.GetSettings().BackgroundMode)
        {
            case "Mica":
                TrySetMicaBackdrop(false);
                break;
            case "MicaAlt":
                TrySetMicaBackdrop(true);
                break;
            case "Acrylic":
                TrySetDesktopAcrylicBackdrop();
                break;
        }
    }
    
    bool TrySetMicaBackdrop(bool useMicaAlt)
    {
        if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
        {
            Microsoft.UI.Xaml.Media.MicaBackdrop micaBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
            micaBackdrop.Kind = useMicaAlt ? Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt : Microsoft.UI.Composition.SystemBackdrops.MicaKind.Base;
            this.SystemBackdrop = micaBackdrop;

            return true; // Succeeded.
        }

        return false; // Mica is not supported on this system.
    }
    
    bool TrySetDesktopAcrylicBackdrop()
    {
        if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
        {
            Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop DesktopAcrylicBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();
            this.SystemBackdrop = DesktopAcrylicBackdrop;

            return true; // Succeeded.
        }

        return false; // DesktopAcrylic is not supported on this system.
    }
    
    

    // this handles updating the caption button colors correctly when indows system theme is changed
    // while the app is open
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        // This calls comes off-thread, hence we will need to dispatch it to current app's thread
        dispatcherQueue.TryEnqueue(() =>
        {
            TitleBarHelper.ApplySystemThemeToCaptionButtons();
        });
    }
}
