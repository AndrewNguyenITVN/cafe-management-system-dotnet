namespace CafeManagement.Models.Domain;

public class PurchaseOrder
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public int? SupplierId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public string Status { get; set; } = "Draft";  // Draft | Confirmed | Received

    public Store Store { get; set; } = null!;
    public Supplier? Supplier { get; set; }
    public ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();
}
