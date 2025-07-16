from fastapi import APIRouter
from app.api.v1.endpoints.orders import router as orders_router
from app.api.v1.endpoints.promo_codes import router as promo_codes_router

api_router = APIRouter()
api_router.include_router(orders_router, prefix="/orders", tags=["orders"])
api_router.include_router(promo_codes_router, prefix="/promo-codes", tags=["promo-codes"])
# ...add your route includes here...
