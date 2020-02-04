using NUnit.Framework;

namespace MonadicResponseHandler.Tests
{
    [TestFixture]
    public class ResolvedOkTests
    {
        [Test]
        public void ResolvedWithEmptyOk_IsOkEqualsTrue()
        {
            Resolved resolved = Resolved.Ok();
            Assert.IsTrue(resolved.IsOk);
        }

        [Test]
        public void ResolvedWithIntegerOk_IsOkEqualsTrue()
        {
            Resolved<int> resolved = Resolved.Ok(15);
            Assert.IsTrue(resolved.IsOk);
        }

        [Test]
        public void ResolvedWithEmptyOk_IsErrEqualsFalse()
        {
            Resolved resolved = Resolved.Ok();
            Assert.IsFalse(resolved.IsErr);
        }

        [Test]
        public void ResolvedWithIntegerOk_IsErrEqualsFalse()
        {
            Resolved<int> resolved = Resolved.Ok(15);
            Assert.IsFalse(resolved.IsErr);
        }

        [Test]
        public void ResolvedWithEmptyOk_MatchOkAndReturnsEmptyOk()
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
        public void ResolvedWithEmptyOk_MatchOkAndReturnsOkWithIntegerValue()
        {
            Resolved resolved = Resolved.Ok();

            var r = resolved.Match<Resolved<int>>(
                Ok: () => Resolved.Ok(10),
                Err: (err) => Resolved.Err(err)
            );

            Assert.IsTrue(r.IsOk);
            Assert.AreEqual(r.Value.GetType(), typeof(Ok<int>));
            Assert.AreEqual(10, ((Ok<int>)r.Value).Value);
        }

        [Test]
        public void ResolvedWithIntegerOk_ReturnsBoolValueTrue()
        {
            Resolved<int, string> resolved = Resolved.Ok(1);

            var result = resolved.Match(
                Ok: (n) => true,
                Err: (e) => false
            );

            Assert.IsTrue(result);
        }

        [Test]
        public void ResolvedWithIntegerOk_UnwrapValue()
        {
            Resolved<int> resolved = Resolved.Ok(5);
            var value = resolved.Unwrap();

            Assert.AreEqual(5, value);
        }

        [Test]
        public void ResolvedWithOkTypeAndErrType_UnwrapValue()
        {
            Resolved<bool, string> resolved = Resolved.Ok(true);
            var value = resolved.Unwrap();

            Assert.IsTrue(value);
        }

        [Test]
        public void NestedResolved_ReturnsResolvedOkWithIntValue()
        {
            Resolved<int> innerResolved = Resolved.Ok(1);
            Resolved<Resolved<int>> resolved = Resolved.Ok(innerResolved);

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

            Assert.IsTrue(result.IsOk);

            result.Match(
                Ok: (n) => Assert.AreEqual(1, n),
                Err: (err) => Assert.Fail("Unexpected error occurred")
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
                )
                .Match<Resolved<int>>(
                    Ok: (n) => Resolved.Ok(3),
                    Err: (err) => Resolved.Err(err)
                );

            Assert.IsTrue(r.IsOk);

            r.Match(
                Ok: (n) => { Assert.AreEqual(3, n); },
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
