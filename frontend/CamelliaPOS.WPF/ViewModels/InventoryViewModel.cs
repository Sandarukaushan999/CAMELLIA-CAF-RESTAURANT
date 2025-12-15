using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CamelliaPOS.WPF.Models;
using CamelliaPOS.WPF.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace CamelliaPOS.WPF.ViewModels;

public partial class InventoryViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<Category> categories = new();

    [ObservableProperty]
    private ObservableCollection<MenuItem> allItems = new();

    // Add Category
    [ObservableProperty] private string categoryName = string.Empty;
    [ObservableProperty] private string categoryIcon = "📦";
    [ObservableProperty] private string categoryColor = "#8B4513";
    [ObservableProperty] private int categoryDisplayOrder = 1;

    // Add Item
    [ObservableProperty] private string itemName = string.Empty;
    [ObservableProperty] private string? itemDescription;
    [ObservableProperty] private decimal itemPrice;
    [ObservableProperty] private decimal itemCost;
    [ObservableProperty] private string? itemBarcode;
    [ObservableProperty] private string? itemImagePath;
    [ObservableProperty] private int itemStock;
    [ObservableProperty] private int itemMinStock;
    [ObservableProperty] private Category? selectedCategory;

    [ObservableProperty] private bool isLoading;

    public InventoryViewModel(ApiService apiService)
    {
        _apiService = apiService;
        _ = LoadCategoriesAsync();
        _ = LoadItemsAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        IsLoading = true;
        try
        {
            var list = await _apiService.GetCategoriesAsync();
            Categories.Clear();
            foreach (var c in list)
                Categories.Add(c);
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

    private async Task LoadItemsAsync()
    {
        try
        {
            var list = await _apiService.GetAllMenuItemsAsync();
            AllItems.Clear();
            foreach (var i in list)
                AllItems.Add(i);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task AddCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(CategoryName))
        {
            MessageBox.Show("Category name is required.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsLoading = true;
        try
        {
            var ok = await _apiService.CreateCategoryAsync(CategoryName, CategoryIcon, CategoryColor, CategoryDisplayOrder);
            if (ok)
            {
                MessageBox.Show("Category created.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                CategoryName = string.Empty;
                CategoryIcon = "📦";
                CategoryColor = "#8B4513";
                CategoryDisplayOrder = 1;
                await LoadCategoriesAsync();
            }
            else
            {
                MessageBox.Show("Failed to create category.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddItemAsync()
    {
        if (SelectedCategory == null)
        {
            MessageBox.Show("Select a category.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(ItemName))
        {
            MessageBox.Show("Item name is required.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsLoading = true;
        try
        {
            var req = new MenuItemCreateRequest
            {
                Name = ItemName,
                Description = ItemDescription,
                Price = ItemPrice,
                Cost = ItemCost,
                Barcode = ItemBarcode,
                ImagePath = ItemImagePath,
                CategoryId = SelectedCategory.Id,
                IsCombo = false,
                StockQuantity = ItemStock,
                MinStockLevel = ItemMinStock
            };

            var ok = await _apiService.CreateMenuItemAsync(req);
            if (ok)
            {
                MessageBox.Show("Item created (awaiting approval if not admin).", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                ItemName = string.Empty;
                ItemDescription = null;
                ItemPrice = 0;
                ItemCost = 0;
                ItemBarcode = null;
                ItemImagePath = null;
                ItemStock = 0;
                ItemMinStock = 0;
                await LoadItemsAsync();
            }
            else
            {
                MessageBox.Show("Failed to create item.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ToggleActiveAsync(MenuItem item)
    {
        if (item == null) return;
        IsLoading = true;
        try
        {
            var ok = await _apiService.SetMenuItemActiveAsync(item.Id, !item.IsActive);
            if (ok)
            {
                await LoadItemsAsync();
            }
            else
            {
                MessageBox.Show("Failed to update item state.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
}


