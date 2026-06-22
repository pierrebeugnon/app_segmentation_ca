import shutil
from datetime import datetime
from pathlib import Path

from fastapi import APIRouter, HTTPException, UploadFile, File
from models.schemas import FichierSource, UploadResponse
from services import mock_data

router = APIRouter()

UPLOAD_DIR = Path("data/uploads")
UPLOAD_DIR.mkdir(parents=True, exist_ok=True)

FICHIERS_ACCEPTES = {
    "points-de-vente":  ("points_de_vente", mock_data.COLONNES_POINTS_DE_VENTE  if hasattr(mock_data, "COLONNES_POINTS_DE_VENTE")  else []),
    "effectifs":        ("effectifs",        []),
    "fonds-de-commerce":("fonds_de_commerce",[]),
}

_etat_fichiers: dict[str, FichierSource] = {
    f.titre.lower().replace(" ", "-"): f
    for f in mock_data.get_fichiers_source()
}


@router.get("", response_model=list[FichierSource])
def get_fichiers():
    """Retourne l'état des fichiers sources (chargés ou non)."""
    return list(_etat_fichiers.values())


@router.post("/upload/{fichier_type}", response_model=UploadResponse)
async def upload_fichier(fichier_type: str, file: UploadFile = File(...)):
    """
    Upload d'un fichier source (CSV ou Excel).

    `fichier_type` : points-de-vente | effectifs | fonds-de-commerce
    """
    if fichier_type not in FICHIERS_ACCEPTES:
        raise HTTPException(
            status_code=400,
            detail=f"Type inconnu : {fichier_type}. Valeurs : {list(FICHIERS_ACCEPTES.keys())}",
        )

    suffix = Path(file.filename or "").suffix.lower()
    if suffix not in (".csv", ".xlsx", ".xls"):
        raise HTTPException(status_code=400, detail="Format non supporté. Acceptés : .csv, .xlsx, .xls")

    dest = UPLOAD_DIR / f"{FICHIERS_ACCEPTES[fichier_type][0]}{suffix}"
    with dest.open("wb") as f:
        shutil.copyfileobj(file.file, f)

    # Lecture rapide pour compter lignes / colonnes
    try:
        import pandas as pd
        if suffix in (".xlsx", ".xls"):
            df = pd.read_excel(dest)
        else:
            df = pd.read_csv(dest, sep=None, engine="python")
        nb_lignes = len(df)
        colonnes  = list(df.columns)
    except Exception as e:
        raise HTTPException(status_code=422, detail=f"Fichier invalide : {e}")

    # Mettre à jour l'état
    cle = fichier_type
    if cle in _etat_fichiers:
        _etat_fichiers[cle].est_charge      = True
        _etat_fichiers[cle].nom_fichier     = file.filename or dest.name
        _etat_fichiers[cle].date_chargement = datetime.now()

    return UploadResponse(
        success=True,
        fichier=file.filename or dest.name,
        lignes=nb_lignes,
        colonnes=colonnes,
        message=f"Fichier '{file.filename}' chargé avec succès ({nb_lignes} lignes).",
    )


@router.delete("/{fichier_type}", status_code=204)
def delete_fichier(fichier_type: str):
    """Supprime un fichier source uploadé."""
    if fichier_type not in FICHIERS_ACCEPTES:
        raise HTTPException(status_code=400, detail=f"Type inconnu : {fichier_type}")

    nom_base = FICHIERS_ACCEPTES[fichier_type][0]
    deleted = False
    for ext in (".csv", ".xlsx", ".xls"):
        p = UPLOAD_DIR / f"{nom_base}{ext}"
        if p.exists():
            p.unlink()
            deleted = True

    if fichier_type in _etat_fichiers:
        _etat_fichiers[fichier_type].est_charge      = False
        _etat_fichiers[fichier_type].nom_fichier     = ""
        _etat_fichiers[fichier_type].date_chargement = None

    if not deleted:
        raise HTTPException(status_code=404, detail="Fichier introuvable")
