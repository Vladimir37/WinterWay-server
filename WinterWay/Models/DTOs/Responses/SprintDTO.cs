using System.Text.Json.Serialization;
using WinterWay.Enums;
using WinterWay.Models.Database;

namespace WinterWay.Models.DTOs.Responses
{
    public class SprintDTO : SprintModel
    {
        public int TasksActive { get; set; }
        public int TasksDone { get; set; }

        public SprintDTO(SprintModel sprint, int tasksActive, int tasksDone)
        {
            Id = sprint.Id;
            Name = sprint.Name;
            Active = sprint.Active;
            Image = sprint.Image;
            CreationDate = sprint.CreationDate;
            ExpirationDate = sprint.ExpirationDate;
            ClosingDate = sprint.ClosingDate;
            Number = sprint.Number;
            BoardId = sprint.BoardId;
            SprintResult = sprint.SprintResult;
            Tasks = sprint.Tasks;
            ExpirationDate = sprint.ExpirationDate;
            
            TasksActive = tasksActive;
            TasksDone = tasksDone;
        }
    }

    public class BoardDTO : BoardModel
    {
        public SprintDTO ActiveSprintDTO { get; set; }

        public BoardDTO(BoardModel board, SprintDTO activeSprint)
        {
            Id = board.Id;
            Name = board.Name;
            RollType = board.RollType;
            RollStart = board.RollStart;
            RollDays = board.RollDays;
            CurrentSprintNumber = board.CurrentSprintNumber;
            Color = board.Color;
            IsBacklog = board.IsBacklog;
            Favorite = board.Favorite;
            Archived = board.Archived;
            SortOrder = board.SortOrder;
            CreationDate = board.CreationDate;

            UserId = board.UserId;
            User = board.User;
            ActualSprintId = board.ActualSprintId;
            ActualSprint = board.ActualSprint;
            AllSprints = board.AllSprints;
            AllTasks = board.AllTasks;

            ActiveSprintDTO = activeSprint;
        }
    }
}
