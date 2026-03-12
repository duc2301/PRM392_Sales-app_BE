using AutoMapper;
using Repositories.Interfaces;
using Repositories.Models;
using Services.DTOs.RequestDTOs;
using Services.DTOs.ResponseDTOs;
using Services.Interfaces;

namespace Services.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CartResponseDTO?> GetCartByUserIdAsync(int userId)
        {
            var cart = await _unitOfWork.CartRepository.GetCartByUserIdAsync(userId);

            if (cart == null || cart.Status != "active") 
                return null;

            return MapCartToDTO(cart);
        }

        public async Task<CartResponseDTO?> GetByIdAsync(int id)
        {
            var cart = await _unitOfWork.CartRepository.GetByIdAsync(id);
            if (cart == null) return null;

            return MapCartToDTO(cart);
        }

        public async Task<CartResponseDTO> AddToCartAsync(int userId, CartItemRequestDTO request)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(request.ProductId);
            if (product == null)
                throw new Exception("Product not found");

            var cart = await _unitOfWork.CartRepository.GetCartByUserIdAsync(userId);

            if (cart == null || cart.Status != "active")
            {
                cart = new Cart
                {
                    UserId = userId,
                    Status = "active",
                    TotalPrice = 0,
                    CartItems = new List<CartItem>()
                };
                await _unitOfWork.CartRepository.CreateAsync(cart);
                await _unitOfWork.SaveChanges();

                cart = await _unitOfWork.CartRepository.GetCartByUserIdAsync(userId);
            }

            var existingItem = await _unitOfWork.CartItemRepository.GetByCartAndProductAsync(cart.CartId, request.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
                _unitOfWork.CartItemRepository.Update(existingItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    Price = product.Price
                };
                await _unitOfWork.CartItemRepository.CreateAsync(cartItem);
            }

            await _unitOfWork.SaveChanges();

            await UpdateCartTotalAsync(cart.CartId);
            await _unitOfWork.SaveChanges();

            cart = await _unitOfWork.CartRepository.GetByIdAsync(cart.CartId);
            return MapCartToDTO(cart!);
        }

        public async Task<bool> UpdateCartItemAsync(int cartItemId, int quantity)
        {
            var cartItem = await _unitOfWork.CartItemRepository.GetByIdAsync(cartItemId);
            if (cartItem == null) return false;

            var cart = await _unitOfWork.CartRepository.GetByIdAsync(cartItem.CartId ?? 0);
            if (cart == null || cart.Status != "active")
                throw new Exception("Cannot update items in non-active cart. Please add items to your active cart instead.");

            if (quantity <= 0)
            {
                return await RemoveCartItemAsync(cartItemId);
            }

            cartItem.Quantity = quantity;
            _unitOfWork.CartItemRepository.Update(cartItem);
            await _unitOfWork.SaveChanges();

            if (cartItem.CartId.HasValue)
            {
                await UpdateCartTotalAsync(cartItem.CartId.Value);
                await _unitOfWork.SaveChanges();
            }

            return true;
        }

        public async Task<bool> RemoveCartItemAsync(int cartItemId)
        {
            var cartItem = await _unitOfWork.CartItemRepository.GetByIdAsync(cartItemId);
            if (cartItem == null) return false;

            var cart = await _unitOfWork.CartRepository.GetByIdAsync(cartItem.CartId ?? 0);
            if (cart == null || cart.Status != "active")
                throw new Exception("Cannot remove items from non-active cart. Please use your active cart instead.");

            var cartId = cartItem.CartId;
            _unitOfWork.CartItemRepository.Delete(cartItem);
            await _unitOfWork.SaveChanges();

            if (cartId.HasValue)
            {
                await UpdateCartTotalAsync(cartId.Value);
                await _unitOfWork.SaveChanges();
            }

            return true;
        }

        public async Task<CartResponseDTO?> ClearCartAsync(int userId)
        {
            var cart = await _unitOfWork.CartRepository.GetCartByUserIdAsync(userId);

            if (cart == null || cart.Status != "active") 
                return null;

            var cartItemIds = cart.CartItems.Select(ci => ci.CartItemId).ToList();

            foreach (var itemId in cartItemIds)
            {
                var item = await _unitOfWork.CartItemRepository.GetByIdAsync(itemId);
                if (item != null)
                {
                    _unitOfWork.CartItemRepository.Delete(item);
                }
            }

            await _unitOfWork.SaveChanges();

            cart.TotalPrice = 0;
            _unitOfWork.CartRepository.Update(cart);
            await _unitOfWork.SaveChanges();

            return MapCartToDTO(cart);

            var clearedCart = await _unitOfWork.CartRepository.GetCartByUserIdAsNoTrackingAsync(userId);
            if (clearedCart == null) return null;

            return MapCartToDTO(clearedCart);
        }

        public async Task<CartBadgeResponseDTO> GetCartBadgeAsync(int userId)
        {
            var cart = await _unitOfWork.CartRepository.GetCartByUserIdAsync(userId);

            if (cart == null || cart.Status != "active")
            {
                return new CartBadgeResponseDTO { CartItemCount = 0, TotalPrice = 0 };
            }

            return new CartBadgeResponseDTO
            {
                CartItemCount = cart.CartItems.Sum(ci => ci.Quantity),
                TotalPrice = cart.TotalPrice
            };
        }

        private CartResponseDTO MapCartToDTO(Cart cart)
        {
            var cartItems = cart.CartItems.Select(ci => new CartItemResponseDTO
            {
                CartItemId = ci.CartItemId,
                ProductId = ci.ProductId ?? 0,
                ProductName = ci.Product?.ProductName,
                ProductImage = ci.Product?.ImageUrl,
                Quantity = ci.Quantity,
                Price = ci.Price,
                SubTotal = ci.Price * ci.Quantity
            }).ToList();

            return new CartResponseDTO
            {
                CartId = cart.CartId,
                UserId = cart.UserId,
                CartItems = cartItems,
                TotalPrice = cart.TotalPrice,
                Status = cart.Status
            };
        }

        private async Task UpdateCartTotalAsync(int cartId)
        {
            var cart = await _unitOfWork.CartRepository.GetByIdAsync(cartId);
            if (cart != null)
            {
                cart.TotalPrice = cart.CartItems.Sum(ci => ci.Price * ci.Quantity);
                _unitOfWork.CartRepository.Update(cart);
                await _unitOfWork.SaveChanges();
            }
        }
    }
}
