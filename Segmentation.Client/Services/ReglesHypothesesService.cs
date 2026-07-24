using System.Net.Http.Json;
using Segmentation.Client.Models;

namespace Segmentation.Client.Services
{
    /// <summary>
    /// Service unifié pour charger et sauvegarder les Règles & Hypothèses
    /// depuis les 4 tables de la BDD.
    /// </summary>
    public class ReglesHypothesesService
    {
        private readonly HttpClient _http;

        public ReglesHypothesesService(HttpClient http)
        {
            _http = http;
        }

        // ═══════════════════════════════════════════════════════════
        //   LECTURE — Charge les 4 tables en parallèle
        // ═══════════════════════════════════════════════════════════
        public async Task<ReglesHypothesesBundle> LoadAllAsync()
        {
            var taskSegments   = _http.GetFromJsonAsync<List<SegmentIntensiteData>>("api/SegmentIntensite");
            var taskProfils    = _http.GetFromJsonAsync<List<ConseillerProfilData>>("api/ConseillerProfil");
            var taskRegles     = _http.GetFromJsonAsync<List<RegleAffectationSegmentData>>("api/RegleAffectationSegment");
            var taskParametres = _http.GetFromJsonAsync<ParametresGenerauxData?>("api/ParametresGeneraux");

            await Task.WhenAll(taskSegments, taskProfils, taskRegles, taskParametres);

            return new ReglesHypothesesBundle
            {
                Segments   = taskSegments.Result   ?? new(),
                Profils    = taskProfils.Result    ?? new(),
                Regles     = taskRegles.Result     ?? new(),
                Parametres = taskParametres.Result
            };
        }

        // ═══════════════════════════════════════════════════════════
        //   SAUVEGARDE — Envoie les 4 tables en parallèle
        // ═══════════════════════════════════════════════════════════
        public async Task<ReglesHypothesesSaveResult> SaveAllAsync(ReglesHypothesesBundle bundle)
        {
            var taskSegments   = SaveListAsync("api/SegmentIntensite",        bundle.Segments);
            var taskProfils    = SaveListAsync("api/ConseillerProfil",        bundle.Profils);
            var taskRegles     = SaveListAsync("api/RegleAffectationSegment", bundle.Regles);
            var taskParametres = SaveObjectAsync("api/ParametresGeneraux",    bundle.Parametres);

            await Task.WhenAll(taskSegments, taskProfils, taskRegles, taskParametres);

            return new ReglesHypothesesSaveResult
            {
                SegmentsSaved   = taskSegments.Result,
                ProfilsSaved    = taskProfils.Result,
                ReglesSaved     = taskRegles.Result,
                ParametresSaved = taskParametres.Result
            };
        }

        // ── Helpers ─────────────────────────────────────────────────
        private async Task<int> SaveListAsync<T>(string url, List<T> items)
        {
            var response = await _http.PostAsJsonAsync(url, items);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SaveResponse>();
            return result?.Saved ?? 0;
        }

        private async Task<bool> SaveObjectAsync<T>(string url, T? item) where T : class
        {
            if (item is null)
                return false;

            var response = await _http.PostAsJsonAsync(url, item);
            response.EnsureSuccessStatusCode();
            return true;
        }

        private class SaveResponse
        {
            public int Saved { get; set; }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //   Container qui regroupe les 4 tables
    // ═══════════════════════════════════════════════════════════════
    public class ReglesHypothesesBundle
    {
        public List<SegmentIntensiteData> Segments { get; set; } = new();
        public List<ConseillerProfilData> Profils { get; set; } = new();
        public List<RegleAffectationSegmentData> Regles { get; set; } = new();
        public ParametresGenerauxData? Parametres { get; set; }
    }

    public class ReglesHypothesesSaveResult
    {
        public int SegmentsSaved { get; set; }
        public int ProfilsSaved { get; set; }
        public int ReglesSaved { get; set; }
        public bool ParametresSaved { get; set; }
    }
}
