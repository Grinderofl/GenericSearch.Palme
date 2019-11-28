using System.ComponentModel.DataAnnotations;

namespace GenericSearch.Core
{
    public enum DateComparators
    {
        [Display(Name = "<")]
        Less,

        [Display(Name = "<=")]
        LessOrEqual,

        [Display(Name = "==")]
        Equal,

        [Display(Name = ">=")]
        GreaterOrEqual,

        [Display(Name = ">")]
        Greater,

        [Display(Name = "Range")]
        InRange
    }
}