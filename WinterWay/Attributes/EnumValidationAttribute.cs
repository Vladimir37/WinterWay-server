using System.ComponentModel.DataAnnotations;

namespace WinterWay.Attributes
{
    public class EnumValidationAttribute : ValidationAttribute
    {
        private readonly Type _enumType;

        public EnumValidationAttribute(Type enumType)
        {
            _enumType = enumType;
        }

        public override bool IsValid(object? value)
        {
            return value != null && Enum.IsDefined(_enumType, value);
        }
    }
}
