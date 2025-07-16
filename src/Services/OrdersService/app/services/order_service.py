from typing import List
from sqlalchemy.orm import Session
from app.crud.order import crud_order
from app.crud.promo_code import crud_promo_code
from app.schemas.order import OrderCreate, OrderUpdate
from app.db.models.order import Order


class OrderService:
    def create_order(self, db: Session, order_data: OrderCreate) -> Order:
        # Apply promo code if provided
        if order_data.promo_code:
            promo_code = crud_promo_code.get_by_code(db, order_data.promo_code)
            if promo_code and promo_code.is_active:
                # Logic to apply discount
                pass
        
        return crud_order.create(db, order_data)

    def get_order(self, db: Session, order_id: int) -> Order:
        return crud_order.get(db, order_id)

    def get_orders(self, db: Session, skip: int = 0, limit: int = 100) -> List[Order]:
        return crud_order.get_multi(db, skip, limit)

    def get_user_orders(self, db: Session, user_id: int) -> List[Order]:
        return crud_order.get_by_user(db, user_id)

    def update_order(self, db: Session, order_id: int, order_data: OrderUpdate) -> Order:
        db_order = crud_order.get(db, order_id)
        if db_order:
            return crud_order.update(db, db_order, order_data)
        return None

    def delete_order(self, db: Session, order_id: int) -> Order:
        return crud_order.remove(db, order_id)


order_service = OrderService()