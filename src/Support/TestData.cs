namespace SauceDemo.E2E.Support;

public static class TestData
{
    public const string StandardUser = "standard_user";
    public const string LockedOutUser = "locked_out_user";
    public const string ProblemUser = "problem_user";
    public const string Password = "secret_sauce";

    public const string FirstName = "Hong";
    public const string LastName = "GilDong";
    public const string PostalCode = "12345";

    public const string Backpack = "Sauce Labs Backpack";
    public const string BikeLight = "Sauce Labs Bike Light";
    public const string FleeceJacket = "Sauce Labs Fleece Jacket";

    /// <summary>SauceDemo 체크아웃 세율. 소계에 곱한 뒤 소수 둘째 자리에서 반올림한다.</summary>
    public const decimal TaxRate = 0.08m;
}
