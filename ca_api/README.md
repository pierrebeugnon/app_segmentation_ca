# CA Segmentation Client — API Python

Backend FastAPI pour le dimensionnement des portefeuilles — **Crédit Agricole Nord de France**.

## Stack

| Composant | Technologie |
|-----------|-------------|
| Framework | FastAPI 0.115 |
| Serveur   | Uvicorn      |
| Data      | Pandas + NumPy |
| Formats   | CSV, Excel (.xlsx/.xls) |

## Installation

```bash
cd ca_api
python -m venv .venv
.venv\Scripts\activate        # Windows
pip install -r requirements.txt
```

## Démarrage

```bash
uvicorn main:app --reload --port 8000
```

Swagger UI : **http://localhost:8000/docs**

## Endpoints principaux

| Méthode | Route | Description |
|---------|-------|-------------|
| GET  | `/api/vision-globale` | KPIs, segments, indicateurs |
| GET  | `/api/portefeuilles/vision-client/cible` | Dimensionnement client cible |
| GET  | `/api/portefeuilles/vision-client/existant` | Dimensionnement client existant |
| GET  | `/api/portefeuilles/vision-etp/cible` | Dimensionnement ETP cible |
| GET  | `/api/portefeuilles/vision-etp/actuel` | Dimensionnement ETP actuel |
| GET  | `/api/regles-hypotheses` | Règles & hypothèses courantes |
| PUT  | `/api/regles-hypotheses` | Sauvegarder les règles |
| GET  | `/api/donnees-sources` | État des fichiers sources |
| POST | `/api/donnees-sources/upload/{type}` | Uploader un fichier |
| DELETE | `/api/donnees-sources/{type}` | Supprimer un fichier |
| POST | `/api/segmentation/run` | **Lancer le moteur de segmentation** |
| GET  | `/api/segmentation/run/mock` | Run avec données mock |

## Fichiers sources attendus

| Type (`fichier_type`) | Colonnes requises |
|-----------------------|------------------|
| `points-de-vente`     | `code_agence`, `nom_agence`, `region`, `type_pdv` |
| `effectifs`           | `code_agence`, `nom_conseiller`, `profil`, `etp` |
| `fonds-de-commerce`   | `code_agence`, `segment`, `nb_clients` |

## Structure du projet

```
ca_api/
├── main.py                       ← Application FastAPI + CORS
├── requirements.txt
├── models/
│   └── schemas.py                ← Pydantic (miroir de DashboardModels.cs)
├── routers/
│   ├── vision_globale.py
│   ├── portefeuilles.py
│   ├── regles_hypotheses.py
│   ├── donnees_sources.py
│   └── segmentation.py           ← Endpoint principal
├── services/
│   ├── mock_data.py              ← Données mock (miroir de MockDataService.cs)
│   └── segmentation_engine.py   ← Moteur de calcul (squelette + TODO)
└── data/uploads/                 ← Fichiers uploadés
```

## Brancher le vrai algorithme

Les méthodes `TODO` dans `services/segmentation_engine.py` sont les points d'extension :

1. `classify_clients()` — appliquer les règles de scoring métier sur `df_fdc`
2. `calculate_etp()` — calculer les ETP requis vs existants depuis `df_eff`
3. `build_kpis()` — agréger les indicateurs Vision Globale depuis les DataFrames réels
