using Google.Api.Gax;
using Google.Cloud.Firestore;
using Grpc.Core;
using System.Net.Http;
using System.Net.Http.Json;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model;
using TCML_Class_library;

namespace TCM_Launcher.Services
{
    public class FirebaseService : IFirebaseService
    {
        private readonly HttpClient client = new();
        private readonly IBackendService backendService;
        private readonly IDownloadedModpacksService downloadedModpacksService;
        private FirestoreDb firestoreDb;

        private FirebaseTokenResponse? authToken;
        private CancellationTokenSource? refreshCts;
        public FirebaseService(IBackendService backendService, IDownloadedModpacksService downloadedModpacksService)
        {
            this.backendService = backendService;
            this.downloadedModpacksService = downloadedModpacksService;
        }

        public List<FirestoreUser> CachedFriends { get; private set; } = new();

        public List<FirestoreModpack> CachedModpacks {  get; private set; } = new();

        private List<FirestoreChangeListener> activeListeners = new();

        public async Task<string?> GetAuthTokenAsync()
        {
            try
            {
                var data = await backendService.GetCustomTokenAsync();
                if(data != null)
                {
                    var payload = new
                    {
                        token = data.Token,
                        returnSecureToken = true
                    };

                    var response = await client.PostAsJsonAsync(
                        $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithCustomToken?key={data.ApiKey}",
                        payload);

                    if (!response.IsSuccessStatusCode) return null;

                    authToken = await response.Content.ReadFromJsonAsync<FirebaseTokenResponse>();
                    return authToken?.IdToken;
                }
                return null;
                
            }
            catch (Exception ex)
            {
                Logger.Error("There was an excetion when authorizing into Firebase", ex);
                return null;
            }
        }

        private void StartTokenRefreshLoop()
        {
            if (authToken == null) return;
            refreshCts?.Cancel();
            refreshCts = new CancellationTokenSource();

            _ = Task.Run(async () =>
            {
                using var timer = new PeriodicTimer(TimeSpan.FromMinutes(50));

                while (!refreshCts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await timer.WaitForNextTickAsync(refreshCts.Token);
                        var refreshed = await backendService.RefreshFirebaseTokenAsync(authToken?.RefreshToken);
                        if (refreshed != null) authToken = refreshed;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("There was an exception while refreshing firebase token.", ex);
                    }
                }
            }, refreshCts.Token);
        }

        public async Task ListenToRequestsAsync(string uuid, Action<FirestoreRequest> onAdded, Action<string> onRemoved)
        {
            if(firestoreDb == null)
            {
                await InitializeFirebaseAsync();
            }

            if (firestoreDb == null)
            {
                Logger.Error("Cannot listen to requests: firestoreDb is not initialized.");
                return;
            }

            Query query = firestoreDb.Collection("requests")
                .WhereEqualTo("target", uuid)
                .WhereEqualTo("status", FirestoreRequestStatus.PENDING);

            var listener = query.Listen(spanshot =>
            {
                foreach (DocumentChange change in spanshot.Changes)
                {
                    var doc = change.Document;

                    if(change.ChangeType == DocumentChange.Type.Added)
                    {
                        var request = doc.ConvertTo<FirestoreRequest>();
                        App.Current?.Dispatcher.Invoke(() =>
                        {
                            onAdded(request);
                        });
                    }
                    else if (change.ChangeType == DocumentChange.Type.Removed)
                    {
                        App.Current?.Dispatcher.Invoke(() =>
                        {
                            onRemoved(doc.Id);
                        });
                    }
                }
            });
            _ = listener.ListenerTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Logger.Error("Firestore requests listener encountered an error.", t.Exception);
                }
            });
            activeListeners.Add(listener);
        }

        public async Task ListenToFriendsList(string uuid, Action<List<FirestoreUser>> onFriendsChanged)
        {
            if(firestoreDb == null)
            {
                await InitializeFirebaseAsync();
            }

            if (firestoreDb == null)
            {
                Logger.Error("Cannot listen to friends list: firestoreDb is not initialized.");
                return;
            }

            DocumentReference userDocRef = firestoreDb.Collection("users").Document(uuid);

            var listener = userDocRef.Listen(snapshot =>
            {
                if (snapshot.Exists)
                {
                    CachedFriends = snapshot.ContainsField("friends")
                        ? snapshot.GetValue<List<FirestoreUser>>("friends") ?? new List<FirestoreUser>()
                        : new List<FirestoreUser>();

                    App.Current?.Dispatcher.Invoke(() =>
                    {
                        onFriendsChanged(CachedFriends);
                    });
                }
            });
            _ = listener.ListenerTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Logger.Error("Firestore friends listener encountered an error.", t.Exception);
                }
            });
            activeListeners.Add(listener);
        }

        public async Task StopListeningAsync()
        {
            if(activeListeners.Count > 0)
            {
                foreach (var change in activeListeners)
                {
                    await change.StopAsync();
                }
                activeListeners.Clear();
            }
            CachedFriends.Clear();
            CachedModpacks.Clear();
        }

        private async Task InitializeFirebaseAsync()
        {
            string? token = await GetAuthTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                Logger.Error("Couldn't connect to cloud services. No token.");
                return;
            }
            StartTokenRefreshLoop();
            var builder = new FirestoreDbBuilder
            {
                ProjectId = "tcm-launcher",
            };
#if DEBUG
            Environment.SetEnvironmentVariable("FIRESTORE_EMULATOR_HOST", "127.0.0.1:8080");
            builder.EmulatorDetection = EmulatorDetection.EmulatorOrProduction;
#else
            Environment.SetEnvironmentVariable("FIRESTORE_EMULATOR_HOST", null);
            var callCredentials = CallCredentials.FromInterceptor((context, metadata) =>
            {
                if(authToken != null && !string.IsNullOrEmpty(authToken.IdToken))
                {
                    metadata.Add("Authorization", $"Bearer {authToken.IdToken}");
                }
                return Task.CompletedTask;
            });
            builder.ChannelCredentials = ChannelCredentials.Create(new SslCredentials(), callCredentials);
            builder.EmulatorDetection = EmulatorDetection.None;
#endif
            firestoreDb = builder.Build();
        }

        public async Task ListenToModpacksAsync(string uuid, Action<List<FirestoreModpack>> OnModpacksChanged)
        {
            if(firestoreDb == null)
            {
                await InitializeFirebaseAsync();
            }

            if (firestoreDb == null)
            {
                Logger.Error("Cannot listen to modpacks: firestoreDb is not initialized.");
                return;
            }

            Query query = firestoreDb.Collection("modpacks")
                .Where(Filter.Or(
                    Filter.EqualTo("ownerUUID", uuid),
                    Filter.ArrayContains("sharedWith", uuid)
                ));

            var listener = query.Listen(async snapshot =>
            {
                foreach (DocumentChange change in snapshot.Changes)
                {
                    var doc = change.Document;

                    if (change.ChangeType == DocumentChange.Type.Added)
                    {
                        var modpack = doc.ConvertTo<FirestoreModpack>();
                        CachedModpacks.Add(modpack);
                    }
                    else if (change.ChangeType == DocumentChange.Type.Removed)
                    {
                        await downloadedModpacksService.DeleteModpackAsync(doc.Id);
                        CachedModpacks.RemoveAll(m => m.Id == doc.Id);
                    }
                    else if (change.ChangeType == DocumentChange.Type.Modified)
                    {
                        var modpack = change.Document.ConvertTo<FirestoreModpack>();
                        if(modpack != null)
                        {
                            var index = CachedModpacks.FindIndex(m => m.Id == doc.Id);
                            if (index != -1)
                            {
                                await downloadedModpacksService.UpdateModpackAsync(modpack);
                                CachedModpacks[index] = modpack;
                            }
                        }
                    }
                }
                App.Current?.Dispatcher.Invoke(() =>
                {
                    OnModpacksChanged(CachedModpacks);
                });
            });
            _ = listener.ListenerTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Logger.Error("Firestore modpacks listener encountered an error.", t.Exception);
                }
            });
            activeListeners.Add(listener);
        }
    }
}
