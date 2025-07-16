import httpx
from typing import List, Dict, Optional
from app.core.config import settings
import logging

logger = logging.getLogger(__name__)

class ProductsClient:
    def __init__(self):
        self.base_url = settings.PRODUCTS_SERVICE_URL
        self.timeout = 30.0
    
    async def get_products_by_ids(self, product_ids: List[int]) -> List[Dict]:
        """Get products information by their IDs"""
        async with httpx.AsyncClient(timeout=self.timeout) as client:
            try:
                response = await client.post(
                    f"{self.base_url}/api/v1/products/batch",
                    json={"product_ids": product_ids}
                )
                response.raise_for_status()
                return response.json()
            except httpx.HTTPError as e:
                logger.error(f"Failed to fetch products: {e}")
                raise Exception(f"Failed to fetch products from products service: {e}")
    
    async def get_product_by_id(self, product_id: int) -> Optional[Dict]:
        """Get single product information by ID"""
        async with httpx.AsyncClient(timeout=self.timeout) as client:
            try:
                response = await client.get(f"{self.base_url}/api/v1/products/{product_id}")
                response.raise_for_status()
                return response.json()
            except httpx.HTTPError as e:
                logger.error(f"Failed to fetch product {product_id}: {e}")
                return None

products_client = ProductsClient()