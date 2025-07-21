from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from app.api.v1.api_router import api_router
from app.core.config import settings
from app.db.session import engine
from app.db.base_class import Base
# Import all models for Alembic
from app.db.models.status import Status
from app.db.models.promo_code import PromoCode
from app.db.models.order import Order
from app.db.models.order_item import OrderItem
from app.db.create_schema import create_schema

import uvicorn
import os
import subprocess
import logging

logger = logging.getLogger(__name__)

# Ensure schema exists before starting the application
try:
    create_schema()
    logger.info("Schema creation completed successfully")
except Exception as e:
    logger.warning(f"Could not create schema - {e}. This might be normal if schema already exists.")

# Run Alembic migrations
# Updated migration code
try:
    logger.info("Running database migrations...")
    # First check migration history
    check_result = subprocess.run(
        ["alembic", "history"],
        cwd="/app",
        capture_output=True,
        text=True
    )

    if check_result.returncode != 0:
        logger.warning(f"Migration history check failed: {check_result.stderr}")
        # Try to stamp the current head to fix versioning
        stamp_result = subprocess.run(
            ["alembic", "stamp", "head"],
            cwd="/app",
            capture_output=True,
            text=True
        )
        logger.info(f"Attempted to stamp head: {'Success' if stamp_result.returncode == 0 else 'Failed'}")

    # Now run the upgrade
    result = subprocess.run(
        ["alembic", "upgrade", "head"],
        cwd="/app",
        capture_output=True,
        text=True
    )
    if result.returncode == 0:
        logger.info("Database migrations completed successfully")
    else:
        logger.error(f"Migration failed: {result.stderr}")
except Exception as e:
    logger.error(f"Error running migrations: {e}")

app = FastAPI(title="Orders Service", version="1.0.0")

# Add CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Allow all origins for development
    allow_credentials=True,
    allow_methods=["*"],  # Allow all methods
    allow_headers=["*"],  # Allow all headers
)

# Include API router
app.include_router(api_router, prefix="/api")

@app.get("/")
async def root():
    return {"message": "Orders Service", "version": "1.0.0"}

@app.get("/health")
async def health_check():
    return {"status": "healthy", "service": "orders-service"}

if __name__ == "__main__":
    port = int(os.getenv("PORT", 8000))
    uvicorn.run(app, host="0.0.0.0", port=port)