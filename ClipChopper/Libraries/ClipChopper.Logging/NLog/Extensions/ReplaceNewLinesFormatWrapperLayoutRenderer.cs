using System;
using System.Text;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.LayoutRenderers.Wrappers;
using NLog.Layouts;

namespace NLog.Extension
{
    /// <remarks>
    /// <see href="https://stackoverflow.com/questions/34333794/print-a-multi-line-message-with-nlog" />
    /// </remarks>
    [LayoutRenderer("replace-newlines-withlayout")]
    [ThreadAgnostic]
    [ThreadSafe]
    public sealed class ReplaceNewLinesFormatWrapperLayoutRenderer : WrapperLayoutRendererBase
    {
        private string _replacementString = " ";

        // Changed from
        // public string Replacement { get; set; }
        public Layout Replacement { get; set; }


        public ReplaceNewLinesFormatWrapperLayoutRenderer()
        {
            // Changed from
            // Replacement = " ";
            Replacement = Layout.FromString(" ");
        }

        // Override Append in order to render the replacement.
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            // Render...
            _replacementString = Replacement.Render(logEvent);

            // The base functionality of append is fine.
            base.Append(builder, logEvent);
        }

        // Called from base.Append().
        protected override string Transform(string text)
        {
            // Changed from
            // return text.Replace(Environment.NewLine, Replacement);

            // Now just put in the rendered replacement string.
            return text.Replace(Environment.NewLine, _replacementString);
        }
    }
}
