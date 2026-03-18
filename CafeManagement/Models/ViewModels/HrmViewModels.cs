namespace CafeManagement.Models.ViewModels;

// ── Dùng chung cho Schedule + Payroll ─────────────────────

public class StoreSelectItem
{
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
}

public class UserSelectItem
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

// ── Schedule ──────────────────────────────────────────────

public class ShiftInfo
{
    public int ShiftId { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public string TimeRange { get; set; } = string.Empty;
}

public class ScheduleCellViewModel
{
    public DateOnly Date { get; set; }
    public int ShiftId { get; set; }
    public List<string> UserIds { get; set; } = new();
    public List<string> UserNames { get; set; } = new();
}

public class ScheduleWeekViewModel
{
    public int SelectedStoreId { get; set; }
    public DateOnly WeekStart { get; set; }
    public DateOnly WeekEnd => WeekStart.AddDays(6);

    public List<DateOnly> WeekDays { get; set; } = new();
    public List<ShiftInfo> Shifts { get; set; } = new();

    // Key: "yyyy-MM-dd_shiftId"
    public Dictionary<string, ScheduleCellViewModel> Cells { get; set; } = new();

    public List<StoreSelectItem> Stores { get; set; } = new();
    public List<UserSelectItem> AllUsers { get; set; } = new();

    public string CellKey(DateOnly date, int shiftId) =>
        $"{date:yyyy-MM-dd}_{shiftId}";

    public ScheduleCellViewModel GetCell(DateOnly date, int shiftId)
    {
        var key = CellKey(date, shiftId);
        return Cells.TryGetValue(key, out var cell) ? cell : new ScheduleCellViewModel();
    }
}

// ── Payroll ───────────────────────────────────────────────

public class PayrollRowViewModel
{
    public int RowNumber { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string JobPosition { get; set; } = string.Empty;
    public decimal TotalHours { get; set; }
    public decimal AverageHourlyRate { get; set; }
    public decimal TotalSalary { get; set; }
}

public class PayrollViewModel
{
    public int? SelectedStoreId { get; set; }
    // Dùng string để binding form date dễ hơn, convert trong controller
    public DateOnly FromDate { get; set; } = DateOnly.FromDateTime(
        new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
    public DateOnly ToDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public List<PayrollRowViewModel> Rows { get; set; } = new();
    public List<StoreSelectItem> Stores { get; set; } = new();

    public decimal GrandTotalHours => Rows.Sum(r => r.TotalHours);
    public decimal GrandTotalSalary => Rows.Sum(r => r.TotalSalary);
}
