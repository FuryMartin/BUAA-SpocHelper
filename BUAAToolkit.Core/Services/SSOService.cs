using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using BUAAToolkit.Core.Contracts.Services;
using BUAAToolkit.Core.Models;

namespace BUAAToolkit.Core.Services;
public class SSOService : ISSOService
{
    private static readonly CookieContainer cookieContainer = new();

    private static readonly HttpClientHandler httpClientHandler = new()
    {
        AllowAutoRedirect = false,
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
    };

    public static readonly HttpClient client = new(httpClientHandler);

    public void InitializeClient()
    {
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36");
        client.DefaultRequestHeaders.Add("Accecpt-Encoding", "gzip, deflate, br");
        client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
        client.DefaultRequestHeaders.Add("Connection", "keep-alive");
        client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
        client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
        client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");

        httpClientHandler.CookieContainer = cookieContainer;
    }

    public async Task<bool> SSOLoginAsync()
    {
        var username = Account.Username;
        var password = Account.Password;

        // 请求sso Token
        try
        {
            var response = await client.GetAsync("https://spoc.buaa.edu.cn/spoc/moocMainIndex/spocWelcome");
            var responseText = await response.Content.ReadAsStringAsync();
            response = await client.GetAsync(response.Headers.Location);
            responseText = await response.Content.ReadAsStringAsync();

            Debug.WriteLine(response.Headers);

            // 正则表达式查找 Token
            var pattern = @"<input name=""execution"" value=""(.+?)""/>";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(responseText);
            var csrfToken = match.Groups[1].Value;

            // 登录
            var values = new Dictionary<string, string>
            {
                { "username", username},
                { "password", password},
                { "submit", "登录"},
                { "type", "username_password"},
                { "execution", csrfToken},
                { "_eventId", "submit"}
            };
            var content = new FormUrlEncodedContent(values);
            response = await client.PostAsync("https://sso.buaa.edu.cn/login?", content);
            response = await client.GetAsync(response.Headers.Location);
            response = await client.GetAsync(response.Headers.Location);
            Debug.WriteLine(response.Headers);
            Debug.WriteLine("Login Successful");
            return true;
        }
        catch (Exception ex) {
            Debug.Write(ex.StackTrace, ex.Message); 
            return false;
        }
    }

    public HttpClient GetHttpClient() => client;
}
