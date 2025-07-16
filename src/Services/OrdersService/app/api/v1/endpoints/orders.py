from typing import List
from fastapi import APIRouter, HTTPException, Depends
from sqlalchemy.orm import Session
from app.core.dependencies import DatabaseSession
from app.schemas.order import Order, OrderCreate, OrderUpdate
from app.services.order_service import order_service

router = APIRouter()


@router.post("/", response_model=Order)
def create_order(
    *,
    db: DatabaseSession,
    order_in: OrderCreate,
):
    """Create new order"""
    return order_service.create_order(db, order_in)


@router.get("/", response_model=List[Order])
def read_orders(
    db: DatabaseSession,
    skip: int = 0,
    limit: int = 100,
):
    """Retrieve orders"""
    return order_service.get_orders(db, skip=skip, limit=limit)


@router.get("/{order_id}", response_model=Order)
def read_order(
    *,
    db: DatabaseSession,
    order_id: int,
):
    """Get order by ID"""
    order = order_service.get_order(db, order_id)
    if not order:
        raise HTTPException(status_code=404, detail="Order not found")
    return order


@router.put("/{order_id}", response_model=Order)
def update_order(
    *,
    db: DatabaseSession,
    order_id: int,
    order_in: OrderUpdate,
):
    """Update order"""
    order = order_service.update_order(db, order_id, order_in)
    if not order:
        raise HTTPException(status_code=404, detail="Order not found")
    return order


@router.delete("/{order_id}")
def delete_order(
    *,
    db: DatabaseSession,
    order_id: int,
):
    """Delete order"""
    order = order_service.delete_order(db, order_id)
    if not order:
        raise HTTPException(status_code=404, detail="Order not found")
    return {"message": "Order deleted successfully"}


@router.get("/user/{user_id}", response_model=List[Order])
def read_user_orders(
    *,
    db: DatabaseSession,
    user_id: int,
):
    """Get orders by user ID"""
    return order_service.get_user_orders(db, user_id)