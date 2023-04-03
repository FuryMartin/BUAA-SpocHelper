using BUAAToolkit.Core.Models;

namespace BUAAToolkit.Core.Contracts.Services;
public interface ISpocService
{
    Task<IEnumerable<Course>> GetCourseListAsync();
    Task<string> DownloadAttachment(string AttachmentName, string cclj);
    Task UploadFile(string filePath, string CourseID);
}
