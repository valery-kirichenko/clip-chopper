using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using NLog.Config;
using NLog.LayoutRenderers;

namespace NLog.Extension
{
    /// <summary>
    /// Renders exception starting from new line with short type exception name followed by message
    /// and stacktrace (optionally).
    /// If exception is logged more than once (catched, logged and re-thrown as inner), stack trace
    /// is not written.
    /// </summary>
    /// <remarks>
    /// Original source: <see href="https://stackoverflow.com/questions/46565639/nlog-exception-layout-to-format-exception-type-message-and-stack-trace?rq=1" />
    /// (there is link to GitHub repository too).
    /// </remarks>
    [LayoutRenderer("indent-exception")]
    [ThreadAgnostic]
    [ThreadSafe]
    public sealed class IndentExceptionLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Default stack trace indent in .NET is 3 space characters.
        /// </summary>
        private const string DefaultStackTraceIndent = "   ";

        /// <summary>
        /// Indent before exception type (default: 4 space characters).
        /// </summary>
        public string Indent { get; set; } = GenerateWhiteSpaceString(4);

        /// <summary>
        /// Indent between each stack trace line (default: 8 space characters).
        /// </summary>
        public string StackTraceIndent { get; set; } = GenerateWhiteSpaceString(8);

        /// <summary>
        /// Is written before exception type name (default: empty string).
        /// </summary>
        public string BeforeType { get; set; } = "";

        /// <summary>
        /// Is written after exception type name (default: empty string).
        /// </summary>
        public string AfterType { get; set; } = "";

        /// <summary>
        /// Separator between exception type and message (default: 1 space character).
        /// </summary>
        public string Separator { get; set; } = " ";

        /// <summary>
        /// Log stack trace or not e.g. for console logger (default: true).
        /// </summary>
        public bool LogStack { get; set; } = true;

        /// <summary>
        /// Is replaced newline characters in exception message or not (default: true).
        /// </summary>
        public bool ReplaceNewlinesInMessage { get; set; } = true;

        /// <summary>
        /// Holds logged already exceptions just to skip stack logging.
        /// </summary>
        private static readonly ConcurrentQueue<Exception> _loggedErrors =
            new ConcurrentQueue<Exception>();


        /// <summary>
        /// Provides default initialization.
        /// </summary>
        public IndentExceptionLayoutRenderer()
        {
        }

        #region LayoutRenderer Overriden Methods

        /// <summary>
        /// Appends formatted message with exception info.
        /// </summary>
        /// <param name="builder">Source string builder to append message.</param>
        /// <param name="logEvent">Specified parameter which contains exception info.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            Exception? ex = logEvent.Exception;
            while (ex != null)
            {
                builder.Append(
                    $"{Indent}{BeforeType}{ex.GetType().FullName}{AfterType}{Separator}"
                );

                string exMessage = ReplaceNewlinesInMessage
                    ? ex.Message.Replace(Environment.NewLine, " ")
                    : ex.Message.Replace(Environment.NewLine, $"{Environment.NewLine}{Indent}");

                builder.Append(exMessage);

                if (LogStack)
                {
                    if (!_loggedErrors.Contains(ex) && ex.StackTrace != null)
                    {
                        builder.AppendLine();
                        _loggedErrors.Enqueue(ex);

                        builder.Append(
                            ex.StackTrace.Replace(DefaultStackTraceIndent, StackTraceIndent)
                        );
                    }

                    // Vasily Vasilyev: Do not know, why it is needed (took from source in GitHub).
                    if (_loggedErrors.Count > 33)
                    {
                        _loggedErrors.TryDequeue(out _);
                        _loggedErrors.TryDequeue(out _);
                    }
                }

                ex = ex.InnerException;
                if (ex != null)
                {
                    builder.AppendLine();
                }
            }
        }

        #endregion

        /// <summary>
        /// Generates string which contains only of specified white-spaces characters number.
        /// </summary>
        /// <param name="spaceNumber">Number of white-spaces in returning string.</param>
        /// <returns>
        /// String which contains only of specified white-spaces characters number.
        /// </returns>
        private static string GenerateWhiteSpaceString(int spaceNumber)
        {
            return string.Concat(Enumerable.Repeat(" ", spaceNumber));
        }
    }
}
