from fastapi import APIRouter
from models.schemas import DimensionnementClientRow, DimensionnementEtpRow
from services import mock_data

router = APIRouter()


@router.get("/vision-client/cible", response_model=list[DimensionnementClientRow])
def get_vision_client_cible():
    """Dimensionnement cible des portefeuilles en nombre de clients."""
    return mock_data.get_dimensionnement_client_cible()


@router.get("/vision-client/existant", response_model=list[DimensionnementClientRow])
def get_vision_client_existant():
    """Dimensionnement existant des portefeuilles en nombre de clients."""
    return mock_data.get_dimensionnement_client_existant()


@router.get("/vision-etp/cible", response_model=list[DimensionnementEtpRow])
def get_vision_etp_cible():
    """Dimensionnement cible des portefeuilles en ETP."""
    return mock_data.get_dimensionnement_etp_cible()


@router.get("/vision-etp/actuel", response_model=list[DimensionnementEtpRow])
def get_vision_etp_actuel():
    """Dimensionnement actuel des portefeuilles en ETP."""
    return mock_data.get_dimensionnement_etp_actuel()
