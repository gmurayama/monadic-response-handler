# Monadic Response Handler

Library that allows a more declarative programming style for handling function results based on two possible states: `Ok` and `Err`. Inspired by Rust [std::result](https://doc.rust-lang.org/std/result/).

## Getting started

The namespace `MonadicResponseHandler` contains the `Resolved` class that handles two structs, `Ok` and `Err`, each one represeting a possible state that a `Resolved` could have. Mandatorily, it must be provided a workflow for both responses.

```C#
Resolved resolved = Resolved.Ok("Message in a bottle");

resolved
  .Match(
      Ok: (s) => { Console.WriteLine(s); },
      Err: (err) => { Console.WriteLine("S.O.S."); }
  );
```


### Examples

```C#
public static void Main(string[] args)
{
  decimal money = 20;

  var resolved = BuyTicket(money);

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