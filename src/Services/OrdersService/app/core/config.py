import os
from pydantic import BaseSettings, Field


class Settings(BaseSettings):
    DATABASE_URL: str = "postgresql://marketplace:marketplace_password@localhost:5432/marketplace_db"
    # Products service URL - try container name first, fallback to localhost
    PRODUCTS_SERVICE_URL: str = Field(
        default="http://products-service:80", 
        env="PRODUCTS_SERVICE_URL",
        description="URL for ProductsService - use http://localhost:5003 for local development"
    )
    database_url: str = os.getenv("DATABASE_URL", "postgresql://marketplace:marketplace_password@localhost:5432/marketplace_db")
    db_schema: str = os.getenv("SCHEMA", "orders_service")
    debug: bool = os.getenv("DEBUG", "false").lower() == "true"
    products_service_url: str = os.getenv("PRODUCTS_SERVICE_URL", "http://localhost:8001")
    
    class Config:
        env_file = ".env"


settings = Settings()