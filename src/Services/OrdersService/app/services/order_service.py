from typing import List, Optional
from decimal import Decimal
from sqlalchemy.orm import Session
from app.db.models.order import Order
from app.db.models.order_item import OrderItem
from app.db.models.promo_code import PromoCode
from app.db.models.status import Status
from app.schemas.order import OrderCreate, OrderItemCreate
from app.services.products_client import products_client
import logging

logger = logging.getLogger(__name__)

class OrderService:
    def __init__(self, db: Session):
        self.db = db
    
    async def create_order(self, order_data: OrderCreate) -> Order:
        """Create new order with calculated total amount"""
        
        # Get products information
        product_ids = [item.product_id for item in order_data.items]
        products_info = await products_client.get_products_by_ids(product_ids)
        
        # Create product lookup dict
        products_dict = {product['id']: product for product in products_info}
        
        # Validate that all products exist
        missing_products = set(product_ids) - set(products_dict.keys())
        if missing_products:
            raise ValueError(f"Products not found: {missing_products}")
        
        # Calculate total amount
        total_amount = Decimal('0.00')
        order_items_data = []
        
        for item in order_data.items:
            product = products_dict[item.product_id]
            product_price = Decimal(str(product['price']))
            item_total = product_price * item.quantity
            total_amount += item_total
            
            order_items_data.append({
                'product_id': item.product_id,
                'product_name': product['name'],
                'quantity': item.quantity,
                'price': product_price,
                'total_price': item_total
            })
        
        # Apply promo code discount if provided
        discount_amount = Decimal('0.00')
        if order_data.promo_code:
            promo_code = self.db.query(PromoCode).filter(
                PromoCode.code == order_data.promo_code,
                PromoCode.is_active == True
            ).first()
            
            if promo_code:
                if promo_code.discount_type == 'percentage':
                    discount_amount = total_amount * (promo_code.discount_value / 100)
                elif promo_code.discount_type == 'fixed':
                    discount_amount = min(promo_code.discount_value, total_amount)
        
        final_amount = total_amount - discount_amount
        
        # Get default status
        default_status = self.db.query(Status).filter(Status.name == 'pending').first()
        if not default_status:
            # Create default status if not exists
            default_status = Status(name='pending', description='Очікує обробки')
            self.db.add(default_status)
            self.db.flush()
        
        # Create order
        order = Order(
            customer_id=order_data.customer_id,
            total_amount=total_amount,
            discount_amount=discount_amount,
            final_amount=final_amount,
            status_id=default_status.id,
            promo_code=order_data.promo_code,
            delivery_address=order_data.delivery_address,
            notes=order_data.notes
        )
        
        self.db.add(order)
        self.db.flush()
        
        # Create order items
        for item_data in order_items_data:
            order_item = OrderItem(
                order_id=order.id,
                product_id=item_data['product_id'],
                product_name=item_data['product_name'],
                quantity=item_data['quantity'],
                price=item_data['price'],
                total_price=item_data['total_price']
            )
            self.db.add(order_item)
        
        self.db.commit()
        self.db.refresh(order)
        
        return order
    
    def get_order_by_id(self, order_id: int) -> Optional[Order]:
        """Get order by ID"""
        return self.db.query(Order).filter(Order.id == order_id).first()
    
    def get_orders_by_customer(self, customer_id: int) -> List[Order]:
        """Get all orders for a customer"""
        return self.db.query(Order).filter(Order.customer_id == customer_id).all()