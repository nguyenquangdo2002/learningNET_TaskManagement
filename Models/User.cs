namespace TaskManagement.Models;

public enum Role { User, Admin }

public class User : BaseEntity
{



    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public Role Role { get; set; } = Role.User;

}




