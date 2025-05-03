using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests.DataBackup;

public class ImportUserDTO
{
    [Required]
    public string User { get; set; }
}