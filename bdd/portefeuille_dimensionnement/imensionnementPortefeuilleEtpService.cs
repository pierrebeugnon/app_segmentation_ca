using System.Net.Http.Json;
using Segmentation.Shared.Models.DimensionnementPortefeuille;

namespace Segmentation.Client.Services;

public class DimensionnementPortefeuilleEtpService
{
    private readonly HttpClient _http;

    public DimensionnementPortefeuilleEtpService(HttpClient http)
    {
        _http = http;
    }

    public async Task<bool> SaveAsync(
        SaveDimensionnementPortefeuilleEtpRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(
                "api/DimensionnementPortefeuilleEtp",
                request);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($">>> DimensionnementPortefeuilleEtp SaveAsync status : {response.StatusCode}");
                return false;
            }

            return await response.Content.ReadFromJsonAsync<bool>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> Erreur DimensionnementPortefeuilleEtp SaveAsync : {ex.Message}");
            return false;
        }
    }
}
