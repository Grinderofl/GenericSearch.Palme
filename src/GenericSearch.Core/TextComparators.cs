using System.ComponentModel.DataAnnotations;

namespace GenericSearch.Core
{
    public enum TextComparators
    {
        [Display(Name = "Contains")]
        Contains,

        [Display(Name = "==")]
        Equals
    }
}