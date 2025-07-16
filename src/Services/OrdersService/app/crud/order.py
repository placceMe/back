from typing import List, Optional
from sqlalchemy.orm import Session
from app.db.models.order import Order
from app.db.models.order_item import OrderItem
from app.schemas.order import OrderCreate, OrderUpdate


class CRUDOrder:
    def get(self, db: Session, id: int) -> Optional[Order]:
        return db.query(Order).filter(Order.id == id).first()

    def get_multi(self, db: Session, skip: int = 0, limit: int = 100) -> List[Order]:
        return db.query(Order).offset(skip).limit(limit).all()

    def get_by_user(self, db: Session, user_id: int) -> List[Order]:
        return db.query(Order).filter(Order.user_id == user_id).all()

    def create(self, db: Session, obj_in: OrderCreate) -> Order:
        # Calculate total amount
        total_amount = sum(item.price * item.quantity for item in obj_in.items)
        
        db_obj = Order(
            user_id=obj_in.user_id,
            status_id=1,  # Default status
            total_amount=total_amount,
            discount_amount=0,
            final_amount=total_amount,
            notes=obj_in.notes
        )
        db.add(db_obj)
        db.commit()
        db.refresh(db_obj)
        
        # Create order items
        for item in obj_in.items:
            db_item = OrderItem(
                order_id=db_obj.id,
                product_id=item.product_id,
                quantity=item.quantity,
                price=item.price,
                total_price=item.price * item.quantity
            )
            db.add(db_item)
        
        db.commit()
        db.refresh(db_obj)
        return db_obj

    def update(self, db: Session, db_obj: Order, obj_in: OrderUpdate) -> Order:
        if obj_in.user_id is not None:
            db_obj.user_id = obj_in.user_id
        if obj_in.status_id is not None:
            db_obj.status_id = obj_in.status_id
        if obj_in.promo_code_id is not None:
            db_obj.promo_code_id = obj_in.promo_code_id
        if obj_in.notes is not None:
            db_obj.notes = obj_in.notes
        
        db.add(db_obj)
        db.commit()
        db.refresh(db_obj)
        return db_obj

    def remove(self, db: Session, id: int) -> Order:
        obj = db.query(Order).get(id)
        db.delete(obj)
        db.commit()
        return obj


crud_order = CRUDOrder()