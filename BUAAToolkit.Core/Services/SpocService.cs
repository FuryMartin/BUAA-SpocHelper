using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BUAAToolkit.Core.Contracts.Services;
using BUAAToolkit.Core.Models;


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
}
