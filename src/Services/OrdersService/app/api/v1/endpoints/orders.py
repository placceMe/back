from typing import List
from fastapi import APIRouter, HTTPException, Depends, status
from sqlalchemy.orm import Session
from app.core.dependencies import DatabaseSession
from app.schemas.order import Order, OrderCreate, OrderUpdate, OrderResponse
from app.services.order_service import OrderService
import logging

logger = logging.getLogger(__name__)

router = APIRouter()


@router.post("/", response_model=OrderResponse, status_code=status.HTTP_201_CREATED)
async def create_order(
    order_data: OrderCreate,
    db: Session = Depends(DatabaseSession),
):
    """Create new order with automatic price calculation"""
    try:
        order_service = OrderService(db)
        order = await order_service.create_order(order_data)
        return OrderResponse(
            id=order.id,
            customer_id=order.customer_id,
            items=[
                {
                    "id": item.id,
                    "product_id": item.product_id,
                    "product_name": item.product_name,
                    "quantity": item.quantity,
                    "price": item.price,
                    "total_price": item.total_price,
                }
                for item in order.items
            ],
            total_amount=order.total_amount,
            discount_amount=order.discount_amount,
            final_amount=order.final_amount,
            status=order.status.name,
            promo_code=order.promo_code,
            delivery_address=order.delivery_address,
            notes=order.notes,
            created_at=order.created_at,
            updated_at=order.updated_at,
        )
    except ValueError as e:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST, detail=str(e)
        )
    except Exception as e:
        logger.error(f"Error creating order: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Internal server error",
        )


@router.get("/{order_id}", response_model=OrderResponse)
async def get_order(
    order_id: int,
    db: Session = Depends(DatabaseSession),
):
    """Get order by ID"""
    order_service = OrderService(db)
    order = order_service.get_order_by_id(order_id)
    if not order:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Order not found",
        )
    return OrderResponse(
        id=order.id,
        customer_id=order.customer_id,
        items=[
            {
                "id": item.id,
                "product_id": item.product_id,
                "product_name": item.product_name,
                "quantity": item.quantity,
                "price": item.price,
                "total_price": item.total_price,
            }
            for item in order.items
        ],
        total_amount=order.total_amount,
        discount_amount=order.discount_amount,
        final_amount=order.final_amount,
        status=order.status.name,
        promo_code=order.promo_code,
        delivery_address=order.delivery_address,
        notes=order.notes,
        created_at=order.created_at,
        updated_at=order.updated_at,
    )


@router.get("/customer/{customer_id}", response_model=List[OrderResponse])
async def get_customer_orders(
    customer_id: int,
    db: Session = Depends(DatabaseSession),
):
    """Get all orders for a customer"""
    order_service = OrderService(db)
    orders = order_service.get_orders_by_customer(customer_id)
    return [
        OrderResponse(
            id=order.id,
            customer_id=order.customer_id,
            items=[
                {
                    "id": item.id,
                    "product_id": item.product_id,
                    "product_name": item.product_name,
                    "quantity": item.quantity,
                    "price": item.price,
                    "total_price": item.total_price,
                }
                for item in order.items
            ],
            total_amount=order.total_amount,
            discount_amount=order.discount_amount,
            final_amount=order.final_amount,
            status=order.status.name,
            promo_code=order.promo_code,
            delivery_address=order.delivery_address,
            notes=order.notes,
            created_at=order.created_at,
            updated_at=order.updated_at,
        )
        for order in orders
    ]


@router.get("/", response_model=List[OrderResponse])
async def get_all_orders(
    db: Session = Depends(DatabaseSession),
):
    """Get all orders"""
    order_service = OrderService(db)
    orders = order_service.get_all_orders()
    return [
        OrderResponse(
            id=order.id,
            customer_id=order.customer_id,
            items=[
                {
                    "id": item.id,
                    "product_id": item.product_id,
                    "product_name": item.product_name,
                    "quantity": item.quantity,
                    "price": item.price,
                    "total_price": item.total_price,
                }
                for item in order.items
            ],
            total_amount=order.total_amount,
            discount_amount=order.discount_amount,
            final_amount=order.final_amount,
            status=order.status.name,
            promo_code=order.promo_code,
            delivery_address=order.delivery_address,
            notes=order.notes,
            created_at=order.created_at,
            updated_at=order.updated_at,
        )
        for order in orders
    ]