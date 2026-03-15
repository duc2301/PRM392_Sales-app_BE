using AutoMapper;
using Repositories.Interfaces;
using Repositories.Models;
using Services.DTOs.RequestDTOs;
using Services.DTOs.ResponseDTOs;
using Microsoft.Extensions.Logging;

namespace Services.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponseDTO> CreatePaymentAsync(PaymentRequestDTO request);
        Task<PaymentResponseDTO?> GetPaymentByIdAsync(int paymentId);
        Task<IEnumerable<PaymentResponseDTO>> GetOrderPaymentsAsync(int orderId);
        Task<bool> UpdatePaymentStatusAsync(int paymentId, string status);
        Task<PaymentResponseDTO?> ConfirmPaymentAsync(int paymentId, string transactionId, string gateway, string paymentMethod);
        Task<PaymentResponseDTO?> FailPaymentAsync(int paymentId, string responseCode);
    }

    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Logging.ILogger<PaymentService> _logger;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper, Microsoft.Extensions.Logging.ILogger<PaymentService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PaymentResponseDTO> CreatePaymentAsync(PaymentRequestDTO request)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
                throw new Exception("Order not found");            

            if (order.OrderStatus != "pending" && order.OrderStatus != "payment_failed")
                throw new Exception($"Cannot create payment for {order.OrderStatus} order. Only pending or failed orders can be paid");

            var payment = new Payment
            {
                OrderId = request.OrderId,
                Amount = order.Cart.TotalPrice,
                PaymentDate = null,
                PaymentStatus = "pending"
            };

            await _unitOfWork.PaymentRepository.CreateAsync(payment);
            await _unitOfWork.SaveChanges();

            return MapPaymentToDTO(payment);
        }

        public async Task<PaymentResponseDTO?> GetPaymentByIdAsync(int paymentId)
        {
            var payment = await _unitOfWork.PaymentRepository.GetByIdAsync(paymentId);
            return payment == null ? null : MapPaymentToDTO(payment);
        }

        public async Task<IEnumerable<PaymentResponseDTO>> GetOrderPaymentsAsync(int orderId)
        {
            var payments = await _unitOfWork.PaymentRepository.GetByOrderIdAsync(orderId);
            return payments.Select(p => MapPaymentToDTO(p)).ToList();
        }

        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, string status)
        {
            var payment = await _unitOfWork.PaymentRepository.GetByIdAsync(paymentId);
            if (payment == null) return false;

            payment.PaymentStatus = status;
            if (status == "success")
                payment.PaymentDate = DateTime.Now;

            _unitOfWork.PaymentRepository.Update(payment);
            await _unitOfWork.SaveChanges();

            if (status == "success" && payment.OrderId.HasValue)
            {
                await UpdateOrderStatusAsync(payment.OrderId.Value, "confirmed");
            }

            return true;
        }

        public async Task<PaymentResponseDTO?> ConfirmPaymentAsync(int paymentId, string transactionId, string gateway, string paymentMethod)
        {
            var payment = await _unitOfWork.PaymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
                throw new Exception($"Payment with ID {paymentId} not found");

            if (payment.PaymentStatus == "success")
            {
                return MapPaymentToDTO(payment);
            }

            var order = payment.Order;
            if (order == null)
                throw new Exception("Order associated with payment not found");

            if (order.OrderStatus != "pending" && order.OrderStatus != "payment_failed")
            {
                _logger.LogWarning($"Order {order.OrderId} status is {order.OrderStatus}. Payment {paymentId} confirming anyway but check logic.");
            }

            // Update Payment
            payment.PaymentStatus = "success";
            payment.PaymentDate = DateTime.Now;

            _unitOfWork.PaymentRepository.Update(payment);

            // Update Order 
            order.OrderStatus = "confirmed";
            _unitOfWork.OrderRepository.Update(order);

            // Commit transaction
            await _unitOfWork.SaveChanges();

            _logger.LogInformation($"Successfully confirmed payment {paymentId} for order {order.OrderId} with TransactionId {transactionId}");

            return MapPaymentToDTO(payment);
        }

        public async Task<PaymentResponseDTO?> FailPaymentAsync(int paymentId, string responseCode)
        {
            var payment = await _unitOfWork.PaymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
                return null;

            if (payment.PaymentStatus == "success")
            {
                _logger.LogWarning($"Attempted to fail a success payment {paymentId}");
                return MapPaymentToDTO(payment);
            }

            payment.PaymentStatus = "failed";
            
            var order = payment.Order;
            if (order != null && order.OrderStatus == "pending")
            {
                order.OrderStatus = "payment_failed";
                _unitOfWork.OrderRepository.Update(order);
            }

            _unitOfWork.PaymentRepository.Update(payment);
            await _unitOfWork.SaveChanges();
            
            _logger.LogInformation($"Marked payment {paymentId} as failed with code {responseCode}");

            var dto = MapPaymentToDTO(payment);
            dto.ResponseCode = responseCode;
            return dto;
        }

        private PaymentResponseDTO MapPaymentToDTO(Payment payment)
        {
            return new PaymentResponseDTO
            {
                PaymentId = payment.PaymentId,
                OrderId = payment.OrderId ?? 0,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate ?? DateTime.UtcNow,
                PaymentStatus = payment.PaymentStatus,      
            };
        }

        private async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
            if (order != null)
            {
                order.OrderStatus = status;
                _unitOfWork.OrderRepository.Update(order);
                await _unitOfWork.SaveChanges();
            }
        }
    }
}
