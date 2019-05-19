using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace PFC.WebApp.Data.Migrations
{
    public partial class Invoices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Invoice",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CallDuration = table.Column<long>(nullable: false),
                    CallPrice = table.Column<decimal>(nullable: false),
                    CallPriceDeduction = table.Column<decimal>(nullable: false),
                    Charged = table.Column<bool>(nullable: false),
                    DataTrafficBytes = table.Column<long>(nullable: false),
                    DataTrafficPrice = table.Column<decimal>(nullable: false),
                    DataTrafficPriceDeduction = table.Column<decimal>(nullable: false),
                    EmissionDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    Identificador = table.Column<string>(nullable: true),
                    SMS = table.Column<long>(nullable: false),
                    SMSPrice = table.Column<long>(nullable: false),
                    SMSPriceDeduction = table.Column<decimal>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    State = table.Column<int>(nullable: false),
                    SubscriptionId = table.Column<int>(nullable: false),
                    Subtotal = table.Column<decimal>(nullable: false),
                    TaxRate = table.Column<decimal>(nullable: false),
                    Total = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoice", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Invoice_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_SubscriptionId",
                table: "Invoice",
                column: "SubscriptionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Invoice");
        }
    }
}
