using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpocHelper.Contracts.Services;
using SpocHelper.Core.Contracts.Services;
using SpocHelper.Core.Models;
using SpocHelper.Core.Services;
using SpocHelper.Models;
using Microsoft.Extensions.Options;

namespace SpocHelper.Services;
public static class AccountService
{
    public static readonly IFileService fileService = new FileService();
    public static readonly ILocalSettingsService Settings = new LocalSettingsService(fileService, Options.Create(new LocalSettingsOptions { }));
    public static bool accountExisted;

    public static async Task LoadAccount()
    {
        Account.Username = await Settings.ReadSettingAsync<string>("Username");
        Account.Password = await Settings.ReadSettingAsync<string>("Password");
        accountExisted = Account.Username != null || Account.Password != null;
    }

    public static void SetAccount(string? Username, string? Password)
    {
        Account.Username = Username;
        Account.Password = Password;
    }

    public static async void SaveAccount(string? Username, string? Password)
    {
        await AccountService.Settings.SaveSettingAsync("Username", Username);
        await AccountService.Settings.SaveSettingAsync("Password", Password);
        AccountService.accountExisted = true;
    }
}
