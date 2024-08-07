using WinterWay.Enums;

namespace WinterWay.Models.DTOs.Error
{
    public class ApiError
    {
        public InnerErrors InnerCode { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public List<string>? Fields { get; set; }
        public string? Info { get; set; } = string.Empty;
        public ApiError(InnerErrors innerCode, string errorMessage, List<string>? fields = null, string? info = null)
        {
            InnerCode = innerCode;
            ErrorMessage = errorMessage;
            Fields = fields;
            Info = info;
        }
    }
}
