using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnergyPlanner.Migrations
{
    /// <inheritdoc />
    public partial class AddSchemesAndDevices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SchemeId",
                table: "Accumulators",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Scheme",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scheme", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scheme_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Device",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SchemeId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PowerWatts = table.Column<double>(type: "float", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    HoursPerDay = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Device", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Device_Scheme_SchemeId",
                        column: x => x.SchemeId,
                        principalTable: "Scheme",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Device_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accumulators_SchemeId",
                table: "Accumulators",
                column: "SchemeId");

            migrationBuilder.CreateIndex(
                name: "IX_Device_SchemeId",
                table: "Device",
                column: "SchemeId");

            migrationBuilder.CreateIndex(
                name: "IX_Device_UserId",
                table: "Device",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Scheme_UserId",
                table: "Scheme",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accumulators_Scheme_SchemeId",
                table: "Accumulators",
                column: "SchemeId",
                principalTable: "Scheme",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accumulators_Scheme_SchemeId",
                table: "Accumulators");

            migrationBuilder.DropTable(
                name: "Device");

            migrationBuilder.DropTable(
                name: "Scheme");

            migrationBuilder.DropIndex(
                name: "IX_Accumulators_SchemeId",
                table: "Accumulators");

            migrationBuilder.DropColumn(
                name: "SchemeId",
                table: "Accumulators");
        }
    }
}
