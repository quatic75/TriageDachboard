using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddJiraTicketFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JiraTicketId",
                table: "PipelineFailures",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JiraTicketUrl",
                table: "PipelineFailures",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JiraTicketId",
                table: "PipelineFailures");

            migrationBuilder.DropColumn(
                name: "JiraTicketUrl",
                table: "PipelineFailures");
        }
    }
}
