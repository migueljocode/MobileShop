namespace MobileShop.Services.DataServices.Api;

public class ApiReportsDataService : IReportsDataService
{
    /// <inheritdoc />
    public Task<IReadOnlyList<ProfitLossRowViewModel>> GetProfitLossRowsAsync(DateTime? from, DateTime? to)
        => throw new NotImplementedException("ApiReportsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<long> GetProfitLossTotalAsync(DateTime? from, DateTime? to)
        => throw new NotImplementedException("ApiReportsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<DateTime?> GetEarliestTransactionDateAsync()
        => throw new NotImplementedException("ApiReportsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IReadOnlyList<DistributionRow>> GetDistributionRowsAsync(long totalProfit)
        => throw new NotImplementedException("ApiReportsDataService is not implemented yet.");
}
