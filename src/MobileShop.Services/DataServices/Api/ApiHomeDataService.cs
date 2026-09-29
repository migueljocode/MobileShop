namespace MobileShop.Services.DataServices.Api;

public class ApiHomeDataService : IHomeDataService
{
    /// <inheritdoc />
    public Task<DashboardStockSummary> GetStockAsync()
        => throw new NotImplementedException("ApiHomeDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IReadOnlyList<TransactionCardViewModel>> GetRecentTransactionsAsync(int count = 20)
        => throw new NotImplementedException("ApiHomeDataService is not implemented yet.");
}
