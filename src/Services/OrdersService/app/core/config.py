import os
from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    database_url: str = os.getenv("DATABASE_URL", "postgresql://marketplace:marketplace_password@postgres-db:5432/marketplace_db")
    db_schema: str = os.getenv("SCHEMA", "orders_service")
    debug: bool = os.getenv("DEBUG", "false").lower() == "true"
    
    class Config:
        env_file = ".env"


settings = Settings()