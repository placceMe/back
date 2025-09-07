namespace Marketplace.Contracts.Constants;

/// <summary>
/// Product state constants
/// </summary>
public static class ProductState
{
    public const string Draft = "draft";
    public const string Moderation = "moderation";
    public const string Active = "active";
    public const string Inactive = "inactive";
    public const string Rejected = "rejected";
    public const string Archived = "archived";
    
    public static readonly string[] AllStates = 
    {
        Draft, Moderation, Active, Inactive, Rejected, Archived
    };
}

/// <summary>
/// Order status constants
/// </summary>
public static class OrderStatus
{
    public const string Pending = "pending";
    public const string Confirmed = "confirmed";
    public const string Processing = "processing";
    public const string Shipped = "shipped";
    public const string Delivered = "delivered";
    public const string Cancelled = "cancelled";
    public const string Returned = "returned";
    
    public static readonly string[] AllStatuses = 
    {
        Pending, Confirmed, Processing, Shipped, Delivered, Cancelled, Returned
    };
}

/// <summary>
/// User role constants
/// </summary>
public static class UserRoles
{
    public const string Admin = "admin";
    public const string Moderator = "moderator";
    public const string Seller = "seller";
    public const string Buyer = "buyer";
    
    public static readonly string[] AllRoles = 
    {
        Admin, Moderator, Seller, Buyer
    };
}

/// <summary>
/// File type constants
/// </summary>
public static class FileTypes
{
    public static readonly string[] AllowedImageTypes = 
    {
        "image/jpeg", "image/jpg", "image/png", "image/webp"
    };
    
    public const int MaxImageSizeMB = 5;
    public const int MaxImagesPerProduct = 10;
}