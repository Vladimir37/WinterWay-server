namespace WinterWay.Models.DTOs.Responses
{
    public class AppStatusDTO
    {
        public string AppName { get; }
        public string AppVersion { get; }
        public bool RegistraionIsPossible { get; set; }
        public bool RegistrationIsAvailable { get; set; }
        public bool ImportIsAvailable { get; set; }
        public AppStatusDTO(bool registraionIsPossible, bool registrationIsAvailable, bool importIsAvailable, string name, string version)
        {
            RegistraionIsPossible = registraionIsPossible;
            RegistrationIsAvailable = registrationIsAvailable;
            ImportIsAvailable = importIsAvailable;
            AppName = name;
            AppVersion = version;
        }
    }
}
