using System.Diagnostics;
using SpocHelper.Core.Models;
using SpocHelper.ViewModels;
using SpocHelper.Helpers;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using Windows.System;

namespace SpocHelper.Views;

public sealed partial class HomeworkPage : Page
{
    public HomeworkViewModel ViewModel
    {
        get;
    }

    public HomeworkPage()
    {
        ViewModel = App.GetService<HomeworkViewModel>();
        InitializeComponent();
    }

    private async void SubmitClick(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        var homework = ((Button)sender).DataContext as Homework;
        var success = await ViewModel.SubmitClicked(homework);
        //button.Flyout.Hide();
        Debug.WriteLine(success);
    }

    private void AttachmentClick(object sender, RoutedEventArgs e)
    {
        var homework = ((HyperlinkButton)sender).DataContext as Homework;
        ViewModel.AttachmentClicked(homework);
    }

    private async void FilePickClick(object sender, RoutedEventArgs e)
    {
        var homework = ((Button)sender).DataContext as Homework;
        // Clear previous returned file name, if it exists, between iterations of this scenario
        var filepath = await FilePickHelper.OpenFilePicker();
        if (filepath != null)
        {
            ViewModel.UploadFile(homework, filepath);
            Debug.WriteLine(filepath);
        }
        ToolTipService.SetToolTip((Button)sender, filepath); 
    }
}
