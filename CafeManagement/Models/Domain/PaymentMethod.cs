namespace CafeManagement.Models.Domain;

public class PaymentMethod
{
    public int Id { get; set; }
    public string MethodName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
