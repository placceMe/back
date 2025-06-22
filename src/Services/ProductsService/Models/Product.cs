using System.ComponentModel.DataAnnotations.Schema;
namespace ProductsService.Models;

public class Product
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public uint Price { get; set; }
    public Guid CategoryId { get; set; }
    public Guid SellerId { get; set; }
    public string State { get; set; } = ProductState.Moderation;
    public Category Category { get; set; }
    public uint Quantity { get; set; }
    public List<Characteristic> Characteristics { get; set; } = new List<Characteristic>();
    public List<Attachment> Attachments { get; set; } = new List<Attachment>();

}
