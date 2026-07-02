# PipeEx

[![.NET](https://github.com/timonkrebs/PipeEx/actions/workflows/dotnet.yml/badge.svg)](https://github.com/timonkrebs/PipeEx/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/dt/PipeEx.svg)](https://www.nuget.org/packages/PipeEx) 
[![NuGet](https://img.shields.io/nuget/vpre/PipeEx.svg)](https://www.nuget.org/packages/PipeEx)

PipeEx is a lightweight C# library that enables fluent, pipe-like function chaining. By leveraging the `I` (Infer) extension method, you can pass the result of one function directly into the next, resulting in cleaner and more maintainable code.

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
  - [Synchronous Operations](#synchronous-operations)
  - [Asynchronous Operations](#asynchronous-operations)
  - [Conditional Expressions](#conditional-expressions)
  - [Result Chaining](#result-chaining)
  - [Structured Concurrency](#structured-concurrency)
- [Planned Features](#planned-features)
- [Contributing](#contributing)

## Features

- **Fluent Syntax:** Create readable chains of function calls.
- **Asynchronous Support:** Seamlessly chains both synchronous and asynchronous operations (`Task<T>`).
- **Conditional Expressions:** Express if / else if / else logic, conditional side effects and guards as fluent expressions, synchronously or asynchronously.
- **Result Chaining:** Railway oriented chaining of methods that can succeed or fail, without exceptions for control flow.
- **Structured Concurrency:** Run asynchronous steps concurrently with `Let` / `Await` and flow a cancellation token along the pipe — and into each stage's job, so work already in flight can be cancelled, not just observed between stages.
- **Simplified Code:** Reduces nesting and complexity, making your code easier to maintain.
- **Lightweight:** No dependencies without compromising on expressiveness.

## Installation

Install PipeEx via NuGet:

```bash
dotnet add package PipeEx
```

For Structured Concurrency support, install:

```bash
dotnet add package PipeEx.StructuredConcurrency
```

For Result Chaining support, install:

```bash
dotnet add package PipeEx.ResultChaining
```

For Conditional Expressions support, install:

```bash
dotnet add package PipeEx.ConditionalExpressions
```

## Usage

### Synchronous Operations

The core feature of PipeEx is the `I` extension method. It lets you pipe the output of one function as the input to the next:

```csharp
public int Calc(int x) => x.I(FuncY)
                           .I(x => x + 2);
```

You can also automatically destructure tuples:

```csharp
public int Calc(int x) => x.I(x => (x + 2, x + 4))
                           .I((x, y) => x + y);
```

### Asynchronous Operations

`PipeEx.StructuredConcurrency` extends the pipe to asynchronous operations (the base `PipeEx` package
is synchronous only). The library automatically handles awaiting tasks:

```csharp
// awaiting is handled automatically (requires PipeEx.StructuredConcurrency)
public Task<int> Calc(int x) => x.I(FuncXAsync)
                                 .I(x => x + 2)
                                 .I(FuncYAsync)
                                 .I(FuncY);
```

### Conditional Expressions

`PipeEx.ConditionalExpressions` turns conditional logic into fluent expressions.

`If()` with both branches produces a value directly. With a single branch it starts a lazily evaluated if / else if / else chain, which is extended with any number of `ElseIf()` branches and terminated with `Else()`, which produces the final value. The first matching branch wins, later predicates and transformations are not evaluated. Branches can be funcs, constant values or asynchronous:

```csharp
public int Calc(int x) => x.If(x => x <= 2, x => x + 2, x => x - 2);
public string Calc(int x) => x.If(x => x <= 2, "Woohoo", "Noooo");              // constant values
public Task<int> Calc(int x) => x.If(x => x <= 2, FuncXAsync).Else(x => x);     // async branch

public string Grade(int score) =>
    score.If(s => s >= 90, "A")
         .ElseIf(s => s >= 80, "B")
         .ElseIf(s => s >= 70, _ => "C")
         .Else("F");
```

`When()` conditionally executes a side effect and returns the source for further chaining. `Guard()` does the same but remembers whether the condition matched, so a chain of guards can be closed with an `Else()` that only runs when the previous condition was skipped:

```csharp
order.When(o => o.IsRush, o => logger.LogRush(o))
     .Guard(o => o.IsValid, o => Submit(o))
     .Else(o => Reject(o));
```

All of these compose with asynchronous pipes: every extension method also accepts a `Task<T>` source and asynchronous transformations or actions (`Func<T, Task>` / `Func<T, Task<TResult>>`), so conditionals can sit in the middle of an async chain:

```csharp
public Task<string> Categorize(int x) =>
    LoadAsync(x).If(v => v.IsCached, v => v, EnrichAsync)
                .If(v => v.Score >= 90, FetchPremiumLabelAsync)
                .ElseIf(v => v.Score >= 50, "standard")
                .Else("basic");
```

### Result Chaining

`PipeEx.ResultChaining` brings fluent, railway oriented method chaining (inspired by [OneOf.Chaining](https://github.com/andrewjpoole/OneOf.Chaining)) without any dependencies. Methods that can succeed or fail simply return a `Result<TSuccess, TFailure>` (or a `Task` of it) and can then be chained. A failure short-circuits the rest of the chain.

Turn this:

```csharp
public async Task<Result<WeatherReport, Failure>> Handle(string region, DateTime date)
{
    var isValidRequest = await regionValidator.Validate(region);
    if (!isValidRequest)
        return new UnsupportedRegionFailure();

    var dateCheckPassed = await dateChecker.CheckDate(date);
    if (!dateCheckPassed)
        return new InvalidRequestFailure();

    var report = WeatherReport.Create(region, date);
    var cacheResult = await cache.TryPopulate(report);
    if (cacheResult.PopulatedFromCache)
        return cacheResult;

    return await weatherForecastGenerator.Generate(cacheResult);
}
```

into this:

```csharp
public async Task<Result<WeatherReport, Failure>> Handle(string region, DateTime date) =>
    await WeatherReport.Create(region, date)
        .ToSuccess<WeatherReport, Failure>()
        .Then(regionValidator.ValidateRegion)
        .Then(dateChecker.CheckDate)
        .Then(cache.TryPopulate)
        .IfThen(report => report.PopulatedFromCache is false,
            weatherForecastGenerator.Generate);
```

The package includes:

- `Then()` which enables fluent chaining of any method that returns a `Result<TSuccess, TFailure>` or a `Task<Result<TSuccess, TFailure>>`. Synchronous and asynchronous jobs can be mixed freely.
- An overload of `Then()` which takes an `onFailure` func, useful for tidying up previous work. It can also mutate the failure result (but not turn it into a success).
- `IfThen()` which takes a condition func; the next job is only invoked when it returns true.
- `ThenForEach()` which invokes a job once per item produced from the current success value, breaking on the first failure.
- `ToResult()` which converts the success value at the end of a chain into a new type.
- `ThenWaitForAll()` and `ThenWaitForFirst()` which execute jobs in parallel, with an optional result merging strategy.
- Versions of all extension methods with cancellation support (`CancellationToken` is checked between links and passed into each job; `ThenWaitForFirst` signals cancellation to the remaining jobs once the first one completes).

```csharp
var result = await report.ToSuccess<WeatherReport, Failure>()
    .Then(ValidateRegion)
    .ThenWaitForAll(FetchTemperature, FetchWind, FetchHumidity)
    .Then(PersistReport, onFailure: (report, failure) => Cleanup(report, failure))
    .ToResult(report => new WeatherReportResponse(report));
```

### Structured Concurrency

`PipeEx.StructuredConcurrency` extends the `I` pipe so asynchronous steps flow through a
`StructuredTask<T>` that carries a `CancellationTokenSource` along the chain. Awaiting the chain
works just like awaiting a `Task<T>`:

```csharp
// awaiting is handled automatically, just like the core async pipe
public Task<int> Calc(int x) => x.I(FuncXAsync)
                                 .I(x => x + 2)
                                 .I(FuncYAsync);
```

Keep the `StructuredTask<T>` instead of awaiting it immediately and you can cancel the whole
pipeline from the outside through its `CancellationTokenSource`:

```csharp
StructuredTask<int> task = x.I(FuncXAsync).I(FuncYAsync);
task.CancellationTokenSource.Cancel();   // observed between pipeline stages
```

By default the carried token is only *observed between stages*: a stage that is already running is
awaited to completion before the next checkpoint throws. To let a running stage stop the moment
cancellation is requested, take a `CancellationToken` as the stage's last parameter and pass it into the
asynchronous work — PipeEx flows the pipe's own token in for you, so cancelling the chain interrupts
whichever stage is in flight:

```csharp
StructuredTask<int> task = x.I((v, ct) => FetchAsync(v, ct))     // ct is the pipe's token
                            .I((v, ct) => ProcessAsync(v, ct));
task.CancellationTokenSource.Cancel();   // interrupts the running stage, not just between stages
```

The same `(…, CancellationToken)` shape works on the tuple-destructuring overloads and on `Let`, so a
concurrently running `Let` observes the token and is interrupted too:

```csharp
x.Let((v, ct) => LoadAAsync(v, ct))      // started immediately, observes the token while running
 .Let((v, ct) => LoadBAsync(v, ct))
 .Await((source, a, b) => source + a + b);
```

#### Concurrent async-let with `Let` / `Await`

`Let` starts an additional asynchronous computation that runs concurrently with the source, and
`Await` joins them back together once you need the results (inspired by Swift's `async let`):

```csharp
public Task<int> Combine(int x) =>
    x.Let(() => LoadAAsync(x))    // started immediately, runs concurrently
     .Let(() => LoadBAsync(x))    // also started immediately
     .Await((source, a, b) => source + a + b);
```

`Await` awaits the source and every deferred result; the first failure propagates, and any deferred
result the projection never reached is still observed in the background (so nothing surfaces as an
unobserved task exception). The projection is free to ignore any argument it does not need:

```csharp
x.Let(() => LoadAAsync(x))
 .Let(() => LoadBAsync(x))
 .Await((source, _, b) => source + b);   // LoadAAsync's result is awaited but not used here
```

> This package is pre-release. A chain holds a `CancellationTokenSource`; dispose the final
> `StructuredTask<T>` (for example with `using`) when you need deterministic cleanup.

## Planned Features

- **Resource Management:** Enhanced handling for resources that are not thread-safe (like EF Core DbContext or WPF UI updates).

## Contributing

Contributions are welcome! If you would like to submit improvements, please fork the repository and open a pull request. For major changes, please open an issue first to discuss what you would like to change.
