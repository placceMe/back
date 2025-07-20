import httpx
from typing import List, Optional, Dict, Any
from uuid import UUID
from app.core.config import settings
import logging

logger = logging.getLogger(__name__)

class ProductsClient:
    def __init__(self):
        self.base_url = settings.PRODUCTS_SERVICE_URL.rstrip('/')
        self.timeout = 30.0
    
    async def get_products_by_ids(self, product_ids: List[UUID]) -> List[Dict[str, Any]]:
        """Get products by list of IDs"""
        try:
            # Convert UUIDs to strings for API call
            string_ids = [str(product_id) for product_id in product_ids]
            
            async with httpx.AsyncClient(timeout=self.timeout) as client:
                response = await client.post(
                    f"{self.base_url}/api/products/batch",
                    json={"ids": string_ids}
                )
                response.raise_for_status()
                products = response.json()
                
                # Convert string IDs back to UUIDs for consistency
                for product in products:
                    product['id'] = UUID(product['id'])
                
                return products
        except Exception as e:
            logger.error(f"Error fetching products: {e}")
            return []
    
    async def get_product_by_id(self, product_id: UUID) -> Optional[Dict[str, Any]]:
        """Get single product by ID"""
        try:
            async with httpx.AsyncClient(timeout=self.timeout) as client:
                response = await client.get(f"{self.base_url}/api/products/{product_id}")
                response.raise_for_status()
                product = response.json()
                product['id'] = UUID(product['id'])
                return product
        except Exception as e:
            logger.error(f"Error fetching product {product_id}: {e}")
            return None

# Global instance
products_client = ProductsClient()