using NUnit.Framework;

namespace MonadicResponseHandler.Tests
{
    [TestFixture]
    public class ResolvedOkTests
    {
        [Test]
        public void ResolvedOk_IsOkTrue()
        {
            Resolved resolved = Resolved.Ok();
            Assert.IsTrue(resolved.IsOk);
        }

        [Test]
        public void ResolvedOk_IsErrFalse()
        {
            Resolved resolved = Resolved.Ok();
            Assert.IsFalse(resolved.IsErr);
        }

        [Test]
        public void Resolved_ReturnsOkWithNoValue()
        {
            Resolved resolved = Resolved.Ok();

            var r =  resolved.Match<Resolved>(
                Ok: () => Resolved.Ok(),
                Err: (err) => Resolved.Err(err)
            );

            Assert.IsTrue(r.IsOk);
            Assert.AreEqual(r.Value.GetType(), typeof(Ok));
        }

        [Test]
        public void ResolvedOkTypeErrType_ReturnsOkWithBoolValue()
        {
            Resolved<int, string> resolved = Resolved.Ok(1);

            var result = resolved.Match(
                Ok: (n) => true,
                Err: (e) => false
            );

            Assert.IsTrue(result);
        }

        [Test]
        public void NestedResolved_ReturnsResolvedOkWithNoValue()
        {
            Resolved<Resolved<int>> resolved = Resolved.Ok<Resolved<int>>(Resolved.Ok(1));

            var result = resolved.Match<Resolved<int>>(
                Ok: (r) =>
                {
                    var number = r.Match(
                        Ok: (n) => 1,
                        Err: (err) => 0
                    );

                    return Resolved.Ok(number);
                },
                Err: (err) => Resolved.Err(err)
            );
        }

        [Test]
        public void SameTypeChainedResolvedOk_ReturnsLastNumberInTheChain()
        {
            Resolved<int> resolved = Resolved.Ok(0);

            var r = resolved
                .Match<Resolved<int>>(
                    Ok: (n) => Resolved.Ok(1),
                    Err: (err) => Resolved.Err(err)
                )
                .Match<Resolved<int>>(
                    Ok: (n) => Resolved.Ok(2),
                    Err: (err) => Resolved.Err(err)
                );

            Assert.IsTrue(r.IsOk);

            r.Match(
                Ok: (n) => { Assert.AreEqual(2, n); },
                Err: (err) => { Assert.Fail("Unexpected error occurred"); }
            );
        }

        [Test]
        public void DifferentTypeChainedResolvedOk_ResolvedValueIsTrue()
        {
            Resolved resolved = Resolved.Ok();

            var r = resolved
                .Match<Resolved<int>>(
                    Ok: () => Resolved.Ok(1),
                    Err: (err) => Resolved.Err(err)
                )
                .Match<Resolved>(
                    Ok: (n) => Resolved.Ok(),
                    Err: (err) => Resolved.Err(err)
                )
                .Match<Resolved<bool>>(
                    Ok: () => Resolved.Ok(true),
                    Err: (err) => Resolved.Err(err)
                );

            Assert.IsTrue(r.IsOk);

            r.Match(
                Ok: (value) => { Assert.IsTrue(value); },
                Err: (err) => { Assert.Fail("Unexpected error occurred"); }
            );
        }
    }
}
