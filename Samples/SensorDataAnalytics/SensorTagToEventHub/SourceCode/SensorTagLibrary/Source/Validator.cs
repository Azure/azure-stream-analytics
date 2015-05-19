#region License
// Copyright © X2CodingLab Sebastian Lang 2013
// <author>Sebastian Lang</author>
// <project>TI Sensor Tag Library</project>
// <website>https://sensortag.codeplex.com/</website>
// <license>See https://sensortag.codeplex.com/license </license>
#endregion License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X2CodingLab.Utils
{
    internal class Validator
    {
        private Validator()
        {

        }

        public static void Requires<TException>(bool predicate) 
            where TException : Exception, new()
        {
            if(!predicate)
                throw new TException();
        }

        public static void RequiresArgument(bool predicate, string reason)
        {
            if (!predicate)
                throw new ArgumentException(reason);
        }

        public static void RequiresNotNull(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();
        }

        public static void RequiresNotNull(object obj, string argumentName)
        {
            if (obj == null)
                throw new ArgumentNullException(argumentName);
        }

        public static void RequiresNotNullOrEmpty(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException();
        }

        public static void RequiresNotNullOrEmpty(string value, string argumentName)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(argumentName);
        }
    }
}
