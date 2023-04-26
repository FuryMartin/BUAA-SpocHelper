using System.Diagnostics;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SpocHelper.Core.Models;
using SpocHelper.ViewModels;

namespace SpocHelper.Views;

public sealed partial class CourseFilePage : Page
{
    public CourseFileViewModel ViewModel
    {
        get;
    }

    public CourseFilePage()
    {
        ViewModel = App.GetService<CourseFileViewModel>();
        InitializeComponent();
    }
    private async void AttachmentClick(object sender, RoutedEventArgs e)
    {
        if (((HyperlinkButton)sender).DataContext is CourseFile courseFile)
        {
            var parent = VisualTreeHelper.GetParent((DependencyObject)sender);
            var progressRing = VisualTreeHelper.GetChild(parent, 3);
            var progress = new Progress<int>(percent => ReportProgress(percent, (ProgressRing)progressRing));
            await ViewModel.AttachmentClicked(courseFile, progress);
        }
    }

    private void ReportProgress(int percent, object proRing)
    {
        if (proRing is ProgressRing progressRing)
        {
            progressRing.Value = (percent < 100) ? percent : 0;
        }
    }
}
