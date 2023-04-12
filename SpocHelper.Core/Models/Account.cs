namespace SpocHelper.Core.Models;
public static class Account
{
    public static string Username 
    {
        get; 
        set; 
    }

    public static string Password
    {
        get; set;
    }

    public static readonly string TestUsername = "00000000";
    public static readonly string TestPassword = "TESTACCOUNT";

    public static bool IsTestAccount()
    {
        if (Username == TestUsername)
        {
            return true;
        }
        return false;
    }
}
