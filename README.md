# Monadic Response Handler

Minimal library that allows a more declarative programming style utilizing *Pattern Matching* for handling function results based on two possible states: `Ok` and `Err`. Inspired by Rust [std::result](https://doc.rust-lang.org/std/result/).

To apply other concepts of functional programming to C#, check out [language-ext](https://github.com/louthy/language-ext), a far more complete library for that purpose.

## Getting started

The namespace `MonadicResponseHandler` contains the `Resolved` class that handles two structs, `Ok` and `Err`, each one representing a possible state that a `Resolved` could have. Mandatorily, it must be provided a callback function for both responses.

```C#
Resolved<string> resolved = Resolved.Ok("Message in a bottle");

resolved
  .Match(
      Ok: (s) => { Console.WriteLine(s); },
      Err: (err) => { Console.WriteLine("S.O.S."); }
  );
```

The main concept regarding using an object to wrap the function result is to manage not only the expected value but the error case as well. This library gives large flexibility on the value that will be passed through the callback function, whatever it may be a primitive value, a list, an object or even **nothing at all**!

## How to use it

| Name                        | Description                                                              |
|-----------------------------|--------------------------------------------------------------------------|
| `Resolved`                  | Object that encapsulate an `Ok` or `Err` struct                          |
| `Resolved<OkType>`          | Object that encapsulate an `Ok<OkType>` or `Err` struct                  |
| `Resolved<OkType, ErrType>` | Object that encapsulate an `Ok<OkType>` or `Err<ErrType>` struct         |
| `Ok`                        | struct that represents an `Ok` state. It does not hold a value.          |
| `Ok<T>`                     | `Ok` struct that holds a value of type `T`                               |
| `Err`                       | `Err` (error) struct that holds a value of type `IEnumerable<Exception>` |
| `Err<T>`                    | `Err` (error) struct that holds a value of type `T`                      |

`Resolved` contains the `Match` function that calls the Ok function (first parameter) or Err function (second parameter) depending on which value is stored. `Match` is overloaded, so it can return a `Resolved` type for instance, a value of type `T` by using `Match<T>` or it can execute an `Action` that does not return a value.

For convenience, a `Behavior` can be provided in the `Err` parameter to either just return the stored `Err` value (`Behavior.Forward`) or to throw an exception (`Behavior.ThrowEx`).

It is also possible to unwrap a `Resolved` using the function `Unwrap()` to force the stored value to be retrieved. However, it is **not** recommended due to trying to unwrap an Err value will throw an `Exception`.

## Examples

```C#
public static void Main(string[] args)
{
    decimal money = 20;

    var resolved = BuyTicket(money);

    // string Match<string>(Func<Ticket, string> Ok, Func<IEnumerable<Exception>, string> Err)
    var r = resolved.Match(
        Ok: (ticket) => $"Ticket #{ticket.Number}",
        Err: (err) => $"No ticket: {err.First().Message}"
    );

    Console.WriteLine(r);
}

public static Resolved<Ticket> BuyTicket(decimal money)
{
    if (money >= 10)
    {
        return Resolved.Ok(new Ticket());
    }
    else
    {
        var err = new List<Exception> { new Exception("Not enought money") } as IEnumerable<Exception>;
        return Resolved.Err(err);
    }
}
```

```C#
public static void Main(string[] args)
{
    var customer = new Customer();
    var resolved = AddCustomer(customer);

    // void Match(Action Ok, Action<IEnumerable<Exception>> Err)
    resolved.Match(
        Ok: () => { Console.WriteLine("Customer added!"); },
        Err: (err) => { Console.WriteLine("Unexpected error: {0}", err.First().Message); }
    );
}

public static Resolved AddCustomer(Customer customer)
{
    try
    {
        customerRepository.Add(customer); // Add customer to database
        return Resolved.Ok();
    }
    catch (Exception e)
    {
        var err = new List<Exception> { e } as IEnumerable<Exception>;
        return Resolved.Err(err);
    }
}
```

* [Chained Resolved with Behavior.Forward on Err](https://github.com/gmurayama/monadic-response-handler/blob/35be1c6ffd1f07879a4ad2727c0cf9636d0ef345/tests/MonadicResponseHandler.Tests/ResolvedErrTests.cs#L83-L108)
* [Unwrap Resolved with Ok value](https://github.com/gmurayama/monadic-response-handler/blob/35be1c6ffd1f07879a4ad2727c0cf9636d0ef345/tests/MonadicResponseHandler.Tests/ResolvedOkTests.cs#L80-L87)
