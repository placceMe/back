from fastapi import APIRouter
from app.api.v1.endpoints import orders, promo_codes

api_router = APIRouter()
api_router.include_router(orders.router, prefix="/orders", tags=["orders"])
api_router.include_router(promo_codes.router, prefix="/promo-codes", tags=["promo-codes"])