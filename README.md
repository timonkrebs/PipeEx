# PipeEx
PipeEx is a simple yet powerful library that provides extension methods for creating a fluent, pipe-like syntax in C#.  It allows you to chain function calls together in a readable and expressive way, improving code clarity and maintainability.

## What is it?

PipeEx introduces the `I` (Infer) extension method, which acts as a "pipe" operator.  This allows you to pass the result of one function directly as input to the next, creating a chain of operations.  This is particularly useful when dealing with asynchronous operations or complex data transformations.


## Usage

The core of PipeEx is the `I` extension method.
```cs

await funcX().I(funcY)
             .I(x => x + 2)
```

## Features
Fluent Syntax: Enables a clean and readable way to chain function calls.
Asynchronous Support: Works seamlessly with both synchronous and asynchronous operations (Task<T>).
Simplified Code: Reduces nesting and improves code maintainability.
Lightweight: A small and focused library with minimal dependencies.
## Contributing
Contributions are welcome!  Feel free to submit pull requests or open issues.
