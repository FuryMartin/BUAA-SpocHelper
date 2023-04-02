using BUAAToolkit.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace BUAAToolkit.Views;

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
