using Microsoft.EntityFrameworkCore;

namespace Calculator.Data;

public sealed class CalculatorContext(DbContextOptions<CalculatorContext> options) : DbContext(options)
{
    public DbSet<DataInputVariant> DataInputVariants => Set<DataInputVariant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<DataInputVariant>();
        entity.ToTable("DataInputVariants");
        entity.HasKey(x => x.ID_DataInputVariant);
        entity.Property(x => x.RequestId).HasColumnType("varchar(36)").HasMaxLength(36);
        entity.HasIndex(x => x.RequestId).IsUnique();
        entity.Property(x => x.Operand_1).HasColumnType("varchar(128)").HasMaxLength(128);
        entity.Property(x => x.Operand_2).HasColumnType("varchar(128)").HasMaxLength(128);
        entity.Property(x => x.Type_operation).HasColumnType("varchar(128)").HasMaxLength(128);
        entity.Property(x => x.Result).HasColumnType("varchar(128)").HasMaxLength(128);
        modelBuilder.HasCharSet("utf8mb4");
    }
}
