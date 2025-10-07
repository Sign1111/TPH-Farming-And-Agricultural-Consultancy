using System;
using System.ComponentModel.DataAnnotations;

namespace Ade_Farming.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RequiredIfSellerAttribute : ValidationAttribute
    {
        private readonly string _booleanPropertyName;

        public RequiredIfSellerAttribute(string booleanPropertyName)
        {
            _booleanPropertyName = booleanPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Get the boolean property value
            var booleanProperty = validationContext.ObjectType.GetProperty(_booleanPropertyName);
            if (booleanProperty == null)
                return ValidationResult.Success;

            var booleanValueObj = booleanProperty.GetValue(validationContext.ObjectInstance);

            // Only validate if boolean property is true
            bool isSeller = booleanValueObj != null && (bool)booleanValueObj;
            if (!isSeller)
                return ValidationResult.Success; // skip validation

            // Now check the value
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.");
            }

            return ValidationResult.Success;
        }
    }
}
