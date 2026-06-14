using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Common.ValueObjects.Errors;

public static class MoneyErrors
{
    public static readonly Error MismatchedCurrencies = new(
        "Money.MismatchedCurrencies",
        "Cannot perform this operation on amounts with different currencies."
    );

    public static readonly Error InvalidExchangeRate = new(
        "Money.InvalidExchangeRate",
        "Exchange rate must be greater than zero."
    );

    public static readonly Error UnknownCurrency = new(
        "Money.UnknownCurrency",
        "The specified currency code is not recognized."
    );
}
