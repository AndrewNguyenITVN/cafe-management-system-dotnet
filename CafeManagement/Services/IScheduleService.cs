using CafeManagement.Models.ViewModels;

namespace CafeManagement.Services;

public interface IScheduleService
{
    Task<ScheduleWeekViewModel> GetWeeklyScheduleAsync(int storeId, DateOnly weekStart);
    Task<bool> AssignShiftAsync(int storeId, DateOnly date, int shiftId, List<string> userIds);
    Task<List<StoreSelectItem>> GetStoresAsync();
    Task<List<UserSelectItem>> GetUsersByStoreAsync(int storeId);
}
