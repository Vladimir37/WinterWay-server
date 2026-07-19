using System.ComponentModel.DataAnnotations;
using WinterWay.Enums;

namespace WinterWay.Models.DTOs.Requests.Planner
{
    public class SetTaskPlanDTO
    {
        [Required]
        public int TaskId { get; set; }
        public DistributionScale? PlannedScale { get; set; }
        public string? PlannedDate { get; set; }
    }
}
