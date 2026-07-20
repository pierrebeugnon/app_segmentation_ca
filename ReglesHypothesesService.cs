using System.Net.Http.Json;
using Segmentation.Shared.Models;

namespace Segmentation.Client.Services
{
    public class ReglesHypothesesService
    {
        private readonly HttpClient _http;

        public ReglesHypothesesService(HttpClient http)
        {
            _http = http;
        }

        // ═══════════════════════════════════════════════════════════
        //   LECTURE
        // ═══════════════════════════════════════════════════════════
        public async Task<ReglesHypothesesBundle> LoadAllAsync()
        {
            var taskSegments   = SafeGetListAsync<SegmentIntensiteData>("api/SegmentIntensite");
            var taskProfils    = SafeGetListAsync<ConseillerProfilData>("api/ConseillerProfil");
            var taskRegles     = SafeGetListAsync<RegleAffectationSegmentData>("api/RegleAffectationSegment");
            var taskParametres = SafeGetObjectAsync<ParametresGenerauxData>("api/ParametresGeneraux");

            await Task.WhenAll(taskSegments, taskProfils, taskRegles, taskParametres);

            return new ReglesHypothesesBundle
            {
                Segments   = taskSegments.Result,
                Profils    = taskProfils.Result,
                Regles     = taskRegles.Result,
                Parametres = taskParametres.Result
            };
        }

        // ═══════════════════════════════════════════════════════════
        //   SAUVEGARDE
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

        // ═══════════════════════════════════════════════════════════
        //   Helpers robustes
        // ═══════════════════════════════════════════════════════════

        private async Task<List<T>> SafeGetListAsync<T>(string url)
        {
            try
            {
                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($">>> {url} : status {response.StatusCode}");
                    return new List<T>();
                }

                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content) || content == "null")
                    return new List<T>();

                return await response.Content.ReadFromJsonAsync<List<T>>() ?? new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> Erreur GET {url} : {ex.Message}");
                return new List<T>();
            }
        }

        private async Task<T?> SafeGetObjectAsync<T>(string url) where T : class
        {
            try
            {
                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($">>> {url} : status {response.StatusCode}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content) || content == "null")
                    return null;

                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> Erreur GET {url} : {ex.Message}");
                return null;
            }
        }

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
    //   Container
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
