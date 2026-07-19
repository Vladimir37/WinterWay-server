using WinterWay.Enums;
using WinterWay.Models.Database.Planner;

namespace WinterWay.Services
{
    public class DistributionService
    {
        private const int CustomDayThreshold = 2;
        private const int CustomWeekThreshold = 8;
        private const int CustomMonthThreshold = 35;

        public DistributionScale GetAllowedScales(RollType rollType, int rollDays)
        {
            switch (rollType)
            {
                case RollType.Week:
                    return DistributionScale.Day;
                case RollType.Month:
                    return DistributionScale.Day | DistributionScale.Week;
                case RollType.Year:
                    return DistributionScale.Day | DistributionScale.Week | DistributionScale.Month;
                case RollType.Custom:
                    var custom = DistributionScale.None;
                    if (rollDays >= CustomDayThreshold)
                    {
                        custom |= DistributionScale.Day;
                    }
                    if (rollDays >= CustomWeekThreshold)
                    {
                        custom |= DistributionScale.Week;
                    }
                    if (rollDays >= CustomMonthThreshold)
                    {
                        custom |= DistributionScale.Month;
                    }
                    return custom;
                default:
                    return DistributionScale.None;
            }
        }

        public bool ValidateModes(DistributionScale requested, RollType rollType, int rollDays)
        {
            var allowed = GetAllowedScales(rollType, rollDays);
            return (requested & ~allowed) == DistributionScale.None;
        }

        public bool ValidatePlan(BoardModel board, SprintModel sprint, DistributionScale scale, DateOnly date)
        {
            if (scale != DistributionScale.Day && scale != DistributionScale.Week && scale != DistributionScale.Month)
            {
                return false;
            }
            if ((board.DistributionModes & scale) != scale)
            {
                return false;
            }
            if (sprint.ExpirationDate == null)
            {
                return false;
            }
            var startDate = DateOnly.FromDateTime(sprint.CreationDate);
            var endDate = DateOnly.FromDateTime(sprint.ExpirationDate.Value);
            return date >= startDate && date < endDate;
        }
    }
}
