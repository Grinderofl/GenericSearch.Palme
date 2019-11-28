﻿using System.ComponentModel.DataAnnotations;

namespace GenericSearch.Core.Tests
{
    public enum MyEnum
    {
        [Display(Name = "First entry")]
        First,

        [Display(Name = "Second entry")]
        Second,

        [Display(Name = "Third entry")]
        Third
    }
}
