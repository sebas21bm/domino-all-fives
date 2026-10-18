using System.IO;
using System.Runtime.Serialization;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using NUnit.Framework;

namespace DominoAllFives.Client.Tests
{
    [TestFixture]
    public class WcfSerializationTests
    {
        [Test]
        public void Credentials_RoundTrip_PreservesIdentifierAndPasswordExactly()
        {
            LoginCredentials copy = RoundTrip(LoginCredentials.ForLogin("  demo  ", "  old-password  "));
            Assert.That(copy.EmailOrUsername, Is.EqualTo("demo"));
            Assert.That(copy.Password, Is.EqualTo("  old-password  "));
        }

        [Test]
        public void LoginResult_Success_RoundTrip_PreservesAccountAndRestrictions()
        {
            LoginResult copy = RoundTrip(LoginResult.Success(PlayerAccount.FromStoredAccount(
                7, "demo", "pt-BR", AccountAccessStatus.FromStoredState(false, false, null))));
            Assert.That(copy.Reason, Is.EqualTo(LoginFailureReason.None));
            Assert.That(copy.Account.PlayerId, Is.EqualTo(7));
            Assert.That(copy.Account.Username, Is.EqualTo("demo"));
            Assert.That(copy.Account.PreferredLanguageId, Is.EqualTo("pt-BR"));
            Assert.That(copy.Account.AccessStatus, Is.Not.Null);
        }

        [TestCase(LoginFailureReason.InvalidCredentials)]
        [TestCase(LoginFailureReason.Banned)]
        [TestCase(LoginFailureReason.Suspended)]
        public void LoginResult_Rejection_RoundTrip_PreservesBusinessReason(LoginFailureReason reason)
        {
            LoginResult copy = RoundTrip(LoginResult.Rejected(reason));
            Assert.That(copy.Reason, Is.EqualTo(reason));
            Assert.That(copy.Account, Is.Null);
        }

        [Test]
        public void Ranking_RoundTrip_PreservesOrderPositionAndImageContent()
        {
            Ranking source = Ranking.FromOrderedEntries(new[] {
                RankingEntry.FromRankedPlayer(1, "first", 12, ProfilePicture.FromStoredContent(new byte[] { 1, 2, 3 })),
                RankingEntry.FromRankedPlayer(2, "second", 5, ProfilePicture.FromStoredContent(null)) }, 9);
            Ranking copy = RoundTrip(source);
            Assert.That(copy.CurrentPlayerRank, Is.EqualTo(9));
            Assert.That(copy.TopPlayers.Count, Is.EqualTo(2));
            Assert.That(copy.TopPlayers[0].Rank, Is.EqualTo(1));
            Assert.That(copy.TopPlayers[0].Username, Is.EqualTo("first"));
            Assert.That(copy.TopPlayers[0].GamesWon, Is.EqualTo(12));
            Assert.That(copy.TopPlayers[0].Picture.ImageData, Is.EqualTo(new byte[] { 1, 2, 3 }));
            Assert.That(copy.TopPlayers[1].Username, Is.EqualTo("second"));
            Assert.That(copy.TopPlayers[1].Picture.ImageData, Is.Null);
            byte[] mutable = copy.TopPlayers[0].Picture.ImageData;
            mutable[0] = 99;
            Assert.That(copy.TopPlayers[0].Picture.ImageData[0], Is.EqualTo(1));
        }

        [Test]
        public void ProfilePicture_RoundTrip_PreservesSelectionMetadata()
        {
            ProfilePicture copy = RoundTrip(ProfilePicture.FromSelectedContent("avatar.png", new byte[] { 4, 5 }));
            Assert.That(copy.OriginalFileName, Is.EqualTo("avatar.png"));
            Assert.That(copy.ImageData, Is.EqualTo(new byte[] { 4, 5 }));
        }

        [Test]
        public void Ranking_Empty_RoundTrip_PreservesEmptySnapshot()
        {
            Ranking copy = RoundTrip(Ranking.FromOrderedEntries(new RankingEntry[0], 0));
            Assert.That(copy.TopPlayers, Is.Empty);
            Assert.That(copy.CurrentPlayerRank, Is.Zero);
        }

        private static T RoundTrip<T>(T source)
        {
            var serializer = new DataContractSerializer(typeof(T));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, source);
                stream.Position = 0;
                return (T)serializer.ReadObject(stream);
            }
        }
    }
}
