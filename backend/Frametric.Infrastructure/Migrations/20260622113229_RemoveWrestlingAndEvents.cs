using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Frametric.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWrestlingAndEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM ""Movies""
WHERE ""Id"" IN (
    SELECT DISTINCT m.""Id""
    FROM ""Movies"" m
    LEFT JOIN ""MovieGenre"" mg ON m.""Id"" = mg.""MoviesId""
    LEFT JOIN ""Genres"" g ON mg.""GenresId"" = g.""Id""
    WHERE
        -- Wrestling
        LOWER(m.""Keywords"") LIKE '%wrestling%' OR
        LOWER(m.""Keywords"") LIKE '%professional wrestling%' OR
        LOWER(m.""Keywords"") LIKE '%sports entertainment%' OR
        LOWER(m.""Keywords"") LIKE '%wwe%' OR
        LOWER(m.""Keywords"") LIKE '%aew%' OR
        LOWER(m.""Keywords"") LIKE '%njpw%' OR
        LOWER(m.""Keywords"") LIKE '%wcw%' OR
        LOWER(m.""Keywords"") LIKE '%ecw%' OR
        LOWER(g.""Name"") = 'wrestling' OR
        
        -- Sports
        LOWER(m.""Keywords"") LIKE '%mma%' OR
        LOWER(m.""Keywords"") LIKE '%mixed martial arts%' OR
        LOWER(m.""Keywords"") LIKE '%ufc%' OR
        LOWER(m.""Keywords"") LIKE '%boxing%' OR
        LOWER(m.""Keywords"") LIKE '%combat sports%' OR
        LOWER(m.""Keywords"") LIKE '%fight card%' OR
        LOWER(m.""Keywords"") LIKE '%sport event%' OR
        LOWER(g.""Name"") = 'sports' OR
        
        -- Concerts
        LOWER(m.""Keywords"") LIKE '%concert%' OR
        LOWER(m.""Keywords"") LIKE '%concert film%' OR
        LOWER(m.""Keywords"") LIKE '%live concert%' OR
        LOWER(m.""Keywords"") LIKE '%live performance%' OR
        LOWER(m.""Keywords"") LIKE '%live recording%' OR
        LOWER(m.""Keywords"") LIKE '%music concert%' OR
        LOWER(m.""Keywords"") LIKE '%tour%' OR
        LOWER(m.""Keywords"") LIKE '%world tour%' OR
        
        -- Stand-up
        LOWER(m.""Keywords"") LIKE '%stand-up comedy%' OR
        LOWER(m.""Keywords"") LIKE '%standup comedy%' OR
        LOWER(m.""Keywords"") LIKE '%comedy special%' OR
        LOWER(m.""Keywords"") LIKE '%comedian%' OR
        
        -- Theater
        LOWER(m.""Keywords"") LIKE '%stage play%' OR
        LOWER(m.""Keywords"") LIKE '%theatre%' OR
        LOWER(m.""Keywords"") LIKE '%theater%' OR
        LOWER(m.""Keywords"") LIKE '%broadway%' OR
        LOWER(m.""Keywords"") LIKE '%west end%' OR
        LOWER(m.""Keywords"") LIKE '%filmed theater%' OR
        
        -- Opera
        LOWER(m.""Keywords"") LIKE '%opera%' OR
        LOWER(m.""Keywords"") LIKE '%operetta%' OR
        
        -- Ballet
        LOWER(m.""Keywords"") LIKE '%ballet%' OR
        LOWER(m.""Keywords"") LIKE '%dance performance%'
);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
