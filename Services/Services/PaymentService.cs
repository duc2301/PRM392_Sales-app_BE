using AutoMapper;
using Repositories.Interfaces;
using Repositories.Models;
using Services.DTOs.RequestDTOs;
using Services.DTOs.ResponseDTOs;

namespace Services.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponseDTO> CreatePaymentAsync(PaymentRequestDTO request);
        Task<PaymentResponseDTO?> GetPaymentByIdAsync(int paymentId);
        Task<IEnumerable<PaymentResponseDTO>> GetOrderPaymentsAsync(int orderId);
        Task<bool> UpdatePaymentStatusAsync(int paymentId, string status);
        Task<PaymentResponseDTO?> ConfirmPaymentAsync(int orderId, decimal amount);
    }

    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaymentResponseDTO> CreatePaymentAsync(PaymentRequestDTO request)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
                throw new Exception("Order not found");

            if (order.OrderStatus != "pending")
                throw new Exception($"Cannot create payment for {order.OrderStatus} order. Only pending orders can be paid");

            var existingPayment = (await _unitOfWork.PaymentRepository.GetByOrderIdAsync(request.OrderId)).FirstOrDefault();
            if (existingPayment != null && existingPayment.PaymentStatus == "success")
                throw new Exception("Payment for this order already confirmed");

            var payment = new Payment
            {
                OrderId = request.OrderId,
                Amount = request.Amount,
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

        public async Task<PaymentResponseDTO?> ConfirmPaymentAsync(int orderId, decimal amount)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new Exception("Order not found");

            if (order.OrderStatus != "pending")
                throw new Exception($"Order status is {order.OrderStatus}, payment already processed");

            var cartTotal = order.Cart?.TotalPrice ?? 0;
            var tolerance = cartTotal * 0.01m;
            if (Math.Abs(amount - cartTotal) > tolerance)
                throw new Exception($"Payment amount mismatch: expected {cartTotal}, got {amount}");

            var payments = await _unitOfWork.PaymentRepository.GetByOrderIdAsync(orderId);
            var payment = payments.FirstOrDefault();

            if (payment == null)
            {
                payment = new Payment
                {
                    OrderId = orderId,
                    Amount = amount,
                    PaymentDate = DateTime.Now,
                    PaymentStatus = "success"
                };
                await _unitOfWork.PaymentRepository.CreateAsync(payment);
            }
            else if (payment.PaymentStatus == "success")
            {
                await _unitOfWork.SaveChanges();
                var existingPayment = await _unitOfWork.PaymentRepository.GetByIdAsync(payment.PaymentId);
                return existingPayment != null ? MapPaymentToDTO(existingPayment) : null;
            }
            else
            {
                payment.Amount = amount;
                payment.PaymentDate = DateTime.Now;
                payment.PaymentStatus = "success";
                _unitOfWork.PaymentRepository.Update(payment);
            }

            await _unitOfWork.SaveChanges();

            var confirmedPayment = await _unitOfWork.PaymentRepository.GetByIdAsync(payment.PaymentId);
            if (confirmedPayment == null)
                return null;

            order.OrderStatus = "confirmed";
            _unitOfWork.OrderRepository.Update(order);
            await _unitOfWork.SaveChanges();

            return MapPaymentToDTO(confirmedPayment);
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
                TransactionId = payment.PaymentId.ToString()
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
