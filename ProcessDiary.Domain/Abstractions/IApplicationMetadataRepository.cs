using ProcessDiary.Domain.Entities;

namespace ProcessDiary.Domain.Abstractions;

/// <summary>Persistence of application metadata.</summary>
public interface IApplicationMetadataRepository
{
    /// <summary>Gets all stored application metadata.</summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>All metadata entries.</returns>
    Task<IReadOnlyList<ApplicationMetadata>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>Inserts or updates the metadata of an application.</summary>
    /// <param name="metadata">The metadata to store.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the metadata is stored.</returns>
    Task UpsertAsync(ApplicationMetadata metadata, CancellationToken cancellationToken);
}
