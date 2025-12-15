using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CamelliaPOS.WPF.Models;
using CamelliaPOS.WPF.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace CamelliaPOS.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private AdminToolsViewModel admin;

    [ObservableProperty]
    private bool canAccessAdmin;

    [ObservableProperty]
    private int lowStockCount;

    [ObservableProperty]
    private ObservableCollection<Category> categories = new();

    [ObservableProperty]
    private ObservableCollection<MenuItem> menuItems = new();

    [ObservableProperty]
    private ObservableCollection<CartItem> cartItems = new();

    [ObservableProperty]
    private Category? selectedCategory;

    [ObservableProperty]
    private decimal subtotal;

    [ObservableProperty]
    private decimal discount;

    [ObservableProperty]
    private decimal tax;

    [ObservableProperty]
    private decimal total;

    [ObservableProperty]
    private decimal paidAmount;

    [ObservableProperty]
    private decimal balance;

    [ObservableProperty]
    private int paymentMethod; // 0 = Cash, 1 = Card

    [ObservableProperty]
    private bool isLoading;

    public MainViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Admin = new AdminToolsViewModel(apiService);
        CanAccessAdmin = App.CurrentRole == "Admin" || App.CurrentRole == "Manager";
        _ = LoadCategoriesAsync(); // fire and forget on startup
        if (CanAccessAdmin)
        {
            _ = RefreshLowStockCountAsync();
        }
    }

    private async Task LoadCategoriesAsync()
    {
        IsLoading = true;
        try
        {
            var cats = await _apiService.GetCategoriesAsync();
            Categories.Clear();
            foreach (var cat in cats)
            {
                Categories.Add(cat);
            }
            
            if (Categories.Count > 0)
            {
                SelectedCategory = Categories[0];
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading categories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedCategoryChanged(Category? value)
    {
        if (value != null)
        {
            _ = LoadMenuItemsAsync(value.Id); // fire and forget
        }
    }
    
    [RelayCommand]
    private void SelectCategory(Category category)
    {
        SelectedCategory = category;
    }

    private async Task LoadMenuItemsAsync(int categoryId)
    {
        IsLoading = true;
        try
        {
            var items = await _apiService.GetMenuItemsByCategoryAsync(categoryId);
            MenuItems.Clear();
            foreach (var item in items)
            {
                MenuItems.Add(item);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading menu items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void AddToCart(MenuItem menuItem)
    {
        var existingItem = CartItems.FirstOrDefault(c => c.MenuItem.Id == menuItem.Id);
        
        if (existingItem != null)
        {
            existingItem.Quantity++;
        }
        else
        {
            CartItems.Add(new CartItem
            {
                MenuItem = menuItem,
                Quantity = 1,
                UnitPrice = menuItem.Price
            });
        }
        
        CalculateTotals();
    }

    [RelayCommand]
    private void RemoveFromCart(CartItem cartItem)
    {
        CartItems.Remove(cartItem);
        CalculateTotals();
    }

    [RelayCommand]
    private void IncreaseQuantity(CartItem cartItem)
    {
        cartItem.Quantity++;
        CalculateTotals();
    }

    [RelayCommand]
    private void DecreaseQuantity(CartItem cartItem)
    {
        if (cartItem.Quantity > 1)
        {
            cartItem.Quantity--;
            CalculateTotals();
        }
        else
        {
            RemoveFromCart(cartItem);
        }
    }

    partial void OnDiscountChanged(decimal value)
    {
        CalculateTotals();
    }

    partial void OnTaxChanged(decimal value)
    {
        CalculateTotals();
    }

    partial void OnPaidAmountChanged(decimal value)
    {
        Balance = PaidAmount - Total;
    }

    private void CalculateTotals()
    {
        Subtotal = CartItems.Sum(c => c.TotalPrice);
        Total = Subtotal - Discount + Tax;
        Balance = PaidAmount - Total;
    }

    public async Task RefreshLowStockCountAsync()
    {
        if (!CanAccessAdmin)
            return;

        try
        {
            var lowStockItems = await _apiService.GetLowStockAsync();
            LowStockCount = lowStockItems?.Count ?? 0;
        }
        catch
        {
            // Ignore errors when refreshing low stock count to avoid impacting POS usage
        }
    }

    [RelayCommand]
    private async Task ProcessPaymentAsync()
    {
        if (CartItems.Count == 0)
        {
            MessageBox.Show("Cart is empty", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (PaymentMethod == 0 && PaidAmount < Total) // Cash payment
        {
            MessageBox.Show("Paid amount is less than total amount", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsLoading = true;
        try
        {
            var orderRequest = new OrderRequest
            {
                Items = CartItems.Select(c => new OrderItemRequest
                {
                    MenuItemId = c.MenuItem.Id,
                    Quantity = c.Quantity
                }).ToList(),
                Discount = Discount,
                Tax = Tax,
                PaymentMethod = PaymentMethod,
                PaidAmount = PaymentMethod == 0 ? PaidAmount : Total // Card payment = total
            };

            var (response, error) = await _apiService.CreateOrderAsync(orderRequest);
            
            if (response != null)
            {
                MessageBox.Show($"Order #{response.OrderNumber} processed successfully!\nTotal: {response.Total:C}\nBalance: {response.Balance:C}", 
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Clear cart
                CartItems.Clear();
                Discount = 0;
                Tax = 0;
                PaidAmount = 0;
                Balance = 0;
                
                // TODO: Print receipt
                // TODO: Open cash drawer if cash payment
            }
            else
            {
                var message = string.IsNullOrWhiteSpace(error) ? "Failed to process order" : error;
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error processing payment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ClearCart()
    {
        CartItems.Clear();
        Discount = 0;
        Tax = 0;
        PaidAmount = 0;
        Balance = 0;
        CalculateTotals();
    }
}

