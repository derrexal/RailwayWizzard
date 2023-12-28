namespace RzdHack_Robot.Core;

public class LoginDetails
{
    public string Username { get; set; }
    public string Password { get; set; }

    public LoginDetails(string userName, string password)
    {
        Username = userName;
        Password = password;
    }

    public LoginDetails(){}
}