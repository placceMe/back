#!/bin/bash

# Development setup script for Orders Service

echo "Setting up Orders Service for development..."

# Install dependencies
echo "Installing Python dependencies..."
pip install -r requirements.txt

# Initialize Alembic (only if not already initialized)
if [ ! -d "alembic/versions" ]; then
    echo "Initializing Alembic..."
    alembic init alembic
fi

# Run migrations
echo "Running database migrations..."
alembic upgrade head

echo "Development setup completed!"
echo "You can now run the service with: uvicorn main:app --reload"