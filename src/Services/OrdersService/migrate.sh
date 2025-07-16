#!/bin/bash

# Migration script for Orders Service

echo "Running database migrations..."

# Run migrations
alembic upgrade head

echo "Migrations completed successfully!"