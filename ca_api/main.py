from contextlib import asynccontextmanager
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from routers import vision_globale, portefeuilles, regles_hypotheses, donnees_sources, segmentation


@asynccontextmanager
async def lifespan(app: FastAPI):
    print("CA Segmentation API démarrée — http://localhost:8000/docs")
    yield
    print("Arrêt de l'API")


app = FastAPI(
    title="CA Segmentation Client API",
    description="API de dimensionnement des portefeuilles — Crédit Agricole Nord de France",
    version="1.0.0",
    lifespan=lifespan,
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:5050", "http://localhost:5001", "http://localhost:3000", "*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(vision_globale.router,    prefix="/api/vision-globale",     tags=["Vision Globale"])
app.include_router(portefeuilles.router,     prefix="/api/portefeuilles",       tags=["Portefeuilles"])
app.include_router(regles_hypotheses.router, prefix="/api/regles-hypotheses",   tags=["Règles & Hypothèses"])
app.include_router(donnees_sources.router,   prefix="/api/donnees-sources",     tags=["Données Sources"])
app.include_router(segmentation.router,      prefix="/api/segmentation",        tags=["Segmentation"])


@app.get("/", tags=["Health"])
def health_check():
    return {
        "status": "ok",
        "service": "CA Segmentation Client API",
        "version": "1.0.0",
    }
