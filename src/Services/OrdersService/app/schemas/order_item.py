from pydantic import BaseModel
from typing import Optional
from uuid import UUID


class OrderItemBase(BaseModel):
    product_id: UUID
    quantity: int
    price: float


class OrderItemCreate(OrderItemBase):
    pass


class OrderItemUpdate(OrderItemBase):
    product_id: Optional[UUID] = None
    quantity: Optional[int] = None
    price: Optional[float] = None


class OrderItemInDBBase(OrderItemBase):
    id: int
    order_id: int
    total_price: float

    class Config:
        from_attributes = True


class OrderItem(OrderItemInDBBase):
    pass


class OrderItemInDB(OrderItemInDBBase):
    pass