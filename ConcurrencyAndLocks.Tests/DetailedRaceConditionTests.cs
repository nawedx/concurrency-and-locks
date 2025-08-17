using System.Collections.Concurrent;
using ConcurrencyAndLocks.Core;
using Xunit.Abstractions;

namespace ConcurrencyAndLocks.Tests;

public class DetailedRaceConditionTests
{
    private readonly ITestOutputHelper _output;

    public DetailedRaceConditionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Demonstrate_Lost_Updates_In_Detail()
    {
        const decimal initialBalance = 1000;
        const int numberOfDeposits = 100;
        const decimal depositAmount = 10;
        const decimal expectedFinalBalance = initialBalance + (numberOfDeposits * depositAmount);

        var account = new UnsafeBankAccount(initialBalance);
        var completedDeposits = new ConcurrentBag<(int ThreadId, decimal Amount, DateTime Timestamp)>();

        _output.WriteLine($"Initial balance: ${initialBalance}");
        _output.WriteLine($"Planning {numberOfDeposits} deposits of ${depositAmount} each");
        _output.WriteLine($"Expected final balance: ${expectedFinalBalance}");

        // Track each deposit completion
        var tasks = new List<Task>();
        for (int i = 0; i < numberOfDeposits; i++)
        {
            int threadId = i;
            tasks.Add(Task.Run(() =>
            {
                account.Deposit(depositAmount);
                // Record that this deposit "completed successfully"
                completedDeposits.Add((threadId, depositAmount, DateTime.Now));
            }));
        }

        await Task.WhenAll(tasks);

        // Analyze results
        var deposits = completedDeposits.ToList().OrderBy(d => d.Timestamp).ToList();

        _output.WriteLine("=== ANALYSIS ===");
        _output.WriteLine($"Deposits that completed: {deposits.Count}");
        _output.WriteLine($"Transaction count in account: {account.TransactionCount}");
        _output.WriteLine($"Expected final balance: ${expectedFinalBalance}");
        _output.WriteLine($"Actual final balance: ${account.Balance}");
        _output.WriteLine($"Money lost due to race conditions: ${expectedFinalBalance - account.Balance}");


        // Show that all deposits "succeeded" from their thread's perspective
        _output.WriteLine("All deposits completed successfully (no exceptions thrown):");
        _output.WriteLine($"✅ {deposits.Count} threads completed their deposit operations");
        _output.WriteLine($"✅ {account.TransactionCount} transaction count increments occurred");


        if (account.Balance < expectedFinalBalance)
        {
            decimal lostMoney = expectedFinalBalance - account.Balance;
            int lostUpdates = (int)(lostMoney / depositAmount);

            _output.WriteLine("❌ LOST UPDATES DETECTED:");
            _output.WriteLine($"   • ${lostMoney} was lost (approximately {lostUpdates} deposit(s))");
            _output.WriteLine($"   • This happened because multiple threads overwrote each other's work");
            _output.WriteLine($"   • No deposits were aborted - they all completed 'successfully'");
        }
    }

    [Fact]
    public async Task Show_Exact_Race_Condition_Mechanism()
    {
        // Create a special account that lets us trace what's happening
        var account = new TrackedUnsafeBankAccount(100);

        _output.WriteLine("Starting detailed trace of race condition...");
        _output.WriteLine($"Initial balance: ${account.Balance}");


        // Create exactly 2 threads to make the race condition clear
        var task1 = Task.Run(async () =>
        {
            _output.WriteLine("[Thread 1] Starting deposit of $50");
            await account.DepositWithTrace(50, "Thread 1", _output);
            _output.WriteLine("[Thread 1] Deposit completed successfully!");
        });

        var task2 = Task.Run(async () =>
        {
            // Small delay to increase chance of race condition
            await Task.Delay(1);
            _output.WriteLine("[Thread 2] Starting deposit of $30");
            await account.DepositWithTrace(30, "Thread 2", _output);
            _output.WriteLine("[Thread 2] Deposit completed successfully!");
        });

        await Task.WhenAll(task1, task2);


        _output.WriteLine("=== FINAL RESULTS ===");
        _output.WriteLine($"Expected balance: $100 + $50 + $30 = $180");
        _output.WriteLine($"Actual balance: ${account.Balance}");
        _output.WriteLine($"Both threads completed without errors: ✅");

        if (account.Balance != 180)
        {
            _output.WriteLine($"Money lost: ${180 - account.Balance}");
            _output.WriteLine("This demonstrates that deposits complete 'successfully' but work is lost!");
        }
    }
}

// Special version of our account that lets us trace exactly what's happening
public class TrackedUnsafeBankAccount
{
    private decimal _balance;
    private int _transactionCount;

    public TrackedUnsafeBankAccount(decimal initialBalance = 0)
    {
        _balance = initialBalance;
        _transactionCount = 0;
    }

    public decimal Balance => _balance;
    public int TransactionCount => _transactionCount;

    public async Task DepositWithTrace(decimal amount, string threadName, ITestOutputHelper output)
    {
        output.WriteLine($"[{threadName}] About to read balance...");

        // Simulate some processing time
        await Task.Delay(5);

        var currentBalance = _balance;
        output.WriteLine($"[{threadName}] Read balance: ${currentBalance}");

        // Simulate more processing time (this is where race conditions happen)
        await Task.Delay(10);

        var newBalance = currentBalance + amount;
        output.WriteLine($"[{threadName}] Calculated new balance: ${currentBalance} + ${amount} = ${newBalance}");

        // More processing time
        await Task.Delay(5);

        _balance = newBalance;
        output.WriteLine($"[{threadName}] Wrote new balance: ${newBalance}");

        _transactionCount++;
        output.WriteLine($"[{threadName}] Incremented transaction count to: {_transactionCount}");
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        Thread.Sleep(1);
        var currentBalance = _balance;
        Thread.Sleep(1);
        _balance = currentBalance + amount;
        _transactionCount++;
    }
}