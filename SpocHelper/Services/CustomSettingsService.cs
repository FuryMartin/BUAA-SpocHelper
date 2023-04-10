using SpocHelper.Contracts.Services;
using SpocHelper.Core.Contracts.Services;
using SpocHelper.Core.Models;
using SpocHelper.Core.Helpers;
using SpocHelper.Core.Services;
using SpocHelper.Models;
using Microsoft.Extensions.Options;

namespace SpocHelper.Services;
public static class CustomSettingsService
{
    public static readonly IFileService fileService = new FileService();
    public static readonly ILocalSettingsService Settings = new LocalSettingsService(fileService, Options.Create(new LocalSettingsOptions { }));
    private static string? downloadDir; 
    private static bool accountExisted;

    static CustomSettingsService()
    {
        LoadDownloadDir();
    }

    public static async Task LoadAccount()
    {
        var Username = await Settings.ReadSettingAsync<string>("Username");
        var Password = await Settings.ReadSettingAsync<string>("Password");
        Account.Username = AESHelper.Decrypt(Username);
        Account.Password = AESHelper.Decrypt(Password);
        accountExisted = Account.Username != null || Account.Password != null;
    }

    public static void SetAccount(string? Username, string? Password)
    {
        Account.Username = Username;
        Account.Password = Password;
    }

    public static async void SaveAccount(string? Username, string? Password)
    {
        await Settings.SaveSettingAsync("Username", AESHelper.Encrypt(Username));
        await Settings.SaveSettingAsync("Password", AESHelper.Encrypt(Password));
        accountExisted = true;
    }

    public static bool CheckAccountExisted() => accountExisted;

    public static async void SetDownloadDir(string DownloadDir)
    {
        await Settings.SaveSettingAsync("DownloadDir", DownloadDir);
        downloadDir = DownloadDir;
    }

    public static async void LoadDownloadDir()
    {
        downloadDir = await Settings.ReadSettingAsync<string>("DownloadDir");
        downloadDir ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
    }

    public static string? GetDownloadDir() => downloadDir;

}