using CafeManagement.Models.ViewModels;

namespace CafeManagement.Services;

public interface IScheduleService
{
    // Lấy lịch làm việc theo tuần
    Task<ScheduleWeekViewModel> GetWeeklyScheduleAsync(int storeId, DateOnly weekStart);

    // Lên lịch: phân công nhân viên vào ca
    Task<bool> AssignShiftAsync(int storeId, DateOnly date, int shiftId, List<string> userIds);

    // Chấm công: tick/bỏ tick nhân viên đã đi làm trong ca → ghi vào Timekeeping
    Task<bool> AttendShiftAsync(int storeId, DateOnly date, int shiftId, List<string> attendedUserIds);

    Task<List<StoreSelectItem>> GetStoresAsync();
    Task<List<UserSelectItem>> GetUsersByStoreAsync(int storeId);
}