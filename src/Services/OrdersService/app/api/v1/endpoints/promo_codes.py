from typing import List
from fastapi import APIRouter, HTTPException, Depends
from sqlalchemy.orm import Session
from app.core.dependencies import DatabaseSession
from app.schemas.promo_code import PromoCode, PromoCodeCreate, PromoCodeUpdate
from app.services.promo_code_service import promo_code_service

router = APIRouter()


@router.post("/", response_model=PromoCode)
def create_promo_code(
    *,
    db: DatabaseSession,
    promo_code_in: PromoCodeCreate,
):
    """Create new promo code"""
    return promo_code_service.create_promo_code(db, promo_code_in)


@router.get("/", response_model=List[PromoCode])
def read_promo_codes(
    db: DatabaseSession,
    skip: int = 0,
    limit: int = 100,
):
    """Retrieve promo codes"""
    return promo_code_service.get_promo_codes(db, skip=skip, limit=limit)


@router.get("/{promo_code_id}", response_model=PromoCode)
def read_promo_code(
    *,
    db: DatabaseSession,
    promo_code_id: int,
):
    """Get promo code by ID"""
    promo_code = promo_code_service.get_promo_code(db, promo_code_id)
    if not promo_code:
        raise HTTPException(status_code=404, detail="Promo code not found")
    return promo_code


@router.put("/{promo_code_id}", response_model=PromoCode)
def update_promo_code(
    *,
    db: DatabaseSession,
    promo_code_id: int,
    promo_code_in: PromoCodeUpdate,
):
    """Update promo code"""
    promo_code = promo_code_service.update_promo_code(db, promo_code_id, promo_code_in)
    if not promo_code:
        raise HTTPException(status_code=404, detail="Promo code not found")
    return promo_code


@router.delete("/{promo_code_id}")
def delete_promo_code(
    *,
    db: DatabaseSession,
    promo_code_id: int,
):
    """Delete promo code"""
    promo_code = promo_code_service.delete_promo_code(db, promo_code_id)
    if not promo_code:
        raise HTTPException(status_code=404, detail="Promo code not found")
    return {"message": "Promo code deleted successfully"}


@router.get("/validate/{code}")
def validate_promo_code(
    *,
    db: DatabaseSession,
    code: str,
    order_amount: float,
):
    """Validate promo code"""
    promo_code = promo_code_service.validate_promo_code(db, code, order_amount)
    if not promo_code:
        raise HTTPException(status_code=400, detail="Invalid or expired promo code")
    return {"valid": True, "promo_code": promo_code}