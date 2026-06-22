# CA Segmentation Client — Crédit Agricole Nord de France

Application de segmentation client composée de :
- **Frontend** : .NET 9 Blazor Server (`CA.SegmentationClient/`)
- **Backend** : Python FastAPI (`ca_api/`)

---

## Prérequis

| Outil | Version minimale | Lien |
|-------|-----------------|------|
| .NET SDK | 9.0 | https://dotnet.microsoft.com/download/dotnet/9.0 |
| Python | 3.11+ | https://www.python.org/downloads/ |

---

## Installation

### 1. Frontend Blazor — restaurer les packages NuGet

```bash
dotnet restore CA.SegmentationClient/CA.SegmentationClient.csproj
```

### 2. Backend Python — installer les dépendances

```bash
cd ca_api
pip install -r requirements.txt
```

---

## Lancement

### Frontend (port 5050)

```bash
dotnet run --project CA.SegmentationClient/CA.SegmentationClient.csproj --urls "http://localhost:5050"
```

### Backend (port 8000)

```bash
cd ca_api
uvicorn main:app --reload --port 8000
```

Accès frontend : http://localhost:5050  
Accès API docs : http://localhost:8000/docs
