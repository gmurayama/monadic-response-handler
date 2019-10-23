﻿using NUnit.Framework;

namespace MonadicResponseHandler.Tests
{
    [TestFixture]
    public class ResolvedOkTests
    {
        [Test]
        public void SameTypeChainedResolved_ReturnsLastNumberInTheChain()
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
        public void DifferentTypeChainedResolved_ResolvedValueIsTrue()
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