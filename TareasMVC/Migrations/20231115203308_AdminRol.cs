using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TareasMVC.Migrations
{
    /// <inheritdoc />
    public partial class AdminRol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(
                @"IF NOT EXISTS (SELECT Id FROM AspNetRoles WHERE Id = 'c6518459-891d-4b71-8a01-4449a3eef6f9')
                BEGIN
                    INSERT AspNetRoles (Id, [Name], [NormalizedName])
                    VALUES ('c6518459-891d-4b71-8a01-4449a3eef6f9', 'admin', 'ADMIN')
                END"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"DELETE AspNetRoles
                WHERE Id = 'c6518459-891d-4b71-8a01-4449a3eef6f9'"
            );
        }
    }
}
