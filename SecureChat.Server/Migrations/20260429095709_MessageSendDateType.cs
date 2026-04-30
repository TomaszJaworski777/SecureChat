using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecureChat.Server.Migrations
{
    /// <inheritdoc />
    public partial class MessageSendDateType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE ""Messages"" DROP COLUMN ""SendDate"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Messages"" ADD COLUMN ""SendDate"" timestamp with time zone NOT NULL DEFAULT now();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SendDate",
                table: "Messages",
                type: "text",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }
    }
}
