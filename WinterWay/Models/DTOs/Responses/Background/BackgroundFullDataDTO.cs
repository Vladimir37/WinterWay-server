namespace WinterWay.Models.DTOs.Responses.Background
{
    public class BackgroundFullDataDTO : BackgroundResponseDTO
    {
        public string ServerURL { get; set; }

        public BackgroundFullDataDTO(BackgroundResponseDTO backgroundResponse, string serverURL)
        {
            AppName = backgroundResponse.AppName;
            Dir = backgroundResponse.Dir;
            Extension = backgroundResponse.Extension;
            Count = backgroundResponse.Count;
            ServerURL = serverURL;
        }
    }
}