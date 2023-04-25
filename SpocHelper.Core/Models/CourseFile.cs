using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpocHelper.Core.Models;
public class CourseFile
{
    [JsonProperty("kcdm")]
    public string CourseID;

    [JsonProperty("zldm")]
    public string FileID;

    [JsonProperty("zlmc")]
    public string FileName
    {
        get; set;
    }

    [JsonProperty("lx")] //意义不明，有时为1，有时为Null
    public string Lx;

    [JsonProperty("cclj")]
    public string FilePath;

    [JsonProperty("cjr")]
    public string _Creator;
    public string Creator => _Creator.Replace(" ", string.Empty);

    [JsonProperty("cjsj")]
    public DateTime CreateDate;

    [JsonProperty("zlsize")]
    public string FileSize { get; set; }

    [JsonProperty("zjmc")]
    public string ClassTime { get; set; }
    public string Week => Regex.Match(ClassTime, @"第(\d+)周").Groups[1].Value;

    [JsonProperty("sjly")] //意义不明
    public string Sjly;

    [JsonProperty("zczt")] //意义不明
    public string Zczt; 

    [JsonProperty("yhdm")] 
    public string TeacherID;

    [JsonProperty("sczjwz")] //课前、课后
    public string FileClassType;
}
