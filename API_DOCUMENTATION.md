# 📱 Product Sale API Documentation

**Base URL:** `http://localhost:8386/api`

---

## 🔐 Authentication APIs

### 1. Register User (Sign Up)
**Endpoint:** `POST /auth/register`

**Request Body:**
```json
{
  "username": "johndoe",
  "password": "Password123!",
  "email": "john@example.com",
  "phoneNumber": "0123456789",
  "address": "123 Main Street, City"
}
```

**Response (201 Created):**
```json
{
  "message": "Registration successful",
  "isSuccess": true,
  "result": {
    "userId": 1,
    "username": "johndoe",
    "email": "john@example.com",
    "phoneNumber": "0123456789",
    "address": "123 Main Street, City",
    "token": "base64_encoded_token_with_user_data",
    "role": "user"
  },
  "errors": null
}
```

---

### 2. User Login
**Endpoint:** `POST /auth/login`

**Request Body:**
```json
{
  "username": "johndoe",
  "password": "Password123!"
}
```

**Response (200 OK):**
```json
{
  "message": "Login successful",
  "isSuccess": true,
  "result": {
    "userId": 1,
    "username": "johndoe",
    "email": "john@example.com",
    "phoneNumber": "0123456789",
    "address": "123 Main Street, City",
    "token": "base64_encoded_token_with_user_data",
    "role": "user"
  },
  "errors": null
}
```

**Token Format (Base64 decoded):**
```json
{
  "UserId": 1,
  "Username": "johndoe",
  "Email": "john@example.com",
  "Role": "user",
  "PhoneNumber": "0123456789",
  "Address": "123 Main Street, City",
  "ExpiresAt": "2024-01-16T10:30:00Z"
}
```

---

## 📦 Product APIs

### 3. Get All Products
**Endpoint:** `GET /products`

**Query Parameters:**
- `pageNumber` (optional): Page number for pagination
- `pageSize` (optional): Items per page

**Response (200 OK):**
```json
{
  "message": "Get products success",
  "isSuccess": true,
  "result": [
    {
      "productId": 1,
      "productName": "Laptop",
      "price": 999.99,
      "description": "High-performance laptop",
      "imageUrl": "https://example.com/laptop.jpg",
      "categoryId": 1,
      "categoryName": "Electronics",
      "stock": 50
    }
  ],
  "errors": null
}
```

---

### 4. Get Product Details
**Endpoint:** `GET /products/{productId}`

**Response (200 OK):**
```json
{
  "message": "Get product success",
  "isSuccess": true,
  "result": {
    "productId": 1,
    "productName": "Laptop",
    "price": 999.99,
    "description": "High-performance laptop",
    "imageUrl": "https://example.com/laptop.jpg",
    "categoryId": 1,
    "categoryName": "Electronics",
    "stock": 50
  },
  "errors": null
}
```

---

### 5. Search Products
**Endpoint:** `GET /products/search`

**Query Parameters:**
- `keyword` (required): Search keyword

**Response (200 OK):**
```json
{
  "message": "Search products success",
  "isSuccess": true,
  "result": [
    {
      "productId": 1,
      "productName": "Laptop",
      "price": 999.99,
      "description": "High-performance laptop",
      "imageUrl": "https://example.com/laptop.jpg",
      "categoryId": 1,
      "categoryName": "Electronics",
      "stock": 50
    }
  ],
  "errors": null
}
```

---

## 🛒 Cart APIs

### 6. Add Item to Cart
**Endpoint:** `POST /carts/items`

**Request Body:**
```json
{
  "cartId": 1,
  "productId": 1,
  "quantity": 2,
  "price": 999.99
}
```

**Response (201 Created):**
```json
{
  "message": "Add item to cart success",
  "isSuccess": true,
  "result": {
    "cartItemId": 1,
    "productId": 1,
    "productName": "Laptop",
    "quantity": 2,
    "price": 999.99,
    "subtotal": 1999.98,
    "productImage": "https://example.com/laptop.jpg"
  },
  "errors": null
}
```

---

### 7. Get Cart
**Endpoint:** `GET /carts/{cartId}`

**Response (200 OK):**
```json
{
  "message": "Get cart success",
  "isSuccess": true,
  "result": {
    "cartId": 1,
    "userId": 1,
    "cartItems": [
      {
        "cartItemId": 1,
        "productId": 1,
        "productName": "Laptop",
        "quantity": 2,
        "price": 999.99,
        "subtotal": 1999.98,
        "productImage": "https://example.com/laptop.jpg"
      }
    ],
    "totalPrice": 1999.98,
    "totalItems": 2
  },
  "errors": null
}
```

---

### 8. Update Cart Item Quantity
**Endpoint:** `PUT /carts/items/{cartItemId}`

**Request Body:**
```json
{
  "quantity": 3
}
```

**Response (200 OK):**
```json
{
  "message": "Update cart item success",
  "isSuccess": true,
  "result": {
    "cartItemId": 1,
    "productId": 1,
    "quantity": 3,
    "subtotal": 2999.97
  },
  "errors": null
}
```

---

### 9. Remove Item from Cart
**Endpoint:** `DELETE /carts/items/{cartItemId}`

**Response (200 OK):**
```json
{
  "message": "Remove item from cart success",
  "isSuccess": true,
  "result": null,
  "errors": null
}
```

---

### 10. Clear Cart
**Endpoint:** `DELETE /carts/{cartId}`

**Response (200 OK):**
```json
{
  "message": "Clear cart success",
  "isSuccess": true,
  "result": null,
  "errors": null
}
```

---

## 📦 Order APIs

### 11. Create Order
**Endpoint:** `POST /orders`

**Request Body:**
```json
{
  "userId": 1,
  "paymentMethod": "vnpay",
  "billingAddress": "123 Main Street, City",
  "shippingAddress": "123 Main Street, City",
  "phoneNumber": "0123456789",
  "totalAmount": 1999.98
}
```

**Response (201 Created):**
```json
{
  "message": "Order created successfully",
  "isSuccess": true,
  "result": {
    "orderId": 1,
    "userId": 1,
    "totalAmount": 1999.98,
    "orderStatus": "pending",
    "paymentMethod": "vnpay",
    "billingAddress": "123 Main Street, City",
    "shippingAddress": "123 Main Street, City",
    "phoneNumber": "0123456789",
    "orderDate": "2024-01-15T10:30:00Z",
    "orderItems": [
      {
        "productId": 1,
        "productName": "Laptop",
        "quantity": 2,
        "price": 999.99,
        "subTotal": 1999.98
      }
    ],
    "payment": null
  },
  "errors": null
}
```

---

### 12. Get Order Details
**Endpoint:** `GET /orders/{orderId}`

**Response (200 OK):**
```json
{
  "message": "Get order success",
  "isSuccess": true,
  "result": {
    "orderId": 1,
    "userId": 1,
    "totalAmount": 1999.98,
    "orderStatus": "confirmed",
    "paymentMethod": "vnpay",
    "orderItems": [
      {
        "productId": 1,
        "productName": "Laptop",
        "quantity": 2,
        "price": 999.99,
        "subTotal": 1999.98
      }
    ],
    "payment": {
      "paymentId": 1,
      "orderId": 1,
      "amount": 1999.98,
      "paymentStatus": "success",
      "paymentDate": "2024-01-15T10:35:00Z",
      "transactionId": "123456"
    }
  },
  "errors": null
}
```

---

### 13. Get User Orders
**Endpoint:** `GET /orders/user/{userId}`

**Response (200 OK):**
```json
{
  "message": "Get user orders success",
  "isSuccess": true,
  "result": [
    {
      "orderId": 1,
      "userId": 1,
      "totalAmount": 1999.98,
      "orderStatus": "confirmed",
      "orderDate": "2024-01-15T10:30:00Z"
    }
  ],
  "errors": null
}
```

---

### 14. Cancel Order
**Endpoint:** `DELETE /orders/{orderId}`

**Response (200 OK):**
```json
{
  "message": "Order cancelled successfully",
  "isSuccess": true,
  "result": null,
  "errors": null
}
```

---

## 💳 Payment APIs

### 15. Create VNPay Payment URL
**Endpoint:** `POST /vnpay/create-payment`

**Request Body:**
```json
{
  "orderId": 1,
  "amount": 1999.98
}
```

**Response (200 OK):**
```json
{
  "message": "Payment URL created",
  "isSuccess": true,
  "result": {
    "paymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?vnp_Version=2.1.0&vnp_Command=pay&..."
  },
  "errors": null
}
```

---

### 16. Payment Return (VNPay Callback)
**Endpoint:** `GET /vnpay/payment-return`

**Query Parameters:**
- `vnp_ResponseCode`: Response code from VNPay
- `vnp_TransactionStatus`: Transaction status
- `vnp_TxnRef`: Order ID
- `vnp_SecureHash`: Security hash
- (Other VNPay parameters)

**Response (200 OK):**
```json
{
  "message": "Payment successful",
  "isSuccess": true,
  "result": {
    "orderId": 1,
    "paymentStatus": "success",
    "responseCode": "00",
    "transactionId": "123456"
  },
  "errors": null
}
```

---

### 17. Confirm Payment
**Endpoint:** `POST /payments/confirm`

**Request Body:**
```json
{
  "orderId": 1,
  "amount": 1999.98,
  "paymentMethod": "vnpay"
}
```

**Response (200 OK):**
```json
{
  "message": "Payment confirmed successfully",
  "isSuccess": true,
  "result": {
    "paymentId": 1,
    "orderId": 1,
    "amount": 1999.98,
    "paymentStatus": "success",
    "paymentDate": "2024-01-15T10:35:00Z",
    "transactionId": "123456"
  },
  "errors": null
}
```

---

## 🗺️ Store Location APIs

### 18. Get All Store Locations
**Endpoint:** `GET /storelocations`

**Response (200 OK):**
```json
{
  "message": "Get store locations success",
  "isSuccess": true,
  "result": [
    {
      "locationId": 1,
      "address": "123 Main Street, Ho Chi Minh City",
      "latitude": 10.7769,
      "longitude": 106.7009
    }
  ],
  "errors": null
}
```

---

### 19. Get Location by ID
**Endpoint:** `GET /storelocations/{locationId}`

**Response (200 OK):**
```json
{
  "message": "Get location success",
  "isSuccess": true,
  "result": {
    "locationId": 1,
    "address": "123 Main Street, Ho Chi Minh City",
    "latitude": 10.7769,
    "longitude": 106.7009
  },
  "errors": null
}
```

---

### 20. Get Locations by City
**Endpoint:** `GET /storelocations/city/{city}`

**Response (200 OK):**
```json
{
  "message": "Get locations by city success",
  "isSuccess": true,
  "result": [
    {
      "locationId": 1,
      "address": "123 Main Street, Ho Chi Minh City",
      "latitude": 10.7769,
      "longitude": 106.7009
    }
  ],
  "errors": null
}
```

---

### 21. Get Nearest Store Location
**Endpoint:** `GET /storelocations/nearest`

**Query Parameters:**
- `latitude` (required): User's latitude
- `longitude` (required): User's longitude

**Response (200 OK):**
```json
{
  "message": "Get nearest location success",
  "isSuccess": true,
  "result": {
    "locationId": 1,
    "address": "123 Main Street, Ho Chi Minh City",
    "latitude": 10.7769,
    "longitude": 106.7009
  },
  "errors": null
}
```

---

## 💬 Chat APIs

### 22. Send Chat Message
**Endpoint:** `POST /chat/send`

**Request Body:**
```json
{
  "senderId": 1,
  "receiverId": 2,
  "message": "Hello, I need help with my order",
  "messageType": "text"
}
```

**Response (200 OK):**
```json
{
  "message": "Message sent successfully",
  "isSuccess": true,
  "result": {
    "messageId": 1,
    "senderId": 1,
    "senderName": "johndoe",
    "receiverId": 2,
    "receiverName": "admin",
    "message": "Hello, I need help with my order",
    "messageType": "text",
    "sentAt": "2024-01-15T10:45:00Z",
    "isRead": false
  },
  "errors": null
}
```

---

### 23. Get Conversation
**Endpoint:** `GET /chat/conversation`

**Query Parameters:**
- `userId1` (required): First user ID
- `userId2` (required): Second user ID

**Response (200 OK):**
```json
{
  "message": "Get conversation success",
  "isSuccess": true,
  "result": [
    {
      "messageId": 1,
      "senderId": 1,
      "senderName": "johndoe",
      "receiverId": 2,
      "receiverName": "admin",
      "message": "Hello, I need help with my order",
      "messageType": "text",
      "sentAt": "2024-01-15T10:45:00Z",
      "isRead": true
    },
    {
      "messageId": 2,
      "senderId": 2,
      "senderName": "admin",
      "receiverId": 1,
      "receiverName": "johndoe",
      "message": "Hello! How can I help?",
      "messageType": "text",
      "sentAt": "2024-01-15T10:46:00Z",
      "isRead": false
    }
  ],
  "errors": null
}
```

---

### 24. Get User Conversations
**Endpoint:** `GET /chat/conversations/{userId}`

**Response (200 OK):**
```json
{
  "message": "Get conversations success",
  "isSuccess": true,
  "result": [
    {
      "conversationId": 2,
      "userId": 2,
      "userName": "admin",
      "lastMessage": "Hello! How can I help?",
      "lastMessageTime": "2024-01-15T10:46:00Z",
      "unreadCount": 0
    }
  ],
  "errors": null
}
```

---

### 25. Get Unread Messages
**Endpoint:** `GET /chat/unread/{userId}`

**Response (200 OK):**
```json
{
  "message": "Get unread messages success",
  "isSuccess": true,
  "result": [
    {
      "messageId": 3,
      "senderId": 2,
      "senderName": "admin",
      "receiverId": 1,
      "receiverName": "johndoe",
      "message": "Your order has been confirmed",
      "messageType": "text",
      "sentAt": "2024-01-15T10:50:00Z",
      "isRead": false
    }
  ],
  "errors": null
}
```

---

### 26. Get Unread Message Count
**Endpoint:** `GET /chat/unread-count/{userId}`

**Response (200 OK):**
```json
{
  "message": "Get unread count success",
  "isSuccess": true,
  "result": {
    "unreadCount": 3
  },
  "errors": null
}
```

---

### 27. Mark Message as Read
**Endpoint:** `PUT /chat/read/{messageId}`

**Response (200 OK):**
```json
{
  "message": "Message marked as read",
  "isSuccess": true,
  "result": null,
  "errors": null
}
```

---

## Error Response Format

All error responses follow this format:

```json
{
  "message": "Error message",
  "isSuccess": false,
  "result": null,
  "errors": "Detailed error information"
}
```

---

## Common Error Codes

| Status Code | Meaning |
|-------------|---------|
| 200 | OK |
| 201 | Created |
| 400 | Bad Request |
| 401 | Unauthorized |
| 404 | Not Found |
| 500 | Internal Server Error |

---

## Authentication Header

For endpoints requiring authentication, include the token in the header:

```
Authorization: Bearer {token}
```

Example:
```
GET /orders/user/1 HTTP/1.1
Authorization: Bearer base64_encoded_token_here
```

---

## Token Decoding (Client Side)

**Kotlin (Android):**
```kotlin
import android.util.Base64
import com.google.gson.Gson

fun decodeToken(token: String): TokenPayload {
    val parts = token.split(".")
    val decodedBytes = Base64.decode(parts[1], Base64.URL_SAFE)
    val decoded = String(decodedBytes)
    return Gson().fromJson(decoded, TokenPayload::class.java)
}
```

**JavaScript (Web):**
```javascript
function decodeToken(token) {
    const base64Url = token;
    const jsonPayload = atob(base64Url);
    return JSON.parse(jsonPayload);
}

// Usage
const payload = decodeToken(token);
console.log(payload.UserId);
console.log(payload.Username);
console.log(payload.Email);
```

---

## VNPay Configuration

**Sandbox Credentials:**
- Terminal Code: `2QXER21D`
- Hash Secret: `EWHPBKMV2EKLYQ872YVGQ1FCNN8UIRX`
- VNPay URL: `https://sandbox.vnpayment.vn/paymentv2/vpcpay.html`

**Test Card Numbers:**
- Visa: `4111111111111111`
- Expiry: Any future date
- OTP: `123456`

---

## Notes

- All timestamps are in UTC format (ISO 8601)
- Prices are in VND (Vietnamese Dong)
- Token expires in 24 hours
- Maximum 100 items per page for list endpoints
- Chat messages support: text, image, file types

---

## Testing

### Using Postman

1. Import this API documentation
2. Set `{{baseUrl}}` to `http://localhost:8386/api`
3. Set `{{token}}` to the token received from login
4. Use `{{baseUrl}}/auth/login` to get token
5. Add token to Authorization header for protected endpoints

### Using cURL

```bash
# Register
curl -X POST http://localhost:8386/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "Test@123",
    "email": "test@example.com"
  }'

# Login
curl -X POST http://localhost:8386/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "Test@123"
  }'

# Get Products
curl -X GET http://localhost:8386/api/products \
  -H "Authorization: Bearer {token}"
```

---

**API Version:** 1.0  
**Last Updated:** January 2024  
**Status:** ✅ Production Ready
