using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Madbestilling.Models;

[TableName("madbestilling_orders")]
[PrimaryKey("id", AutoIncrement = true)]
[ExplicitColumns]
public class OrderRecord
{
    [Column("id")]
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }

    [Column("childName")]
    [Length(200)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string ChildName { get; set; } = string.Empty;

    [Column("childClass")]
    [Length(100)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string ChildClass { get; set; } = string.Empty;

    [Column("phone")]
    [Length(50)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Phone { get; set; } = string.Empty;

    [Column("email")]
    [Length(320)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Email { get; set; } = string.Empty;

    [Column("cartJson")]
    [SpecialDbType(SpecialDbTypes.NTEXT)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string CartJson { get; set; } = string.Empty;

    [Column("totalAmount")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public decimal TotalAmount { get; set; }

    [Column("status")]
    [Length(50)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Status { get; set; } = "ny";

    [Column("createdAt")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
