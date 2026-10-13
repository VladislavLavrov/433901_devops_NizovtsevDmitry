using Calculator.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Calculator.Migrations;

[DbContext(typeof(CalculatorContext))]
[Migration("20261013000100_m_init")]
public partial class m_init
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
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
            entity.HasKey("ID_DataInputVariant");
            entity.ToTable("DataInputVariants");
        });
    }
}
