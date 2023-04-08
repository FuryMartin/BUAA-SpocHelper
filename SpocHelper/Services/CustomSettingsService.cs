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
public static class CustomSettingsService
{
    public static readonly IFileService fileService = new FileService();
    public static readonly ILocalSettingsService Settings = new LocalSettingsService(fileService, Options.Create(new LocalSettingsOptions { }));
    private static string downloadDir; 
    private static bool accountExisted;

    static CustomSettingsService()
    {
        //LoadAccount();
        LoadDownloadDir();
    }

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
        await Settings.SaveSettingAsync("Username", Username);
        await Settings.SaveSettingAsync("Password", Password);
        accountExisted = true;
    }

    public static bool CheckAccountExisted() => accountExisted;

    public static async void SetDownloadDir(string DownloadDir)
    {
        await Settings.SaveSettingAsync("DownloadDir", DownloadDir);
        downloadDir = DownloadDir;
        Debug.WriteLine($"SetDownloadDir:{downloadDir}");
    }

    public static async void LoadDownloadDir()
    {
        downloadDir = await Settings.ReadSettingAsync<string>("DownloadDir");
        downloadDir ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        Debug.WriteLine($"DownloadDir:{downloadDir}");
    }

    public static string GetDownloadDir() => downloadDir;

}
