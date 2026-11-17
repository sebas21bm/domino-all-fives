using System;
using System.Threading.Tasks;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Gameplay;
using DominoAllFives.Contracts.Gameplay;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public sealed class MatchUpdateQueueTests
    {
        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_InvalidMatchId_ReportsArgument(int matchId)
        {
            // Arrange
            ArgumentOutOfRangeException failure = null;
            // Act
            try
            {
                new MatchUpdateQueue(matchId);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                failure = ex;
            }
            // Assert
            Assert.That(failure?.ParamName, Is.EqualTo("matchId"));
        }

        [Test]
        public void TryStartPublishing_EmptyQueue_DoesNotAcquireConsumer()
        {
            // Arrange
            MatchUpdateQueue queue = new MatchUpdateQueue(1);
            // Act
            bool started = queue.TryStartPublishing();
            MatchUpdate update;
            bool taken = queue.TryDequeue(out update);
            // Assert
            Assert.That(
                new object[] { started, taken, update },
                Is.EqualTo(new object[] { false, false, null }));
        }

        [Test]
        public void TryDequeue_WithoutConsumer_KeepsPendingUpdate()
        {
            // Arrange
            MatchUpdateQueue queue = new MatchUpdateQueue(1);
            MatchUpdate first = CreateUpdate(1, false);
            queue.Enqueue(first);
            // Act
            MatchUpdate unclaimed;
            MatchUpdate taken;
            bool receivedWithoutConsumer = queue.TryDequeue(out unclaimed);
            bool started = queue.TryStartPublishing();
            bool received = queue.TryDequeue(out taken);
            // Assert
            Assert.That(
                new object[] { receivedWithoutConsumer, unclaimed, started, received, taken },
                Is.EqualTo(new object[] { false, null, true, true, first }));
        }

        [Test]
        public void TryStartPublishing_LaterUpdateActivatesQueue_PreservesCommittedOrder()
        {
            // Arrange
            MatchUpdateQueue queue = new MatchUpdateQueue(1);
            MatchUpdate first = CreateUpdate(1, false);
            MatchUpdate final = CreateUpdate(2, true);
            queue.Enqueue(first);
            queue.Enqueue(final);
            // Act
            bool started = final.PendingUpdates.TryStartPublishing();
            MatchUpdate firstTaken;
            MatchUpdate finalTaken;
            MatchUpdate empty;
            bool hasFirst = queue.TryDequeue(out firstTaken);
            bool hasFinal = queue.TryDequeue(out finalTaken);
            bool hasMore = queue.TryDequeue(out empty);
            bool lateStart = first.PendingUpdates.TryStartPublishing();
            // Assert
            Assert.That(
                new object[]
                {
                    started, hasFirst, firstTaken, hasFinal, finalTaken, hasMore, empty, lateStart
                },
                Is.EqualTo(new object[] { true, true, first, true, final, false, null, false }));
        }

        [TestCase(0, typeof(ArgumentNullException))]
        [TestCase(1, typeof(ArgumentException))]
        [TestCase(2, typeof(ArgumentException))]
        [TestCase(3, typeof(ArgumentException))]
        public void Enqueue_InvalidSnapshot_RejectsWithoutScheduling(
            int scenario,
            Type exceptionType)
        {
            // Arrange
            MatchUpdateQueue queue = new MatchUpdateQueue(1);
            MatchUpdate update = CreateUpdate(1, false);
            switch (scenario)
            {
                case 0:
                    update = null;
                    break;
                case 1:
                    update.View = null;
                    break;
                case 2:
                    update.View.MatchId = 2;
                    break;
                case 3:
                    update.HasStateChange = false;
                    break;
            }

            ArgumentException failure = null;
            // Act
            try
            {
                queue.Enqueue(update);
            }
            catch (ArgumentException ex)
            {
                failure = ex;
            }
            // Assert
            Assert.That(
                new object[] { failure?.GetType(), queue.TryStartPublishing() },
                Is.EqualTo(new object[] { exceptionType, false }));
        }

        [TestCase(-1L)]
        [TestCase(0L)]
        [TestCase(2L)]
        public void Enqueue_NonConsecutiveVersion_PreservesExpectedVersion(long version)
        {
            // Arrange
            MatchUpdateQueue queue = new MatchUpdateQueue(1);
            InvalidOperationException failure = null;
            MatchUpdate first = CreateUpdate(1, false);
            // Act
            try
            {
                queue.Enqueue(CreateUpdate(version, false));
            }
            catch (InvalidOperationException ex)
            {
                failure = ex;
            }

            queue.Enqueue(first);
            queue.TryStartPublishing();
            MatchUpdate taken;
            bool received = queue.TryDequeue(out taken);
            // Assert
            Assert.That(
                new object[] { failure?.GetType(), received, taken },
                Is.EqualTo(new object[] { typeof(InvalidOperationException), true, first }));
        }

        [Test]
        public void Enqueue_DuplicateVersion_KeepsOnlyOriginalUpdate()
        {
            // Arrange
            MatchUpdateQueue queue = new MatchUpdateQueue(1);
            MatchUpdate first = CreateUpdate(1, false);
            queue.Enqueue(first);
            InvalidOperationException failure = null;
            // Act
            try
            {
                queue.Enqueue(CreateUpdate(1, false));
            }
            catch (InvalidOperationException ex)
            {
                failure = ex;
            }

            queue.TryStartPublishing();
            MatchUpdate taken;
            MatchUpdate empty;
            queue.TryDequeue(out taken);
            bool hasMore = queue.TryDequeue(out empty);
            // Assert
            Assert.That(
                new object[] { failure?.GetType(), taken, hasMore, empty },
                Is.EqualTo(new object[] { typeof(InvalidOperationException), first, false, null }));
        }

        [Test]
        public void Enqueue_AfterTerminalUpdate_RejectsFurtherChanges()
        {
            // Arrange
            MatchUpdateQueue queue = new MatchUpdateQueue(1);
            MatchUpdate final = CreateUpdate(1, true);
            queue.Enqueue(final);
            InvalidOperationException failure = null;
            // Act
            try
            {
                queue.Enqueue(CreateUpdate(2, false));
            }
            catch (InvalidOperationException ex)
            {
                failure = ex;
            }

            queue.TryStartPublishing();
            MatchUpdate taken;
            MatchUpdate empty;
            queue.TryDequeue(out taken);
            bool hasMore = queue.TryDequeue(out empty);
            // Assert
            Assert.That(
                new object[] { failure?.GetType(), taken, hasMore, queue.TryStartPublishing() },
                Is.EqualTo(new object[]
                {
                    typeof(InvalidOperationException), final, false, false
                }));
        }

        [Test]
        public void TryStartPublishing_AfterDrain_AcquiresNewWork()
        {
            // Arrange
            MatchUpdateQueue queue = new MatchUpdateQueue(1);
            queue.Enqueue(CreateUpdate(1, false));
            queue.TryStartPublishing();
            MatchUpdate taken;
            queue.TryDequeue(out taken);
            queue.TryDequeue(out taken);
            MatchUpdate second = CreateUpdate(2, false);
            // Act
            queue.Enqueue(second);
            bool started = queue.TryStartPublishing();
            bool received = queue.TryDequeue(out taken);
            // Assert
            Assert.That(
                new object[] { started, received, taken },
                Is.EqualTo(new object[] { true, true, second }));
        }

        [Test]
        public void TryStartPublishing_ActiveConsumer_DrainsNewWorkWithoutAnotherConsumer()
        {
            // Arrange
            MatchUpdateQueue queue = new MatchUpdateQueue(1);
            queue.Enqueue(CreateUpdate(1, false));
            queue.TryStartPublishing();
            MatchUpdate taken;
            queue.TryDequeue(out taken);
            MatchUpdate second = CreateUpdate(2, false);
            // Act
            queue.Enqueue(second);
            bool anotherStarted = queue.TryStartPublishing();
            bool received = queue.TryDequeue(out taken);
            // Assert
            Assert.That(
                new object[] { anotherStarted, received, taken },
                Is.EqualTo(new object[] { false, true, second }));
        }

        [Test]
        public void TryStartPublishing_ConcurrentRequests_AcquiresExactlyOneConsumer()
        {
            // Arrange
            MatchUpdateQueue queue = new MatchUpdateQueue(1);
            queue.Enqueue(CreateUpdate(1, false));
            bool[] started = new bool[2];
            StartAttempt attempt = new StartAttempt(queue, started);
            // Act
            Parallel.For(0, started.Length, attempt.Execute);
            // Assert
            int acquired = 0;
            foreach (bool result in started)
            {
                if (result)
                {
                    acquired++;
                }
            }

            Assert.That(acquired, Is.EqualTo(1));
        }

        [Test]
        public void TryDequeue_DifferentMatches_PreservesIndependentQueues()
        {
            // Arrange
            MatchUpdateQueue firstQueue = new MatchUpdateQueue(1);
            MatchUpdateQueue secondQueue = new MatchUpdateQueue(2);
            MatchUpdate first = CreateUpdate(1, false);
            MatchUpdate second = CreateUpdate(1, false);
            second.View.MatchId = 2;
            firstQueue.Enqueue(first);
            secondQueue.Enqueue(second);
            firstQueue.TryStartPublishing();
            secondQueue.TryStartPublishing();
            // Act
            MatchUpdate firstTaken;
            MatchUpdate secondTaken;
            firstQueue.TryDequeue(out firstTaken);
            secondQueue.TryDequeue(out secondTaken);
            // Assert
            Assert.That(
                new object[] { firstTaken, secondTaken },
                Is.EqualTo(new object[] { first, second }));
        }

        private static MatchUpdate CreateUpdate(long version, bool isFinal)
        {
            MatchUpdate update = new MatchUpdate
            {
                View = new MatchView
                {
                    MatchId = 1,
                    Version = version,
                    Phase = MatchStatus.InProgress
                },
                HasStateChange = true
            };
            if (isFinal)
            {
                update.View.Phase = MatchStatus.Cancelled;
                update.Result = new MatchResult
                {
                    MatchId = 1,
                    Version = version,
                    Phase = MatchStatus.Cancelled
                };
            }

            return update;
        }

        private sealed class StartAttempt
        {
            private readonly MatchUpdateQueue _queue;
            private readonly bool[] _started;

            public StartAttempt(MatchUpdateQueue queue, bool[] started)
            {
                _queue = queue;
                _started = started;
            }

            public void Execute(int index)
            {
                _started[index] = _queue.TryStartPublishing();
            }
        }
    }
}
