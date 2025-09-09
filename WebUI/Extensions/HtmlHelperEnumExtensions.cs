using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace WebUI.Extensions
{
    public static class HtmlHelperEnumExtensions
    {
        public static IEnumerable<SelectListItem> GetEnumSelectListWithDisplay<TEnum>(this Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper html) where TEnum : struct, Enum
        {
            var enumType = typeof(TEnum);
            var values = Enum.GetValues(enumType).Cast<TEnum>();
            foreach (var val in values)
            {
                var mem = enumType.GetMember(val.ToString()).FirstOrDefault();
                var display = mem?.GetCustomAttributes(typeof(DisplayAttribute), false)
                              .Cast<DisplayAttribute>()
                              .FirstOrDefault()?.GetName() ?? val.ToString();
                yield return new SelectListItem
                {
                    Value = Convert.ToInt32(val).ToString(),
                    Text = display
                };
            }
        }

        public static string GetEnumDisplayName(this Enum value)
        {
            var enumType = value.GetType();
            var mem = enumType.GetMember(value.ToString()).FirstOrDefault();
            var display = mem?.GetCustomAttributes(typeof(DisplayAttribute), false)
                          .Cast<DisplayAttribute>()
                          .FirstOrDefault()?.GetName() ?? value.ToString();
            return display;
        }
    }
}
