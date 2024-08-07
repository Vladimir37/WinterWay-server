namespace WinterWay.Models.DTOs.Responses
{
    public class RegStatusDTO
    {
        public bool RegistraionIsPossible { get; set; }
        public bool RegistrationIsAvailable { get; set; }
        public RegStatusDTO(bool registraionIsPossible, bool registrationIsAvailable)
        {
            RegistraionIsPossible = registraionIsPossible;
            RegistrationIsAvailable = registrationIsAvailable;
        }
    }
}
