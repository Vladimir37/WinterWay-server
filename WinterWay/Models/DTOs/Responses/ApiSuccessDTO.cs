namespace WinterWay.Models.DTOs.Responses;

public class ApiSuccessDTO
{
    public bool Success { get; set; } = true;
    public string Operation { get; set; }

    public ApiSuccessDTO(string operation)
    {
        Operation = operation;
    }
}