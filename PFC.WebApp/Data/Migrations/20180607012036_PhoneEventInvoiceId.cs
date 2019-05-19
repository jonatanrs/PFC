using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace PFC.WebApp.Data.Migrations
{
    public partial class PhoneEventInvoiceId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Identificador",
                table: "Invoice");

            migrationBuilder.AddColumn<int>(
                name: "InvoiceId",
                table: "PhoneEvents",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "PhoneEvents");

            migrationBuilder.AddColumn<string>(
                name: "Identificador",
                table: "Invoice",
                nullable: true);
        }
    }
}
