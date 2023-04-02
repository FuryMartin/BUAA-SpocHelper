using BUAAToolkit.Core.Models;

namespace BUAAToolkit.Core.Contracts.Services;
public interface ISSOService
{
    HttpClient GetHttpClient();
    Task<bool> SSOLoginAsync();
}
