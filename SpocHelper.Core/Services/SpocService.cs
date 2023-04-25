using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpocHelper.Core.Contracts.Services;
using SpocHelper.Core.Models;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using SpocHelper.Core.Helpers;

namespace SpocHelper.Core.Services;
public class SpocService : ISpocService
{
    private HttpClient client;
    public JObject CourseListJson { get; set; }
    public static List<Course> CourseList { get; set; }

    public string StudentID;


    public SpocService()
    {
        client = SSOService.GetHttpClient();
    }


    public async Task<bool> IsConnected()
    {
        try
        {
            var response = await client.GetAsync("https://spoc.buaa.edu.cn/spoc/moocMainIndex/spocWelcome");
            if (response.Headers.Location == null)
            {
                return true;
            }

            return false;
        }
        catch (Exception ex) { Debug.WriteLine(ex.Message); Debug.WriteLine("Exception At IsConnected"); return false; }
    }

    public async Task EnsureConnected()
    {
        var isConnected = await IsConnected();
        var count = 0;
        while(!isConnected)
        {
            if(count >= 3)
            {
                throw new Exception("Login Error");
            }
            await SSOService.SSOLoginAsync();
            isConnected = await IsConnected();
            count++;
        }
    }

    public async Task GetCourseListAsync()
    {
        client = SSOService.GetHttpClient();
        await EnsureConnected();
        var data = new { kcmcTab = "", xnxq = "", sfzjkc = 0 };
        var httpContent = new StringContent(JsonConvert.SerializeObject(data));
        var response = await client.PostAsync("https://spoc.buaa.edu.cn/spoc/rdmooc/GetIndividualClassList.do", httpContent);
        var responseText = await response.Content.ReadAsStringAsync();
        CourseListJson = JObject.Parse(responseText);
        CourseList = JsonConvert.DeserializeObject<List<Course>>(CourseListJson["result"].ToString());

        //var stopwatch = Stopwatch.StartNew();
        //stopwatch.Start();
        await GetHomeworkList();
        //stopwatch.Stop();
        //Debug.WriteLine("GetHomeworkList: " + stopwatch.ElapsedMilliseconds + " ms");
        
        //stopwatch.Reset();
        //stopwatch.Start();
        await GetCourseFileList();
        //stopwatch.Stop();
        //Debug.WriteLine("GetCourseFile: " + stopwatch.ElapsedMilliseconds + " ms");
    }

    public async Task GetHomeworkList()
    {
        List<Task> tasks = new List<Task>();
        foreach (var course in CourseList)
        {
            tasks.Add(Task.Run(async () =>
            {
                var values = new Dictionary<string, string>
                {
                    { "kcdm", course.Id},
                    { "bjdm", course.Bjdm}
                };
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("https://spoc.buaa.edu.cn/spoc/mooczygl/queryZylmList_xs", content);
                var responseText = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(responseText);
                course.HomeworkList = JsonConvert.DeserializeObject<List<Homework>>(obj["result"].ToString());

            }));
        }
        await Task.WhenAll(tasks);
        
    }

    public async Task<IEnumerable<Course>> GetUndoneHomeworkList()
    {
        await GetCourseListAsync();
        var res = CourseList.Where(course => course.UnSubmitedCount > 0)
                               .Select(course => {
                                   course.HomeworkList = course.HomeworkList.Where(homework => homework.Details.Count > 0).ToList();
                                   return course;
                               })
                               .ToList();
        if (res.Count() > 0)
        {
            StudentID = res[0].HomeworkList[0].StudentID;
        }
        return res;
    }

    public async Task<string> DownloadAttachment(string AttachmentName, string cclj, string DowloadDir, IProgress<int> progress)
    {
        var fileName = Path.GetFileName(AttachmentName).ToLower(); // 将文件名中的后缀名部分转换为小写
        var filePath = Path.Combine(DowloadDir, fileName);

        if (!File.Exists(filePath))
        {
            var downloadUrl = "https://spoc.buaa.edu.cn/spoc/moocwdkc/downloadTask.do";
            var encodedFJMC = Convert.ToBase64String(Encoding.ASCII.GetBytes(Uri.EscapeDataString(AttachmentName)));
            var encodedCCLJ = Convert.ToBase64String(Encoding.ASCII.GetBytes(cclj));

            var requestUrl = $"{downloadUrl}?fjmc={encodedFJMC}&cclj={encodedCCLJ}";

            var response = await client.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead);
            if (response.IsSuccessStatusCode)
            {
                var contentLength = response.Content.Headers.ContentLength; 
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                {
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        var buffer = new byte[4096];
                        var totalBytesRead = 0L;
                        var bytesRead = 0;
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;

                            if (contentLength.HasValue)
                            {
                                var percentage = (int)Math.Round((double)totalBytesRead / contentLength.Value * 100);
                                progress?.Report(percentage);
                            }
                        }
                    }
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

    public async Task<IEnumerable<Course>> GetCourseFileList()
    {
        List<Task> tasks = new ();
        foreach (var course in CourseList)
        {
            tasks.Add(Task.Run(async () =>
            {
                var contentDictionary = new Dictionary<string, string>
                {
                    {"kcdm",  course.Id },
                    {"zlmcTab", string.Empty},
                };
                var urlSubmit = "https://spoc.buaa.edu.cn/spoc/courseGroup/queryCourseZlList";
                var content = new FormUrlEncodedContent(contentDictionary);
                var response = await client.PostAsync(urlSubmit, content);
                var responseText = await response?.Content.ReadAsStringAsync();
                var obj = JObject.Parse(responseText);
                course.CourseFiles = JsonConvert.DeserializeObject<List<CourseFile>>(obj["result"].ToString());
                course.CourseFiles.RemoveAll(courseFile => !course.TeacherName.Contains(courseFile.Creator));
                course.CourseFiles = course.CourseFiles.GroupBy(x => x.FileName).Select(x => x.First()).OrderBy(x => x.FileName).ThenBy(x => x.ClassTime).ToList();
            }));
        }
        await Task.WhenAll(tasks);
        var res = CourseList.Where(course => course.CourseFiles.Count > 0).ToList();

        //foreach (var course in res)
        //{
        //    if (course.CourseFiles.Count > 0)
        //    {
        //        Debug.WriteLine($"{course.Name}:{course.CourseFiles.Count}");
        //        //continue;
        //    }
        //    foreach (var courseFile in course.CourseFiles)
        //    {
        //        Debug.WriteLine($"Week {courseFile.Week} {courseFile.FileName} {courseFile.CreateDate}");
        //    }
        //}

        return res;
    }

}


