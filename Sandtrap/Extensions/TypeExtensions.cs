using System;
using System.Collections.Generic;

namespace Sandtrap.Extensions
{

    public static class TypeExtensions
    {

        #region .Declarations 

        // Integral = sbyte, byte, short, ushort, int, unint, long, ulong
        private static HashSet<Type> IntegralNumericTypes = new HashSet<Type>
        {
            typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong)
        };
        // Fractional = float, double, decimal
        private static HashSet<Type> FractionalNumericTypes = new HashSet<Type>
        {
            typeof(float), typeof(double), typeof(decimal)
        };

        #endregion

        #region .Methods 

        /// <summary>
        /// Extension method to determine if a type if numeric.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// True if the type is numeric, otherwise false.
        /// </returns>
        public static bool IsNumeric(this Type type)
        {
            Type t = Nullable.GetUnderlyingType(type) ?? type;
            return IntegralNumericTypes.Contains(t) || FractionalNumericTypes.Contains(t);
        }

        /// <summary>
        /// Extension method to determine if a type if integral numeric.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// True if the type is integral numeric, otherwise false.
        /// </returns>
        public static bool IsIntegralNumeric(this Type type)
        {
            Type t = Nullable.GetUnderlyingType(type) ?? type;
            return IntegralNumericTypes.Contains(t);
        }

        #endregion

    }

}
