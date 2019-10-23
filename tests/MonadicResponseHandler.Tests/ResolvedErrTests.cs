using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonadicResponseHandler.Tests
{
    [TestFixture]
    public class ResolvedErrTests
    {
        [Test]
        public void ResolvedErr_IsErrTrue()
        {
            IEnumerable<Exception> errors = new List<Exception>();
            Resolved resolved = Resolved.Err(errors);

            Assert.IsTrue(resolved.IsErr);            
        }

        [Test]
        public void ResolvedErr_IsOkFalse()
        {
            IEnumerable<Exception> err = new List<Exception>();
            Resolved resolved = Resolved.Err(err);
            Assert.IsFalse(resolved.IsOk);
        }

        [Test]
        public void ResolvedErrWithNullValue_ArgumentNullException()
        {
            var resolved = new Resolved(null);

            Assert.Throws(
                typeof(ArgumentNullException),
                () => resolved.Match(
                    Ok: () => true,
                    Err: (err) => true
                ));
        }

        [Test]
        public void SameTypeChainedResolvedErr_ReturnsLastNumberInTheChain()
        {
            IEnumerable<Exception> errors = new List<Exception> { new Exception(), new Exception() };
            Resolved resolved = Resolved.Err(errors);

            var r = resolved
                .Match<Resolved>(
                    Ok: () => Resolved.Ok(),
                    Err: (err) => Resolved.Err(new List<Exception> { new Exception(), new Exception() } as IEnumerable<Exception>)
                )
                .Match<Resolved>(
                    Ok: () => Resolved.Ok(),
                    Err: (err) => Resolved.Err(new List<Exception> { new Exception() } as IEnumerable<Exception>)
                );

            Assert.IsTrue(r.IsErr);

            r.Match(
                Ok: () => { Assert.Fail("Unexpected error occurred"); },
                Err: (err) => { Assert.AreEqual(1, err.Count()); }
            );
        }

        [Test]
        public void DifferentTypeChainedResolvedErr_ResolvedValueIsString()
        {
            IEnumerable<Exception> errors = new List<Exception>();
            Resolved resolved = Resolved.Err(errors);

            var r = resolved
                .Match<Resolved<bool>>(
                    Ok: () => Resolved.Ok(true),
                    Err: (err) => Resolved.Err(err)
                )
                .Match<Resolved<int, string>>(
                    Ok: (b) => Resolved.Ok(1),
                    Err: (err) => Resolved.Err("Erro")
                );

            Assert.IsTrue(r.IsErr);

            r.Match(
                Ok: (n) => { Assert.Fail("Unexpected error occurred"); },
                Err: (err) => 
                {
                    Assert.AreEqual(typeof(string), err.GetType());
                    Assert.AreEqual("Erro", err);
                }
            );
        }
    }
}
