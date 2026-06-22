using CA.SegmentationClient.Models;

namespace CA.SegmentationClient.Services
{
    public class MockDataService
    {
        public VisionGlobaleModel GetVisionGlobale(bool parAgence = false)
        {
            return new VisionGlobaleModel
            {
                Kpis = new KpiGlobal
                {
                    TotalClients = 1_000_000,
                    TotalConseillers = 1200,
                    TaillePortefeuilleMoyenne = 833
                },
                ClientsParSegment = new List<ClientParSegment>
                {
                    new() { Segment = "Faible Potentiel", NombreClients = 200_000, Pourcentage = 20.0 },
                    new() { Segment = "Intermédiaire",    NombreClients = 778_000, Pourcentage = 77.8 },
                    new() { Segment = "Haut de Gamme",    NombreClients =  22_000, Pourcentage =  2.2 },
                    new() { Segment = "Total",             NombreClients = 1_000_000, Pourcentage = 100.0 }
                },
                ProfilsConseillers = new List<ProfilConseiller>
                {
                    new() { Profil = "Portefeuilles Mutualisés",   NombreConseillers = 150,  Pourcentage = 12.5 },
                    new() { Profil = "Dédiés",                     NombreConseillers = 900,  Pourcentage = 75.0 },
                    new() { Profil = "Dédiés - Haut de Gamme",     NombreConseillers = 150,  Pourcentage = 12.5 },
                    new() { Profil = "Total",                      NombreConseillers = 1200, Pourcentage = 100.0 }
                },
                TaillesPortefeuilles = new List<TaillePortefeuille>
                {
                    new() { Profil = "Portefeuilles Mutualisés", ClientsParConseiller = 1333, TauxRemplissage = 85 },
                    new() { Profil = "Dédiés",                   ClientsParConseiller =  865, TauxRemplissage = 92 },
                    new() { Profil = "Dédiés - Haut de Gamme",   ClientsParConseiller =  147, TauxRemplissage = 78 }
                },
                Indicateurs = new List<IndicateurApplicabilite>
                {
                    new() { Libelle = "Taux de concordance par segment client",      Valeur = 87,   EstPositif = true,  EstPourcentage = true },
                    new() { Libelle = "Taux de concordance par profil de conseiller", Valeur = 92,   EstPositif = true,  EstPourcentage = true },
                    new() { Libelle = "Taux d'évolution des effectifs",               Valeur = 5.3,  EstPositif = true,  EstPourcentage = true },
                    new() { Libelle = "Taux de rotation des clients",                 Valeur = 5.2,  EstPositif = false, EstPourcentage = true }
                },
                AgencesProblematiques = new List<AgenceProblematique>
                {
                    new() { NomAgence = "Lille Nord",  ScoreGlobal = 41, NiveauScore = "danger",  Remplissage = 62, ConcordanceSegment = 58, ConcordanceProfil = 61, EvolutionEffectifs = -8.2, RotationClients = 9.1 },
                    new() { NomAgence = "Roubaix",     ScoreGlobal = 58, NiveauScore = "warning", Remplissage = 65, ConcordanceSegment = 62, ConcordanceProfil = 64, EvolutionEffectifs =  7.8, RotationClients = 8.4 },
                    new() { NomAgence = "Dunkerque",   ScoreGlobal = 62, NiveauScore = "warning", Remplissage = 66, ConcordanceSegment = 66, ConcordanceProfil = 67, EvolutionEffectifs = -7.1, RotationClients = 7.0 },
                    new() { NomAgence = "Lens",        ScoreGlobal = 65, NiveauScore = "warning", Remplissage = 70, ConcordanceSegment = 68, ConcordanceProfil = 69, EvolutionEffectifs =  6.5, RotationClients = 6.8 },
                    new() { NomAgence = "Valenciennes", ScoreGlobal = 68, NiveauScore = "warning", Remplissage = 72, ConcordanceSegment = 70, ConcordanceProfil = 71, EvolutionEffectifs = -6.0, RotationClients = 6.5 }
                }
            };
        }

        public List<DimensionnementClientRow> GetDimensionnementClientCible()
        {
            return new List<DimensionnementClientRow>
            {
                new() { Segment = "Faible Potentiel",  Mutualise = 190_000, Dedie =  10_000, DedieHautDeGamme =       0, Total =   200_000 },
                new() { Segment = "Intermédiaires",    Mutualise =  77_600, Dedie = 661_500, DedieHautDeGamme =  38_900, Total =   778_000 },
                new() { Segment = "Haut de Gamme",     Mutualise =       0, Dedie =   4_200, DedieHautDeGamme =  17_800, Total =    22_000 },
                new() { Segment = "Total",             Mutualise = 267_600, Dedie = 675_700, DedieHautDeGamme =  56_700, Total = 1_000_000, IsTotal = true }
            };
        }

        public List<DimensionnementClientRow> GetDimensionnementClientExistant()
        {
            return new List<DimensionnementClientRow>
            {
                new() { Segment = "Faible Potentiel", Mutualise =       0, Dedie =  185_000, DedieHautDeGamme =      0, Total =   185_000 },
                new() { Segment = "Intermédiaires",   Mutualise =       0, Dedie =  755_000, DedieHautDeGamme = 38_000, Total =   793_000 },
                new() { Segment = "Haut de Gamme",    Mutualise =       0, Dedie =    4_000, DedieHautDeGamme = 18_000, Total =    22_000 },
                new() { Segment = "Total",            Mutualise =       0, Dedie =  944_000, DedieHautDeGamme = 56_000, Total = 1_000_000, IsTotal = true }
            };
        }

        public List<DimensionnementEtpRow> GetDimensionnementEtpCible()
        {
            return new List<DimensionnementEtpRow>
            {
                new() { Libelle = "Faible Potentiel",           Mutualise = 0.08, Dedie = 0.02, DedieHautDeGamme = null, Total = 0.70, Concordance = 92 },
                new() { Libelle = "Intermédiaires",             Mutualise = 0.03, Dedie = 4.33, DedieHautDeGamme = 0.06, Total = 4.41, Concordance = 97 },
                new() { Libelle = "Haut de Gamme",              Mutualise = null, Dedie = 0.06, DedieHautDeGamme = 0.67, Total = 0.73, Concordance = 97 },
                new() { Libelle = "Charge non-commerciale",     Mutualise = null, Dedie = 0.15, DedieHautDeGamme = null, Total = 0.25, Concordance = null },
                new() { Libelle = "Charge ETP",                 Mutualise = 0.70, Dedie = 4.40, DedieHautDeGamme = 0.73, Total = 5.83, Concordance = null, IsBold = true },
                new() { Libelle = "Effectif Cible",             Mutualise = 0.75, Dedie = 4.50, DedieHautDeGamme = 0.75, Total = 6.00, Concordance = null, IsBold = true },
                new() { Libelle = "Effectif Cible – Charge ETP",Mutualise = 0.05, Dedie = 0.10, DedieHautDeGamme = 0.03, Total = 0.15, Concordance = null, IsEcart = true },
                new() { Libelle = "Effectif Actuel",            Mutualise = 0.78, Dedie = 4.45, DedieHautDeGamme = 0.78, Total = 6.00, Concordance = null, IsBold = true },
                new() { Libelle = "Ecart : Cible – Actuel",     Mutualise = -0.03, Dedie = 0.05, DedieHautDeGamme = -0.03, Total = 0.00, Concordance = null, IsEcart = true },
                new() { Libelle = "Concordance par profil",     Mutualise = 94, Dedie = 93, DedieHautDeGamme = 96, Total = null, Concordance = null }
            };
        }

        public List<DimensionnementEtpRow> GetDimensionnementEtpActuel()
        {
            return new List<DimensionnementEtpRow>
            {
                new() { Libelle = "Faible Potentiel",         Mutualise = 0.72, Dedie = 0.02, DedieHautDeGamme = null,  Total = 0.74, Concordance = 97 },
                new() { Libelle = "Intermédiaires",           Mutualise = 0.00, Dedie = 4.25, DedieHautDeGamme = 0.06,  Total = 4.34, Concordance = 92 },
                new() { Libelle = "Haut de Gamme",            Mutualise = null, Dedie = 0.06, DedieHautDeGamme = 0.70,  Total = 0.76, Concordance = 88 },
                new() { Libelle = "Charge non-commerciale",   Mutualise = null, Dedie = 0.15, DedieHautDeGamme = null,  Total = 0.25, Concordance = null },
                new() { Libelle = "Charge ETP",               Mutualise = 0.74, Dedie = 4.33, DedieHautDeGamme = 0.76, Total = 5.81, Concordance = null, IsBold = true },
                new() { Libelle = "Effectif Actuel",          Mutualise = 0.78, Dedie = 4.45, DedieHautDeGamme = 0.78, Total = 6.00, Concordance = null, IsBold = true },
                new() { Libelle = "Effectif Actuel – Charge ETP", Mutualise = 0.04, Dedie = 0.13, DedieHautDeGamme = 0.02, Total = 0.18, Concordance = null, IsEcart = true },
                new() { Libelle = "Concordance par profil",   Mutualise = 88, Dedie = 88, DedieHautDeGamme = 85, Total = null, Concordance = null }
            };
        }

        public ReglesHypothesesModel GetReglesHypotheses()
        {
            return new ReglesHypothesesModel
            {
                CriteresApproche = new List<CritereApproche>
                {
                    new() { Segment = "Faible Potentiel", FrequenceRdvParAn = 2,  DureeRdvMin = 30 },
                    new() { Segment = "Intermédiaire",    FrequenceRdvParAn = 4,  DureeRdvMin = 45 },
                    new() { Segment = "Haut de gamme",    FrequenceRdvParAn = 6,  DureeRdvMin = 60 }
                },
                PartTempsCommercial = 75,
                PortefeuillesTheoriques = new List<PortefeuilleTheoriqueCard>
                {
                    new() { Profil = "Portefeuilles Mutualisés", ClientsParConseiller = 1560, CssClass = "card-mutualise" },
                    new() { Profil = "Dédiés",                   ClientsParConseiller =  520, CssClass = "card-dedie"     },
                    new() { Profil = "Dédiés - Haut de Gamme",   ClientsParConseiller =  260, CssClass = "card-hdg"       }
                },
                ReglesAffectation = new List<RegleAffectation>
                {
                    new() { ProfilPortefeuille = "Portefeuilles Mutualisés", CorrespondanceIdeale = "Faible Potentiel", AutreCorrespondance = "Intermédiaires", SurchargeMax = 0,  SousChargeMax = 0  },
                    new() { ProfilPortefeuille = "Dédiés",                   CorrespondanceIdeale = "Intermédiaires",   AutreCorrespondance = "Haut de gamme",   SurchargeMax = 15, SousChargeMax = 5  },
                    new() { ProfilPortefeuille = "Dédiés - Haut de Gamme",   CorrespondanceIdeale = "Haut de gamme",    AutreCorrespondance = "Intermédiaires",  SurchargeMax = 5,  SousChargeMax = 5  }
                }
            };
        }

        public List<FichierSource> GetFichiersSource()
        {
            return new List<FichierSource>
            {
                new() { Titre = "Liste des points de vente",          Description = "Fichier contenant la liste complète des agences et points de vente",              NomFichier = "", EstCharge = false },
                new() { Titre = "Effectifs par point de vente",       Description = "Fichier avec les effectifs de conseillers par agence",                            NomFichier = "", EstCharge = false },
                new() { Titre = "Fonds de commerce par point de vente", Description = "Fichier avec les données sur le fonds de commerce par agence",                  NomFichier = "", EstCharge = false }
            };
        }
    }
}
