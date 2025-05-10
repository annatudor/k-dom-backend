using Ganss.Xss;

namespace KDomBackend.Helpers
{
    public static class HtmlSanitizerHelper
    {
        private static readonly HtmlSanitizer _sanitizer = new HtmlSanitizer();

        public static string Sanitize(string html)
        {
            return _sanitizer.Sanitize(html);
        }
    }
}
