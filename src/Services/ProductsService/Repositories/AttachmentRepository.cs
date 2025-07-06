using Microsoft.EntityFrameworkCore;
using ProductsService.Data;
using ProductsService.Models;
using ProductsService.Repositories.Interfaces;

namespace ProductsService.Repositories;

public class AttachmentRepository : IAttachmentRepository
{
    private readonly ProductsDBContext _context;

    public AttachmentRepository(ProductsDBContext context)
    {
        _context = context;
    }

    public async Task<Attachment?> GetByIdAsync(Guid id)
    {
        return await _context.Attachments
            .Include(a => a.Product)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Attachment>> GetByProductIdAsync(Guid productId)
    {
        return await _context.Attachments
            .Where(a => a.ProductId == productId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Attachment>> GetAllAsync()
    {
        return await _context.Attachments
            .Include(a => a.Product)
            .ToListAsync();
    }

    public async Task<Attachment> CreateAsync(Attachment attachment)
    {
        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync();
        return attachment;
    }

    public async Task<Attachment> UpdateAsync(Attachment attachment)
    {
        _context.Attachments.Update(attachment);
        await _context.SaveChangesAsync();
        return attachment;
    }

    public async Task DeleteAsync(Guid id)
    {
        var attachment = await _context.Attachments.FindAsync(id);
        if (attachment != null)
        {
            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync();
        }
    }
}