using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpocHelper.Core.Contracts.Services;
using SpocHelper.Core.Models;
using System.Text;
using System.Net.Http;
using System.Web;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using System.IO;
using SpocHelper.Core.Helpers;

namespace SpocHelper.Core.Services;
public class SpocService : ISpocService
{
    private HttpClient client;
    public JObject CourseListJson { get; set; }
    public List<Course> CourseList { get; set; }

    public string StudentID;


    public SpocService()
    {
        client = SSOService.GetHttpClient();
    }


    public async Task<bool> IsConnected()
    {
        Debug.WriteLine("Moving into IsConnected");
        try
        {
            var response = await client.GetAsync("https://spoc.buaa.edu.cn/spoc/moocMainIndex/spocWelcome");
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
            await SSOService.SSOLoginAsync();
        }
        var data = new { kcmcTab = "", xnxq = "", sfzjkc = 0 };
        var httpContent = new StringContent(JsonConvert.SerializeObject(data));
        var response = await client.PostAsync("https://spoc.buaa.edu.cn/spoc/rdmooc/GetIndividualClassList.do", httpContent);
        Debug.WriteLine("Course Get");
        var responseText = await response.Content.ReadAsStringAsync();
        CourseListJson = JObject.Parse(responseText);
        CourseList = JsonConvert.DeserializeObject<List<Course>>(CourseListJson["result"].ToString());
        
        await GetHomeworkList();
        if (CourseList.Count() > 0)
        {
            StudentID = CourseList[0].HomeworkList[0].StudentID;
        }

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
        CourseList.RemoveAll(course => course.UnSubmitedCount == 0);
        Debug.WriteLine("Break");
        for (var i = 0; i < CourseList.Count; i++)
        {
            CourseList[i].HomeworkList.RemoveAll(homework => homework.Details.Count() == 0);
        }
    }

    public async Task<string> DownloadAttachment(string AttachmentName, string cclj, string DowloadDir)
    {
        var fileName = Path.GetFileName(AttachmentName).ToLower(); // 将文件名中的后缀名部分转换为小写
        var filePath = Path.Combine(DowloadDir, fileName);

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

    public async Task UploadFile(string filePath, string CourseID)
    {
        var uploadURL = "https://doc.spoc.buaa.edu.cn:18919/fileManager/fileManagerSystem/uploadFile";
        var chunkSize = 5242880;
        var fileinfo = new FileInfo(filePath);
        var totalSize = fileinfo.Length;
        var currentChunkSize = (totalSize > chunkSize && totalSize < 2*chunkSize)? totalSize : chunkSize;
        var totalChunks = (int)Math.Ceiling((double)totalSize / chunkSize);

        var queryDictionary = new Dictionary<string, string>
        {
            {"chunkNumber", "1"},
            {"ChunkSize", "5242880" },
            {"currentChunkSize", currentChunkSize.ToString() },
            {"totalSize", totalSize.ToString() },
            {"identifier", MD5Helper.GetFileMD5(filePath) },
            {"filename", Path.GetFileName(filePath) },
            {"reletivePath", Path.GetFileName(filePath) },
            {"totalChunks", totalChunks.ToString() },
            {"maxSize", "300" },
            {"czxt", "kcjxpt" },
            {"czmk", "作业管理" },
            {"czr", StudentID },
            {"cclj", CourseID }
        };
        var queryURL = QueryHelpers.AddQueryString(uploadURL, queryDictionary);

        var response = await client.GetAsync(queryURL);
        var responseText = await response.Content.ReadAsStringAsync();
        var fileExisted = (bool)JObject.Parse(responseText)["mc"];
        Debug.WriteLine(queryURL);
        Debug.WriteLine(responseText);
        if (fileExisted)
        {
            Debug.WriteLine("File Existed");
        }
        else
        {
            Debug.WriteLine("File UnExisted");
            var success = await StreamUpload(filePath, uploadURL, queryDictionary);
            if (success)
            {
                await MergeUpload(queryDictionary);
            }
        }

    }

    public async Task<bool> StreamUpload(string filePath, string uploadURL, Dictionary<string, string> queryDictionary)
    {
        var stream = File.OpenRead(filePath);
        var ChunkSize = int.Parse(queryDictionary["ChunkSize"]);
        var buffer = new byte[ChunkSize];
        int bytesRead;
        var chunkNumber = 0;
        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            chunkNumber++;
            queryDictionary["currentChunkSize"] = bytesRead.ToString();

            var content = new MultipartFormDataContent();
            foreach (var kvp in queryDictionary)
            {
                content.Add(new StringContent(kvp.Value), kvp.Key);
            }
            content.Add(new ByteArrayContent(buffer, 0, bytesRead), "file");
            var response = await client.PostAsync(uploadURL, content);
            response.EnsureSuccessStatusCode();
            Debug.WriteLine($"Chunk {chunkNumber} uploaded successfully!");
        }
        Debug.WriteLine("File uploaded successfully!");
        return true;
    }

    public async Task<bool> MergeUpload(Dictionary<string, string> queryDictionary)
    {
        var urlMerge = "https://doc.spoc.buaa.edu.cn:18919/fileManager/fileManagerSystem/mergeFile";
        var mergeQueryDictionary = new Dictionary<string, string>
        {
            { "identifier", queryDictionary["identifier"]},
            {"filename", queryDictionary["filename"] },
            {"totalSize", queryDictionary["totalSize"] },
            {"czxt",queryDictionary["czxt"] },
            {"czmk",queryDictionary["czmk"] },
            {"czr",queryDictionary["czr"] },
            {"cclj", queryDictionary["cclj"]}
        };
        var content = new FormUrlEncodedContent(mergeQueryDictionary);
        var response = await client.PostAsync(urlMerge, content);
        var responseText = await response.Content.ReadAsStringAsync();
        Debug.WriteLine(responseText);
        return true;
    }

    public async Task<bool> SubmitHomework(HomeworkDetails detail)
    {
        var zyfjFileName = Path.GetFileName(detail.FilePathToUpload);
        var zyfjPath = $"{detail.kcdm}/{MD5Helper.GetFileMD5(detail.FilePathToUpload)}{Path.GetExtension(zyfjFileName)}"; 
        var contentDictionary = new Dictionary<string, string>
        {
            {"zjdm",  detail.zjdm },
            {"kcdm",  detail.kcdm },
            {"zynrdm", detail.kcnr },
            {"zynr", null },
            {"zyfjPath", zyfjPath },
            {"zyfjFileName", zyfjFileName },
            {"zyzt", "1" }
        };
        var urlSubmit = "https://spoc.buaa.edu.cn/spoc/moocxsxx/saveFjZy.do";
        var content = new FormUrlEncodedContent (contentDictionary);
        var response = await client.PostAsync(urlSubmit, content);
        var responseText = await response?.Content.ReadAsStringAsync();
        Debug.WriteLine(responseText);

        return true;
    }
}


