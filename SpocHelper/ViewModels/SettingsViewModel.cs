﻿using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;

using SpocHelper.Contracts.Services;
using SpocHelper.Core.Contracts.Services;
using SpocHelper.Core.Models;
using SpocHelper.Core.Services;
using SpocHelper.Helpers;
using SpocHelper.Services;
using SpocHelper.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;
using Windows.System;

namespace SpocHelper.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly INavigationService _navigationService;
    private ElementTheme _elementTheme;
    private string _versionDescription;

    public string? currentDownloadDir => CustomSettingsService.GetDownloadDir();

    public ElementTheme ElementTheme
    {
        get => _elementTheme;
        set => SetProperty(ref _elementTheme, value);
    }

    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }

    public RelayCommand<ElementTheme> SwitchThemeCommand
    {
        get;
    }

    public SettingsViewModel(IThemeSelectorService themeSelectorService)
    {
        _themeSelectorService = themeSelectorService;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();
        _navigationService = App.GetService<INavigationService>();

        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);
                }
            });
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    [RelayCommand]
    public async Task ChangeAccount()
    {
        CustomSettingsService.SaveAccount(null,null);
        _navigationService.NavigateTo(typeof(LoginViewModel).FullName!);
        await Task.CompletedTask;
    }

    [RelayCommand]
    public async Task RateThisApp()
    {
        await Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9PM0GCZ6ZD53"));
    }
}
