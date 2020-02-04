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
        public void ResolvedWithDefaultErr_IsErrEqualsTrue()
        {
            IEnumerable<Exception> errors = new List<Exception>();
            Resolved resolved = Resolved.Err(errors);

            Assert.IsTrue(resolved.IsErr);            
        }

        [Test]
        public void ResolvedWithIntegerErr_IsErrEqualsTrue()
        {
            Resolved<object, int> resolved = Resolved.Err(10);
            Assert.IsTrue(resolved.IsErr);
        }

        [Test]
        public void ResolvedWithDefaultErr_IsOkEqualsFalse()
        {
            IEnumerable<Exception> err = new List<Exception>();
            Resolved resolved = Resolved.Err(err);
            Assert.IsFalse(resolved.IsOk);
        }

        [Test]
        public void ResolvedWithIntegerErr_IsOkEqualsFalse()
        {
            Resolved<object, int> resolved = Resolved.Err(10);
            Assert.IsFalse(resolved.IsOk);
        }

        [Test]
        public void ErrAsIEnumerable_ReturnsResolvedWithDefaultErr()
        {
            Resolved resolved = Resolved.ErrAsIEnumerable(new Exception());
            Assert.IsTrue(resolved.IsErr);

            resolved.Match(
                Ok: () => Assert.Fail("Unexpected error"),
                Err: (err) => Assert.IsTrue(err.GetType().GetInterfaces().Contains(typeof(IEnumerable<Exception>)))
            );
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
        public void ResolvedWithDefaultErr_UnwrapValueThrowsException()
        {
            Resolved<int> resolved = Resolved.ErrAsIEnumerable(new Exception());
            Assert.Throws(typeof(InvalidOperationException),
                          () => resolved.Unwrap());
        }

        [Test]
        public void ResolvedWithBoolErr_UnwrapValueThrowsException()
        {
            Resolved<int, bool> resolved = Resolved.Err(true);
            Assert.Throws(typeof(InvalidOperationException),
                          () => resolved.Unwrap());
        }

        [Test]
        public void SameTypeChainedResolvedErr_ReturnsLastNumberInTheChain()
        {
            IEnumerable<Exception> errors = new[] { new Exception(), new Exception() };
            Resolved resolved = Resolved.Err(errors);

            var r = resolved
                .Match<Resolved>(
                    Ok: () => Resolved.Ok(),
                    Err: (err) => Resolved.Err(new Exception[] { new Exception(), new Exception() } as IEnumerable<Exception>)
                )
                .Match<Resolved>(
                    Ok: () => Resolved.Ok(),
                    Err: (err) => Resolved.Err(new Exception[] { new Exception() } as IEnumerable<Exception>)
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
            IEnumerable<Exception> errors = new Exception[0];
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
