from fastapi import FastAPI
from app.api.v1.api_router import api_router
from app.core.config import settings
from app.db.session import engine
from app.db.base_class import Base# Import all models for Alembic
from app.db.models.status import Status
from app.db.models.promo_code import PromoCode
from app.db.models.order import Order
from app.db.models.order_item import OrderItem
from app.db.create_schema import create_schema

import uvicorn
import os

create_schema()

app = FastAPI(title="Orders Service", version="1.0.0")

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