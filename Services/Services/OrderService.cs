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
        Task<OrderResponseDTO?> UpdateOrderStatusAsync(int orderId, string status);
        Task<OrderResponseDTO?> CancelOrderAsync(int orderId);
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
            var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new Exception($"User with ID {request.UserId} not found");

            var order = new Order
            {
                CartId = request.CartId,
                UserId = request.UserId,
                OrderStatus = "pending",
                PaymentMethod = "Online banking",
                BillingAddress = "",
                OrderDate = DateTime.Now
            };

            try
            {
                await _unitOfWork.OrderRepository.CreateAsync(order);
                await _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create order: {ex.InnerException?.Message ?? ex.Message}");
            }

            var createdOrder = await _unitOfWork.OrderRepository.GetByIdAsync(order.OrderId);
            if (createdOrder == null)
                throw new Exception("Failed to create order - could not retrieve created order from database");            

            var newCart = new Cart
            {
                UserId = request.UserId,
                Status = "active",
                TotalPrice = 0,
                CartItems = new List<CartItem>()
            };
            try
            {
                await _unitOfWork.CartRepository.CreateAsync(newCart);
                await _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Failed to create new cart for user: {ex.Message}");
            }

            return await MapOrderToDTO(createdOrder);
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

        public async Task<OrderResponseDTO?> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
            if (order == null) return null;

            order.OrderStatus = status;
            _unitOfWork.OrderRepository.Update(order);
            await _unitOfWork.SaveChanges();

            var updatedOrder = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
            return updatedOrder != null ? await MapOrderToDTO(updatedOrder) : null;
        }

        public async Task<OrderResponseDTO?> CancelOrderAsync(int orderId)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
            if (order == null) return null;

            if (order.OrderStatus != "pending")
                throw new Exception($"Cannot cancel {order.OrderStatus} order. Only pending orders can be cancelled");

            order.OrderStatus = "cancelled";
            _unitOfWork.OrderRepository.Update(order);
            await _unitOfWork.SaveChanges();

            if (order.CartId.HasValue)
            {
                var cart = await _unitOfWork.CartRepository.GetByIdAsync(order.CartId.Value);
                if (cart != null && cart.Status == "ordered")
                {
                    cart.Status = "active";
                    _unitOfWork.CartRepository.Update(cart);
                    try
                    {
                        await _unitOfWork.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: Failed to restore cart status: {ex.Message}");
                    }
                }
            }

            var cancelledOrder = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
            return cancelledOrder != null ? await MapOrderToDTO(cancelledOrder) : null;
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
                    PaymentDate = payment.PaymentDate ?? DateTime.Now,
                    PaymentStatus = payment.PaymentStatus,
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
                ShippingAddress = order.BillingAddress,
                PhoneNumber = order.User?.PhoneNumber ?? "",
                OrderDate = order.OrderDate ?? DateTime.Now,
                OrderItems = orderItems,
                Payment = paymentDTO
            };
        }
    }
}
