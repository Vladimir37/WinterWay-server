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

        public async Task<TimerSessionModel> StartTimer(TimerModel timerModel)
        {
            var newTimerSession = new TimerSessionModel
            {
                CreationDate = DateTime.UtcNow,
                StopDate = null,
                Active = true,
                Timer = timerModel,
            };
            _db.TimerSessions.Add(newTimerSession);
            await _db.SaveChangesAsync();
            return newTimerSession;
        }

        public async Task StopTimer(TimerModel timerModel)
        {
            _db.Entry(timerModel).Collection(t => t.TimerSessions).Load();

            var targetTimerSession = timerModel.TimerSessions.Where(ts => ts.Active).FirstOrDefault();
            if (targetTimerSession != null)
            {
                targetTimerSession.StopDate = DateTime.UtcNow;
                targetTimerSession.Active = false;
                await _db.SaveChangesAsync();
            }
        }
    }
}
