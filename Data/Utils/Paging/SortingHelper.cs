using MongoDB.Driver;
using System;
using System.Linq.Expressions;

namespace Data.Utils.Paging;

public static class SortingHelper
{
    public static IOrderedFindFluent<TDocument, TDocument> GetWithSorting<TDocument>(
        this IFindFluent<TDocument, TDocument> source,
        string sortKey,
        PagingConstant.OrderCriteria sortOrder)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (string.IsNullOrEmpty(sortKey))
        {
            return (IOrderedFindFluent<TDocument, TDocument>)source;
        }

        var sortDefinitionBuilder = new SortDefinitionBuilder<TDocument>();
        SortDefinition<TDocument> sortDefinition;

        switch (sortOrder)
        {
            case PagingConstant.OrderCriteria.ASC:
                sortDefinition = sortDefinitionBuilder.Ascending(sortKey);
                break;
            case PagingConstant.OrderCriteria.DESC:
                sortDefinition = sortDefinitionBuilder.Descending(sortKey);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, null);
        }

        return (IOrderedFindFluent<TDocument, TDocument>)source.Sort(sortDefinition);
    }
}

