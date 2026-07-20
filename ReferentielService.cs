using System.Net.Http.Json;
using Segmentation.Shared.Models;

namespace Segmentation.Client.Services
{
    /// <summary>
    /// Fournit le référentiel dynamique (segments, profils, régions, secteurs, agences)
    /// construit à partir de la table SegmentationDistributives.
    /// </summary>
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

            _cache = await _http.GetFromJsonAsync<ReferentielData>(
                "api/SegmentationDistributive/referentiel"
            ) ?? new ReferentielData();

            return _cache;
        }

        public void InvalidateCache()
        {
            _cache = null;
        }
    }
}
