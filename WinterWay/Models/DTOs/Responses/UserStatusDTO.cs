using WinterWay.Enums;

namespace WinterWay.Models.DTOs.Responses
{
    public class UserStatusDTO
    {
        public string ID { get; set; }
        public string Username { get; set; }
        public ThemeType ThemeType { get; set; }
        public bool DefaultAutocomplete { get; set; }
        public UserStatusDTO(string id, string username, ThemeType themeType, bool defaultAutocomplete)
        {
            ID = id; 
            Username = username; 
            ThemeType = themeType; 
            DefaultAutocomplete = defaultAutocomplete;
        }
    }
}
