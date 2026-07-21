using WinterWay.Models.Database.Mood;

namespace WinterWay.Models.DTOs.Responses.Mood
{
    public class MoodDayDTO
    {
        public DateOnly Date { get; set; }
        public List<MoodRecordModel> Good { get; set; }
        public List<MoodRecordModel> Bad { get; set; }

        public MoodDayDTO(DateOnly date, List<MoodRecordModel> good, List<MoodRecordModel> bad)
        {
            Date = date;
            Good = good;
            Bad = bad;
        }
    }
}
