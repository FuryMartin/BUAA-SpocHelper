using SpocHelper.Core.Helpers;
using Newtonsoft.Json;
using System.Net.Mail;

namespace SpocHelper.Core.Models;
public class Homework
{
    [JsonProperty("zjmc")]  // 上课时间
    public string ClassName
    {
        get; set;
    }

    [JsonProperty("wtjsl")] // 未提交数量
    public int UnSubmitedCount
    {
        get; set;
    }

    [JsonProperty("kcdm")]  // 课程代码
    public string CourseID
    {
        get; set; 
    }

    [JsonProperty("lmzwList")] // 作业列表
    public List<HomeworkDetails> Details
    {
        get; set;
    }

    [JsonProperty("xh")]
    public string StudentID
    {
        get; set;
    }
}

public class HomeworkDetails
{

    [JsonProperty("mc11")]  // 作业文本描述
    public string MC11
    {
        get; set;
    }

    public string Description
    {
        get
        {
            var att_prefix = "附件：";

            if (MC11 == null)
            {
                return att_prefix + AttachmentName;
            }
            else if (AttachmentName == null)
            {
                var des_prefix = MC11.Contains("\n") ? "作业要求：\n" : "作业要求：";
                return des_prefix + MC11.Replace("<p>", "").Replace("</p>", "");
            }
            else
            {
                var des_prefix = MC11.Contains("\n") ? "作业要求：\n" : "作业要求：";
                return des_prefix + MC11.Replace("<p>", "").Replace("</p>", "") + "\n\n" + att_prefix + AttachmentName;
            }
        }
    }

    [JsonProperty("fjzyyxcftj")] // 允许重复提交
    public string fjzyyxcftj
    {
        get; set;
    }

    public bool ResubmitEnable => fjzyyxcftj == "1" ? true : false;
    //public string ResubmitEnableDescription => ResubmitEnable ? "允许重复提交" : "不允许重复提交";

    [JsonProperty("cclj")]  // 附件路径
    public string cclj
    {
        get; set;
    }

    [JsonProperty("fjmc")]  // 附件名称
    public string AttachmentName
    {
        get; set;
    }

    public bool AttachmentExisted  => AttachmentName != null;

    [JsonProperty("fjzykssj")] // 作业发布时间
    public string BeginDate
    {
        get; set;
    }

    [JsonProperty("fjzyjzsj")] // 作业截止时间
    public string EndDate
    {
        get; set;
    }

    [JsonProperty("zjdm")]
    public string zjdm
    {
        get; set;
    }

    [JsonProperty("kcdm")]
    public string kcdm
    {
        get; set;
    }

    [JsonProperty("kcnr")]
    public string kcnr
    {
        get; set;
    }

    [JsonProperty("zyzt")]
    public string zyzt
    {
        get; set;
    }

    [JsonProperty("zysfktj")]
    public string zysfktj
    {
        get; set;
    }

    public string FilePathToUpload
    {
        get; set;
    }

    public bool WasSubmitted => zyzt != null;

}
