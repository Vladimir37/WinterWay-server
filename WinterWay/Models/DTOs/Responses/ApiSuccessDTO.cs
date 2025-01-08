namespace WinterWay.Models.DTOs.Responses;

public class ApiSuccessDTO
{
    public bool Success = true;
    public string Operation;

    public ApiSuccessDTO(string operation)
    {
        Operation = operation;
    }
}