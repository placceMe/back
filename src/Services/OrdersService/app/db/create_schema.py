from sqlalchemy import create_engine, text
from app.core.config import settings

def create_schema():
    engine = create_engine(settings.DATABASE_URL)
    schema_name = settings.db_schema
    with engine.connect() as conn:
        conn.execute(text(f"CREATE SCHEMA IF NOT EXISTS {schema_name};"))
        print(f"Схема '{schema_name}' створена (або вже існує).")
