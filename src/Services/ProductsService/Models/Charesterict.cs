namespace ProductsService.Models;

public class Characteristic
{
    public Guid Id { get; set; }
    public string Value { get; set; }
    public Guid ProductId { get; set; }
    public Guid CharesteristicDictId { get; set; }



}