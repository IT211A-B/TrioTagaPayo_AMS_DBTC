using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Attendance_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class AddQRModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QRScanId",
                table: "Attendances",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_QRScanId",
                table: "Attendances",
                column: "QRScanId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_QRScans_QRScanId",
                table: "Attendances",
                column: "QRScanId",
                principalTable: "QRScans",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_QRScans_QRScanId",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_QRScanId",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "QRScanId",
                table: "Attendances");
        }
    }
}
