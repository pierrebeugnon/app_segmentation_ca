"""
Moteur de segmentation client — Crédit Agricole Nord de France.

Pipeline :
  1. load_data()              → charge les 3 fichiers sources (CSV / Excel)
  2. classify_clients()       → affecte chaque client à un profil de portefeuille
  3. calculate_portfolios()   → taille théorique par profil (règles & hypothèses)
  4. calculate_etp()          → ETP nécessaire vs ETP existant
  5. build_kpis()             → agrégats Vision Globale
  6. run()                    → exécute le pipeline complet → SegmentationResult

Les méthodes marquées TODO sont les points d'extension à implémenter
quand les vrais fichiers seront disponibles.
"""
from __future__ import annotations

import math
from pathlib import Path
from typing import Optional

import pandas as pd

from models.schemas import (
    AgenceProblematique, ClientParSegment, CritereApproche,
    DimensionnementClientRow, DimensionnementEtpRow,
    IndicateurApplicabilite, KpiGlobal, PortefeuilleTheoriqueCard,
    ProfilConseiller, RegleAffectation, ReglesHypothesesModel,
    SegmentationConfig, SegmentationResult, TaillePortefeuille,
    VisionGlobaleModel,
)
from services import mock_data

# ---------------------------------------------------------------------------
# Colonnes attendues dans chaque fichier source
# ---------------------------------------------------------------------------
COLONNES_POINTS_DE_VENTE  = ["code_agence", "nom_agence", "region", "type_pdv"]
COLONNES_EFFECTIFS         = ["code_agence", "nom_conseiller", "profil", "etp"]
COLONNES_FONDS_DE_COMMERCE = ["code_agence", "segment", "nb_clients"]

PROFIL_MAPPING = {
    "Faible Potentiel": "Portefeuilles Mutualisés",
    "Intermédiaire":    "Dédiés",
    "Haut de gamme":    "Dédiés - Haut de Gamme",
}


class SegmentationEngine:
    """
    Initialiser avec les chemins vers les fichiers uploadés et la config.
    Si un fichier est absent, le moteur retourne les données mock.
    """

    def __init__(
        self,
        config: SegmentationConfig,
        path_points_de_vente: Optional[Path] = None,
        path_effectifs: Optional[Path] = None,
        path_fonds_de_commerce: Optional[Path] = None,
    ):
        self.config = config
        self.path_pdv  = path_points_de_vente
        self.path_eff  = path_effectifs
        self.path_fdc  = path_fonds_de_commerce

        self.df_pdv: Optional[pd.DataFrame] = None
        self.df_eff: Optional[pd.DataFrame] = None
        self.df_fdc: Optional[pd.DataFrame] = None

    # ------------------------------------------------------------------
    # 1. Chargement des données
    # ------------------------------------------------------------------

    def load_data(self) -> bool:
        """
        Charge les fichiers sources dans des DataFrames pandas.
        Retourne True si au moins le fonds de commerce est disponible.
        """
        ok = False
        for attr, path, cols in [
            ("df_pdv", self.path_pdv,  COLONNES_POINTS_DE_VENTE),
            ("df_eff", self.path_eff,  COLONNES_EFFECTIFS),
            ("df_fdc", self.path_fdc,  COLONNES_FONDS_DE_COMMERCE),
        ]:
            if path and Path(path).exists():
                df = _read_file(path)
                _validate_columns(df, cols, path)
                setattr(self, attr, df)
                if attr == "df_fdc":
                    ok = True
        return ok

    # ------------------------------------------------------------------
    # 2. Classification des clients
    # ------------------------------------------------------------------

    def classify_clients(self) -> pd.DataFrame:
        """
        TODO: Implémenter la règle de classification métier.

        Logique attendue :
          - Joindre df_fdc + df_pdv sur code_agence
          - Appliquer les seuils de scoring (NBI, épargne, encours…)
            pour affecter chaque client à : Faible Potentiel / Intermédiaire / Haut de gamme
          - Retourner un DataFrame avec colonnes :
            [code_agence, segment_cible, profil_portefeuille_cible, etp_requis]

        Pour l'instant, retourne un DataFrame vide (les données mock prennent le relais).
        """
        if self.df_fdc is None:
            return pd.DataFrame()

        # TODO: remplacer par la vraie logique de scoring
        df = self.df_fdc.copy()
        df["profil_portefeuille"] = df["segment"].map(PROFIL_MAPPING).fillna("Portefeuilles Mutualisés")
        return df

    # ------------------------------------------------------------------
    # 3. Tailles théoriques des portefeuilles
    # ------------------------------------------------------------------

    def calculate_portfolios(self) -> list[PortefeuilleTheoriqueCard]:
        """
        Calcule la taille théorique de chaque portefeuille à partir des règles.

        Formule :
          minutes_dispo = jours × heures × 60 × (part_temps / 100)
          taille = minutes_dispo / (frequence × duree)
        """
        minutes_dispo = (
            self.config.jours_travailles_par_an
            * self.config.heures_par_jour
            * 60
            * (self.config.part_temps_commercial / 100)
        )

        mapping = {
            "Portefeuilles Mutualisés": "Faible Potentiel",
            "Dédiés":                   "Intermédiaire",
            "Dédiés - Haut de Gamme":  "Haut de gamme",
        }
        css_map = {
            "Portefeuilles Mutualisés": "card-mutualise",
            "Dédiés":                   "card-dedie",
            "Dédiés - Haut de Gamme":  "card-hdg",
        }

        criteres_index = {c.segment: c for c in self.config.criteres_approche}
        result = []
        for profil, segment in mapping.items():
            c = criteres_index.get(segment)
            if c and c.frequence_rdv_par_an > 0 and c.duree_rdv_min > 0:
                taille = int(minutes_dispo / (c.frequence_rdv_par_an * c.duree_rdv_min))
            else:
                taille = 0
            result.append(PortefeuilleTheoriqueCard(profil=profil, clients_par_conseiller=taille, css_class=css_map[profil]))
        return result

    # ------------------------------------------------------------------
    # 4. Calcul ETP
    # ------------------------------------------------------------------

    def calculate_etp(
        self,
        df_classified: pd.DataFrame,
        portefeuilles: list[PortefeuilleTheoriqueCard],
    ) -> tuple[list[DimensionnementEtpRow], list[DimensionnementEtpRow]]:
        """
        TODO: Implémenter à partir des DataFrames réels.

        Logique attendue :
          - Pour chaque profil de portefeuille :
              etp_requis = nb_clients_affectés / taille_théorique
          - Comparer avec l'ETP existant (df_eff agrégé par profil)
          - Calculer les écarts et taux de concordance

        Pour l'instant, retourne les données mock.
        """
        return (
            mock_data.get_dimensionnement_etp_cible(),
            mock_data.get_dimensionnement_etp_actuel(),
        )

    # ------------------------------------------------------------------
    # 5. KPIs Vision Globale
    # ------------------------------------------------------------------

    def build_kpis(self, df_classified: pd.DataFrame) -> VisionGlobaleModel:
        """
        TODO: Calculer les KPIs depuis les DataFrames réels.
        Pour l'instant, retourne les données mock.
        """
        return mock_data.get_vision_globale()

    # ------------------------------------------------------------------
    # 6. Pipeline complet
    # ------------------------------------------------------------------

    def run(self) -> SegmentationResult:
        """
        Exécute le pipeline complet et retourne un SegmentationResult.
        Si les données sources sont absentes, retourne les données mock.
        """
        has_real_data = self.load_data()

        portefeuilles = self.calculate_portfolios()

        if has_real_data:
            df_classified = self.classify_clients()
            vision = self.build_kpis(df_classified)
            etp_cible, etp_actuel = self.calculate_etp(df_classified, portefeuilles)

            # TODO: construire les dimensionnements client depuis df_classified
            dim_client_cible    = mock_data.get_dimensionnement_client_cible()
            dim_client_existant = mock_data.get_dimensionnement_client_existant()
        else:
            vision              = mock_data.get_vision_globale()
            etp_cible           = mock_data.get_dimensionnement_etp_cible()
            etp_actuel          = mock_data.get_dimensionnement_etp_actuel()
            dim_client_cible    = mock_data.get_dimensionnement_client_cible()
            dim_client_existant = mock_data.get_dimensionnement_client_existant()

        regles = ReglesHypothesesModel(
            criteres_approche=self.config.criteres_approche,
            part_temps_commercial=self.config.part_temps_commercial,
            portefeuilles_theoriques=portefeuilles,
            regles_affectation=self.config.regles_affectation,
        )

        return SegmentationResult(
            vision_globale=vision,
            dimensionnement_client_cible=dim_client_cible,
            dimensionnement_client_existant=dim_client_existant,
            dimensionnement_etp_cible=etp_cible,
            dimensionnement_etp_actuel=etp_actuel,
            regles_hypotheses=regles,
            message="Données mock — uploadez les fichiers sources pour lancer le vrai calcul" if not has_real_data else "Segmentation calculée depuis les données sources",
        )


# ---------------------------------------------------------------------------
# Helpers internes
# ---------------------------------------------------------------------------

def _read_file(path: Path) -> pd.DataFrame:
    suffix = Path(path).suffix.lower()
    if suffix in (".xlsx", ".xls"):
        return pd.read_excel(path)
    return pd.read_csv(path, sep=None, engine="python")


def _validate_columns(df: pd.DataFrame, expected: list[str], path: Path) -> None:
    missing = [c for c in expected if c not in df.columns]
    if missing:
        raise ValueError(f"Fichier {path.name} — colonnes manquantes : {missing}")
