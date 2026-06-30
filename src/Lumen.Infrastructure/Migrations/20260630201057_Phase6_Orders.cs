using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumen.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase6_Orders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrderNumber = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ShippingName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ShippingLine1 = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ShippingLine2 = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ShippingCity = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ShippingRegion = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    ShippingPostalCode = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ShippingCountry = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    BillingName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    BillingLine1 = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    BillingLine2 = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    BillingCity = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    BillingRegion = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    BillingPostalCode = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    BillingCountry = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    OrderNotes = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Subtotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OrderLineItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Sku = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ProductName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderLineItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderLineItems_OrderId_ProductId_ProductVariantId",
                table: "OrderLineItems",
                columns: new[] { "OrderId", "ProductId", "ProductVariantId" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Email",
                table: "Orders",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderLineItems");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
