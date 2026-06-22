from pathlib import Path
from fastapi import APIRouter, BackgroundTasks
from models.schemas import SegmentationConfig, SegmentationResult
from services.segmentation_engine import SegmentationEngine
from services import mock_data

router = APIRouter()

UPLOAD_DIR = Path("data/uploads")


def _find_upload(name: str) -> Path | None:
    for ext in (".csv", ".xlsx", ".xls"):
        p = UPLOAD_DIR / f"{name}{ext}"
        if p.exists():
            return p
    return None


@router.post("/run", response_model=SegmentationResult)
def run_segmentation(config: SegmentationConfig):
    """
    Lance le moteur de segmentation avec la configuration fournie.

    - Si les fichiers sources ont été uploadés, le calcul réel est effectué.
    - Sinon, les données mock sont retournées avec un message explicite.

    Corps JSON attendu : `SegmentationConfig`
    (critères approche, part temps commercial, règles affectation)
    """
    engine = SegmentationEngine(
        config=config,
        path_points_de_vente=_find_upload("points_de_vente"),
        path_effectifs=_find_upload("effectifs"),
        path_fonds_de_commerce=_find_upload("fonds_de_commerce"),
    )
    return engine.run()


@router.get("/run/mock", response_model=SegmentationResult)
def run_segmentation_mock():
    """
    Exécute le pipeline avec les données mock (utile pour tester sans fichiers).
    Utilise les règles & hypothèses par défaut.
    """
    regles = mock_data.get_regles_hypotheses()
    config = SegmentationConfig(
        criteres_approche=regles.criteres_approche,
        part_temps_commercial=regles.part_temps_commercial,
        regles_affectation=regles.regles_affectation,
    )
    engine = SegmentationEngine(config=config)
    return engine.run()
