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
        if (((Button)sender).DataContext is HomeworkDetails homeworkDetails)
        {
            var success = await ViewModel.SubmitClicked(homeworkDetails);
            //button.Flyout.Hide();
            Debug.WriteLine(success);
        }
    }

    private void AttachmentClick(object sender, RoutedEventArgs e)
    {
        var homeworkDetails = ((HyperlinkButton)sender).DataContext as HomeworkDetails;
        ViewModel.AttachmentClicked(homeworkDetails);
    }

    private async void FilePickClick(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).DataContext is HomeworkDetails homeworkDetails)
        {
            // Clear previous returned file name, if it exists, between iterations of this scenario
            var filepath = await FilePickHelper.OpenFilePicker();
            if (filepath != null)
            {
                ViewModel.UploadFile(homeworkDetails, filepath);
                Debug.WriteLine(filepath);
            }
            ToolTipService.SetToolTip((Button)sender, filepath);
        }

    }
}
