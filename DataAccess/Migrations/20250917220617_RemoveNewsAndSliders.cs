using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class RemoveNewsAndSliders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Guard: drop FKs if exist (older migration might not have them now)
            try { migrationBuilder.Sql("ALTER TABLE \"SliderItems\" DROP CONSTRAINT IF EXISTS \"FK_SliderItems_Sliders_SliderId\";"); } catch {}
            try { migrationBuilder.Sql("ALTER TABLE \"SliderItems\" DROP CONSTRAINT IF EXISTS \"FK_SliderItems_Slider_SliderId\";"); } catch {}

            // Drop child then parent and news
            migrationBuilder.DropTable(name: "SliderItems");
            migrationBuilder.DropTable(name: "Sliders");
            migrationBuilder.DropTable(name: "News");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // intentionally left blank
        }
    }
}
