namespace WeatherSeaWebApplication.Models
{
    public class UserModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public UserModel(int userId, string username, string password)
        {
            UserId = userId;
            Username = username;
            Password = password;
        }   
    }
}
