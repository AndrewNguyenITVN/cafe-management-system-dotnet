using CafeManagement.Data;
using CafeManagement.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Services
{
    public class ScheduleService
    {
        private readonly AppDbContext _context;

        public ScheduleService(AppDbContext context)
        {
            _context = context;
        }

        // ===============================
        // GET ALL SCHEDULES
        // ===============================
        public async Task<List<Schedule>> GetAllAsync()
        {
            return await _context.Schedules
                .Include(s => s.User)
                .Include(s => s.Shift)
                .Include(s => s.Store)
                .ToListAsync();
        }

        // ===============================
        // GET SCHEDULE BY ID
        // ===============================
        public async Task<Schedule?> GetByIdAsync(int id)
        {
            return await _context.Schedules
                .Include(s => s.User)
                .Include(s => s.Shift)
                .Include(s => s.Store)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        // ===============================
        // CREATE SCHEDULE
        // ===============================
        public async Task CreateAsync(Schedule schedule)
        {
            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();
        }

        // ===============================
        // UPDATE SCHEDULE
        // ===============================
        public async Task UpdateAsync(Schedule schedule)
        {
            _context.Schedules.Update(schedule);
            await _context.SaveChangesAsync();
        }

        // ===============================
        // DELETE SCHEDULE
        // ===============================
        public async Task DeleteAsync(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);

            if (schedule != null)
            {
                _context.Schedules.Remove(schedule);
                await _context.SaveChangesAsync();
            }
        }

        // ===============================
        // GET SHIFTS
        // ===============================
        public async Task<List<Shift>> GetShiftsAsync()
        {
            return await _context.Shifts.ToListAsync();
        }

        // ===============================
        // GET USERS
        // ===============================
        public async Task<List<AppUser>> GetUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        // ===============================
        // GET STORES
        // ===============================
        public async Task<List<Store>> GetStoresAsync()
        {
            return await _context.Stores.ToListAsync();
        }
    }
}