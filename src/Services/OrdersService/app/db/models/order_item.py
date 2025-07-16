from sqlalchemy import Column, Integer, String, Numeric, ForeignKey, DateTime
from sqlalchemy.orm import relationship
from sqlalchemy.sql import func
from app.db.base import Base
from app.core.config import settings


class OrderItem(Base):
    __tablename__ = "order_items"
    __table_args__ = {"schema": settings.db_schema}

    id = Column(Integer, primary_key=True, index=True)
    order_id = Column(Integer, ForeignKey(f"{settings.db_schema}.orders.id"), nullable=False)
    product_id = Column(Integer, nullable=False)
    product_name = Column(String(255), nullable=False)
    quantity = Column(Integer, nullable=False)
    price = Column(Numeric(10, 2), nullable=False)  # Ціна за одиницю
    total_price = Column(Numeric(10, 2), nullable=False)  # Загальна ціна за позицію
    created_at = Column(DateTime, default=func.now())
    updated_at = Column(DateTime, default=func.now(), onupdate=func.now())

    # Relationships
    order = relationship("Order", back_populates="items")