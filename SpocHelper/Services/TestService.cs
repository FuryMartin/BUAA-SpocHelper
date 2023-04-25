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
    private static readonly HttpClient client = SSOService.GetHttpClient();
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

    public static async Task<bool> TestLogin()
    {

        var values = new Dictionary<string, string>
            {
                { "username", Account.Username},
                { "password", Account.Password}
            };
        var content = new FormUrlEncodedContent(values);
        var response = await client.PostAsync("http://47.115.210.183:5000/account", content);
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
        var response = await client.PostAsync("http://47.115.210.183:5000/courses", httpContent);
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
            var response = await client.PostAsync("http://47.115.210.183:5000/homeworks", content);
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
            var downloadUrl = "http://47.115.210.183:5000/download";
            var requestUrl = $"{downloadUrl}?fjmc={AttachmentName}&cclj={cclj}";

            var response = await client.GetAsync(requestUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsByteArrayAsync();
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.WriteAsync(content);
                }
                Debug.WriteLine($"文件已保存至 {filePath}");

                return filePath;
            }
            throw new Exception("Download Failed");
        }
        return filePath;
    }

    public static async Task UploadFile(string filePath, string CourseID)
    {
        var uploadURL = "http://47.115.210.183:5000/upload";

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
            {"zynr", String.Empty },
            {"zyfjPath", zyfjPath },
            {"zyfjFileName", zyfjFileName },
            {"zyzt", "1" }
        };
        var urlSubmit = "http://47.115.210.183:5000/submit";
        var content = new FormUrlEncodedContent(contentDictionary);
        var response = await client.PostAsync(urlSubmit, content);
        var responseText = await response.Content.ReadAsStringAsync();
        Debug.WriteLine(responseText);

        return true;
    }

}
