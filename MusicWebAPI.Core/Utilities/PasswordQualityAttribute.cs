using MusicWebAPI.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public class PasswordQualityAttribute : ValidationAttribute
{
    private readonly int minLength;
    private readonly int requiredUniqueChars;
    private readonly Regex regex;

    public PasswordQualityAttribute(int minLength, int requiredUniqueChars)
    {
        this.minLength = minLength;
        this.requiredUniqueChars = requiredUniqueChars;
        this.regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$");
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        string password = value.ToString();

        if (password.Length < minLength)
        {
            return new ValidationResult(Resource.PasswordMin);
        }

        if (requiredUniqueChars > 0 && password.Distinct().Count() < requiredUniqueChars)
        {
            return new ValidationResult(Resource.PasswordUnique);
        }

        if (!regex.IsMatch(password))
        {
            return new ValidationResult(Resource.PasswordQuality);
        }

        return ValidationResult.Success;
    }
}