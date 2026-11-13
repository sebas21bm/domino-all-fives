using System;
using DominoAllFives.Client.WPF.Services.Threading;
using NUnit.Framework;

namespace DominoAllFives.Client.Tests
{
    [TestFixture]
    public class BackgroundJobTests
    {
        [Test]
        public void Execute_WorkFails_DeliversOriginalExceptionToFailureCallback()
        {
            // Arrange
            InvalidOperationException expected = new InvalidOperationException("Worker failure.");
            Exception received = null;
            BackgroundJob<object, bool> job = new BackgroundJob<object, bool>(
                Fail, null, IgnoreCompletion, CaptureFailure);

            // Act
            ((IBackgroundJob)job).Execute();
            ((IBackgroundJob)job).Complete();

            // Assert
            Assert.That(received, Is.SameAs(expected));
            Assert.That(job.Error, Is.SameAs(expected));

            bool Fail(object state)
            {
                throw expected;
            }

            void CaptureFailure(Exception ex, object state)
            {
                received = ex;
            }
        }

        [Test]
        public void Execute_WorkSucceeds_DeliversResultAndState()
        {
            // Arrange
            object expectedState = new object();
            object receivedState = null;
            bool receivedResult = false;
            BackgroundJob<object, bool> job = new BackgroundJob<object, bool>(
                state => true, expectedState, CaptureResult, FailUnexpectedly);

            // Act
            ((IBackgroundJob)job).Execute();
            ((IBackgroundJob)job).Complete();

            // Assert
            Assert.That(receivedResult, Is.True);
            Assert.That(receivedState, Is.SameAs(expectedState));
            Assert.That(job.Error, Is.Null);

            void CaptureResult(bool result, object state)
            {
                receivedResult = result;
                receivedState = state;
            }
        }

        [Test]
        public void Complete_CancelledFailure_SuppressesFailureCallback()
        {
            // Arrange
            bool callbackInvoked = false;
            BackgroundJob<object, bool> job = new BackgroundJob<object, bool>(
                Fail, null, IgnoreCompletion, CaptureFailure);
            job.Cancel();

            // Act
            ((IBackgroundJob)job).Execute();
            ((IBackgroundJob)job).Complete();

            // Assert
            Assert.That(callbackInvoked, Is.False);
            Assert.That(job.Error, Is.TypeOf<NotSupportedException>());

            bool Fail(object state)
            {
                throw new NotSupportedException("Worker failure.");
            }

            void CaptureFailure(Exception ex, object state)
            {
                callbackInvoked = true;
            }
        }

        private static void IgnoreCompletion(bool result, object state)
        {
        }

        private static void FailUnexpectedly(Exception ex, object state)
        {
            Assert.Fail("Unexpected failure: {0}", ex);
        }
    }
}
