"""
Pydantic schemas — miroir exact des C# models (DashboardModels.cs).
Nommage snake_case côté Python, identique au JSON sérialisé par ASP.NET (camelCase géré par alias).
"""
from __future__ import annotations
from datetime import datetime
from typing import Optional
from pydantic import BaseModel, ConfigDict


# ---------------------------------------------------------------------------
# Vision Globale
# ---------------------------------------------------------------------------

class KpiGlobal(BaseModel):
    total_clients: int
    total_conseillers: int
    taille_portefeuille_moyenne: int


class ClientParSegment(BaseModel):
    segment: str
    nombre_clients: int
    pourcentage: float


class ProfilConseiller(BaseModel):
    profil: str
    nombre_conseillers: int
    pourcentage: float


class TaillePortefeuille(BaseModel):
    profil: str
    clients_par_conseiller: int
    taux_remplissage: float


class IndicateurApplicabilite(BaseModel):
    libelle: str
    valeur: float
    est_positif: bool
    est_pourcentage: bool = True


class AgenceProblematique(BaseModel):
    nom_agence: str
    score_global: int
    niveau_score: str  # "danger" | "warning" | "ok"
    remplissage: float
    concordance_segment: float
    concordance_profil: float
    evolution_effectifs: float
    rotation_clients: float


class VisionGlobaleModel(BaseModel):
    kpis: KpiGlobal
    clients_par_segment: list[ClientParSegment]
    profils_conseillers: list[ProfilConseiller]
    tailles_portefeuilles: list[TaillePortefeuille]
    indicateurs: list[IndicateurApplicabilite]
    agences_problematiques: list[AgenceProblematique]


# ---------------------------------------------------------------------------
# Portefeuilles
# ---------------------------------------------------------------------------

class DimensionnementClientRow(BaseModel):
    segment: str
    mutualise: Optional[int] = None
    dedie: Optional[int] = None
    dedie_haut_de_gamme: Optional[int] = None
    total: int
    is_total: bool = False


class DimensionnementEtpRow(BaseModel):
    libelle: str
    mutualise: Optional[float] = None
    dedie: Optional[float] = None
    dedie_haut_de_gamme: Optional[float] = None
    total: Optional[float] = None
    concordance: Optional[float] = None
    is_header: bool = False
    is_bold: bool = False
    is_ecart: bool = False


# ---------------------------------------------------------------------------
# Règles & Hypothèses
# ---------------------------------------------------------------------------

class CritereApproche(BaseModel):
    segment: str
    frequence_rdv_par_an: int
    duree_rdv_min: int


class PortefeuilleTheoriqueCard(BaseModel):
    profil: str
    clients_par_conseiller: int
    css_class: str


class RegleAffectation(BaseModel):
    profil_portefeuille: str
    correspondance_ideale: str
    autre_correspondance: str
    surcharge_max: int
    sous_charge_max: int


class ReglesHypothesesModel(BaseModel):
    criteres_approche: list[CritereApproche]
    part_temps_commercial: int
    portefeuilles_theoriques: list[PortefeuilleTheoriqueCard]
    regles_affectation: list[RegleAffectation]


# ---------------------------------------------------------------------------
# Données Sources
# ---------------------------------------------------------------------------

class FichierSource(BaseModel):
    titre: str
    description: str
    nom_fichier: str = ""
    est_charge: bool = False
    date_chargement: Optional[datetime] = None


class UploadResponse(BaseModel):
    success: bool
    fichier: str
    lignes: int
    colonnes: list[str]
    message: str


# ---------------------------------------------------------------------------
# Segmentation Engine I/O
# ---------------------------------------------------------------------------

class SegmentationConfig(BaseModel):
    """Paramètres issus de Règles & Hypothèses envoyés avec chaque run."""
    criteres_approche: list[CritereApproche]
    part_temps_commercial: int               # % du temps alloué aux rdv
    jours_travailles_par_an: int = 220
    heures_par_jour: float = 8.0
    regles_affectation: list[RegleAffectation]


class SegmentationResult(BaseModel):
    """Résultat complet renvoyé après exécution du moteur de segmentation."""
    vision_globale: VisionGlobaleModel
    dimensionnement_client_cible: list[DimensionnementClientRow]
    dimensionnement_client_existant: list[DimensionnementClientRow]
    dimensionnement_etp_cible: list[DimensionnementEtpRow]
    dimensionnement_etp_actuel: list[DimensionnementEtpRow]
    regles_hypotheses: ReglesHypothesesModel
    message: str = "Segmentation exécutée avec succès"
