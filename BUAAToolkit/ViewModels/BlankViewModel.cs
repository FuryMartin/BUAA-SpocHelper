using CommunityToolkit.Mvvm.ComponentModel;
using BUAAToolkit.Core.Contracts.Services;
using BUAAToolkit.Core.Services;
using BUAAToolkit.Contracts.ViewModels;
using BUAAToolkit.Core.Models;
using BUAAToolkit.Services;
using System.Diagnostics;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Windows.System;

namespace BUAAToolkit.ViewModels;

public partial class BlankViewModel : ObservableRecipient, INavigationAware
{
    ISpocService spocService = new SpocService();
    public ObservableCollection<Course> Courses
    {
        get; set;
    }

    [ObservableProperty]
    string courseName;

    public BlankViewModel()
    {
        Courses = new ObservableCollection<Course>();
        AccountService.LoadAccount();
    }

    public void OnNavigatedFrom()
    {
    }

    public void OnNavigatedTo(object parameter)
    {

        try
        {
            Courses.Clear();
            GetCourses();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    public async void GetCourses()
    {
        var CourseList = await spocService.GetCourseListAsync();
        foreach (var course in CourseList)
        {
            Courses.Add(course);
        }
        Debug.WriteLine("Break");
    }

    public void SubmitClicked(Homework? homework)
    {
        Debug.WriteLine(homework?.Details[0].cclj);
        IFileService fileService = new FileService();
        //fileService.Save("C:\\Users\\Fury\\Downloads", "homework.txt", homework?.Details[0].cclj);
    }

    public async void AttachmentClicked(Homework? homework)
    {
        var filename = homework?.Details[0].AttachmentName;
        var cclj = homework?.Details[0].cclj;
        var filePath = await spocService.DownloadAttachment(filename, cclj);
        //_ = Task.Run(() => Process.Start(filePath));
    }
}
