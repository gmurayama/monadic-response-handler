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
            Resolved resolved = Resolved.Err(null);

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
        public void ResolvedWithDefaultErr_ForwardErrThroughTheChain()
        {
            string errorMessage = "Forwarded Err";
            Resolved resolved = Resolved.ErrAsIEnumerable(new Exception(errorMessage));

            var result = resolved
                .Match(
                    Ok: () => Resolved.Ok(),
                    Err: Behavior.Forward
                )
                .Match(
                    Ok: () => Resolved.Ok(),
                    Err: Behavior.Forward
                )
                .Match<int>(
                    Ok: () => Resolved.Ok(1),
                    Err: Behavior.Forward
                );

            Assert.AreEqual(typeof(Err), result.Value.GetType());

            var err = (Err)result.Value;

            Assert.AreEqual(err.Value.First().Message, errorMessage);
        }

        [Test]
        public void ResolvedWithDefaultErrAndIntegerOk_ForwardErrThroughTheChain()
        {
            string errorMessage = "Forwarded Err";
            Resolved<int> resolved = Resolved.ErrAsIEnumerable(new Exception(errorMessage));

            var result = resolved
                .Match(
                    Ok: (innerValue) => Resolved.Ok(1),
                    Err: Behavior.Forward
                )
                .Match<int>(
                    Ok: (innerValue) => Resolved.Ok(2),
                    Err: Behavior.Forward
                )
                .Match(
                    Ok: (innerValue) => Resolved.Ok(),
                    Err: Behavior.Forward
                );

            Assert.AreEqual(typeof(Err), result.Value.GetType());

            var err = (Err)result.Value;

            Assert.AreEqual(err.Value.First().Message, errorMessage);
        }

        [Test]
        public void ResolvedWithStringErr_ForwardErrThroughTheChain()
        {
            string errorMessage = "Forwarded Err";
            Resolved<string, string> resolved = Resolved.Err(errorMessage);

            var result = resolved
                .Match<int>(
                    Ok: (innerValue) => Resolved.Ok(1),
                    Err: Behavior.Forward
                )
                .Match<bool>(
                    Ok: (innerValue) => Resolved.Ok(true),
                    Err: Behavior.Forward
                )
                .Match<char>(
                    Ok: (innerValue) => Resolved.Ok('\a'),
                    Err: Behavior.Forward
                )
                .Match(
                    Ok: (innerValue) => Resolved.Ok('\b'),
                    Err: Behavior.Forward
                );

            Assert.AreEqual(typeof(Err<string>), result.Value.GetType());

            var err = (Err<string>)result.Value;

            Assert.AreEqual(err.Value, errorMessage);
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
