from sqlalchemy import Column, Integer, String, Numeric, ForeignKey, DateTime
from sqlalchemy.dialects.postgresql import UUID
from sqlalchemy.orm import relationship
from sqlalchemy.sql import func
from app.db.base_class import Base
from app.core.config import settings
import uuid


class Order(Base):
    __tablename__ = "orders"
    __table_args__ = {"schema": settings.db_schema}

    id = Column(Integer, primary_key=True, index=True)
    customer_id = Column(UUID(as_uuid=True), nullable=False)
    total_amount = Column(Numeric(10, 2), nullable=False)  # Сума без знижки
    discount_amount = Column(Numeric(10, 2), default=0.00)  # Сума знижки
    final_amount = Column(Numeric(10, 2), nullable=False)  # Підсумкова сума
    status_id = Column(Integer, ForeignKey(f"{settings.db_schema}.statuses.id"))
    promo_code = Column(String(50), nullable=True)
    delivery_address = Column(String(500), nullable=False)
    notes = Column(String(1000), nullable=True)
    created_at = Column(DateTime, default=func.now())
    updated_at = Column(DateTime, default=func.now(), onupdate=func.now())

    # Relationships
    status = relationship("Status", back_populates="orders")
    items = relationship("OrderItem", back_populates="order")