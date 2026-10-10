using System;
using Acolyte.Assertions;

namespace ClipChopper.Logging
{
    public static class LoggerFactory
    {
        /// <summary>
        /// Creates logger instance for passed type.
        /// </summary>
        /// <typeparam name="T">Type for which instance is created.</typeparam>
        /// <returns>Created logger instance.</returns>
        /// <exception cref="ArgumentException">
        /// Cannot get full name of type <typeparamref name="T" />.
        /// </exception>
        public static ILogger CreateLoggerFor<T>()
        {
            Type type = typeof(T);
            string fullName = type.FullName ?? throw new ArgumentException(
                $"Could not get full name of class {type}."
            );
            return new NLogLoggerAdapter(fullName);
        }

        /// <summary>
        /// Creates logger instance for passed class type.
        /// </summary>
        /// <param name="type">Class name. Try to pass it with <c>typeof</c> operator.</param>
        /// <returns>Created logger instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="type" /> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Cannot get full name of passed type <paramref name="type" />.
        /// </exception>
        public static ILogger CreateLoggerFor(Type type)
        {
            type.ThrowIfNull(nameof(type));

            string fullName = type.FullName ?? throw new ArgumentException(
                $"Could not get full name of class {nameof(type)}"
            );
            return new NLogLoggerAdapter(fullName);
        }
    }
}
