# Monadic Response Handler

Library that allows a more declarative programming style for handling function results based on two possible states: `Ok` and `Err`. Inspired by Rust [std::result](https://doc.rust-lang.org/std/result/).

## Getting started

The namespace `MonadicResponseHandler` contains the `Resolved` class that handles two structs, `Ok` and `Err`, each one represeting a possible state that a `Resolved` could have. Mandatorily, it must be provided a callback function for both responses.

```C#
Resolved resolved = Resolved.Ok("Message in a bottle");

resolved
  .Match(
      Ok: (s) => { Console.WriteLine(s); },
      Err: (err) => { Console.WriteLine("S.O.S."); }
  );
```

The main concept regarding using an object to wrap the function result is to manage not only the expected value but the error case as well. This library gives large flexibility on the value that will be passed through the callback function, whatever it may be an primitive value, a list, an object or even **nothing at all**!

## How to use it

| Name                     | Description                                                             |
|---------------------------|-------------------------------------------------------------------------|
| `Resolved`                  | Object that encapsulate an `Ok` or `Err` struct                         |
| `Resolved<OkType>`          | Object that encapsulate an `Ok<OkType>` or `Err` struct                 |
| `Resolved<OkType, ErrType>` | Object that encapsulate an `Ok<OkType>` or `Err<ErrType>` struct        |
| `Ok`                        | struct that represents an `Ok` state. It does not hold a value.        |
| `Ok<T>`                     | `Ok` struct that holds a value of type `T`                               |
| `Err`                       | `Err` (error) struct that holds a value of type `IEnumerable<Exception>` |
| `Err<T>`                    | `Err` (error) struct that holds a value of type `T`                      |

`Resolved` contains the `Match` function that calls the Ok function (first parameter) or Err function (second parameter) depending on which value is stored. `Match` is overloaded, so it can returns a value of type `T` by using `Match<T>`, a `Resolved` type or it can execute an `Action` that does not returns a value.

## Examples

```C#
public static void Main(string[] args)
{
    decimal money = 20;

    var resolved = BuyTicket(money);

    // Match(Action Ok<Ticket>, Action Err)
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
        var err = new List<Exception> { new Exception("Not enought money") };
        return Resolved.Err(err);
    }
}
```