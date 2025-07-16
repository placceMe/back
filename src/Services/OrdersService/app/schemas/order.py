from pydantic import BaseModel
from typing import Optional, List
from datetime import datetime
from app.schemas.order_item import OrderItem, OrderItemCreate
from app.schemas.status import Status
from app.schemas.promo_code import PromoCode


class OrderBase(BaseModel):
    user_id: int
    notes: Optional[str] = None


class OrderCreate(OrderBase):
    items: List[OrderItemCreate]
    promo_code: Optional[str] = None


class OrderUpdate(OrderBase):
    user_id: Optional[int] = None
    status_id: Optional[int] = None
    promo_code_id: Optional[int] = None
    notes: Optional[str] = None


class OrderInDBBase(OrderBase):
    id: int
    status_id: int
    promo_code_id: Optional[int] = None
    total_amount: float
    discount_amount: float
    final_amount: float
    created_at: datetime
    updated_at: Optional[datetime] = None

    class Config:
        from_attributes = True


class Order(OrderInDBBase):
    status: Status
    promo_code: Optional[PromoCode] = None
    items: List[OrderItem] = []


class OrderInDB(OrderInDBBase):
    pass