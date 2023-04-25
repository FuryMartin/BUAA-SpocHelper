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
    private readonly ISpocService spocService = new SpocService();
    private readonly DialogService dialogService = new ();
    private delegate Task<IEnumerable<Course>> GetCourseListAsyncDelegate();
    private delegate Task<string> DownloadAttachmentDelegate(string AttachmentName, string cclj, string DowloadDir, IProgress<int> progress);
    private delegate Task UploadFileDelegate(string filePath, string CourseID);
    private delegate Task<bool> SubmitHomeworkDelegate(HomeworkDetails detail);

    public Progress<int> downloadProgress
    {
        get; set;
    }

    [ObservableProperty]
    public bool pageLoading = true;

    [ObservableProperty]
    public bool allDone = false;

    public string ReSubmitEnabledDescription => "ReSubmitEnabled".GetLocalized();
    public string ReSubmitDisabledDescription => "ReSubmitDisabled".GetLocalized();
    public ObservableCollection<Course> Courses
    {
        get; set;
    }

    public HomeworkViewModel()
    {
        Courses = new ObservableCollection<Course>();
        downloadProgress = new Progress<int> ();
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
            AllDone = false;
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
            getCourseListAsync = spocService.GetUndoneHomeworkList;
        }

        var CourseList = await getCourseListAsync();
        if (CourseList.Count() != 0)
        {
            foreach (var course in CourseList)
            {
                Courses.Add(course);
            }
        }
        else
        {
            AllDone = true;
        }
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

        if (cclj == null || filename == null || downloadDir == null)
        {
            throw new Exception("CCLJ, Filename or downloadDir Maybe NULL");
        }

        var filePath = await downloadAttachment(filename, cclj, downloadDir, downloadProgress);
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
