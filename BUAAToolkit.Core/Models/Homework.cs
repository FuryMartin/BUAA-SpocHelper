using Newtonsoft.Json;

namespace BUAAToolkit.Core.Models;
public class Homework
{
    [JsonProperty("zjmc")]
    public string ClassName
    {
        get; set;
    }

    [JsonProperty("wtjsl")]
    public int UnSubmitedCount
    {
        get; set;
    }

    [JsonProperty("lmzwList")]
    public List<HomeworkDetails> Details
    {
        get; set;
    }
}

public class HomeworkDetails
{

    [JsonProperty("mc11")]
    public string Description
    {
        get; set;
    }

    [JsonProperty("cclj")]
    //不知道是啥，但是下载附件要用，具体来说是先URI编码再转Base64编码，与下面的
    //Get 方法 https://spoc.buaa.edu.cn/spoc/moocwdkc/downloadTask.do?fjmc={Base64(Uri(fjmc))}&cclj={Base64(Uri(AcctachedFileName))}
    public string cclj
    {
        get; set;
    }

    private string _AttachmentName;

    [JsonProperty("fjmc")]
    public string AttachmentName
    {
        get => _AttachmentName; 
        set
        {
            AttachmentExisted = (value==null) ? false : true;
            _AttachmentName = value ?? Description.Replace("<p>","").Replace("</p>","");
        }
    }

    public bool AttachmentExisted { get; set; }

    [JsonProperty("fjzykssj")]
    public string BeginDate
    {
        get; set;
    }

    [JsonProperty("fjzyjzsj")]
    public string EndDate
    {
        get; set;
    }
}
