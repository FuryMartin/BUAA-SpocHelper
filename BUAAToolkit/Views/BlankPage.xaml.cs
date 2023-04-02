using System.Diagnostics;
using BUAAToolkit.Core.Models;
using BUAAToolkit.ViewModels;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml.Controls;

namespace BUAAToolkit.Views;

public sealed partial class BlankPage : Page
{
    public BlankViewModel ViewModel
    {
        get;
    }

    public BlankPage()
    {
        ViewModel = App.GetService<BlankViewModel>();
        InitializeComponent();
    }

    private void SubmitClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var homework = ((Button)sender).DataContext as Homework;
        ViewModel.SubmitClicked(homework);
    }

    private void AttachmentClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var homework = ((HyperlinkButton)sender).DataContext as Homework;
        ViewModel.AttachmentClicked(homework);
    }
}
