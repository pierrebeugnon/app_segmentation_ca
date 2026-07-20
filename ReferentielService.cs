using System.Net.Http.Json;
using Segmentation.Shared.Models;

namespace Segmentation.Client.Services
{
    public class ReferentielService
    {
        private readonly HttpClient _http;
        private ReferentielData? _cache;

        public ReferentielService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ReferentielData> GetAsync(bool forceReload = false)
        {
            if (_cache != null && !forceReload)
                return _cache;

            try
            {
                var response = await _http.GetAsync("api/SegmentationDistributive/referentiel");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($">>> Referentiel : status {response.StatusCode}");
                    _cache = new ReferentielData();
                    return _cache;
                }

                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content) || content == "null")
                {
                    _cache = new ReferentielData();
                    return _cache;
                }

                _cache = await response.Content.ReadFromJsonAsync<ReferentielData>()
                         ?? new ReferentielData();

                return _cache;
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> Erreur chargement Referentiel : {ex.Message}");
                _cache = new ReferentielData();
                return _cache;
            }
        }

        public void InvalidateCache()
        {
            _cache = null;
        }
    }
}
