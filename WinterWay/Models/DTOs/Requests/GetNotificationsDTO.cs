using System.ComponentModel.DataAnnotations;

namespace WinterWay.Models.DTOs.Requests
{
    public class GetNotificationsDTO
    {
        [Range(0, int.MaxValue)]
        public int? Count { get; set; }
        public bool? Read { get; set; }
    }
}