using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BUAAToolkit.Core.Contracts.Services;
using BUAAToolkit.Core.Models;
using System.Text;
using System.Net.Http;

namespace BUAAToolkit.Core.Services;
public class SpocService : ISpocService
{
    readonly ISSOService ssoService = new SSOService();
    private readonly HttpClient client;
    public JObject CourseListJson { get; set; }
    public List<Course> CourseList { get; set; }

    public string UserID;


    public SpocService()
    {
        client = ssoService.GetHttpClient();
    }


    public async Task<bool> IsConnected()
    {
        Debug.WriteLine("Moving into IsConnected");
        try
        {
            var response = await client.GetAsync("https://spoc.buaa.edu.cn/spoc/moocMainIndex/spocWelcome");
            var responseText = await response.Content.ReadAsStringAsync();
            //Debug.WriteLine(responseText);
            if (response.Headers.Location == null)
            {
                Debug.WriteLine("Connected");
                return true;
            }

            return false;
        }
        catch (Exception ex) { Debug.WriteLine(ex.Message); Debug.WriteLine("Exception At IsConnected"); return false; }
    }

    public async Task<IEnumerable<Course>> GetCourseListAsync()
    {
        var isConnected = await IsConnected();
        if(!isConnected) {
            await ssoService.SSOLoginAsync();
        }
        var data = new { kcmcTab = "", xnxq = "", sfzjkc = 0 };
        var httpContent = new StringContent(JsonConvert.SerializeObject(data));
        var response = await client.PostAsync("https://spoc.buaa.edu.cn/spoc/rdmooc/GetIndividualClassList.do", httpContent);
        Debug.WriteLine("Course Get");
        var responseText = await response.Content.ReadAsStringAsync();
        CourseListJson = JObject.Parse(responseText);
        CourseList = JsonConvert.DeserializeObject<List<Course>>(CourseListJson["result"].ToString());
        
        //UserID = CourseList[0].UserID;
        
        await GetHomeworkList();

        return CourseList;
    }

    public async Task GetHomeworkList()
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
            var response = await client.PostAsync("https://spoc.buaa.edu.cn/spoc/mooczygl/queryZylmList_xs", content);
            var responseText = await response.Content.ReadAsStringAsync();
            JObject obj = JObject.Parse(responseText);
            CourseList[i].HomeworkList = JsonConvert.DeserializeObject<List<Homework>>(obj["result"].ToString());
        }
        ParserUndoneHomework();
    }

    public void ParserUndoneHomework()
    {
        for (var i = 0; i < CourseList.Count; i++)
        {
            CourseList[i].HomeworkList.RemoveAll(homework => homework.UnSubmitedCount == 0);
        }
        CourseList.RemoveAll(course => course.HomeworkList.Count == 0);
    }

    public async Task<string> DownloadAttachment(string AttachmentName, string cclj)
    {
        var fileName = Path.GetFileName(AttachmentName).ToLower(); // 将文件名中的后缀名部分转换为小写
        var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", fileName);

        if (!File.Exists(filePath))
        {
            var downloadUrl = "https://spoc.buaa.edu.cn/spoc/moocwdkc/downloadTask.do";
            var encodedFJMC = Convert.ToBase64String(Encoding.ASCII.GetBytes(Uri.EscapeDataString(AttachmentName)));
            var encodedCCLJ = Convert.ToBase64String(Encoding.ASCII.GetBytes(cclj));

            var requestUrl = $"{downloadUrl}?fjmc={encodedFJMC}&cclj={encodedCCLJ}";

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
            return null;
        }
        return filePath;
    }
}
