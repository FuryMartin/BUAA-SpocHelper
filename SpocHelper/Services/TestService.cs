using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpocHelper.Contracts.Services;
using SpocHelper.Core.Helpers;
using SpocHelper.Core.Models;
using SpocHelper.Core.Services;

namespace SpocHelper.Services;

public static class TestService
{
    private static readonly HttpClient client = new()
    {
        BaseAddress = new Uri("http://47.115.210.183:5000")
    };
    private static JObject CourseListJson
    {
        get; set;
    }
    private static List<Course> CourseList
    {
        get; set;
    }

    private static string? StudentID;

    private static readonly INavigationService _navigationService;
    static TestService()
    {
        _navigationService = App.GetService<INavigationService>();
        CourseListJson = new JObject();
        CourseList = new List<Course>();
    }

    public static async Task<bool> Login()
    {

        var values = new Dictionary<string, string>
            {
                { "username", Account.Username},
                { "password", Account.Password}
            };
        var content = new FormUrlEncodedContent(values);
        var response = await client.PostAsync("/account", content);
        var responseText = await response.Content.ReadAsStringAsync();
        if (responseText == "1")
            return true;
        else
            return false;
    }


    public static async Task<IEnumerable<Course>> GetCourseListAsync()
    {
        var data = new { kcmcTab = "", xnxq = "", sfzjkc = 0 };
        var httpContent = new StringContent(JsonConvert.SerializeObject(data));
        var response = await client.PostAsync("/courses", httpContent);
        var responseText = await response.Content.ReadAsStringAsync();
        CourseListJson = JObject.Parse(responseText) ?? throw new Exception("Response is Null");
        var obj = JObject.Parse(responseText)["result"] ?? throw new Exception("Response Result is Null");
        CourseList = JsonConvert.DeserializeObject<List<Course>>(obj.ToString()) ?? throw new Exception("CourseList is Null");
        await GetHomeworkList();

        if (CourseList.Count() > 0)
        {
            StudentID = CourseList[0].HomeworkList[0].StudentID;
        }

        return CourseList;
    }

    public static async Task GetHomeworkList()
    {
        for (var i = 0; i < CourseList.Count; i++)
        {
            var course = CourseList[i];

            var values = new Dictionary<string, string>
            {
                { "kcdm", course.Id},
                { "bjdm", course.Bjdm}
            };
            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("/homeworks", content);
            var responseText = await response.Content.ReadAsStringAsync();
            var obj = JObject.Parse(responseText)["result"] ?? throw new Exception("Response is Null");
            CourseList[i].HomeworkList = JsonConvert.DeserializeObject<List<Homework>>(obj.ToString());
        }
        ParserUndoneHomework();
    }

    public static void ParserUndoneHomework()
    {
        CourseList.RemoveAll(course => course.UnSubmitedCount == 0);
        Debug.WriteLine("Break");
        for (var i = 0; i < CourseList.Count; i++)
        {
            CourseList[i].HomeworkList.RemoveAll(homework => homework.Details.Count() == 0);
        }
    }

    public static async Task<string> DownloadAttachment(string AttachmentName, string cclj, string DowloadDir, IProgress<int> progress)
    {
        var fileName = Path.GetFileName(AttachmentName).ToLower(); // 将文件名中的后缀名部分转换为小写
        var filePath = Path.Combine(DowloadDir, fileName);

        if (!File.Exists(filePath))
        {
            var downloadUrl = "/download";
            var requestUrl = $"{downloadUrl}?fjmc={AttachmentName}&cclj={cclj}";

            var response = await client.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead);
            if (response.IsSuccessStatusCode)
            {
                var contentLength = response.Content.Headers.ContentLength;
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 262144, true))
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    var buffer = new byte[262144]; //256KB per buffer
                    var totalBytesRead = 0L;
                    var bytesRead = 0;
                    var lastPercentage = 0;
                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;

                        if (contentLength.HasValue)
                        {
                            var percentage = (int)Math.Round((double)totalBytesRead / contentLength.Value * 100);
                            if (percentage > lastPercentage)
                            {
                                progress?.Report(percentage);
                            }
                            lastPercentage = percentage;
                        }
                    }
                }
                Debug.WriteLine($"文件已保存至 {filePath}");

                return filePath;
            }
            throw new Exception("Download Error");
        }
        return filePath;
    }

    public static async Task UploadFile(string filePath, string CourseID)
    {
        var uploadURL = "/upload";

        var content = new MultipartFormDataContent();
        var fileName = Path.GetFileName(filePath);
        var fileData = File.ReadAllBytes(filePath);
        var fileContent = new ByteArrayContent(fileData);
        content.Add(new StringContent(CourseID), "courseID");
        content.Add(fileContent, "file", fileName);

        // 发送 POST 请求，并获取响应
        var response = await client.PostAsync(uploadURL, content);
        var result = await response.Content.ReadAsStringAsync();
        Console.WriteLine(result);
    }

    public static async Task<bool> SubmitHomework(HomeworkDetails detail)
    {
        var zyfjFileName = Path.GetFileName(detail.FilePathToUpload);
        var zyfjPath = $"{detail.kcdm}/{MD5Helper.GetFileMD5(detail.FilePathToUpload)}{Path.GetExtension(zyfjFileName)}";
        var contentDictionary = new Dictionary<string, string>
        {
            {"zjdm",  detail.zjdm },
            {"kcdm",  detail.kcdm },
            {"zynrdm", detail.kcnr },
            {"zynr", string.Empty },
            {"zyfjPath", zyfjPath },
            {"zyfjFileName", zyfjFileName },
            {"zyzt", "1" }
        };
        var urlSubmit = "/submit";
        var content = new FormUrlEncodedContent(contentDictionary);
        var response = await client.PostAsync(urlSubmit, content);
        var responseText = await response.Content.ReadAsStringAsync();
        Debug.WriteLine(responseText);

        return true;
    }

    public static async Task<IEnumerable<Course>> GetCourseFileList()
    {
        List<Task> tasks = new();
        foreach (var course in CourseList)
        {
            tasks.Add(Task.Run(async () =>
            {
                var contentDictionary = new Dictionary<string, string>
                {
                    {"kcdm",  course.Id },
                    {"zlmcTab", string.Empty},
                };
                var urlFile = "/files";
                var content = new FormUrlEncodedContent(contentDictionary);
                var response = await client.PostAsync(urlFile, content);
                var responseText = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(responseText)["result"] ?? throw new Exception("Response is Null");
                course.CourseFiles = JsonConvert.DeserializeObject<List<CourseFile>>(obj.ToString());
                course.CourseFiles?.RemoveAll(courseFile => !course.TeacherName.Contains(courseFile.Creator));
                course.CourseFiles = course.CourseFiles?.GroupBy(x => x.FileName).Select(x => x.First()).OrderBy(x => x.FileName).ThenBy(x => x.ClassTime).ToList();
            }));
        }
        await Task.WhenAll(tasks);
        var res = CourseList.Where(course => course.CourseFiles.Count > 0).ToList();
        return res;
    }

}
