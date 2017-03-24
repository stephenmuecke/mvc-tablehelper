using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Sandtrap.Extensions
{
    
    public static class EnumExtensions
    {

        /// <summary>
        /// Extension method to return the display value of an enum. 
        /// </summary>
        /// <param name="value">
        /// The enum value.
        /// </param>
        /// <returns>
        /// The value of a DisplayAttribute if applied, otherwise the value of the enum.
        /// </returns>
        public static string ToDisplayName(this Enum value)
        {
            if (value == null)
            {
                return null;
            }
            FieldInfo field = value.GetType().GetField(value.ToString());
            DisplayAttribute[] attributes = (DisplayAttribute[])field.GetCustomAttributes(typeof(DisplayAttribute), false);
            if (attributes.Length > 0)
            {
                return attributes[0].Name;
            }
            return value.ToString();
        }

        public static string ToDescription(this Enum value)
        {
            if (value == null)
            {
                return null;
            }
            FieldInfo field = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])field
                .GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            return value.ToString();
        }

    }
}
