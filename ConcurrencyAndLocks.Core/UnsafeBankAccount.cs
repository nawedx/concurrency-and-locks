namespace ConcurrencyAndLocks.Core;

public class UnsafeBankAccount
{
    private decimal _balance;
    private int _transactionCount;

    public UnsafeBankAccount(decimal initialBalance = 0)
    {
        _balance = initialBalance;
        _transactionCount = 0;
    }

    public decimal Balance => _balance;
    public int TransactionCount => _transactionCount;

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        // Simulate some processing time to increase chance of race conditions
        Thread.Sleep(1);

        var currentBalance = _balance;

        // More processing time
        Thread.Sleep(1);

        _balance = currentBalance + amount;
        _transactionCount++;
    }

    public bool Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        // Simulate some processing time
        Thread.Sleep(1);

        var currentBalance = _balance;

        if (currentBalance >= amount)
        {
            // More processing time to increase race condition window
            Thread.Sleep(1);

            _balance = currentBalance - amount;
            _transactionCount++;
            return true;
        }

        return false;
    }

    public bool Transfer(UnsafeBankAccount targetAccount, decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        if (targetAccount == null)
            throw new ArgumentNullException(nameof(targetAccount));

        // Check if we have sufficient balance
        if (_balance >= amount)
        {
            // Simulate processing time
            Thread.Sleep(2);

            // Withdraw from this account
            _balance -= amount;
            _transactionCount++;

            // Simulate network delay or processing
            Thread.Sleep(2);

            // Deposit to target account
            targetAccount._balance += amount;
            targetAccount._transactionCount++;

            return true;
        }

        return false;
    }

    public void Reset(decimal newBalance = 0)
    {
        _balance = newBalance;
        _transactionCount = 0;
    }
}