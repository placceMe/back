# Функція розділення замовлень за продавцями

## Опис

Реалізовано логіку автоматичного розділення замовлень, якщо товари в замовленні належать різним продавцям.

## Як це працює

1. **Одиночний продавець**: Якщо всі товари в замовленні від одного продавця - створюється одне замовлення (як раніше).

2. **Кілька продавців**: Якщо товари від різних продавців - створюються окремі замовлення для кожного продавця.

## Зміни в API

### Запит (без змін)
```json
POST /api/orders
{
  "userId": "guid",
  "notes": "Примітки до замовлення",
  "deliveryAddress": "Адреса доставки", 
  "items": [
    {
      "productId": "product1_from_seller_A",
      "quantity": 2
    },
    {
      "productId": "product2_from_seller_B", 
      "quantity": 1
    }
  ]
}
```

### Відповідь (нова структура)
```json
{
  "orders": [
    {
      "id": "order_for_seller_A",
      "userId": "guid",
      "totalAmount": 100.00,
      "status": "Pending",
      "deliveryAddress": "Адреса доставки",
      "notes": "Примітки до замовлення",
      "items": [
        {
          "productId": "product1_from_seller_A",
          "quantity": 2,
          "price": 50.00
        }
      ]
    },
    {
      "id": "order_for_seller_B", 
      "userId": "guid",
      "totalAmount": 80.00,
      "status": "Pending",
      "deliveryAddress": "Адреса доставки",
      "notes": "Примітки до замовлення",
      "items": [
        {
          "productId": "product2_from_seller_B",
          "quantity": 1,
          "price": 80.00
        }
      ]
    }
  ],
  "totalAmount": 180.00,
  "ordersCount": 2,
  "message": "Ваші товари від 2 різних продавців були розділені на окремі замовлення."
}
```

## Переваги

1. **Незалежна обробка**: Кожен продавець може незалежно обробляти своє замовлення
2. **Email сповіщення**: Для кожного замовлення відправляється окремий email
3. **Простіша логістика**: Кожен продавець відповідає тільки за свої товари
4. **Зворотна сумісність**: API залишається сумісним для одного продавця

## Тестування

Використовуйте `OrdersServiceNet.http` файл з тестом "Multi-Seller Order Split" для перевірки функціональності.

## Логи

Система логує створення кожного окремого замовлення:
- `Order {OrderId} created for user {UserId} with seller {SellerId}`
- `Created {OrdersCount} orders with total amount {TotalAmount} for user {UserId}`
