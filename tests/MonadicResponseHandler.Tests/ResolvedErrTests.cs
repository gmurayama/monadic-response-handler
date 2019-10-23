using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace MonadicResponseHandler.Tests
{
    [TestFixture]
    public class ResolvedErrTests
    {
        [Test]
        public void ResolvedErrWithExceptionList_IsErrTrue()
        {
            IEnumerable<Exception> errors = new List<Exception>();
            Resolved resolved = Resolved.Err(errors);

            Assert.IsTrue(resolved.IsErr);            
        }

        [Test]
        public void ResolvedOk_IsErrFalse()
        {
            Resolved resolved = Resolved.Ok();
            Assert.IsFalse(resolved.IsErr);
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
