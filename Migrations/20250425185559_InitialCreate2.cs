using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspnetCoreMvcFull.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PicRejectReason",
                table: "Bookings",
                newName: "PICRejectReason");

            migrationBuilder.RenameColumn(
                name: "PicName",
                table: "Bookings",
                newName: "DoneByPIC");

            migrationBuilder.RenameColumn(
                name: "PicApprovalTime",
                table: "Bookings",
                newName: "DoneAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAtByPIC",
                table: "Bookings",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByPIC",
                table: "Bookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Bookings",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CancelledBy",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CancelledByName",
                table: "Bookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledReason",
                table: "Bookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Bookings",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Bookings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RevisionCount",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAtByPIC",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ApprovedByPIC",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CancelledBy",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CancelledByName",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CancelledReason",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "RevisionCount",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "PICRejectReason",
                table: "Bookings",
                newName: "PicRejectReason");

            migrationBuilder.RenameColumn(
                name: "DoneByPIC",
                table: "Bookings",
                newName: "PicName");

            migrationBuilder.RenameColumn(
                name: "DoneAt",
                table: "Bookings",
                newName: "PicApprovalTime");
        }
    }
}
