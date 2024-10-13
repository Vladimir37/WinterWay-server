using WinterWay.Data;
using WinterWay.Models.Database;

namespace WinterWay.Services
{
    public class TimerService
    {
        private readonly ApplicationContext _db;

        public TimerService(ApplicationContext db)
        {
            _db = db;
        }

        public TimerSessionModel StartTimer(TimerModel timerModel)
        {
            var newTimerSession = new TimerSessionModel
            {
                CreationDate = DateTime.UtcNow,
                StopDate = null,
                Active = true,
                Timer = timerModel,
            };
            _db.TimerSessions.Add(newTimerSession);
            _db.SaveChanges();
            return newTimerSession;
        }

        public void StopTimer(TimerModel timerModel)
        {
            _db.Entry(timerModel).Collection(t => t.TimerSessions).Load();

            var targetTimerSession = timerModel.TimerSessions.Where(ts => ts.Active).FirstOrDefault();
            if (targetTimerSession != null)
            {
                targetTimerSession.StopDate = DateTime.UtcNow;
                targetTimerSession.Active = false;
                _db.SaveChanges();
            }
        }
    }
}
