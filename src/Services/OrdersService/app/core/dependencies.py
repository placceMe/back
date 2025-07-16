from typing import Annotated
from fastapi import Depends
from sqlalchemy.orm import Session
from app.db.session import get_db

# Dependency to get database session
DatabaseSession = Annotated[Session, Depends(get_db)]