namespace MobileShop.Models.Entities.Base;

public abstract class BaseEntity
{
    // Id is picked up as the primary key + identity column by EF Core convention alone - no attribute needed
    public int Id { get; set; }

    // concurrency token - this one DOES need the attribute, convention won't infer it
    [Timestamp]
    public byte[]? TimeStamp { get; set; }

    public bool IsDeleted { get; set; }
}
