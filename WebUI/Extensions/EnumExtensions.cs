using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace WebUI.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            if (enumValue == null) return string.Empty;
            
            return enumValue.GetType()
                           .GetMember(enumValue.ToString())
                           .FirstOrDefault()
                           ?.GetCustomAttribute<DisplayAttribute>()
                           ?.GetName() ?? enumValue.ToString();
        }
    }
}