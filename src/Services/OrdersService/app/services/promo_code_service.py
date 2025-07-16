from typing import List
from sqlalchemy.orm import Session
from app.crud.promo_code import crud_promo_code
from app.schemas.promo_code import PromoCodeCreate, PromoCodeUpdate
from app.db.models.promo_code import PromoCode


class PromoCodeService:
    def create_promo_code(self, db: Session, promo_data: PromoCodeCreate) -> PromoCode:
        return crud_promo_code.create(db, promo_data)

    def get_promo_code(self, db: Session, promo_id: int) -> PromoCode:
        return crud_promo_code.get(db, promo_id)

    def get_promo_codes(self, db: Session, skip: int = 0, limit: int = 100) -> List[PromoCode]:
        return crud_promo_code.get_multi(db, skip, limit)

    def validate_promo_code(self, db: Session, code: str, order_amount: float) -> PromoCode:
        promo_code = crud_promo_code.get_by_code(db, code)
        if not promo_code:
            return None
        
        if not promo_code.is_active:
            return None
        
        if promo_code.min_order_amount > order_amount:
            return None
        
        if promo_code.max_uses and promo_code.current_uses >= promo_code.max_uses:
            return None
        
        return promo_code

    def update_promo_code(self, db: Session, promo_id: int, promo_data: PromoCodeUpdate) -> PromoCode:
        db_promo = crud_promo_code.get(db, promo_id)
        if db_promo:
            return crud_promo_code.update(db, db_promo, promo_data)
        return None

    def delete_promo_code(self, db: Session, promo_id: int) -> PromoCode:
        return crud_promo_code.remove(db, promo_id)


promo_code_service = PromoCodeService()