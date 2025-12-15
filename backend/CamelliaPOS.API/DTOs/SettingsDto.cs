namespace CamelliaPOS.API.DTOs;

public class SettingsDto
{
    public decimal TaxPercentage { get; set; }
    public string Currency { get; set; } = "LKR";
    public int SessionTimeoutMinutes { get; set; }
}

public class UpdateSettingsDto
{
    public decimal TaxPercentage { get; set; }
    public string Currency { get; set; } = "LKR";
    public int SessionTimeoutMinutes { get; set; } = 30;
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

