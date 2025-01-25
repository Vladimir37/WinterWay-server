using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class GetNotificationsDTO
    {
        [Range(0, int.MaxValue)]
        public int? Count { get; set; }
        [Range(0, int.MaxValue)]
        public int? Skip { get; set; }
        public bool? Read { get; set; }
    }
}