﻿using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace PFC.WebApp.Data.Migrations
{
    public partial class InvoicesFixSMSPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "SMSPrice",
                table: "Invoice",
                nullable: false,
                oldClrType: typeof(long));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "SMSPrice",
                table: "Invoice",
                nullable: false,
                oldClrType: typeof(decimal));
        }
    }
}
