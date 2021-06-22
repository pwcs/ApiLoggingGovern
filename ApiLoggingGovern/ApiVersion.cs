using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ApiLoggingGovern
{
    public enum ApiVersion
    {
        /// <summary>
        /// v1版本
        /// </summary>
        [Description("V1")]
        V1 = 1,
        /// <summary>
        /// v2版本
        /// </summary>
        [Description("V2")]
        V2 = 2
    }


    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            return value.GetType()
                .GetMember(value.ToString())
                .FirstOrDefault()?
                .GetCustomAttribute<DescriptionAttribute>()?
                .Description;
        }
    }
}
