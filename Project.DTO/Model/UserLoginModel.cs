namespace Project.DTO.Model
{
    public class UserLoginModel
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? AccessToken { get; set; }
    }


    public class UserResetPassword
    {
        public string UserName { get; set; }
    }


    public class UserChangePassword
    {
        public Guid UserId { get; set; }
        public string NewPassword { get; set; }
        public string OldPassword { get; set; }
    }
}
