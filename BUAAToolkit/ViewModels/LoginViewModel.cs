using System.Diagnostics;
using BUAAToolkit.Contracts.Services;
using BUAAToolkit.Core.Contracts.Services;
using BUAAToolkit.Core.Models;
using BUAAToolkit.Core.Services;
using BUAAToolkit.Models;
using BUAAToolkit.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;

namespace BUAAToolkit.ViewModels;

public partial class LoginViewModel : ObservableRecipient
{
    readonly ISSOService ssoService = new SSOService();

    [ObservableProperty]
    public string? username;

    [ObservableProperty]
    public string? password;

    public LoginViewModel()
    {
        LoadAccount();
    }

    public void LoadAccount()
    {
        Username = Account.Username;
        Password = Account.Password;
    }

    [ICommand]
    public async void Login()
    {
        AccountService.SetAccount(Username, Password);
        var success = await ssoService.SSOLoginAsync();
        if (!success)
        {
            Debug.WriteLine("Failed");
        }
        else
        {
            Debug.WriteLine("Success");
            await AccountService.Settings.SaveSettingAsync("Username", Username);
            await AccountService.Settings.SaveSettingAsync("Password", Password);
        }
    }


}
