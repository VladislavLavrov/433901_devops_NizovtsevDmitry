using Microsoft.EntityFrameworkCore.Migrations;

namespace Calculator.Migrations;

public partial class AddKafkaRequestId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "RequestId", table: "DataInputVariants", type: "varchar(36)",
            maxLength: 36, nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");
        migrationBuilder.CreateIndex(
            name: "IX_DataInputVariants_RequestId", table: "DataInputVariants",
            column: "RequestId", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_DataInputVariants_RequestId", table: "DataInputVariants");
        migrationBuilder.DropColumn(name: "RequestId", table: "DataInputVariants");
    }
}
