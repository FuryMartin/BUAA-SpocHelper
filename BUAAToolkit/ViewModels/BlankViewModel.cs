using CommunityToolkit.Mvvm.ComponentModel;
using BUAAToolkit.Core.Contracts.Services;
using BUAAToolkit.Core.Services;
using BUAAToolkit.Contracts.ViewModels;
using BUAAToolkit.Core.Models;
using System.Diagnostics;
using System.Collections.ObjectModel;

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
}
