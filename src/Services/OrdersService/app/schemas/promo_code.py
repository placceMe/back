from pydantic import BaseModel
from typing import Optional
from datetime import datetime


class PromoCodeBase(BaseModel):
    code: str
    discount_percentage: Optional[float] = None
    discount_amount: Optional[float] = None
    min_order_amount: float = 0
    max_uses: Optional[int] = None
    expires_at: Optional[datetime] = None
    is_active: bool = True


class PromoCodeCreate(PromoCodeBase):
    pass


class PromoCodeUpdate(PromoCodeBase):
    code: Optional[str] = None


class PromoCodeInDBBase(PromoCodeBase):
    id: int
    current_uses: int
    created_at: datetime
    updated_at: Optional[datetime] = None

    class Config:
        from_attributes = True


class PromoCode(PromoCodeInDBBase):
    pass


class PromoCodeInDB(PromoCodeInDBBase):
    pass