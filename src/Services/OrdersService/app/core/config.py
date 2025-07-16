import os
from pydantic import BaseSettings


class Settings(BaseSettings):
    DATABASE_URL: str = "postgresql://marketplace:marketplace_password@postgres-db:5432/marketplace_db"
    PRODUCTS_SERVICE_URL: str = os.getenv("PRODUCTS_SERVICE_URL", "http://localhost:8001")
    database_url: str = os.getenv("DATABASE_URL", "postgresql://marketplace:marketplace_password@postgres-db:5432/marketplace_db")
    db_schema: str = os.getenv("SCHEMA", "orders_service")
    debug: bool = os.getenv("DEBUG", "false").lower() == "true"
    products_service_url: str = os.getenv("PRODUCTS_SERVICE_URL", "http://localhost:8001")
    
    class Config:
        env_file = ".env"


settings = Settings()