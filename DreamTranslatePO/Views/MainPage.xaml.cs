using DreamTranslatePO.Contracts.Services;
using DreamTranslatePO.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace DreamTranslatePO.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
    }

    private void QuickStartPoButton_OnClick(object sender, RoutedEventArgs e)
    {
        App.GetService<INavigationService>().Frame.Navigate(typeof(POTranslatePage));
    }

    private void QuickStartCsvButton_OnClick(object sender, RoutedEventArgs e)
    {
        App.GetService<INavigationService>().Frame.Navigate(typeof(CsvTranslatePage));
    }
}