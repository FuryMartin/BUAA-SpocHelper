using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
    private void AttachmentClick(object sender, RoutedEventArgs e)
    {
        if (((HyperlinkButton)sender).DataContext is CourseFile courseFile)
        {
            ViewModel.AttachmentClicked(courseFile);
        }
    }
}
