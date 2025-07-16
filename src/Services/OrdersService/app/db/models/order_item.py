from sqlalchemy import Column, Integer, Float, ForeignKey
from sqlalchemy.orm import relationship
from app.db.base import Base
from app.core.config import settings


class OrderItem(Base):
    __tablename__ = "order_items"
    __table_args__ = {"schema": settings.db_schema}

    id = Column(Integer, primary_key=True, index=True)
    order_id = Column(Integer, ForeignKey(f"{settings.db_schema}.orders.id"))
    product_id = Column(Integer)
    quantity = Column(Integer)
    price = Column(Float)
    total_price = Column(Float)

    # Relationships
    order = relationship("Order", back_populates="items")