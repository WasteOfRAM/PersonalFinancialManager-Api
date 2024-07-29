using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalFinancialManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangedTypetoAccountType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Accounts",
                newName: "AccountType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AccountType",
                table: "Accounts",
                newName: "Type");
        }
    }
}
