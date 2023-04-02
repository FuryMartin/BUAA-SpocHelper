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
    static readonly IFileService fileService = new FileService();
    readonly ILocalSettingsService Settings;

    public LoginViewModel()
    {
        var options = Options.Create(new LocalSettingsOptions
        {
        });
        Settings = new LocalSettingsService(fileService, options);
        LoadAccount();
    }

    public async void LoadAccount()
    {
        Username = await Settings.ReadSettingAsync<string>("Username");
        Password = await Settings.ReadSettingAsync<string>("Password");
        SetAccount();
    }

    public void SetAccount()
    {
        Account.Username = Username;
        Account.Password = Password;
    }

    [ObservableProperty]
    public string? username;

    [ObservableProperty]
    public string? password;

    [ICommand]
    public async void Login()
    {
        SetAccount();
        Debug.WriteLine("UserName:" + Account.Username);
        Debug.WriteLine("PassWord:" + Account.Password);
        var success = await ssoService.SSOLoginAsync();

        if (!success)
        {
            Debug.WriteLine("Failed");
        }
        else
        {
            Debug.WriteLine("Success");
            await Settings.SaveSettingAsync("Username", Username);
            await Settings.SaveSettingAsync("Password", Password);
        }
    }


}
