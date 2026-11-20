using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using DominoAllFives.BusinessLogic.Managers;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Services;
using NUnit.Framework;
using Proxy = global::DominoAllFivesService;

namespace DominoAllFives.Server.AdditionalTest
{
    [TestFixture]
    [NonParallelizable]
    public sealed class ProfileServiceWcfTests
    {
        private const int UnstoredPlayerId = 1795301001;
        private const int MaximumPictureSize = 1024 * 1024;

        public static IEnumerable<TestCaseData> InvalidSessionCases
        {
            get
            {
                string[] operations =
                {
                    "GetMyProfile",
                    "UpdateProfileName",
                    "UpdateProfileWithPicture",
                    "SetProfilePicture",
                    "RemoveProfilePicture"
                };
                string[] sessionKinds = { "null", "empty", "whitespace", "unknown", "revoked" };
                foreach (string operation in operations)
                {
                    foreach (string sessionKind in sessionKinds)
                    {
                        yield return new TestCaseData(operation, sessionKind);
                    }
                }
            }
        }

        [TestCaseSource(nameof(InvalidSessionCases))]
        public void Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence(
            string operation,
            string sessionKind)
        {
            // Arrange
            string sessionKey = CreateInvalidSessionKey(sessionKind);
            using (ProfileChannel channel = new ProfileChannel())
            {
                // Act
                Proxy.AccessIssue issue = InvokeRejectedOperation(
                    channel.Client,
                    operation,
                    sessionKey);
                // Assert
                Assert.That(issue, Is.EqualTo(Proxy.AccessIssue.SessionRequired));
            }
        }

        [TestCase("null", Proxy.ProfileIssue.InvalidProfilePicture)]
        [TestCase("empty", Proxy.ProfileIssue.InvalidProfilePicture)]
        [TestCase("format", Proxy.ProfileIssue.InvalidProfilePictureFormat)]
        [TestCase("format-before-size", Proxy.ProfileIssue.InvalidProfilePictureFormat)]
        [TestCase("large", Proxy.ProfileIssue.ProfilePictureTooLarge)]
        public void SetProfilePicture_InvalidSelection_ReturnsTypedFaultBeforePersistence(
            string selection,
            Proxy.ProfileIssue expectedIssue)
        {
            // Arrange
            string sessionKey = SessionManager.CreateSession(UnstoredPlayerId);
            Proxy.ProfilePicture picture = CreateInvalidPicture(selection);
            try
            {
                using (ProfileChannel channel = new ProfileChannel())
                {
                    // Act
                    Proxy.ProfileIssue issue = InvokeRejectedPicture(
                        channel.Client,
                        sessionKey,
                        picture);
                    // Assert
                    Assert.That(issue, Is.EqualTo(expectedIssue));
                }
            }
            finally
            {
                SessionManager.RemoveSession(sessionKey);
            }
        }

        [Test]
        public void UpdateProfileWithPicture_NullSelection_ReturnsTypedFaultBeforePersistence()
        {
            // Arrange
            string sessionKey = SessionManager.CreateSession(UnstoredPlayerId);
            try
            {
                using (ProfileChannel channel = new ProfileChannel())
                {
                    Proxy.ProfileIssue? issue = null;
                    // Act
                    try
                    {
                        channel.Client.UpdateProfileWithPicture(sessionKey, "player", null);
                    }
                    catch (FaultException<Proxy.ProfileIssue> ex)
                    {
                        issue = ex.Detail;
                    }

                    // Assert
                    Assert.That(issue, Is.EqualTo(Proxy.ProfileIssue.InvalidProfilePicture));
                }
            }
            finally
            {
                SessionManager.RemoveSession(sessionKey);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Serialize_Profile_PreservesIdentityStatisticsAndOptionalPicture(bool hasPicture)
        {
            // Arrange
            byte[] bytes = null;
            if (hasPicture)
            {
                bytes = new byte[] { 1, 2, 3, 4 };
            }

            PlayerProfile profile = PlayerProfile.FromStoredProfile(
                "player",
                PlayerStatistics.FromStoredStatistics(0, 120, 7),
                ProfilePicture.FromStoredContent(bytes));
            // Act
            PlayerProfile restored = Roundtrip<PlayerProfile, Proxy.PlayerProfile>(profile);
            // Assert
            Assert.That(
                new object[]
                {
                    restored.Username,
                    restored.Statistics.GamesWon,
                    restored.Statistics.TotalPointsScored,
                    restored.Statistics.GamesPlayed,
                    restored.Picture.OriginalFileName,
                    restored.Picture.ImageData
                },
                Is.EqualTo(new object[] { "player", 0, 120, 7, null, bytes }));
        }

        [Test]
        public void Serialize_SelectedPicture_PreservesOriginalNameAndCopiedContent()
        {
            // Arrange
            byte[] bytes = { 8, 6, 4, 2 };
            ProfilePicture selected = ProfilePicture.FromSelectedContent("avatar.PNG", bytes);
            bytes[0] = 0;
            // Act
            ProfilePicture restored = Roundtrip<ProfilePicture, Proxy.ProfilePicture>(selected);
            // Assert
            Assert.That(
                new object[] { restored.OriginalFileName, restored.ImageData },
                Is.EqualTo(new object[] { "avatar.PNG", new byte[] { 8, 6, 4, 2 } }));
        }

        private static string CreateInvalidSessionKey(string sessionKind)
        {
            switch (sessionKind)
            {
                case "null":
                    return null;
                case "empty":
                    return string.Empty;
                case "whitespace":
                    return "   ";
                case "unknown":
                    return "unknown-profile-session";
                case "revoked":
                    string sessionKey = SessionManager.CreateSession(UnstoredPlayerId);
                    SessionManager.RemoveSession(sessionKey);
                    return sessionKey;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sessionKind));
            }
        }

        private static Proxy.AccessIssue InvokeRejectedOperation(
            Proxy.ProfileServiceClient client,
            string operation,
            string sessionKey)
        {
            try
            {
                switch (operation)
                {
                    case "GetMyProfile":
                        client.GetMyProfile(sessionKey);
                        break;
                    case "UpdateProfileName":
                        client.UpdateProfileName(sessionKey, "player");
                        break;
                    case "UpdateProfileWithPicture":
                        client.UpdateProfileWithPicture(sessionKey, "player", null);
                        break;
                    case "SetProfilePicture":
                        client.SetProfilePicture(sessionKey, null);
                        break;
                    case "RemoveProfilePicture":
                        client.RemoveProfilePicture(sessionKey);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(operation));
                }
            }
            catch (FaultException<Proxy.AccessIssue> ex)
            {
                return ex.Detail;
            }

            Assert.Fail("The unauthenticated operation must return an access fault.");
            return default(Proxy.AccessIssue);
        }

        private static Proxy.ProfilePicture CreateInvalidPicture(string selection)
        {
            switch (selection)
            {
                case "null":
                    return null;
                case "empty":
                    return new Proxy.ProfilePicture
                    {
                        OriginalFileName = "avatar.png",
                        ImageData = Array.Empty<byte>()
                    };
                case "format":
                    return new Proxy.ProfilePicture
                    {
                        OriginalFileName = "avatar.txt",
                        ImageData = new byte[] { 1 }
                    };
                case "format-before-size":
                    return new Proxy.ProfilePicture
                    {
                        OriginalFileName = "avatar.txt",
                        ImageData = new byte[MaximumPictureSize + 1]
                    };
                case "large":
                    return new Proxy.ProfilePicture
                    {
                        OriginalFileName = "avatar.png",
                        ImageData = new byte[MaximumPictureSize + 1]
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(selection));
            }
        }

        private static Proxy.ProfileIssue InvokeRejectedPicture(
            Proxy.ProfileServiceClient client,
            string sessionKey,
            Proxy.ProfilePicture picture)
        {
            try
            {
                client.SetProfilePicture(sessionKey, picture);
            }
            catch (FaultException<Proxy.ProfileIssue> ex)
            {
                return ex.Detail;
            }

            Assert.Fail("The invalid picture must return a typed rejection fault.");
            return default(Proxy.ProfileIssue);
        }

        private static TServer Roundtrip<TServer, TProxy>(TServer value)
        {
            DataContractSerializer serverSerializer = new DataContractSerializer(typeof(TServer));
            DataContractSerializer proxySerializer = new DataContractSerializer(typeof(TProxy));
            using (MemoryStream stream = new MemoryStream())
            {
                serverSerializer.WriteObject(stream, value);
                stream.Position = 0;
                TProxy generated = (TProxy)proxySerializer.ReadObject(stream);
                stream.SetLength(0);
                stream.Position = 0;
                proxySerializer.WriteObject(stream, generated);
                stream.Position = 0;
                return (TServer)serverSerializer.ReadObject(stream);
            }
        }

        private sealed class ProfileChannel : IDisposable
        {
            private const int MaximumMessageSize = 2 * 1024 * 1024;
            private readonly ServiceHost _host;
            public Proxy.ProfileServiceClient Client { get; }

            public ProfileChannel()
            {
                Uri address = new Uri(
                    "net.pipe://localhost/player-profile/" + Guid.NewGuid().ToString("N"));
                NetNamedPipeBinding binding = new NetNamedPipeBinding(
                    NetNamedPipeSecurityMode.None)
                {
                    MaxReceivedMessageSize = MaximumMessageSize,
                    MaxBufferSize = MaximumMessageSize
                };
                binding.ReaderQuotas.MaxArrayLength = MaximumMessageSize;
                _host = new ServiceHost(typeof(DominoService), address);
                _host.AddServiceEndpoint(typeof(IProfileService), binding, string.Empty);
                Client = new Proxy.ProfileServiceClient(
                    binding,
                    new EndpointAddress(address));
                try
                {
                    _host.Open();
                }
                catch (CommunicationException ex)
                {
                    TestContext.Error.WriteLine(ex);
                    Dispose();
                    throw;
                }
                catch (TimeoutException ex)
                {
                    TestContext.Error.WriteLine(ex);
                    Dispose();
                    throw;
                }
            }

            public void Dispose()
            {
                Client.Abort();
                _host.Abort();
            }
        }
    }
}
