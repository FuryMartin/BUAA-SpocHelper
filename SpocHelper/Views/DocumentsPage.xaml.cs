using Microsoft.UI.Xaml.Controls;

using SpocHelper.ViewModels;

namespace SpocHelper.Views;

public sealed partial class DocumentsPage : Page
{
    public DocumentsViewModel ViewModel
    {
        get;
    }

    public DocumentsPage()
    {
        ViewModel = App.GetService<DocumentsViewModel>();
        InitializeComponent();
    }
}
