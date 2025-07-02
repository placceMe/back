
using System.Data;

namespace Contracts;

public class CreateProductContract
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public uint Price { get; set; }
    public Guid CategoryId { get; set; }
    public Guid SellerId { get; set; }
    public string State { get; set; } = string.Empty;
    public uint Quantity { get; set; }
    public List<Dictionary<string, string>> Characteristics { get; set; } = new List<Dictionary<string, string>>();
    public List<Stream> Attachments { get; set; } = new List<Stream>();
}