﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GenericSearch.Paging
{
    /// <summary>
    /// Wraps a list of items together with the total number of available items.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    [DataContract]
    public class PagedResult<T> : IEnumerable<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedResult&lt;T&gt;"/> class.
        /// </summary>
        public PagedResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedResult&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="totalNumberOfItems">The total number of items.</param>
        /// <param name="paging">The paging.</param>
        public PagedResult(IEnumerable<T> items, int totalNumberOfItems, Paging<T> paging)
        {
            Items = items;
            TotalNumberOfItems = totalNumberOfItems;
            Paging = paging;
        }

        /// <summary>
        /// Gets the items.
        /// </summary>
        [DataMember]
        public IEnumerable<T> Items { get; private set; }

        public Paging<T> Paging { get; private set; }

        /// <summary>
        /// Gets the total number of items.
        /// </summary>
        [DataMember]
        public int TotalNumberOfItems { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}