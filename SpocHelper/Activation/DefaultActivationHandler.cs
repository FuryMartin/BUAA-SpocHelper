﻿using SpocHelper.Contracts.Services;
using SpocHelper.Services;
using SpocHelper.ViewModels;

using Microsoft.UI.Xaml;

namespace SpocHelper.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;

    public DefaultActivationHandler(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        // None of the ActivationHandlers has handled the activation.
        return _navigationService.Frame?.Content == null;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        await CustomSettingsService.LoadAccount();
        //_navigationService.NavigateTo(typeof(LoginViewModel).FullName!, args.Arguments);
        if (CustomSettingsService.CheckAccountExisted())
        {
            _navigationService.NavigateTo(typeof(HomeworkViewModel).FullName!, args.Arguments);
        }
        else
        {
            _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, args.Arguments);
        }

        await Task.CompletedTask;
    }
}
