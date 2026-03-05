using CafeManagement.Data;
using CafeManagement.Models.Domain;
using CafeManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Services;

public class JobPositionService
{
    private readonly AppDbContext _db;
    public JobPositionService(AppDbContext db) => _db = db;

    public Task<List<JobPosition>> GetAllAsync()
        => _db.JobPositions.OrderBy(p => p.PositionName).ToListAsync();

    public Task<List<JobPosition>> GetActiveAsync()
        => _db.JobPositions.Where(p => p.IsActive).OrderBy(p => p.PositionName).ToListAsync();

    public Task<JobPosition?> GetByIdAsync(int id)
        => _db.JobPositions.FindAsync(id).AsTask();

    public async Task CreateAsync(JobPositionViewModel model)
    {
        _db.JobPositions.Add(new JobPosition
        {
            PositionName = model.PositionName,
            HourlyRate   = model.HourlyRate
        });
        await _db.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(int id, JobPositionViewModel model)
    {
        var pos = await _db.JobPositions.FindAsync(id);
        if (pos == null) return false;
        pos.PositionName = model.PositionName;
        pos.HourlyRate   = model.HourlyRate;
        pos.IsActive     = model.IsActive;
        await _db.SaveChangesAsync();
        return true;
    }
}
