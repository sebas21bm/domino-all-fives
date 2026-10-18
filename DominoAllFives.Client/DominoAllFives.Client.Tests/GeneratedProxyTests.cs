using System;
using System.IO;
using System.Runtime.Serialization;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using NUnit.Framework;
using Proxy = DominoAllFivesService;

namespace DominoAllFives.Client.Tests
{
    [TestFixture]
    public class GeneratedProxyTests
    {
        [Test]
        public void ServiceReference_IsGeneratedInsideWpfWithoutReusingServerTypes()
        {
            Assert.That(typeof(Proxy.AuthenticationServiceClient).Assembly,
                Is.SameAs(typeof(DominoAllFives.Client.WPF.Services.ILoginClient).Assembly));
            Assert.That(typeof(Proxy.IAuthenticationService).GetMethod("Login").ReturnType,
                Is.EqualTo(typeof(Proxy.LoginResult)));
            Assert.That(typeof(Proxy.IAuthenticationService).GetMethod("Login").GetParameters()[0].ParameterType,
                Is.EqualTo(typeof(Proxy.LoginCredentials)));
            Assert.That(typeof(Proxy.IRankingRemoteService).GetMethod("GetTopRanking").ReturnType,
                Is.EqualTo(typeof(Proxy.Ranking)));
            Assert.That(typeof(Proxy.LoginResult).GetProperty("Reason").PropertyType,
                Is.EqualTo(typeof(Proxy.LoginFailureReason)));
            Assert.That(typeof(Proxy.LoginFailureReason).IsEnum, Is.True);
            Assert.That(typeof(Proxy.LoginResult).Assembly, Is.Not.EqualTo(typeof(LoginResult).Assembly));
        }

        [Test]
        public void ProxyCredentials_ServerReadsIdentifierAndUnmodifiedPassword()
        {
            LoginCredentials received = Transfer<Proxy.LoginCredentials, LoginCredentials>(
                new Proxy.LoginCredentials { EmailOrUsername = "demo", Password = "  secret  " });
            Assert.That(received.EmailOrUsername, Is.EqualTo("demo"));
            Assert.That(received.Password, Is.EqualTo("  secret  "));
            Assert.That(received.EnsureValid(), Is.EqualTo(LoginFailureReason.None));
        }

        [Test]
        public void RemoteClientInterfaces_UseGeneratedTypesOnly()
        {
            var login = typeof(DominoAllFives.Client.WPF.Services.ILoginClient).GetMethod("Login");
            Assert.That(login.ReturnType, Is.EqualTo(typeof(Proxy.LoginResult)));
            Assert.That(login.GetParameters()[0].ParameterType, Is.EqualTo(typeof(Proxy.LoginCredentials)));
            var ranking = typeof(DominoAllFives.Client.WPF.Services.IRankingClient).GetMethod("GetTopRanking");
            Assert.That(ranking.ReturnType, Is.EqualTo(typeof(Proxy.Ranking)));
        }

        [TestCase(LoginFailureReason.None)]
        [TestCase(LoginFailureReason.InvalidCredentials)]
        [TestCase(LoginFailureReason.Banned)]
        [TestCase(LoginFailureReason.Suspended)]
        public void ServerLoginResult_ProxyReadsTypedReasonAndAccount(LoginFailureReason reason)
        {
            LoginResult source = reason == LoginFailureReason.None
                ? LoginResult.Success(PlayerAccount.FromStoredAccount(17, "demo", "pt-BR",
                    AccountAccessStatus.FromStoredState(false, false, null)))
                : LoginResult.Rejected(reason);
            Proxy.LoginResult received = Transfer<LoginResult, Proxy.LoginResult>(source);
            Assert.That(received.Reason.ToString(), Is.EqualTo(reason.ToString()));
            if (reason == LoginFailureReason.None)
            {
                Assert.That(received.Account.PlayerId, Is.EqualTo(17));
                Assert.That(received.Account.Username, Is.EqualTo("demo"));
                Assert.That(received.Account.PreferredLanguageId, Is.EqualTo("pt-BR"));
                Assert.That(received.Account.AccessStatus.IsBanned, Is.False);
            }
            else
            {
                Assert.That(received.Account, Is.Null);
            }
        }

        [Test]
        public void ServerEnum_AllDeclaredMembers_AreTypedAndReadableByProxy()
        {
            foreach (LoginFailureReason reason in Enum.GetValues(typeof(LoginFailureReason)))
            {
                Proxy.LoginFailureReason received = Transfer<LoginFailureReason, Proxy.LoginFailureReason>(reason);
                Assert.That(received.ToString(), Is.EqualTo(reason.ToString()));
                Assert.That((int)received, Is.EqualTo((int)reason));
            }
        }

        [Test]
        public void ServerRanking_ProxyReadsOrderPositionAndImageBytes()
        {
            Ranking source = Ranking.FromOrderedEntries(new[] {
                RankingEntry.FromRankedPlayer(1, "first", 12,
                    ProfilePicture.FromStoredContent(new byte[] { 1, 2, 3 })),
                RankingEntry.FromRankedPlayer(2, "second", 4, ProfilePicture.FromStoredContent(null)) }, 7);
            Proxy.Ranking received = Transfer<Ranking, Proxy.Ranking>(source);
            Assert.That(received.CurrentPlayerRank, Is.EqualTo(7));
            Assert.That(received.TopPlayers.Length, Is.EqualTo(2));
            Assert.That(received.TopPlayers[0].Rank, Is.EqualTo(1));
            Assert.That(received.TopPlayers[0].Username, Is.EqualTo("first"));
            Assert.That(received.TopPlayers[0].GamesWon, Is.EqualTo(12));
            Assert.That(received.TopPlayers[0].Picture.ImageData, Is.EqualTo(new byte[] { 1, 2, 3 }));
            Assert.That(received.TopPlayers[1].Username, Is.EqualTo("second"));
            Assert.That(received.TopPlayers[1].Picture.ImageData, Is.Null);
        }

        private static TTarget Transfer<TSource, TTarget>(TSource source)
        {
            using (var stream = new MemoryStream())
            {
                new DataContractSerializer(typeof(TSource)).WriteObject(stream, source);
                stream.Position = 0;
                return (TTarget)new DataContractSerializer(typeof(TTarget)).ReadObject(stream);
            }
        }
    }
}
