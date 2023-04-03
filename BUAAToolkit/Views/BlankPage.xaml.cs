using System.Diagnostics;
using BUAAToolkit.Core.Models;
using BUAAToolkit.ViewModels;
using BUAAToolkit.Helpers;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using Windows.System;

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

    private void SubmitClick(object sender, RoutedEventArgs e)
    {
        var homework = ((Button)sender).DataContext as Homework;
        ViewModel.SubmitClicked(homework);
    }

    private void AttachmentClick(object sender, RoutedEventArgs e)
    {
        var homework = ((HyperlinkButton)sender).DataContext as Homework;
        ViewModel.AttachmentClicked(homework);
    }

    private async void FilePickClick(object sender, RoutedEventArgs e)
    {

        // Clear previous returned file name, if it exists, between iterations of this scenario
        var filepath = await FilePickHelper.OpenFilePicker();
        Debug.WriteLine(filepath);
        ToolTipService.SetToolTip((Button)sender, filepath); 
    }
}
