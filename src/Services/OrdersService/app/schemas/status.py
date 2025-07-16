from pydantic import BaseModel
from typing import Optional
from datetime import datetime


class StatusBase(BaseModel):
    name: str
    description: Optional[str] = None
    is_active: bool = True


class StatusCreate(StatusBase):
    pass


class StatusUpdate(StatusBase):
    pass


class StatusInDBBase(StatusBase):
    id: int
    created_at: datetime
    updated_at: Optional[datetime] = None

    class Config:
        from_attributes = True


class Status(StatusInDBBase):
    pass


class StatusInDB(StatusInDBBase):
    pass