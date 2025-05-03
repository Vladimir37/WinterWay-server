namespace WinterWay.Models.DTOs.Responses.Shared
{
    public class ApiSuccessDTO
    {
        public bool Success { get; set; } = true;
        public string Operation { get; set; }
        public string Info { get; set; }
    
        public ApiSuccessDTO(string operation, string info = "")
        {
            Operation = operation;
            Info = info;
        }
    }
}