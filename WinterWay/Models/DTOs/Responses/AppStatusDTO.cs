namespace WinterWay.Models.DTOs.Responses
{
    public class AppStatusDTO
    {
        public string AppName { get; }
        public string AppVersion { get; }
        public bool RegistraionIsPossible { get; set; }
        public bool RegistrationIsAvailable { get; set; }
        public AppStatusDTO(bool registraionIsPossible, bool registrationIsAvailable, string name, string version)
        {
            RegistraionIsPossible = registraionIsPossible;
            RegistrationIsAvailable = registrationIsAvailable;
            AppName = name;
            AppVersion = version;
        }
    }
}
