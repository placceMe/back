import httpx
from typing import List, Optional, Dict, Any
from uuid import UUID
from app.core.config import settings
import logging

logger = logging.getLogger(__name__)

class ProductsClient:
    def __init__(self):
        self.base_url = settings.PRODUCTS_SERVICE_URL.rstrip('/')
        # Fallback URL for local development
        self.fallback_url = "http://localhost:5003"
        self.timeout = 30.0
        
    async def _make_request(self, method: str, url: str, **kwargs):
        """Make HTTP request with fallback URL on DNS failure"""
        async with httpx.AsyncClient(timeout=self.timeout) as client:
            try:
                if method == "GET":
                    return await client.get(url, **kwargs)
                elif method == "POST":
                    return await client.post(url, **kwargs)
            except (httpx.ConnectError, httpx.TimeoutException, httpx.NetworkError) as e:
                if "products-service" in url:
                    # Try fallback URL for local development
                    fallback_url = url.replace(self.base_url, self.fallback_url)
                    logger.warning(f"Failed to connect to {url}, trying fallback: {fallback_url}")
                    if method == "GET":
                        return await client.get(fallback_url, **kwargs)
                    elif method == "POST":
                        return await client.post(fallback_url, **kwargs)
                raise e
    
    async def get_products_by_ids(self, product_ids: List[UUID]) -> List[Dict[str, Any]]:
        """Get products by list of IDs"""
        try:
            # Convert UUIDs to strings for API call
            string_ids = [str(product_id) for product_id in product_ids]
            logger.info(f"Fetching products with IDs: {string_ids}")
            
            # Try batch endpoint first
            try:
                url = f"{self.base_url}/api/products/many"
                logger.info(f"Making request to: {url}")
                response = await self._make_request("POST", url, json={"ids": string_ids})
                response.raise_for_status()
                products = response.json()
                logger.info(f"Batch endpoint returned {len(products)} products")
                
                # Convert string IDs back to UUIDs for consistency
                for product in products:
                    if isinstance(product.get('id'), str):
                        product['id'] = UUID(product['id'])
                
                return products
            except httpx.HTTPStatusError as e:
                logger.warning(f"Batch endpoint failed with status {e.response.status_code}, trying individual calls")
                # Fallback to individual calls
                products = []
                for product_id in product_ids:
                    product = await self.get_product_by_id(product_id)
                    if product:
                        products.append(product)
                return products
        except Exception as e:
            logger.error(f"Error fetching products: {e}")
            # Fallback to individual calls
            products = []
            for product_id in product_ids:
                product = await self.get_product_by_id(product_id)
                if product:
                    products.append(product)
            return products
    
    async def get_product_by_id(self, product_id: UUID) -> Optional[Dict[str, Any]]:
        """Get single product by ID"""
        try:
            url = f"{self.base_url}/api/products/{product_id}"
            logger.info(f"Making request to: {url}")
            response = await self._make_request("GET", url)
            response.raise_for_status()
            product = response.json()
            if isinstance(product.get('id'), str):
                product['id'] = UUID(product['id'])
            return product
        except httpx.HTTPStatusError as e:
            logger.error(f"Product {product_id} not found, status: {e.response.status_code}")
            return None
        except Exception as e:
            logger.error(f"Error fetching product {product_id}: {e}")
            return None

# Global instance
products_client = ProductsClient()