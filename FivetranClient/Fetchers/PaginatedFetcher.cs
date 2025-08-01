﻿using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using FivetranClient.Models;

namespace FivetranClient.Fetchers;

public sealed class PaginatedFetcher(HttpRequestHandler requestHandler) : BaseFetcher(requestHandler)
{
    private const ushort PageSize = 100;

    public IAsyncEnumerable<T> FetchItemsAsync<T>(string endpoint, CancellationToken cancellationToken)
    {
        var firstPageTask = FetchPageAsync<T>(endpoint, cancellationToken);
        return ProcessPagesRecursivelyAsync(endpoint, firstPageTask, cancellationToken);
    }

    private async Task<PaginatedRoot<T>?> FetchPageAsync<T>(
        string endpoint,
        CancellationToken cancellationToken,
        string? cursor = null)
    {
        var response = cursor is null
            ? await RequestHandler.GetAsync($"{endpoint}?limit={PageSize}", cancellationToken)
            : await RequestHandler.GetAsync($"{endpoint}?limit={PageSize}&cursor={WebUtility.UrlEncode(cursor)}", cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedRoot<T>>(content, SerializerOptions);
    }

    // This implementation provides items as soon as they are available but also in the meantime fetches the next page
    private async IAsyncEnumerable<T> ProcessPagesRecursivelyAsync<T>(
        string endpoint,
        Task<PaginatedRoot<T>?> currentPageTask,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var currentPage = await currentPageTask;
        var nextCursor = currentPage?.Data?.NextCursor;

        IAsyncEnumerable<T>? nextResults = null;
        if (!string.IsNullOrWhiteSpace(nextCursor))
        {
            // fire and forget (await after yielding current items)
            var nextTask = FetchPageAsync<T>(endpoint, cancellationToken, nextCursor);
            nextResults = ProcessPagesRecursivelyAsync(endpoint, nextTask, cancellationToken);
        }

        foreach (var item in currentPage?.Data?.Items ?? [])
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
        }

        if (nextResults is null)
            yield break;
        await foreach (var nextItem in nextResults.WithCancellation(cancellationToken))
        {
            yield return nextItem;
        }

        cancellationToken.ThrowIfCancellationRequested();
    }
}