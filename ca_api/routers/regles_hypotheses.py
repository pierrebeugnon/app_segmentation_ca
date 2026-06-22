from fastapi import APIRouter
from models.schemas import ReglesHypothesesModel, PortefeuilleTheoriqueCard
from services import mock_data
from services.segmentation_engine import SegmentationEngine
from models.schemas import SegmentationConfig

router = APIRouter()

_current_regles: ReglesHypothesesModel = mock_data.get_regles_hypotheses()


@router.get("", response_model=ReglesHypothesesModel)
def get_regles_hypotheses():
    """Retourne la configuration courante des règles et hypothèses."""
    return _current_regles


@router.put("", response_model=ReglesHypothesesModel)
def save_regles_hypotheses(body: ReglesHypothesesModel):
    """
    Sauvegarde les règles et hypothèses et recalcule les tailles théoriques.
    Les nouvelles tailles sont recalculées à la volée via le moteur.
    """
    global _current_regles

    config = SegmentationConfig(
        criteres_approche=body.criteres_approche,
        part_temps_commercial=body.part_temps_commercial,
        regles_affectation=body.regles_affectation,
    )
    engine = SegmentationEngine(config=config)
    portefeuilles_recalcules: list[PortefeuilleTheoriqueCard] = engine.calculate_portfolios()

    _current_regles = ReglesHypothesesModel(
        criteres_approche=body.criteres_approche,
        part_temps_commercial=body.part_temps_commercial,
        portefeuilles_theoriques=portefeuilles_recalcules,
        regles_affectation=body.regles_affectation,
    )
    return _current_regles
