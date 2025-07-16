from pydantic import BaseModel
from typing import Optional


class OrderItemBase(BaseModel):
    product_id: int
    quantity: int
    price: float


class OrderItemCreate(OrderItemBase):
    pass


class OrderItemUpdate(OrderItemBase):
    product_id: Optional[int] = None
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