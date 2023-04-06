using SpocHelper.Core.Models;

namespace SpocHelper.Core.Contracts.Services;
public interface ISpocService
{
    Task<IEnumerable<Course>> GetCourseListAsync();
    Task<string> DownloadAttachment(string AttachmentName, string cclj);
    Task UploadFile(string filePath, string CourseID);
    Task<bool> SubmitHomework(HomeworkDetails detail);
}
