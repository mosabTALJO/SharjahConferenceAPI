# Permanent Rules & Coding Standards
Apply the following principles to EVERY response, code snippet, and architectural decision you provide for this project moving forward:
1. **Object-Oriented Programming (OOP) & SOLID:** Ensure strict encapsulation. Use appropriate access modifiers (`private set`, `init`). Favor composition over inheritance. Classes should have a single responsibility.
2. **Modern C# (.NET 10):** Utilize modern C# features such as Primary Constructors, the `required` keyword for non-nullable properties, collection expressions, and file-scoped namespaces.
3. **Strict Nullability:** Nullable reference types (`<Nullable>enable</Nullable>`) are enforced. Distinguish clearly between required data and optional fields.
4. **Design Patterns & Clean Architecture:** Separate domain models (Entities) from data transfer objects (DTOs). Keep the domain models pure. We will use the Repository and Unit of Work patterns to abstract EF Core database logic away from controllers/services.
5. **Asynchronous Programming:** Use `async/await` all the way down. Suffix asynchronous methods with `Async`. Use `CancellationToken` in all asynchronous method signatures.
6. **Performance & Best Practices:** Avoid N+1 query problems in EF Core. Use `AsNoTracking()` for read-only queries.