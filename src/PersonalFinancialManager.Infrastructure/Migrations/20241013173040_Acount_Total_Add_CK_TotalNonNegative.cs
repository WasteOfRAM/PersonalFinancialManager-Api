using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalFinancialManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Acount_Total_Add_CK_TotalNonNegative : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_TotalNonNegative",
                table: "Accounts",
                sql: "[Total] >= 0.0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TotalNonNegative",
                table: "Accounts");
        }
    }
}
