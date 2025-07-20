#!/usr/bin/env python3
"""
Test script to verify ProductsService connectivity
"""
import httpx
import asyncio
import sys

async def test_products_service():
    urls = [
        "http://products-service:80/api/products",
        "http://localhost:5003/api/products"
    ]
    
    for url in urls:
        try:
            print(f"Testing connection to: {url}")
            async with httpx.AsyncClient(timeout=10.0) as client:
                response = await client.get(url)
                print(f"✓ Success: {url} - Status: {response.status_code}")
                return url
        except Exception as e:
            print(f"✗ Failed: {url} - Error: {e}")
    
    print("❌ All connection attempts failed")
    return None

if __name__ == "__main__":
    result = asyncio.run(test_products_service())
    if result:
        print(f"🎉 ProductsService accessible at: {result}")
        sys.exit(0)
    else:
        print("💥 ProductsService not accessible")
        sys.exit(1)