from fastapi import APIRouter
from models.schemas import VisionGlobaleModel
from services import mock_data

router = APIRouter()


@router.get("", response_model=VisionGlobaleModel)
def get_vision_globale():
    """Retourne les KPIs, segments, profils conseillers, indicateurs et agences problématiques."""
    return mock_data.get_vision_globale()
