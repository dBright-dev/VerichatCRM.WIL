using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Interfaces;
using VerichatCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace VerichatCRM.API.Services
{
    public partial class MobileService : IMobileService
    {
        private static readonly string[] WeeklyLabels = { "M", "T", "W", "T", "F", "S", "S" };
        private static readonly string[] KnownGoalTypes = { "steps", "water", "sleep", "exercise" };

        private readonly AppDbContext _db;
        private readonly IUserRepository _users;

        public MobileService(AppDbContext db, IUserRepository users)
        {
            _db = db;
            _users = users;
        }

        private static DateTime WeekStartUtc(DateTime utc)
        {
            var date = utc.Date;
            var delta = ((int)date.DayOfWeek + 6) % 7; // Monday = 0
            return date.AddDays(-delta);
        }
    }
}
