using SpocHelper.ViewModels;

using Microsoft.UI.Xaml.Controls;
using SpocHelper.Helpers;
using SpocHelper.Services;

namespace SpocHelper.Views;

// TODO: Set the URL for your privacy policy by updating SettingsPage_PrivacyTermsLink.NavigateUri in Resources.resw.
public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
    }

    private async void SetDowloadDirectory_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var filepath = await FilePickHelper.OpenFolderPicker();
        if (filepath != null)
        {
            CustomSettingsService.SetDownloadDir(filepath);
            ToolTipService.SetToolTip((Button)sender, filepath);
        }
    }
}
