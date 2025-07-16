"""Initial migration

Revision ID: 001
Revises: 
Create Date: 2024-01-01 00:00:00.000000

"""
from alembic import op
import sqlalchemy as sa
from sqlalchemy.sql import func
import os

# revision identifiers, used by Alembic.
revision = '001'
down_revision = None
branch_labels = None
depends_on = None

SCHEMA_NAME = os.getenv("SCHEMA", "orders_service")

def upgrade() -> None:
    # Create schema
    op.execute(f'CREATE SCHEMA IF NOT EXISTS {SCHEMA_NAME}')
    
    # Create statuses table
    op.create_table(
        'statuses',
        sa.Column('id', sa.Integer(), nullable=False),
        sa.Column('name', sa.String(), nullable=True),
        sa.Column('description', sa.String(), nullable=True),
        sa.Column('is_active', sa.Boolean(), nullable=True, default=True),
        sa.Column('created_at', sa.DateTime(timezone=True), server_default=func.now(), nullable=True),
        sa.Column('updated_at', sa.DateTime(timezone=True), nullable=True),
        sa.PrimaryKeyConstraint('id'),
        schema=SCHEMA_NAME
    )
    op.create_index(f'ix_{SCHEMA_NAME}_statuses_id', 'statuses', ['id'], schema=SCHEMA_NAME)
    op.create_index(f'ix_{SCHEMA_NAME}_statuses_name', 'statuses', ['name'], unique=True, schema=SCHEMA_NAME)
    
    # Create promo_codes table
    op.create_table(
        'promo_codes',
        sa.Column('id', sa.Integer(), nullable=False),
        sa.Column('code', sa.String(), nullable=True),
        sa.Column('discount_percentage', sa.Float(), nullable=True),
        sa.Column('discount_amount', sa.Float(), nullable=True),
        sa.Column('min_order_amount', sa.Float(), nullable=True, default=0),
        sa.Column('max_uses', sa.Integer(), nullable=True),
        sa.Column('current_uses', sa.Integer(), nullable=True, default=0),
        sa.Column('is_active', sa.Boolean(), nullable=True, default=True),
        sa.Column('expires_at', sa.DateTime(timezone=True), nullable=True),
        sa.Column('created_at', sa.DateTime(timezone=True), server_default=func.now(), nullable=True),
        sa.Column('updated_at', sa.DateTime(timezone=True), nullable=True),
        sa.PrimaryKeyConstraint('id'),
        schema=SCHEMA_NAME
    )
    op.create_index(f'ix_{SCHEMA_NAME}_promo_codes_id', 'promo_codes', ['id'], schema=SCHEMA_NAME)
    op.create_index(f'ix_{SCHEMA_NAME}_promo_codes_code', 'promo_codes', ['code'], unique=True, schema=SCHEMA_NAME)
    
    # Create orders table
    op.create_table(
        'orders',
        sa.Column('id', sa.Integer(), nullable=False),
        sa.Column('user_id', sa.Integer(), nullable=True),
        sa.Column('status_id', sa.Integer(), nullable=True),
        sa.Column('promo_code_id', sa.Integer(), nullable=True),
        sa.Column('total_amount', sa.Float(), nullable=True),
        sa.Column('discount_amount', sa.Float(), nullable=True, default=0),
        sa.Column('final_amount', sa.Float(), nullable=True),
        sa.Column('notes', sa.String(), nullable=True),
        sa.Column('created_at', sa.DateTime(timezone=True), server_default=func.now(), nullable=True),
        sa.Column('updated_at', sa.DateTime(timezone=True), nullable=True),
        sa.ForeignKeyConstraint(['promo_code_id'], [f'{SCHEMA_NAME}.promo_codes.id'], ),
        sa.ForeignKeyConstraint(['status_id'], [f'{SCHEMA_NAME}.statuses.id'], ),
        sa.PrimaryKeyConstraint('id'),
        schema=SCHEMA_NAME
    )
    op.create_index(f'ix_{SCHEMA_NAME}_orders_id', 'orders', ['id'], schema=SCHEMA_NAME)
    op.create_index(f'ix_{SCHEMA_NAME}_orders_user_id', 'orders', ['user_id'], schema=SCHEMA_NAME)
    
    # Create order_items table
    op.create_table(
        'order_items',
        sa.Column('id', sa.Integer(), nullable=False),
        sa.Column('order_id', sa.Integer(), nullable=True),
        sa.Column('product_id', sa.Integer(), nullable=True),
        sa.Column('quantity', sa.Integer(), nullable=True),
        sa.Column('price', sa.Float(), nullable=True),
        sa.Column('total_price', sa.Float(), nullable=True),
        sa.ForeignKeyConstraint(['order_id'], [f'{SCHEMA_NAME}.orders.id'], ),
        sa.PrimaryKeyConstraint('id'),
        schema=SCHEMA_NAME
    )
    op.create_index(f'ix_{SCHEMA_NAME}_order_items_id', 'order_items', ['id'], schema=SCHEMA_NAME)
    
    # Insert default statuses
    op.execute(f"""
        INSERT INTO {SCHEMA_NAME}.statuses (name, description, is_active) VALUES
        ('pending', 'Order is pending', true),
        ('confirmed', 'Order is confirmed', true),
        ('processing', 'Order is being processed', true),
        ('shipped', 'Order has been shipped', true),
        ('delivered', 'Order has been delivered', true),
        ('cancelled', 'Order has been cancelled', true)
    """)


def downgrade() -> None:
    op.drop_table('order_items', schema=SCHEMA_NAME)
    op.drop_table('orders', schema=SCHEMA_NAME)
    op.drop_table('promo_codes', schema=SCHEMA_NAME)
    op.drop_table('statuses', schema=SCHEMA_NAME)
    op.execute(f'DROP SCHEMA IF EXISTS {SCHEMA_NAME} CASCADE')