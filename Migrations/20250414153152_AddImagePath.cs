﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspnetCoreMvcFull.Migrations
{
    /// <inheritdoc />
    public partial class AddImagePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Cranes",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Cranes",
                keyColumn: "Id",
                keyValue: 1,
                column: "ImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Cranes",
                keyColumn: "Id",
                keyValue: 2,
                column: "ImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Cranes",
                keyColumn: "Id",
                keyValue: 3,
                column: "ImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Cranes",
                keyColumn: "Id",
                keyValue: 4,
                column: "ImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Cranes",
                keyColumn: "Id",
                keyValue: 5,
                column: "ImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Cranes",
                keyColumn: "Id",
                keyValue: 6,
                column: "ImagePath",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Cranes");
        }
    }
}
