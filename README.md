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
- [Planned Features](#planned-features)
- [Contributing](#contributing)

## Features

- **Fluent Syntax:** Create readable chains of function calls.
- **Asynchronous Support:** Seamlessly chains both synchronous and asynchronous operations (`Task<T>`).
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

PipeEx supports chaining asynchronous operations. The library automatically handles awaiting tasks:

```csharp
// awaiting is handled automatically
public Task<int> Calc(int x) => x.I(FuncXAsync)
                                 .I(x => x + 2)
                                 .I(FuncYAsync)
                                 .I(FuncY);
```

## Planned Features

- **Structured Concurrency:** Declare asynchronous variables that are resolved at a later point.
- **Cancellation:** Support for initializing and propagating cancellation tokens (e.g., StructuredTask<T>).
- **Resource Management:** Enhanced handling for resources that are not thread-safe (like EF Core DbContext or WPF UI updates).

## Contributing

Contributions are welcome! If you would like to submit improvements, please fork the repository and open a pull request. For major changes, please open an issue first to discuss what you would like to change.
