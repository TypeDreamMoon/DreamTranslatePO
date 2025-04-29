using DreamTranslatePO.ViewModels;
using Microsoft.UI.Xaml.Controls;

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
}