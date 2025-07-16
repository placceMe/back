from typing import List, Optional
from sqlalchemy.orm import Session
from app.db.models.promo_code import PromoCode
from app.schemas.promo_code import PromoCodeCreate, PromoCodeUpdate


class CRUDPromoCode:
    def get(self, db: Session, id: int) -> Optional[PromoCode]:
        return db.query(PromoCode).filter(PromoCode.id == id).first()

    def get_by_code(self, db: Session, code: str) -> Optional[PromoCode]:
        return db.query(PromoCode).filter(PromoCode.code == code).first()

    def get_multi(self, db: Session, skip: int = 0, limit: int = 100) -> List[PromoCode]:
        return db.query(PromoCode).offset(skip).limit(limit).all()

    def create(self, db: Session, obj_in: PromoCodeCreate) -> PromoCode:
        db_obj = PromoCode(**obj_in.dict())
        db.add(db_obj)
        db.commit()
        db.refresh(db_obj)
        return db_obj

    def update(self, db: Session, db_obj: PromoCode, obj_in: PromoCodeUpdate) -> PromoCode:
        obj_data = obj_in.dict(exclude_unset=True)
        for field, value in obj_data.items():
            setattr(db_obj, field, value)
        db.add(db_obj)
        db.commit()
        db.refresh(db_obj)
        return db_obj

    def remove(self, db: Session, id: int) -> PromoCode:
        obj = db.query(PromoCode).get(id)
        db.delete(obj)
        db.commit()
        return obj


crud_promo_code = CRUDPromoCode()