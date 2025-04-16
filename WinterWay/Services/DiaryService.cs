using Microsoft.EntityFrameworkCore;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Models.Database.Diary;
using WinterWay.Models.DTOs.Requests.Diary;
using WinterWay.Models.DTOs.Responses.Shared;

namespace WinterWay.Services
{
    public class DiaryService
    {
        private readonly ApplicationContext _db;
        private readonly DateTimeService _dateTimeService;

        public DiaryService(ApplicationContext db, DateTimeService dateTimeService)
        {
            _db = db;
            _dateTimeService = dateTimeService;
        }

        public bool Transform(
            string userId, 
            DiaryRecordDTO form, 
            bool ifDoesNotExists,
            out ApiErrorDTO? error, 
            out DiaryRecordModel? recordModel
        )
        {
            var isValidDate = CheckDate(userId, form.Date, ifDoesNotExists, out var formattedDate, out var dateValidationError);

            if (!isValidDate)
            {
                error = dateValidationError;
                recordModel = null;
                return false;
            }
            
            var emptyGroups = form.Activities
                .Where(kv => kv.Value.Count == 0)
                .Select(kv => kv.Key)
                .ToList();
            var filteredActivitiesForm = form.Activities
                .ToDictionary(
                    entry => entry.Key,
                    entry => new List<int>(entry.Value)
                );

            foreach (var emptyKey in emptyGroups)
            {
                filteredActivitiesForm.Remove(emptyKey);
            }
            
            var allGroups = _db.DiaryGroups
                .Include(dg => dg.Activities)
                .Where(dg => !dg.Archived)
                .Where(dg => dg.UserId == userId)
                .ToList();
            
            var isActivitiesValid = Validate(allGroups, filteredActivitiesForm, out var validationError);

            if (!isActivitiesValid)
            {
                error = validationError;
                recordModel = null;
                return false;
            }

            recordModel = CreateRecord(userId, formattedDate!.Value, form, filteredActivitiesForm);
            error = null;
            return true;
        }

        private bool CheckDate(
            string userId, 
            string dateString, 
            bool ifDoesNotExists,
            out DateOnly? date, 
            out ApiErrorDTO? error
        )
        {
            var validDay = _dateTimeService.ParseDate(dateString, out DateOnly targetDay);

            if (!validDay)
            {
                error = new ApiErrorDTO(InternalError.InvalidForm, "Invalid date format");
                date = null;
                return false;
            }

            var isDateAlreadyExists =  _db.DiaryRecords
                .Where(dr => dr.Date == targetDay)
                .Where(dr => dr.UserId == userId)
                .Any();

            if (ifDoesNotExists && isDateAlreadyExists)
            {
                error = new ApiErrorDTO(InternalError.InvalidForm, "A record for this day already exists in diary");
                date = null;
                return false;
            }

            if (!ifDoesNotExists && !isDateAlreadyExists)
            {
                error = new ApiErrorDTO(InternalError.InvalidForm, "A record for this day does not exists in diary");
                date = null;
                return false;
            }
            
            error = null;
            date = targetDay;
            return true;
        }

        private bool Validate(
            List<DiaryGroupModel> allGroups, 
            Dictionary<int, List<int>> userActivities, 
            out ApiErrorDTO? error
        )
        {
            var allGroupsDict = allGroups.ToDictionary(group => group.Id);
            var allActivities = allGroups.ToDictionary(
                group => group.Id,
                group => group.Activities.ToDictionary(activity => activity.Id)
            );
            
            var allRequiredGroups = allGroups
                .Where(group => !group.CanBeEmpty)
                .Select(group => group.Id)
                .ToList();
            var allSingleOptionsGroups = allGroups
                .Where(group => !group.Multiple)
                .Select(group => group.Id)
                .ToList();

            var allGroupsKeys = userActivities.Keys.ToList();
            
            var missedRequiredGroups = allRequiredGroups
                .Where(groupId => !allGroupsKeys.Contains(groupId))
                .ToList();
            var incorrectMultipleGroups = allSingleOptionsGroups
                .Where(groupId => allGroupsKeys.Contains(groupId) && userActivities[groupId].Count > 1)
                .ToList();

            if (missedRequiredGroups.Count > 0)
            {
                var missedGroupIds = missedRequiredGroups
                    .Select(num => num.ToString())
                    .ToList();
                error = new ApiErrorDTO(InternalError.InvalidForm, "Required groups are missing", missedGroupIds);
                return false;
            }

            if (incorrectMultipleGroups.Count > 0)
            {
                var incorrectGroupIds = incorrectMultipleGroups
                    .Select(num => num.ToString())
                    .ToList();
                error = new ApiErrorDTO(InternalError.InvalidForm, "Multiple values in a group with only one option", incorrectGroupIds);
                return false;
            }

            var incorrectGroups = new List<string>();
            var incorrectActivities = new List<string>();
            foreach (var groupKey in allGroupsKeys)
            {
                if (!allActivities.ContainsKey(groupKey))
                {
                    incorrectGroups.Add(groupKey.ToString());
                    continue;
                }
                foreach (var activityKey in userActivities[groupKey])
                {
                    if (!allActivities[groupKey].ContainsKey(activityKey) && !incorrectActivities.Contains(activityKey.ToString()))
                    {
                        incorrectActivities.Add(activityKey.ToString());
                    }
                }
            }

            if (incorrectGroups.Count > 0)
            {
                error = new ApiErrorDTO(InternalError.InvalidForm, "Incorrect groups", incorrectGroups);
                return false;
            }
            if (incorrectActivities.Count > 0)
            {
                error = new ApiErrorDTO(InternalError.InvalidForm, "Incorrect activities in groups", incorrectActivities);
                return false;
            }

            error = null;
            return true;
        }

        private DiaryRecordModel CreateRecord(
            string userId, 
            DateOnly recordDate,
            DiaryRecordDTO form, 
            Dictionary<int, List<int>> userActivities
        )
        {
            var newDiaryRecord = new DiaryRecordModel
            {
                Date = recordDate,
                Info = form.Info,
                UserId = userId,
                Groups = new List<DiaryRecordGroupModel>()
            };

            foreach (var groupKey in userActivities.Keys.ToList())
            {
                var newGroup = new DiaryRecordGroupModel
                {
                    DiaryGroupId = groupKey,
                    Activities = new List<DiaryRecordActivityModel>()
                };
                foreach (var activityKey in userActivities[groupKey])
                {
                    var newActivity = new DiaryRecordActivityModel
                    {
                        DiaryActivityId = activityKey,
                    };
                    newGroup.Activities.Add(newActivity);
                }
                newDiaryRecord.Groups.Add(newGroup);
            }

            return newDiaryRecord;
        }
    }
}