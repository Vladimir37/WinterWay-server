using WinterWay.Models.Database.Auth;

namespace WinterWay.Models.DTOs.Responses.DataBackup;

public class ExportUserDTO
{
    public string User { get; set; }

    public ExportUserDTO(string user)
    {
        User = user;
    }
}