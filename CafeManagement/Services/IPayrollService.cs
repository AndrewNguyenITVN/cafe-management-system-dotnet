using CafeManagement.Models.ViewModels;

namespace CafeManagement.Services;

public interface IPayrollService
{
    Task<List<PayrollRowViewModel>> GetPayrollAsync(int? storeId, DateOnly fromDate, DateOnly toDate);
    Task<List<StoreSelectItem>> GetStoresAsync();
}
