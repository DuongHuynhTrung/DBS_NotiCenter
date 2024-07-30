using MongoDB.Driver;
using System;
using static Data.Utils.Paging.PagingConstant;

namespace Data.Utils.Paging;

public static class PagingHelper
{
    public static IFindFluent<TDocument, TDocument> GetWithPaging<TDocument>(
       this IFindFluent<TDocument, TDocument> source,
       int page,
       int pageSize,
       int safePageSizeLimit = FixedPagingConstant.MaxPageSize)
    {
        if (pageSize > safePageSizeLimit)
        {
            throw new Exception("Input page size is over safe limitation.");
        }

        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        pageSize = pageSize < 1 ? 1 : pageSize;
        page = page < 1 ? 1 : page;

        return source.Skip(page == 1 ? 0 : pageSize * (page - 1))
                     .Limit(pageSize);
    }
}

