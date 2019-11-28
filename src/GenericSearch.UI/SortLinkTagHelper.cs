using System.Collections.Generic;
using GenericSearch.Paging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GenericSearch.UI
{
    [HtmlTargetElement("a", Attributes = "paging,property-name")]
    public class SortLinkTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public SortLinkTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public dynamic Paging { get; set; }

        public string PropertyName { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var url = httpContextAccessor.HttpContext.Request.QueryString.Value;

            string extendedUrl = url.SetParameters(new KeyValuePair<string, string>("sortColumn", PropertyName),
                                                   new KeyValuePair<string, string>("sortDirection", GetSortDirection().ToString()),
                                                   new KeyValuePair<string, string>("skip", "0"));

            output.Attributes.Add("href", extendedUrl);
        }

        private SortDirection GetSortDirection()
        {
            var sortDirection = SortDirection.Ascending;

            if (Paging != null && PropertyName.Equals(Paging.SortColumn) && Paging?.SortDirection == SortDirection.Ascending)
            {
                sortDirection = SortDirection.Descending;
            }

            return sortDirection;
        }
    }
}