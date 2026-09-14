using Google.Cloud.Firestore;
using System.Text.Json.Serialization;
using TCML_Class_library;

namespace TCM_Launcher.Model
{
    public class FirebaseTokenData
    {
        public string Token { get; set; }
        public string Uuid { get; set; }
        public string ApiKey { get; set; }
    }

    public class FirebaseTokenResponse
    {
        [JsonPropertyName("idToken")]
        public string IdToken { get; set; } = string.Empty;

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("expiresIn")]
        public string ExpiresIn { get; set; } = string.Empty;

        [JsonPropertyName("isNewUser")]
        public bool IsNewUser { get; set; }
    }

    [FirestoreData]
    public class FirebaseNotification
    {
        public string Id { get; set; }
        public FirestoreRequestType Type { get; set; }
        public string Sender { get; set; }
        public string SenderName { get; set; }
        public Timestamp CreatedAt { get; set; }

        public static string ConvertEnumToString(FirestoreRequestType type)
        {
            switch (type)
            {
                case FirestoreRequestType.FRIEND_REQUEST:
                    return "Sent a friend request";
                case FirestoreRequestType.SHARED_MODPACK:
                    return "Wants to share their modpack";
            }
            return string.Empty;
        }
    }
}
