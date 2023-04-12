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
    delegate Task<IEnumerable<Course>> GetCourseListAsyncDelegate();
    delegate Task<string> DownloadAttachmentDelegate(string AttachmentName, string cclj, string DowloadDir);
    delegate Task UploadFileDelegate(string filePath, string CourseID);
    delegate Task<bool> SubmitHomeworkDelegate(HomeworkDetails detail);

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
        GetCourseListAsyncDelegate getCourseListAsync;
        if (Account.IsTestAccount())
        {
            getCourseListAsync = TestService.GetCourseListAsync;
        }
        else
        {
            getCourseListAsync = spocService.GetCourseListAsync;
        }

        var CourseList = await getCourseListAsync();
        foreach (var course in CourseList)
        {
            Courses.Add(course);
        }
        Debug.WriteLine("Break");
    }

    public async Task<bool> SubmitClicked(Homework homework)
    {
        SubmitHomeworkDelegate submitHomework;
        if (Account.IsTestAccount())
        {
            submitHomework = TestService.SubmitHomework;
        }
        else
        {
            submitHomework = spocService.SubmitHomework;
        }

        Debug.WriteLine("Clicked");
        if (homework.Details[0].FilePathToUpload == null)
        {
            await dialogService.ShowConfirmationDialog("SubmitError".GetLocalized(), "FileUnselected".GetLocalized());
            return false;
        }
        var res = await submitHomework(homework.Details[0]);
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
        DownloadAttachmentDelegate downloadAttachment;
        if (Account.IsTestAccount())
        {
            downloadAttachment = TestService.DownloadAttachment;
        }
        else
        {
            downloadAttachment = spocService.DownloadAttachment;
        }

        var downloadDir = CustomSettingsService.GetDownloadDir();
        var filename = homework?.Details[0].AttachmentName;
        var cclj = homework?.Details[0].cclj;
        var filePath = await downloadAttachment(filename, cclj, downloadDir);
        var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(filePath);
        _ = await Launcher.LaunchFileAsync(file);
    }

    public async void UploadFile(Homework homework, string filePath)
    {
        UploadFileDelegate uploadFile;
        if (Account.IsTestAccount())
        {
            uploadFile = TestService.UploadFile;
        }
        else
        {
            uploadFile = spocService.UploadFile;
        }

        var courseID = homework.CourseID;
        homework.Details[0].FilePathToUpload = filePath;
        await uploadFile(filePath, courseID);
    }
}
