using Calculator.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Calculator.Migrations;

[DbContext(typeof(CalculatorContext))]
public partial class CalculatorContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.14")
            .HasAnnotation("Relational:MaxIdentifierLength", 64)
            .HasAnnotation("MySql:CharSet", "utf8mb4");
        modelBuilder.Entity("Calculator.Data.DataInputVariant", entity =>
        {
            entity.Property<int>("ID_DataInputVariant")
                .ValueGeneratedOnAdd()
                .HasColumnType("int")
                .HasAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);
            entity.Property<string>("Operand_1")
                .HasMaxLength(128)
                .HasColumnType("varchar(128)")
                .HasAnnotation("MySql:CharSet", "utf8mb4");
            entity.Property<string>("Operand_2")
                .HasMaxLength(128)
                .HasColumnType("varchar(128)")
                .HasAnnotation("MySql:CharSet", "utf8mb4");
            entity.Property<string>("Type_operation")
                .HasMaxLength(128)
                .HasColumnType("varchar(128)")
                .HasAnnotation("MySql:CharSet", "utf8mb4");
            entity.Property<string>("Result")
                .HasMaxLength(128)
                .HasColumnType("varchar(128)")
                .HasAnnotation("MySql:CharSet", "utf8mb4");
            entity.Property<string>("RequestId")
                .HasMaxLength(36)
                .HasColumnType("varchar(36)")
                .HasAnnotation("MySql:CharSet", "utf8mb4");
            entity.HasKey("ID_DataInputVariant");
            entity.HasIndex("RequestId").IsUnique();
            entity.ToTable("DataInputVariants");
        });
    }
}
