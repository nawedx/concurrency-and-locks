# Concurrency and Locks: Race Conditions Demonstration

A C# .NET educational project demonstrating concurrency issues, race conditions, and the critical importance of thread synchronization in multi-threaded applications.

## Overview

This project provides a hands-on demonstration of how race conditions occur in concurrent programming through an intentionally unsafe bank account implementation. It showcases real-world scenarios where thread-unsafe code leads to data corruption, lost updates, and inconsistent state.

## Key Concepts Demonstrated

### Race Conditions
- **Lost Updates**: Multiple threads overwriting each other's changes
- **Read-Modify-Write Problems**: Non-atomic operations leading to data loss
- **Money Creation/Destruction Bugs**: Invalid states caused by concurrent transfers
- **Overdraft Vulnerabilities**: Balance checks bypassed by timing issues

### Test Coverage

The project includes comprehensive test suites demonstrating:

1. **Lost Update Detection** (`DetailedRaceConditionTests.cs`)
   - Detailed tracing of race condition mechanisms
   - Step-by-step analysis of how updates get lost
   - Statistical analysis of data corruption

2. **Concurrent Operation Failures** (`UnsafeBankAccountTests.cs`)
   - Concurrent deposits causing incorrect balances
   - Concurrent withdrawals leading to overdrafts
   - Mixed operations showing data inconsistencies
   - Bi-directional transfers creating/destroying money
   - Statistical frequency analysis of race conditions

## Project Structure

```
ConcurrencyAndLocks/
├── ConcurrencyAndLocks.Core/
│   └── UnsafeBankAccount.cs          # Intentionally unsafe implementation
├── ConcurrencyAndLocks.Tests/
│   ├── DetailedRaceConditionTests.cs  # Detailed race condition analysis
│   └── UnsafeBankAccountTests.cs      # Comprehensive test scenarios
└── ConcurrencyAndLocks.sln
```

## Technical Highlights

- **Thread Synchronization**: Demonstrates why locks/synchronization are essential
- **Concurrency Testing**: Uses `Task.Run()` and `Task.WhenAll()` for parallel execution
- **Real-World Scenarios**: Bank account operations mirror actual production use cases
- **Test Output Logging**: Detailed test output showing exact failure mechanisms
- **Statistical Analysis**: Measures frequency and impact of race conditions

## Running the Tests

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter UnsafeBankAccountTests

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

## Sample Test Results

The tests intentionally demonstrate failures:
- ❌ **Lost updates**: Expected $2000, Actual $1850
- ❌ **Overdrafts**: Balance drops below $0
- ❌ **Money creation**: Total balance increases without deposits

## Learning Outcomes

After exploring this project, you will understand:

1. How race conditions occur at a mechanical level
2. Why atomic operations are crucial in concurrent systems
3. The difference between thread-safe and thread-unsafe code
4. Real-world implications of concurrency bugs in financial systems
5. Testing strategies for detecting concurrency issues

## Technologies Used

- **.NET 9.0**: Modern C# features and async/await patterns
- **xUnit**: Testing framework with advanced output capabilities
- **Task Parallel Library (TPL)**: Concurrent task execution
- **Interlocked Operations**: For safe counter increments in tests

## Future Extensions

This project is intentionally limited to demonstrating problems. Potential additions:

- Implementation of thread-safe versions using `lock` statements
- Comparison with `Monitor`, `Mutex`, and `Semaphore`
- Performance benchmarks: thread-unsafe vs thread-safe
- Advanced patterns: ReaderWriterLock, concurrent collections
- Distributed locking scenarios

## Educational Value

This project is ideal for:
- Understanding concurrency fundamentals
- Interview preparation (concurrency/threading questions)
- Teaching thread safety concepts
- Demonstrating testing strategies for concurrent code

## Resume Value

**Yes, this project adds value to your resume** because it demonstrates:

✅ **Deep Understanding**: Shows you understand concurrency at a fundamental level, not just surface knowledge

✅ **Testing Expertise**: Comprehensive test coverage including edge cases and statistical analysis

✅ **Real-World Application**: Uses a relatable domain (banking) that mirrors production systems

✅ **Quality Documentation**: Clear README demonstrating communication skills

✅ **Best Practices**: Proper project structure, naming conventions, and .NET standards

### Recommended Resume Description

> **Concurrency & Thread Safety Research Project**
> Developed comprehensive demonstration of race conditions and concurrency issues in multi-threaded applications using C#/.NET. Implemented detailed test suites showing lost updates, data corruption, and synchronization failures in banking scenarios. Showcases deep understanding of thread safety, atomic operations, and concurrent programming challenges.

## Author

Developed as a learning exercise to master concurrent programming concepts and demonstrate advanced testing capabilities.

## License

Educational use only.
