using System.Diagnostics;
using BUAAToolkit.Contracts.Services;
using BUAAToolkit.Contracts.ViewModels;
using BUAAToolkit.Core.Contracts.Services;
using BUAAToolkit.Core.Models;
using BUAAToolkit.Core.Services;
using BUAAToolkit.Models;
using BUAAToolkit.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;

namespace BUAAToolkit.ViewModels;

public partial class LoginViewModel : ObservableRecipient, INavigationAware
{
    private ISSOService ssoService = new SSOService();
    private readonly INavigationService _navigationService;
    readonly DialogService dialogService = new DialogService();

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

    [ICommand]
    public async Task Login()
    {
        AccountService.SetAccount(Username, Password);
        var success = await ssoService.SSOLoginAsync();
        if (!success)
        {
            Debug.WriteLine("Failed");
            await dialogService.ShowConfirmationDialog("登录失败", "请检查账号密码是否正确");
        }
        else
        {
            Debug.WriteLine("Success");
            AccountService.SaveAccount(Username, Password);
            _navigationService.NavigateTo(typeof(BlankViewModel).FullName!);
            await Task.CompletedTask;
        }
    }

    public void OnNavigatedFrom()
    {
    }
    public void OnNavigatedTo(object parameter)
    {
        ssoService = new SSOService();
    }
}
