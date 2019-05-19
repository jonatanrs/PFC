using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace PFC.WebApp.Data.Migrations
{
    public partial class AsociacionUsuariosEntidades : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Subscriptions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComercialId",
                table: "Invoice",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComercialId",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_ComercialId",
                table: "Invoice",
                column: "ComercialId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ComercialId",
                table: "AspNetUsers",
                column: "ComercialId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ComercialId",
                table: "AspNetUsers",
                column: "ComercialId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoice_AspNetUsers_ComercialId",
                table: "Invoice",
                column: "ComercialId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_AspNetUsers_UserId",
                table: "Subscriptions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ComercialId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoice_AspNetUsers_ComercialId",
                table: "Invoice");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_AspNetUsers_UserId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Invoice_ComercialId",
                table: "Invoice");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ComercialId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "ComercialId",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "ComercialId",
                table: "AspNetUsers");
        }
    }
}
