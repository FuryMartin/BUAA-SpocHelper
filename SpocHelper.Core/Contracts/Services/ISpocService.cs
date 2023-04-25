using SpocHelper.Core.Models;

namespace SpocHelper.Core.Contracts.Services;
public interface ISpocService
{
    //Task<IEnumerable<Course>> GetCourseListAsync();
    Task<string> DownloadAttachment(string AttachmentName, string cclj, string DowloadDir, IProgress<int> progress);
    Task UploadFile(string filePath, string CourseID);
    Task<bool> SubmitHomework(HomeworkDetails detail);
    Task<IEnumerable<Course>> GetUndoneHomeworkList();
    Task<IEnumerable<Course>> GetCourseFileList();
}
