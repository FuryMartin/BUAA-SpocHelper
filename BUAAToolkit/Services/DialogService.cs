using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BUAAToolkit.Services;
public class DialogService
{
    public async Task<bool> ShowConfirmationDialog(string title, string? content = null)
    {
        ContentDialog contentDialog = new()
        {
            Title = title,
            Content = content,

            PrimaryButtonText = "明白了",
            //SecondaryButtonText = "No",
            DefaultButton = ContentDialogButton.Primary,

            XamlRoot = App.MainWindow.Content.XamlRoot,
            RequestedTheme = ElementTheme.Default,
        };

        ContentDialogResult result = await contentDialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    public async Task ShowDialog_Click()
    {
        ContentDialog dialog = new ContentDialog();

        // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
        dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
        dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.Title = "Save your work?";
        dialog.PrimaryButtonText = "Save";
        //dialog.SecondaryButtonText = "Don't Save";
        dialog.CloseButtonText = "Cancel";
        dialog.DefaultButton = ContentDialogButton.Primary;
        dialog.Content = "Hello World";

        var result = await dialog.ShowAsync();
    }
}
