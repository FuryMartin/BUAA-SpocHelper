using Newtonsoft.Json;

namespace SpocHelper.Core.Models;
public class Course
{
    [JsonProperty("kcid")]
    public string Id
    {
        get; set;
    }

    [JsonProperty("kcmc")]
    public string Name
    {
        get; set;
    }
    [JsonProperty("bjdm")]
    //bjdm，似乎是一个学生的ID
    public string Bjdm
    {
        get; set;
    }

    [JsonProperty("jsxm")]
    public string TeacherName
    {
        get; set;
    }

    [JsonProperty("yhdm")]
    public string Yhdm
    {
        get; set;
    }

    [JsonProperty("xnxq")]
    public string Term
    {
        get; set;
    }

    public List<Homework> HomeworkList
    {
        get; set;
    }

    public int UnSubmitedCount
    {
        get
        {
            var sum = 0;
            foreach (var homework in HomeworkList)
            {
                homework.Details.RemoveAll(detail => detail.zysfktj == "0");
                sum += homework.Details.Count();
                //sum += homework.UnSubmitedCount;
            }
            return sum;
        }
    }
    
}