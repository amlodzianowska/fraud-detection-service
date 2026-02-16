using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FraudDetection.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVelocityCounters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrdersLast24Hours",
                table: "UserBehaviorProfiles");

            migrationBuilder.DropColumn(
                name: "OrdersLast30Days",
                table: "UserBehaviorProfiles");

            migrationBuilder.DropColumn(
                name: "OrdersLast7Days",
                table: "UserBehaviorProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrdersLast24Hours",
                table: "UserBehaviorProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrdersLast30Days",
                table: "UserBehaviorProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrdersLast7Days",
                table: "UserBehaviorProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
