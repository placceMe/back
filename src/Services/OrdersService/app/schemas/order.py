from pydantic import BaseModel, Field
from typing import Optional, List
from datetime import datetime
from decimal import Decimal
from uuid import UUID
from app.schemas.order_item import OrderItem, OrderItemCreate
from app.schemas.status import Status
from app.schemas.promo_code import PromoCode


class OrderItemCreate(BaseModel):
    product_id: UUID = Field(..., description="ID товару")
    quantity: int = Field(..., gt=0, description="Кількість товару")


class OrderBase(BaseModel):
    customer_id: UUID
    notes: Optional[str] = None


class OrderCreate(OrderBase):
    items: List[OrderItemCreate]
    promo_code: Optional[str] = None
    delivery_address: str = Field(..., min_length=1, description="Адреса доставки")


class OrderUpdate(OrderBase):
    customer_id: Optional[UUID] = None
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


class OrderItemResponse(BaseModel):
    id: int
    product_id: UUID
    product_name: str
    quantity: int
    price: Decimal
    total_price: Decimal


class OrderResponse(BaseModel):
    id: int
    customer_id: UUID
    items: List[OrderItemResponse]
    total_amount: Decimal
    discount_amount: Decimal
    final_amount: Decimal
    status: str
    promo_code: Optional[str]
    delivery_address: str
    notes: Optional[str]
    created_at: datetime
    updated_at: datetime

    class Config:
        orm_mode = True