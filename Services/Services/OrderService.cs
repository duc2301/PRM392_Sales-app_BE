using AutoMapper;
using Repositories.Interfaces;
using Repositories.Models;
using Services.DTOs.RequestDTOs;
using Services.DTOs.ResponseDTOs;
using Services.Interfaces;

namespace Services.Services
{
    public interface IOrderService
    {
        Task<OrderResponseDTO> CreateOrderAsync(OrderRequestDTO request);
        Task<OrderResponseDTO?> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<OrderResponseDTO>> GetUserOrdersAsync(int userId);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
        Task<bool> CancelOrderAsync(int orderId);
    }

    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<OrderResponseDTO> CreateOrderAsync(OrderRequestDTO request)
        {
            // Get user cart
            var cart = await _unitOfWork.CartRepository.GetCartByUserIdAsync(request.UserId);
            if (cart == null || !cart.CartItems.Any())
                throw new Exception("Cart is empty");

            // Create order
            var order = new Order
            {
                CartId = cart.CartId,
                UserId = request.UserId,
                PaymentMethod = request.PaymentMethod,
                BillingAddress = request.BillingAddress,
                OrderStatus = "pending",
                OrderDate = DateTime.UtcNow
            };

            await _unitOfWork.OrderRepository.CreateAsync(order);
            await _unitOfWork.SaveChanges();

            return await MapOrderToDTO(order);
        }

        public async Task<OrderResponseDTO?> GetOrderByIdAsync(int orderId)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdWithPaymentAsync(orderId);
            if (order == null) return null;

            return await MapOrderToDTO(order);
        }

        public async Task<IEnumerable<OrderResponseDTO>> GetUserOrdersAsync(int userId)
        {
            var orders = await _unitOfWork.OrderRepository.GetByUserIdAsync(userId);
            var result = new List<OrderResponseDTO>();

            foreach (var order in orders)
            {
                var dto = await MapOrderToDTO(order);
                result.Add(dto);
            }

            return result;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
            if (order == null) return false;

            order.OrderStatus = status;
            _unitOfWork.OrderRepository.Update(order);
            await _unitOfWork.SaveChanges();
            return true;
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
            if (order == null) return false;

            if (order.OrderStatus == "confirmed" || order.OrderStatus == "shipped")
                throw new Exception("Cannot cancel confirmed/shipped order");

            order.OrderStatus = "cancelled";
            _unitOfWork.OrderRepository.Update(order);
            await _unitOfWork.SaveChanges();
            return true;
        }

        private async Task<OrderResponseDTO> MapOrderToDTO(Order order)
        {
            var orderItems = order.Cart?.CartItems.Select(ci => new OrderItemDTO
            {
                ProductId = ci.ProductId ?? 0,
                ProductName = ci.Product?.ProductName ?? "Unknown",
                Quantity = ci.Quantity,
                Price = ci.Price,
                SubTotal = ci.Price * ci.Quantity
            }).ToList() ?? new List<OrderItemDTO>();

            var payment = order.Payments?.FirstOrDefault();
            PaymentResponseDTO? paymentDTO = null;

            if (payment != null)
            {
                paymentDTO = new PaymentResponseDTO
                {
                    PaymentId = payment.PaymentId,
                    OrderId = payment.OrderId ?? 0,
                    Amount = payment.Amount,
                    PaymentDate = payment.PaymentDate ?? DateTime.UtcNow,
                    PaymentStatus = payment.PaymentStatus,
                    TransactionId = payment.PaymentId.ToString()
                };
            }

            return new OrderResponseDTO
            {
                OrderId = order.OrderId,
                UserId = order.UserId ?? 0,
                TotalAmount = order.Cart?.TotalPrice ?? 0,
                OrderStatus = order.OrderStatus,
                PaymentMethod = order.PaymentMethod,
                BillingAddress = order.BillingAddress,
                ShippingAddress = order.BillingAddress, // Same as billing for now
                PhoneNumber = order.User?.PhoneNumber ?? "",
                OrderDate = order.OrderDate ?? DateTime.UtcNow,
                OrderItems = orderItems,
                Payment = paymentDTO
            };
        }
    }
}
