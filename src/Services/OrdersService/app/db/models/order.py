from sqlalchemy import Column, Integer, Float, DateTime, String, ForeignKey
from sqlalchemy.orm import relationship
from sqlalchemy.sql import func
from app.db.base import Base
from app.core.config import settings


class Order(Base):
    __tablename__ = "orders"
    __table_args__ = {"schema": settings.db_schema}

    id = Column(Integer, primary_key=True, index=True)
    user_id = Column(Integer, index=True)
    status_id = Column(Integer, ForeignKey(f"{settings.db_schema}.statuses.id"))
    promo_code_id = Column(Integer, ForeignKey(f"{settings.db_schema}.promo_codes.id"), nullable=True)
    total_amount = Column(Float)
    discount_amount = Column(Float, default=0)
    final_amount = Column(Float)
    notes = Column(String)
    created_at = Column(DateTime(timezone=True), server_default=func.now())
    updated_at = Column(DateTime(timezone=True), onupdate=func.now())

    # Relationships
    status = relationship("Status")
    promo_code = relationship("PromoCode")
    items = relationship("OrderItem", back_populates="order")