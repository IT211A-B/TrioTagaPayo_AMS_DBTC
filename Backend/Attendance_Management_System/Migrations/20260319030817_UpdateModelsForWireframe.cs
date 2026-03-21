using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Attendance_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelsForWireframe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Teachers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TeacherNo",
                table: "Teachers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "Students",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MobileNo",
                table: "Students",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Section",
                table: "Students",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Schedule",
                table: "Courses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Section",
                table: "Courses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Units",
                table: "Courses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "Attendances",
                type: "text",
                nullable: false,
                defaultValue: "");


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "TeacherNo",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "MobileNo",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Section",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Schedule",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Section",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Units",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "Attendances");
        }
    }
}
