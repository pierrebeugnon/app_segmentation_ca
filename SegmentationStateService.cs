using Segmentation.Client.Models;

namespace Segmentation.Client.Services;

public class SegmentationStateService
{
    private const int MinutesParAn = 220 * 8 * 60;

    private static readonly Dictionary<string, int> ClientsFixe = new()
    {
        ["Faible Potentiel"] = 200_000,
        ["Intermédiaire"]    = 778_000,
        ["Haut de gamme"]    = 22_000,
    };

    private readonly Dictionary<string, int> _taillesDefaut = new();

    public event Action? OnChange;

    public ReglesHypothesesModel Regles { get; private set; }
    public List<TaillePortefeuille> TaillesPortefeuilles { get; private set; } = new();
    public int TotalConseillersCible { get; private set; }
    public int TaillePortefeuilleMoyenne { get; private set; }
    public List<DimensionnementEtpRow> EtpCibleRows { get; private set; } = new();

    public SegmentationStateService()
    {
        Regles = BuildDefaultRegles();

        foreach (var pf in Regles.PortefeuillesTheoriques)
            _taillesDefaut[pf.Profil] = pf.ClientsParConseiller;

        Recalculer();
    }

    public void UpdateRegles(ReglesHypothesesModel regles)
    {
        Regles = regles;
        Recalculer();
        OnChange?.Invoke();
    }

    // ── Valeurs par défaut (anciennement MockDataService) ─────────────────
    private static ReglesHypothesesModel BuildDefaultRegles() => new()
    {
        SegmentsIntensite = new List<SegmentIntensite>
        {
            new() { LigneMetier = "banque privée",                   Segment = "HDG Premium Potentiel", NombreRdvParAn = 2.5,  DureeRdvHeures = 2,    NombreClients =    50, Region = "Pas-de-Calais", Secteur = "Lens-Béthune",    Agence = "BP Lens"            },
            new() { LigneMetier = "banque privée",                   Segment = "HDG Premium Standard",  NombreRdvParAn = 2,    DureeRdvHeures = 2,    NombreClients =   150, Region = "Pas-de-Calais", Secteur = "Lens-Béthune",    Agence = "BP Lens"            },
            new() { LigneMetier = "retail",                          Segment = "HDG Potentiel",         NombreRdvParAn = 2,    DureeRdvHeures = 1.75, NombreClients =   300, Region = "Nord",          Secteur = "Lille Métropole", Agence = "Agence Lille Centre"},
            new() { LigneMetier = "retail",                          Segment = "HDG Senior Epargnant",  NombreRdvParAn = 1.5,  DureeRdvHeures = 1.5,  NombreClients =   200, Region = "Nord",          Secteur = "Lille Métropole", Agence = "Agence Lille Centre"},
            new() { LigneMetier = "retail",                          Segment = "HDG Standard",          NombreRdvParAn = 1.5,  DureeRdvHeures = 1.5,  NombreClients =   600, Region = "Nord",          Secteur = "Lille Métropole", Agence = "Agence Roubaix"     },
            new() { LigneMetier = "retail",                          Segment = "CI Potentiel",          NombreRdvParAn = 2,    DureeRdvHeures = 1.25, NombreClients =   800, Region = "Nord",          Secteur = "Valenciennes",    Agence = "Agence Valenciennes"},
            new() { LigneMetier = "retail",                          Segment = "CI Standard",           NombreRdvParAn = 1.25, DureeRdvHeures = 1,    NombreClients =  3000, Region = "Nord",          Secteur = "Valenciennes",    Agence = "Agence Valenciennes"},
            new() { LigneMetier = "retail",                          Segment = "GP Potentiel",          NombreRdvParAn = 1,    DureeRdvHeures = 1.25, NombreClients =  1500, Region = "Nord",          Secteur = "Douai",           Agence = "Agence Douai"       },
            new() { LigneMetier = "retail",                          Segment = "GP Standard",           NombreRdvParAn = 0.5,  DureeRdvHeures = 0.5,  NombreClients =  8000, Region = "Nord",          Secteur = "Douai",           Agence = "Agence Douai"       },
            new() { LigneMetier = "retail - portefeuille mutualisé", Segment = "Non segmenté",          NombreRdvParAn = 0,    DureeRdvHeures = 0,    NombreClients =  5400, Region = "Nord",          Secteur = "Lille Métropole", Agence = "Agence Tourcoing"   },
        },

        HeuresParSemaine = 37.3,
        NbSemainesParAn  = 42.5,

        ConseillersProfils = new List<ConseillerTempsProfil>
        {
            new() { LigneMetier = "banque privée", Profil = "DIR. BP",              PartTempsCommercialPct = 60 },
            new() { LigneMetier = "banque privée", Profil = "BANQUIER PRIVÉ",       PartTempsCommercialPct = 60 },
            new() { LigneMetier = "banque privée", Profil = "CGP",                  PartTempsCommercialPct = 60 },
            new() { LigneMetier = "retail",        Profil = "RESP. AGENCE",         PartTempsCommercialPct = 40 },
            new() { LigneMetier = "retail",        Profil = "RCP",                  PartTempsCommercialPct = 40 },
            new() { LigneMetier = "retail",        Profil = "CONSEILLER CLIENTELE", PartTempsCommercialPct = 40 },
            new() { LigneMetier = "retail",        Profil = "CONSEILLER COMMERCIAL",PartTempsCommercialPct = 40 },
        },

        ReglesAffectationSegments = new List<RegleAffectationSegment>
        {
            new() { Segment = "HDG Premium Potentiel", ConseillerPrioritaire = "BANQUIER PRIVÉ",       ConseillerSecondaire = "DIR. BP"                },
            new() { Segment = "HDG Premium Standard",  ConseillerPrioritaire = "CGP",                  ConseillerSecondaire = "BANQUIER PRIVÉ"         },
            new() { Segment = "HDG Potentiel",         ConseillerPrioritaire = "RESP. AGENCE",         ConseillerSecondaire = "RCP"                    },
            new() { Segment = "HDG Senior Epargnant",  ConseillerPrioritaire = "RCP",                  ConseillerSecondaire = "RESP. AGENCE"           },
            new() { Segment = "HDG Standard",          ConseillerPrioritaire = "RESP. AGENCE",         ConseillerSecondaire = "CONSEILLER CLIENTELE"   },
            new() { Segment = "CI Potentiel",          ConseillerPrioritaire = "CONSEILLER CLIENTELE", ConseillerSecondaire = "RCP"                    },
            new() { Segment = "CI Standard",           ConseillerPrioritaire = "CONSEILLER CLIENTELE", ConseillerSecondaire = "CONSEILLER COMMERCIAL"  },
            new() { Segment = "GP Potentiel",          ConseillerPrioritaire = "CONSEILLER CLIENTELE", ConseillerSecondaire = "CONSEILLER CLIENTELE"   },
            new() { Segment = "GP Standard",           ConseillerPrioritaire = "CONSEILLER COMMERCIAL",ConseillerSecondaire = "CONSEILLER CLIENTELE"   },
            new() { Segment = "Non segmenté",          ConseillerPrioritaire = "CONSEILLER COMMERCIAL",ConseillerSecondaire = "CONSEILLER D'ACCUEIL"   },
        },

        TaillesTheoretiques = new List<TailleTheoriqueRow>
        {
            new() { LigneMetier = "banque privée",                   Profil = "DIR. BP",              SegmentCouvert = ""                       },
            new() { LigneMetier = "banque privée",                   Profil = "BANQUIER PRIVÉ",       SegmentCouvert = "HDG Premium Potentiel"  },
            new() { LigneMetier = "banque privée",                   Profil = "CGP",                  SegmentCouvert = "HDG Premium Standard"   },
            new() { LigneMetier = "retail",                          Profil = "RESP. AGENCE",         SegmentCouvert = "HDG Potentiel"          },
            new() { LigneMetier = "retail",                          Profil = "RESP. AGENCE",         SegmentCouvert = "HDG Senior Epargnant"   },
            new() { LigneMetier = "retail",                          Profil = "RCP",                  SegmentCouvert = "HDG Standard"           },
            new() { LigneMetier = "retail",                          Profil = "CONSEILLER CLIENTELE", SegmentCouvert = "CI Potentiel"           },
            new() { LigneMetier = "retail",                          Profil = "CONSEILLER CLIENTELE", SegmentCouvert = "CI Standard"            },
            new() { LigneMetier = "retail",                          Profil = "CONSEILLER COMMERCIAL",SegmentCouvert = "GP Potentiel"           },
            new() { LigneMetier = "retail - portefeuille mutualisé", Profil = "CONSEILLER COMMERCIAL",SegmentCouvert = "GP Standard"            },
            new() { LigneMetier = "N/A",                             Profil = "",                     SegmentCouvert = "Non segmenté"           },
        },

        PartTempsCommercial = 40,

        PortefeuillesTheoriques = new List<PortefeuilleTheoriqueCard>
        {
            new() { Profil = "Portefeuilles Mutualisés", ClientsParConseiller = 2536, CssClass = "card-mutualise" },
            new() { Profil = "Dédiés",                   ClientsParConseiller =  507, CssClass = "card-dedie"     },
            new() { Profil = "Dédiés - Haut de Gamme",   ClientsParConseiller =  190, CssClass = "card-hdg"       },
        },
    };

    // ── Helpers de calcul (inchangés) ─────────────────────────────────────
    private double GetVolumeHoraire(string profil) =>
        Regles.ConseillersProfils.FirstOrDefault(p => p.Profil == profil) is { } cp
            ? Math.Round(Regles.HeuresTravailParAn * cp.PartTempsCommercialPct / 100.0, 1)
            : 0;

    private double GetIntensite(string segment) =>
        Regles.SegmentsIntensite.FirstOrDefault(s => s.Segment == segment)?.IntensiteRelationnelle ?? 0;

    private int GetTailleTheorique(string profil, string segment)
    {
        var intensite = GetIntensite(segment);
        var volume = GetVolumeHoraire(profil);
        return intensite > 0 ? (int)Math.Round(volume / intensite) : 0;
    }

    private void Recalculer()
    {
        if (Regles.SegmentsIntensite.Any() && Regles.ConseillersProfils.Any())
        {
            foreach (var pf in Regles.PortefeuillesTheoriques)
            {
                pf.ClientsParConseiller = pf.Profil switch
                {
                    "Portefeuilles Mutualisés" => GetTailleTheorique("CONSEILLER COMMERCIAL", "GP Standard"),
                    "Dédiés"                   => GetTailleTheorique("CONSEILLER CLIENTELE",  "CI Standard"),
                    "Dédiés - Haut de Gamme"   => GetTailleTheorique("BANQUIER PRIVÉ",        "HDG Premium Potentiel"),
                    _ => pf.ClientsParConseiller
                };
            }
        }

        var profilToSeg = new Dictionary<string, string>
        {
            ["Portefeuilles Mutualisés"] = "Faible Potentiel",
            ["Dédiés"]                   = "Intermédiaire",
            ["Dédiés - Haut de Gamme"]   = "Haut de gamme",
        };

        TaillesPortefeuilles = Regles.PortefeuillesTheoriques.Select(pf =>
        {
            var seg = profilToSeg.GetValueOrDefault(pf.Profil, "");
            var clients = ClientsFixe.GetValueOrDefault(seg, 0);
            var etp = pf.ClientsParConseiller > 0 ? (double)clients / pf.ClientsParConseiller : 0;
            double etpActuel = pf.Profil switch
            {
                "Portefeuilles Mutualisés" => 150,
                "Dédiés"                   => 900,
                _                          => 150,
            };
            double taux = etp > 0 ? Math.Min(etpActuel / etp * 100, 150) : 0;
            return new TaillePortefeuille
            {
                Profil = pf.Profil,
                ClientsParConseiller = pf.ClientsParConseiller,
                TauxRemplissage = Math.Round(taux, 1),
            };
        }).ToList();

        double etpMut = Etp("Portefeuilles Mutualisés", profilToSeg);
        double etpDed = Etp("Dédiés",                   profilToSeg);
        double etpHDG = Etp("Dédiés - Haut de Gamme",   profilToSeg);
        double totalEtp = etpMut + etpDed + etpHDG;
        TotalConseillersCible = (int)Math.Ceiling(totalEtp);
        TaillePortefeuilleMoyenne = TotalConseillersCible > 0 ? (int)(1_000_000.0 / TotalConseillersCible) : 0;

        EtpCibleRows = new List<DimensionnementEtpRow>();
    }

    private double Etp(string profil, Dictionary<string, string> profilToSeg)
    {
        var pf = Regles.PortefeuillesTheoriques.FirstOrDefault(p => p.Profil == profil);
        if (pf == null || pf.ClientsParConseiller <= 0) return 0;
        var seg = profilToSeg.GetValueOrDefault(profil, "");
        return (double)ClientsFixe.GetValueOrDefault(seg, 0) / pf.ClientsParConseiller;
    }
}
