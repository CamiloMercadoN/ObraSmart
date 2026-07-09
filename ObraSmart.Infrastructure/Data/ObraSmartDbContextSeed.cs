using ObraSmart.Domain.Entities;
using System.Text.Json;

namespace ObraSmart.Infrastructure.Data
{
    public static class ObraSmartDbContextSeed
    {
        public static async Task SeedAsync(ObraSmartDbContext context)
        {
            var basePath = AppContext.BaseDirectory;
            if (!context.Paises.Any())
            {
                var filePathTerritorios = Path.Combine(basePath, "Data", "SeedData", "territorios.json");

                if (File.Exists(filePathTerritorios))
                {
                    var jsonTerritoriosData = await File.ReadAllTextAsync(filePathTerritorios);

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    var paises = JsonSerializer.Deserialize<List<Pais>>(jsonTerritoriosData, options);

                    if (paises != null && paises.Any())
                    {
                        await context.Paises.AddRangeAsync(paises);
                        await context.SaveChangesAsync();
                    }
                }
                else
                {
                    Console.WriteLine($"Advertencia: No se encontró el archivo de semilla en {filePathTerritorios}");
                }
            }
        }
    }
}
