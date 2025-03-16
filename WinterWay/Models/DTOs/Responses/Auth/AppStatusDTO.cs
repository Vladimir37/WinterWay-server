namespace WinterWay.Models.DTOs.Responses.Auth
{
    public class AppStatusDTO
    {
        public string AppName { get; }
        public string AppVersion { get; }
        public bool RegistrationIsPossible { get; set; }
        public bool RegistrationIsAvailable { get; set; }
        public bool ImportIsAvailable { get; set; }
        public AppStatusDTO(bool registrationIsPossible, bool registrationIsAvailable, bool importIsAvailable, string name, string version)
        {
            RegistrationIsPossible = registrationIsPossible;
            RegistrationIsAvailable = registrationIsAvailable;
            ImportIsAvailable = importIsAvailable;
            AppName = name;
            AppVersion = version;
        }
    }
}
