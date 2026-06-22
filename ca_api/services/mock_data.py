"""
Données mock — miroir de MockDataService.cs.
Remplacées progressivement par les vraies données issues des fichiers uploadés.
"""
from models.schemas import (
    KpiGlobal, ClientParSegment, ProfilConseiller, TaillePortefeuille,
    IndicateurApplicabilite, AgenceProblematique, VisionGlobaleModel,
    DimensionnementClientRow, DimensionnementEtpRow,
    CritereApproche, PortefeuilleTheoriqueCard, RegleAffectation,
    ReglesHypothesesModel, FichierSource,
)


def get_vision_globale() -> VisionGlobaleModel:
    return VisionGlobaleModel(
        kpis=KpiGlobal(
            total_clients=1_000_000,
            total_conseillers=1_200,
            taille_portefeuille_moyenne=833,
        ),
        clients_par_segment=[
            ClientParSegment(segment="Faible Potentiel",  nombre_clients=700_000, pourcentage=70.0),
            ClientParSegment(segment="Intermédiaires",    nombre_clients=177_000, pourcentage=17.7),
            ClientParSegment(segment="Haut de Gamme",     nombre_clients=123_000, pourcentage=12.3),
        ],
        profils_conseillers=[
            ProfilConseiller(profil="Portefeuilles Mutualisés",     nombre_conseillers=750, pourcentage=62.5),
            ProfilConseiller(profil="Dédiés",                       nombre_conseillers=300, pourcentage=25.0),
            ProfilConseiller(profil="Dédiés – Haut de Gamme",       nombre_conseillers=150, pourcentage=12.5),
        ],
        tailles_portefeuilles=[
            TaillePortefeuille(profil="Portefeuilles Mutualisés",   clients_par_conseiller=1_333, taux_remplissage=89.0),
            TaillePortefeuille(profil="Dédiés",                     clients_par_conseiller=865,   taux_remplissage=92.0),
            TaillePortefeuille(profil="Dédiés – Haut de Gamme",     clients_par_conseiller=147,   taux_remplissage=78.0),
        ],
        indicateurs=[
            IndicateurApplicabilite(libelle="Taux de concordance par segment client",  valeur=87.0, est_positif=True),
            IndicateurApplicabilite(libelle="Taux de concordance par profil conseiller", valeur=92.0, est_positif=True),
            IndicateurApplicabilite(libelle="Taux d'évolution des effectifs",           valeur=-5.3, est_positif=False, est_pourcentage=False),
            IndicateurApplicabilite(libelle="Taux de rotation des clients",             valeur=5.2,  est_positif=False),
        ],
        agences_problematiques=[
            AgenceProblematique(nom_agence="Lille Nord",  score_global=41, niveau_score="danger",  remplissage=62, concordance_segment=58, concordance_profil=61, evolution_effectifs=-8.2, rotation_clients=9.1),
            AgenceProblematique(nom_agence="Roubaix",     score_global=56, niveau_score="warning", remplissage=69, concordance_segment=62, concordance_profil=64, evolution_effectifs=7.8,  rotation_clients=8.4),
            AgenceProblematique(nom_agence="Dunkerque",   score_global=58, niveau_score="warning", remplissage=69, concordance_segment=66, concordance_profil=67, evolution_effectifs=-7.1, rotation_clients=7.8),
            AgenceProblematique(nom_agence="Valenciennes",score_global=61, niveau_score="warning", remplissage=71, concordance_segment=68, concordance_profil=66, evolution_effectifs=6.4,  rotation_clients=7.2),
            AgenceProblematique(nom_agence="Calais",      score_global=63, niveau_score="warning", remplissage=73, concordance_segment=69, concordance_profil=70, evolution_effectifs=5.9,  rotation_clients=6.8),
        ],
    )


def get_dimensionnement_client_cible() -> list[DimensionnementClientRow]:
    return [
        DimensionnementClientRow(segment="Faible Potentiel", mutualise=180_000, dedie=None,    dedie_haut_de_gamme=None,   total=200_000),
        DimensionnementClientRow(segment="Intermédiaires",   mutualise=77_800,  dedie=661_300, dedie_haut_de_gamme=38_900, total=778_000),
        DimensionnementClientRow(segment="Haut de Gamme",    mutualise=None,    dedie=4_400,   dedie_haut_de_gamme=17_600, total=22_000),
        DimensionnementClientRow(segment="Total",            mutualise=267_800, dedie=675_700, dedie_haut_de_gamme=56_500, total=1_000_000, is_total=True),
    ]


def get_dimensionnement_client_existant() -> list[DimensionnementClientRow]:
    return [
        DimensionnementClientRow(segment="Faible Potentiel", mutualise=None, dedie=185_000, dedie_haut_de_gamme=None,   total=185_000),
        DimensionnementClientRow(segment="Intermédiaires",   mutualise=None, dedie=755_000, dedie_haut_de_gamme=38_000, total=793_000),
        DimensionnementClientRow(segment="Haut de Gamme",    mutualise=None, dedie=4_000,   dedie_haut_de_gamme=18_000, total=22_000),
        DimensionnementClientRow(segment="Total",            mutualise=None, dedie=944_000, dedie_haut_de_gamme=56_000, total=1_000_000, is_total=True),
    ]


def get_dimensionnement_etp_cible() -> list[DimensionnementEtpRow]:
    return [
        DimensionnementEtpRow(libelle="Faible Potentiel",       mutualise=0.88, dedie=0.02, dedie_haut_de_gamme=None, total=0.70,  concordance=92.0),
        DimensionnementEtpRow(libelle="Intermédiaires",          mutualise=0.01, dedie=4.33, dedie_haut_de_gamme=0.08, total=4.41, concordance=87.0),
        DimensionnementEtpRow(libelle="Haut de Gamme",           mutualise=None, dedie=0.06, dedie_haut_de_gamme=0.67, total=0.73, concordance=97.0),
        DimensionnementEtpRow(libelle="Charge non-commerciale",  mutualise=None, dedie=0.15, dedie_haut_de_gamme=None, total=0.25),
        DimensionnementEtpRow(libelle="Charge ETP",              mutualise=0.70, dedie=4.40, dedie_haut_de_gamme=0.75, total=5.83, is_bold=True),
        DimensionnementEtpRow(libelle="Effectif Cible",          mutualise=0.75, dedie=4.50, dedie_haut_de_gamme=0.75, total=6.00, is_bold=True),
        DimensionnementEtpRow(libelle="Effectif Cible – Charge ETP", mutualise=0.65, dedie=0.30, dedie_haut_de_gamme=0.03, total=0.15, is_ecart=True),
        DimensionnementEtpRow(libelle="Effectif Actuel",         mutualise=0.78, dedie=4.45, dedie_haut_de_gamme=0.78, total=6.00, is_bold=True),
        DimensionnementEtpRow(libelle="Écart : Cible – Actuel",  mutualise=-0.03, dedie=0.05, dedie_haut_de_gamme=-0.03, total=0.0, is_ecart=True),
        DimensionnementEtpRow(libelle="Concordance par profil",  mutualise=None, dedie=None, dedie_haut_de_gamme=None, concordance=None, mutualise_concordance=94, dedie_concordance=93, hdg_concordance=96),  # type: ignore
    ]


def get_dimensionnement_etp_actuel() -> list[DimensionnementEtpRow]:
    return [
        DimensionnementEtpRow(libelle="Faible Potentiel",       mutualise=0.72, dedie=0.02, dedie_haut_de_gamme=None, total=0.74, concordance=87.0),
        DimensionnementEtpRow(libelle="Intermédiaires",          mutualise=0.03, dedie=4.25, dedie_haut_de_gamme=0.06, total=4.34, concordance=92.0),
        DimensionnementEtpRow(libelle="Haut de Gamme",           mutualise=None, dedie=0.08, dedie_haut_de_gamme=0.70, total=0.76, concordance=86.0),
        DimensionnementEtpRow(libelle="Charge non-commerciale",  mutualise=None, dedie=0.15, dedie_haut_de_gamme=None, total=0.25),
        DimensionnementEtpRow(libelle="Charge ETP",              mutualise=0.74, dedie=4.33, dedie_haut_de_gamme=0.76, total=5.81, is_bold=True),
        DimensionnementEtpRow(libelle="Effectif Actuel",         mutualise=0.78, dedie=4.45, dedie_haut_de_gamme=0.78, total=6.00, is_bold=True),
        DimensionnementEtpRow(libelle="Effectif Actuel – Charge ETP", mutualise=0.04, dedie=0.13, dedie_haut_de_gamme=0.02, total=0.18, is_ecart=True),
        DimensionnementEtpRow(libelle="Concordance par profil",  concordance=None, mutualise=None, dedie=None, dedie_haut_de_gamme=None),
    ]


def get_regles_hypotheses() -> ReglesHypothesesModel:
    criteres = [
        CritereApproche(segment="Faible Potentiel", frequence_rdv_par_an=1,  duree_rdv_min=30),
        CritereApproche(segment="Intermédiaire",    frequence_rdv_par_an=2,  duree_rdv_min=45),
        CritereApproche(segment="Haut de gamme",    frequence_rdv_par_an=4,  duree_rdv_min=60),
    ]
    part_temps = 70
    minutes_par_an = 220 * 8 * 60
    portefeuilles = []
    mapping = {
        "Portefeuilles Mutualisés": ("Faible Potentiel", "card-mutualise"),
        "Dédiés":                   ("Intermédiaire",    "card-dedie"),
        "Dédiés - Haut de Gamme":  ("Haut de gamme",    "card-hdg"),
    }
    for profil, (seg, css) in mapping.items():
        c = next(x for x in criteres if x.segment == seg)
        clients = int((minutes_par_an * part_temps / 100) / (c.frequence_rdv_par_an * c.duree_rdv_min))
        portefeuilles.append(PortefeuilleTheoriqueCard(profil=profil, clients_par_conseiller=clients, css_class=css))

    regles = [
        RegleAffectation(profil_portefeuille="Portefeuilles Mutualisés", correspondance_ideale="Faible Potentiel",  autre_correspondance="Intermédiaires", surcharge_max=20, sous_charge_max=15),
        RegleAffectation(profil_portefeuille="Dédiés",                   correspondance_ideale="Intermédiaires",    autre_correspondance="Faible Potentiel", surcharge_max=15, sous_charge_max=10),
        RegleAffectation(profil_portefeuille="Dédiés - Haut de Gamme",  correspondance_ideale="Haut de gamme",     autre_correspondance="Intermédiaires",   surcharge_max=10, sous_charge_max=10),
    ]
    return ReglesHypothesesModel(
        criteres_approche=criteres,
        part_temps_commercial=part_temps,
        portefeuilles_theoriques=portefeuilles,
        regles_affectation=regles,
    )


def get_fichiers_source() -> list[FichierSource]:
    return [
        FichierSource(titre="Points de vente",   description="Liste des agences, codes et régions"),
        FichierSource(titre="Effectifs",          description="Conseillers par agence et profil (ETP)"),
        FichierSource(titre="Fonds de commerce",  description="Clients par agence et segment"),
    ]
