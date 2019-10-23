using NUnit.Framework;
using System;
using System.Collections.Generic;

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
    }
}
