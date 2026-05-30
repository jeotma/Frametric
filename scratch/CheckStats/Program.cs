using System;
using System.Threading.Tasks;
using Npgsql;

var autoReset = args.Contains("--reset");
var connString = "Host=localhost;Port=5432;Database=Frametric;Username=postgres;Password=1234";
await using var conn = new NpgsqlConnection(connString);
await conn.OpenAsync();

Console.WriteLine("=== Estado de Enriquecimiento ===\n");

// 1. Summary by status
await using var summaryCmd = new NpgsqlCommand(
    "SELECT \"EnrichmentStatus\", COUNT(*) as total FROM \"Movies\" GROUP BY \"EnrichmentStatus\" ORDER BY \"EnrichmentStatus\";",
    conn);
await using var summaryReader = await summaryCmd.ExecuteReaderAsync();
Console.WriteLine("Status:");
while (await summaryReader.ReadAsync())
{
    var status = summaryReader.GetString(0);
    var count  = summaryReader.GetInt64(1);
    Console.WriteLine($"  {status,-20}: {count}");
}
await summaryReader.CloseAsync();

Console.WriteLine();

// 2. List non-completed movies
await using var listCmd = new NpgsqlCommand(
    "SELECT \"Title\", \"ReleaseYear\", \"EnrichmentStatus\" FROM \"Movies\" WHERE \"EnrichmentStatus\" != 'Completed' ORDER BY \"EnrichmentStatus\", \"Title\" LIMIT 100;",
    conn);
await using var listReader = await listCmd.ExecuteReaderAsync();
Console.WriteLine("Películas NO completadas (máx 100):");
Console.WriteLine($"  {"Título",-45} {"Año",-6} {"Estado"}");
Console.WriteLine($"  {new string('-', 65)}");
while (await listReader.ReadAsync())
{
    var title  = listReader.GetString(0);
    var year   = listReader.IsDBNull(1) ? "?" : listReader.GetInt32(1).ToString();
    var status = listReader.GetString(2);
    Console.WriteLine($"  {title[..Math.Min(title.Length, 44)],-45} {year,-6} {status}");
}
await listReader.CloseAsync();

Console.WriteLine();
Console.Write("¿Resetear todos los Failed y Pending a Pending para reprocesarlos? (s/n): ");
string? answer;
if (autoReset)
{
    answer = "s";
    Console.WriteLine("s (auto)");
}
else
{
    answer = Console.ReadLine()?.Trim().ToLower();
}
if (answer == "s")
{
    await using var resetCmd = new NpgsqlCommand(
        "UPDATE \"Movies\" SET \"EnrichmentStatus\" = 'Pending' WHERE \"EnrichmentStatus\" != 'Completed';",
        conn);
    var affected = await resetCmd.ExecuteNonQueryAsync();
    Console.WriteLine($"\n✅ {affected} película(s) reseteadas a Pending. Arranca la API para que el job las reprocese.");
}
else
{
    Console.WriteLine("Cancelado. No se ha modificado nada.");
}
