using System.Diagnostics;
using SpocHelper.Contracts.Services;
using SpocHelper.Contracts.ViewModels;
using SpocHelper.Core.Contracts.Services;
using SpocHelper.Core.Models;
using SpocHelper.Core.Services;
using SpocHelper.Helpers;
using SpocHelper.Models;
using SpocHelper.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;

namespace SpocHelper.ViewModels;

public partial class LoginViewModel : ObservableRecipient, INavigationAware
{
    //private ISSOService ssoService = new SSOService();
    private readonly INavigationService _navigationService;
    private readonly DialogService dialogService = new ();
    private delegate Task<bool> SSOLoginAsyncDelegate();

    [ObservableProperty]
    public string? username;

    [ObservableProperty]
    public string? password;

    public LoginViewModel()
    {
        _navigationService = App.GetService<INavigationService>();
        LoadAccount();
    }

    public void LoadAccount()
    {
        Username = Account.Username;
        Password = Account.Password;
    }

    [RelayCommand]
    public async Task Login()
    {
        // 用于微软商店进行测试
        CustomSettingsService.SetAccount(Username, Password);
        Debug.WriteLine(Account.Username+ " " + Account.Password);
        SSOLoginAsyncDelegate loginAsyncDelegate;
        if (Account.IsTestAccount())
        {
            loginAsyncDelegate = TestService.Login;
        }
        else
        {
            loginAsyncDelegate = SSOService.SSOLoginAsync;
        }

        var success = await loginAsyncDelegate();
        if (!success)
        {
            Debug.WriteLine("Failed");
            await dialogService.ShowConfirmationDialog("LoginError".GetLocalized(), "ConfirmAccountInfo".GetLocalized());
        }
        else
        {
            Debug.WriteLine("Success");
            CustomSettingsService.SaveAccount(Username, Password);
            _navigationService.NavigateTo(typeof(HomeworkViewModel).FullName!);
            await Task.CompletedTask;
        }
    }

    public void OnNavigatedFrom()
    {
    }
    public void OnNavigatedTo(object parameter)
    {
        SSOService.InitializeClient();
    }
}
