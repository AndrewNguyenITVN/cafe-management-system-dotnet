namespace CafeManagement.Models.Domain;

public class Topping
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<OrderDetailTopping> OrderDetailToppings { get; set; } = new List<OrderDetailTopping>();
}
