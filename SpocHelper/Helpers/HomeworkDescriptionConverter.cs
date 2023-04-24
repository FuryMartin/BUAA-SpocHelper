using System.Net.Mail;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using SpocHelper.Core.Models;

namespace SpocHelper.Helpers;

public partial class HomeworkDescriptionConverter : IValueConverter
{
    public HomeworkDescriptionConverter(){}
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        // 检查值是否为对象类型
        if (value is HomeworkDetails homeworkDetails)
        {

            // 拼接两个属性
            return ParseDescription(homeworkDetails);
        }
        throw new ArgumentException("Exception HomeworkDescriptionConverter Object Must Be A Homeworkd.Detail");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();

    public static string ParseDescription(HomeworkDetails details)
    {
        var MC11 = StripHTML(details.MC11);
        var AttachmentName = details.AttachmentName;

        var att_prefix = "附件：";

        if (MC11 == null)
        {
            return att_prefix + AttachmentName;
        }
        else if (AttachmentName == null)
        {
            return MC11;
        }
        else
        {
            return MC11 + "\n\n" + att_prefix + AttachmentName;
        }
    }

    public static string StripHTML(string input)
    {
        var res = Regex.Replace(input, @"&\w+;", string.Empty); //去除&nbsp;等实体字符
        res = Regex.Replace(res, "<.*?>", string.Empty); //去除HTML标签
        res = res.Replace("\n", "\n\n"); //将单换行转变为多换行
        return res;
    }
}