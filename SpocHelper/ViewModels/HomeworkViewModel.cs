using CommunityToolkit.Mvvm.ComponentModel;
using SpocHelper.Core.Contracts.Services;
using SpocHelper.Core.Services;
using SpocHelper.Contracts.ViewModels;
using SpocHelper.Core.Models;
using SpocHelper.Services;
using System.Diagnostics;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Windows.System;
using SpocHelper.Helpers;

namespace SpocHelper.ViewModels;

public partial class HomeworkViewModel : ObservableRecipient, INavigationAware
{
    ISpocService spocService = new SpocService();
    DialogService dialogService = new DialogService();
    
    [ObservableProperty]
    public bool pageLoading = true;

    public string ReSubmitEnabledDescription => "ReSubmitEnabled".GetLocalized();
    public string ReSubmitDisabledDescription => "ReSubmitDisabled".GetLocalized();
    public ObservableCollection<Course> Courses
    {
        get; set;
    }

    public HomeworkViewModel()
    {
        Initialize();
    }

    public void Initialize()
    {
        Courses = new ObservableCollection<Course>();
    }

    public void OnNavigatedFrom()
    {
    }

    public void OnNavigatedTo(object parameter)
    {
        FreshPage();
    }

    public async void FreshPage()
    {
        try
        {
            PageLoading = true;
            Courses.Clear();
            await GetCourses();
            PageLoading = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    public async Task GetCourses()
    {
        var CourseList = await spocService.GetCourseListAsync();
        foreach (var course in CourseList)
        {
            Courses.Add(course);
        }
        Debug.WriteLine("Break");
    }

    public async Task<bool> SubmitClicked(Homework homework)
    {
        Debug.WriteLine("Clicked");
        if (homework.Details[0].FilePathToUpload == null)
        {
            await dialogService.ShowConfirmationDialog("SubmitError".GetLocalized(), "FileUnselected".GetLocalized());
            return false;
        }
        var res = await spocService.SubmitHomework(homework.Details[0]);
        if (res)
        {
            await dialogService.ShowConfirmationDialog("SubmitSuccess".GetLocalized(), "SubmitCongratulation".GetLocalized());
            FreshPage();
        }
        else
        {
            await dialogService.ShowConfirmationDialog("SubmitError".GetLocalized(), "SubmitErrorGuess".GetLocalized());
        }
        return res;
    }

    public async void AttachmentClicked(Homework? homework)
    {
        var downloadDir = CustomSettingsService.GetDownloadDir();
        var filename = homework?.Details[0].AttachmentName;
        var cclj = homework?.Details[0].cclj;
        var filePath = await spocService.DownloadAttachment(filename, cclj, downloadDir);
        var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(filePath);
        _ = await Launcher.LaunchFileAsync(file);
    }

    public async void UploadFile(Homework homework, string filePath)
    {
        var courseID = homework.CourseID;
        homework.Details[0].FilePathToUpload = filePath;
        await spocService.UploadFile(filePath, courseID);
    }
}
