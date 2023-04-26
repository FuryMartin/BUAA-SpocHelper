using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using SpocHelper.Contracts.ViewModels;
using SpocHelper.Core.Contracts.Services;
using SpocHelper.Core.Models;
using SpocHelper.Core.Services;
using SpocHelper.Services;
using Windows.System;

namespace SpocHelper.ViewModels;

public partial class CourseFileViewModel : ObservableRecipient, INavigationAware
{
    private readonly ISpocService spocService = new SpocService();

    private delegate Task<string> DownloadAttachmentDelegate(string AttachmentName, string cclj, string DowloadDir, IProgress<int> progress);
    private delegate Task<IEnumerable<Course>> GetCourseListAsyncDelegate();

    public ObservableCollection<Course> Courses
    {
        get; set;
    }
    public CourseFileViewModel()
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

    [ObservableProperty]
    public bool pageLoading = true;

    [ObservableProperty]
    public bool allDone = false;

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
            getCourseListAsync = TestService.GetCourseFileList;
        }
        else
        {
            getCourseListAsync = spocService.GetCourseFileList;
        }

        var CourseList = await getCourseListAsync();
        if (CourseList.Any())
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

    public async Task AttachmentClicked(CourseFile courseFile, Progress<int> progress)
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
        var filename = courseFile.FileName;
        var cclj = courseFile.FilePath;

        if (cclj == null || filename == null || downloadDir == null)
        {
            throw new Exception("CCLJ, Filename or downloadDir Maybe NULL");
        }

        var filePath = await downloadAttachment(filename, cclj, downloadDir, progress);
        var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(filePath);
        _ = await Launcher.LaunchFileAsync(file);
    }


}
