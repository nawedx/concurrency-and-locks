using ConcurrencyAndLocks.Core;
using Xunit.Abstractions;

namespace ConcurrencyAndLocks.Tests;

public class UnsafeBankAccountTests
{
    private readonly ITestOutputHelper _output;

    public UnsafeBankAccountTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void SingleThreaded_Operations_Work_Correctly()
    {
        // Arrange
        var account = new UnsafeBankAccount(1000);

        // Act
        account.Deposit(500);
        account.Withdraw(200);
        account.Deposit(100);

        // Assert
        Assert.Equal(1400, account.Balance);
        Assert.Equal(3, account.TransactionCount);
    }

    [Fact]
    public async Task Concurrent_Deposits_Cause_Race_Conditions()
    {
        // This test will likely fail due to race conditions
        const int numberOfThreads = 10;
        const int depositsPerThread = 100;
        const decimal depositAmount = 1;
        const decimal expectedBalance = numberOfThreads * depositsPerThread * depositAmount;

        var account = new UnsafeBankAccount(0);
        var tasks = new List<Task>();

        _output.WriteLine(
            $"Starting {numberOfThreads} threads, each making {depositsPerThread} deposits of ${depositAmount}");
        _output.WriteLine($"Expected final balance: ${expectedBalance}");

        // Create multiple tasks that deposit concurrently
        for (int i = 0; i < numberOfThreads; i++)
        {
            int threadId = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < depositsPerThread; j++)
                {
                    account.Deposit(depositAmount);
                }

                _output.WriteLine($"Thread {threadId} completed all deposits");
            }));
        }

        // Wait for all tasks to complete
        await Task.WhenAll(tasks);

        // Results
        _output.WriteLine($"Actual final balance: ${account.Balance}");
        _output.WriteLine($"Expected transaction count: {numberOfThreads * depositsPerThread}");
        _output.WriteLine($"Actual transaction count: {account.TransactionCount}");

        // These assertions will likely fail due to race conditions
        _output.WriteLine($"Balance difference: ${expectedBalance - account.Balance}");

        // For demonstration, we'll check if there's a race condition rather than exact equality
        if (account.Balance != expectedBalance)
        {
            _output.WriteLine("❌ RACE CONDITION DETECTED: Balance is incorrect!");
        }
        else
        {
            _output.WriteLine("✅ No race condition detected (rare!)");
        }
    }

    [Fact]
    public async Task Concurrent_Withdrawals_Can_Cause_Overdrafts()
    {
        const decimal initialBalance = 1000;
        const int numberOfThreads = 20;
        const decimal withdrawAmount = 100; // Total potential withdrawals: 2000 (more than balance)

        var account = new UnsafeBankAccount(initialBalance);
        var tasks = new List<Task<bool>>();
        var successfulWithdrawals = 0;

        _output.WriteLine($"Starting balance: ${initialBalance}");
        _output.WriteLine($"Starting {numberOfThreads} threads, each trying to withdraw ${withdrawAmount}");

        // Create multiple tasks that withdraw concurrently
        for (int i = 0; i < numberOfThreads; i++)
        {
            int threadId = i;
            tasks.Add(Task.Run(() =>
            {
                bool success = account.Withdraw(withdrawAmount);
                if (success)
                {
                    Interlocked.Increment(ref successfulWithdrawals);
                    _output.WriteLine($"Thread {threadId} successfully withdrew ${withdrawAmount}");
                }
                else
                {
                    _output.WriteLine($"Thread {threadId} failed to withdraw ${withdrawAmount}");
                }

                return success;
            }));
        }

        // Wait for all tasks to complete
        var results = await Task.WhenAll(tasks);

        _output.WriteLine($"Final balance: ${account.Balance}");
        _output.WriteLine($"Successful withdrawals: {successfulWithdrawals}");
        _output.WriteLine(
            $"Expected minimum balance: ${Math.Max(0, initialBalance - (successfulWithdrawals * withdrawAmount))}");

        // Check for overdraft (negative balance due to race condition)
        if (account.Balance < 0)
        {
            _output.WriteLine($"❌ OVERDRAFT DETECTED: Account balance is negative! (${account.Balance})");
        }
        else if (account.Balance != initialBalance - (successfulWithdrawals * withdrawAmount))
        {
            _output.WriteLine("❌ RACE CONDITION: Balance calculation is incorrect!");
        }
        else
        {
            _output.WriteLine("✅ No race condition detected");
        }
    }

    [Fact]
    public async Task Concurrent_Mixed_Operations_Show_Inconsistencies()
    {
        const decimal initialBalance = 1000;
        const int operationsPerType = 50;

        var account = new UnsafeBankAccount(initialBalance);
        var tasks = new List<Task>();

        var depositTasks = 0;
        var withdrawTasks = 0;

        _output.WriteLine($"Starting balance: ${initialBalance}");

        // Concurrent deposits
        for (int i = 0; i < operationsPerType; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                account.Deposit(10);
                Interlocked.Increment(ref depositTasks);
            }));
        }

        // Concurrent withdrawals
        for (int i = 0; i < operationsPerType; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                account.Withdraw(5);
                Interlocked.Increment(ref withdrawTasks);
            }));
        }

        await Task.WhenAll(tasks);

        decimal expectedBalance = initialBalance + (operationsPerType * 10) - (operationsPerType * 5);
        int expectedTransactions = operationsPerType * 2; // deposits + successful withdrawals (approximately)

        _output.WriteLine($"Expected balance: ${expectedBalance}");
        _output.WriteLine($"Actual balance: ${account.Balance}");
        _output.WriteLine($"Expected transactions: ~{expectedTransactions}");
        _output.WriteLine($"Actual transactions: {account.TransactionCount}");

        if (Math.Abs(account.Balance - expectedBalance) > 0.01m)
        {
            _output.WriteLine("❌ RACE CONDITION: Balance is inconsistent!");
            _output.WriteLine($"Difference: ${Math.Abs(account.Balance - expectedBalance)}");
        }
    }

    [Fact]
    public async Task Concurrent_Transfers_Show_Money_Creation_Bug()
    {
        const decimal initialBalance = 1000;
        const int numberOfTransfers = 50;
        const decimal transferAmount = 10;

        var account1 = new UnsafeBankAccount(initialBalance);
        var account2 = new UnsafeBankAccount(initialBalance);
        var tasks = new List<Task>();

        decimal totalInitialBalance = account1.Balance + account2.Balance;
        _output.WriteLine($"Initial total balance: ${totalInitialBalance}");
        _output.WriteLine($"Account 1: ${account1.Balance}, Account 2: ${account2.Balance}");

        // Concurrent transfers in both directions
        for (int i = 0; i < numberOfTransfers; i++)
        {
            // Transfer from account1 to account2
            tasks.Add(Task.Run(() => account1.Transfer(account2, transferAmount)));

            // Transfer from account2 to account1
            tasks.Add(Task.Run(() => account2.Transfer(account1, transferAmount)));
        }

        await Task.WhenAll(tasks);

        decimal finalTotalBalance = account1.Balance + account2.Balance;

        _output.WriteLine($"Final total balance: ${finalTotalBalance}");
        _output.WriteLine($"Account 1: ${account1.Balance}, Account 2: ${account2.Balance}");
        _output.WriteLine($"Difference: ${finalTotalBalance - totalInitialBalance}");

        if (Math.Abs(finalTotalBalance - totalInitialBalance) > 0.01m)
        {
            if (finalTotalBalance > totalInitialBalance)
            {
                _output.WriteLine("❌ MONEY CREATION BUG: Total balance increased!");
            }
            else
            {
                _output.WriteLine("❌ MONEY DESTRUCTION BUG: Total balance decreased!");
            }
        }
        else
        {
            _output.WriteLine("✅ No money creation/destruction detected");
        }
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public async Task Demonstrate_Race_Condition_Frequency(int numberOfRuns)
    {
        int raceConditionCount = 0;
        const int threadsPerRun = 10;
        const decimal depositAmount = 1;
        const decimal expectedBalance = threadsPerRun * depositAmount;

        _output.WriteLine($"Running {numberOfRuns} tests with {threadsPerRun} threads each");

        for (int run = 0; run < numberOfRuns; run++)
        {
            var account = new UnsafeBankAccount(0);
            var tasks = new List<Task>();

            for (int i = 0; i < threadsPerRun; i++)
            {
                tasks.Add(Task.Run(() => account.Deposit(depositAmount)));
            }

            await Task.WhenAll(tasks);

            if (account.Balance != expectedBalance)
            {
                raceConditionCount++;
                _output.WriteLine(
                    $"Run {run + 1}: Race condition! Expected: ${expectedBalance}, Actual: ${account.Balance}");
            }
        }

        double raceConditionPercentage = (double)raceConditionCount / numberOfRuns * 100;
        _output.WriteLine(
            $"\nRace conditions detected: {raceConditionCount}/{numberOfRuns} ({raceConditionPercentage:F1}%)");

        if (raceConditionCount > 0)
        {
            _output.WriteLine("❌ Race conditions are occurring!");
        }
        else
        {
            _output.WriteLine("⚠️  No race conditions detected - try running more tests or increasing thread count");
        }
    }
}