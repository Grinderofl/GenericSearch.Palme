using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;

namespace GenericSearch.UI
{
    public static class HighlightingHtmlHelper
    {
        private const string HighlightStart = "<span class=\"bg-info\">";

        private const string HighlightEnd = "</span>";

        public static HtmlString HighlightKeyWords(this string term, string keyword)
        {
            return term.HighlightKeyWords(new[] { keyword });
        }

        public static HtmlString HighlightKeyWords(this string term, IEnumerable<string> keywords)
        {
            if (keywords == null)
            {
                throw new ArgumentNullException(nameof(keywords));
            }

            if (term == null)
            {
                return HtmlString.Empty;
            }

            // Collect all characters that must be highlighted
            var highlightCharIndices = new bool[term.Length];

            foreach (var keyword in keywords.Where(k => !string.IsNullOrEmpty(k)).OrderByDescending(k => k.Length))
            {
                var currentIndex = 0;

                while (currentIndex != -1 && currentIndex < term.Length)
                {
                    currentIndex = term.IndexOf(keyword, currentIndex, StringComparison.CurrentCultureIgnoreCase);

                    if (currentIndex > -1)
                    {
                        for (var i = 0; i < keyword.Length; i++)
                        {
                            highlightCharIndices[currentIndex + i] = true;
                        }

                        currentIndex += keyword.Length;
                    }
                }
            }

            // Highlight all necessary characters
            var offset = 0;

            for (var i = 0; i < highlightCharIndices.Length; i++)
            {
                if (!highlightCharIndices[i])
                {
                    continue;
                }

                var start = i;
                var end = i + 1;
                var current = i;

                while (current < highlightCharIndices.Length - 1 && highlightCharIndices[++current])
                {
                    end++;
                }

                term = term.Insert(offset + end, HighlightEnd);
                term = term.Insert(offset + start, HighlightStart);

                offset += HighlightStart.Length + HighlightEnd.Length;
                i += end - start;
            }

            return new HtmlString(term);
        }
    }
}