using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoogleOauth2Client
{
    internal class Program
    {


        static void Main(string[] args)
        {
            string ClientID = "943041189633-rtudcngddr3n3mlpg2em7e79ad7ianv5.apps.googleusercontent.com";
            string SecretKey = "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";

            var ClientSecret = new ClientSecrets();
            ClientSecret.ClientId = ClientID;
            ClientSecret.ClientSecret = SecretKey;

            string[] Scopes = {
                "https://www.googleapis.com/auth/gmail.readonly",
                "https://www.googleapis.com/auth/userinfo.email",
                "https://www.googleapis.com/auth/youtube"
            };

            try
            {
                UserCredential Credentials = GoogleWebAuthorizationBroker.AuthorizeAsync(ClientSecret, Scopes, "user", CancellationToken.None).Result;


                // Revoke
                //Credentials.Flow.RevokeTokenAsync(Credentials.UserId, Credentials.Token.RefreshToken, CancellationToken.None).Wait();


                Console.WriteLine(String.Format("User Id : {0}", Credentials.UserId));

                Console.WriteLine("#############################################################################################");
                Console.WriteLine("[ Token Information ]");
                Console.WriteLine(String.Format("AccessToken : {0}", Credentials.Token.AccessToken));
                Console.WriteLine(String.Format("RefreshToken : {0}", Credentials.Token.RefreshToken));
                Console.WriteLine(String.Format("Scope : {0}", Credentials.Token.Scope));
                Console.WriteLine(String.Format("TokenType : {0}", Credentials.Token.TokenType));
                Console.WriteLine(String.Format("ExpiresInSeconds : {0}", Credentials.Token.ExpiresInSeconds));
                Console.WriteLine(String.Format("IdToken : {0}", Credentials.Token.IdToken));
                Console.WriteLine(String.Format("IssuedUtc : {0}", Credentials.Token.IssuedUtc));

                Console.WriteLine("#############################################################################################");

                Console.WriteLine(String.Format("Flow.Clock.UtcNow : {0}", Credentials.Flow.Clock.UtcNow));
                Console.WriteLine(String.Format("DataStore : {0}", Credentials.Flow.DataStore.ToString()));
                Console.WriteLine(String.Format("AccessMethod : {0}", Credentials.Flow.AccessMethod.ToString()));
                Console.WriteLine(String.Format("QuotaProject : {0}", Credentials.QuotaProject));

                Console.WriteLine("#############################################################################################");

                if (Credentials.Token.IsExpired(SystemClock.Default))
                {
                    Credentials.Flow.RefreshTokenAsync(Credentials.UserId, Credentials.Token.RefreshToken, CancellationToken.None).Wait();
                }

                var baseClientService = new BaseClientService.Initializer();
                baseClientService.HttpClientInitializer = Credentials;

                GmailService gmailService = new GmailService(baseClientService);

                Console.WriteLine(String.Format("Name : {0}", gmailService.Name));
                Console.WriteLine(String.Format("ApplicationName : {0}", gmailService.ApplicationName));
                Console.WriteLine(String.Format("BatchPath : {0}", gmailService.BatchPath));
                Console.WriteLine(String.Format("BatchUri : {0}", gmailService.BatchUri));
                Console.WriteLine(String.Format("ApiKey : {0}", gmailService.ApiKey));
                Console.WriteLine(String.Format("GZipEnabled : {0}", gmailService.GZipEnabled));

                Profile gmailProfile = gmailService.Users.GetProfile("satitza21@gmail.com").Execute();
                Console.WriteLine(String.Format("EmailAddress : {0}", gmailProfile.EmailAddress));
                Console.WriteLine(String.Format("MessagesTotal : {0}", gmailProfile.MessagesTotal));
                Console.WriteLine(String.Format("ETag : {0}", gmailProfile.ETag));
                Console.WriteLine(String.Format("ThreadsTotal : {0}", gmailProfile.ThreadsTotal));
                Console.WriteLine(String.Format("HistoryId : {0}", gmailProfile.HistoryId));

                Console.WriteLine("#############################################################################################");

                YouTubeService youTubeService = new YouTubeService(baseClientService);
                var channelsListRequest = youTubeService.Channels.List("contentDetails");
                channelsListRequest.Mine = true;

                // Retrieve the contentDetails part of the channel resource for the authenticated user's channel.
                var channelsListResponse = channelsListRequest.Execute();

                foreach (var channel in channelsListResponse.Items)
                {
                    // From the API response, extract the playlist ID that identifies the list
                    // of videos uploaded to the authenticated user's channel.
                    var uploadsListId = channel.ContentDetails.RelatedPlaylists.Uploads;

                    Console.WriteLine("Videos in list {0}", uploadsListId);

                    var nextPageToken = "";
                    while (nextPageToken != null)
                    {
                        var playlistItemsListRequest = youTubeService.PlaylistItems.List("snippet");
                        playlistItemsListRequest.PlaylistId = uploadsListId;
                        playlistItemsListRequest.MaxResults = 50;
                        playlistItemsListRequest.PageToken = nextPageToken;

                        // Retrieve the list of videos uploaded to the authenticated user's channel.
                        var playlistItemsListResponse = playlistItemsListRequest.Execute();

                        foreach (var playlistItem in playlistItemsListResponse.Items)
                        {
                            // Print information about each video.
                            Console.WriteLine("{0} ({1})", playlistItem.Snippet.Title, playlistItem.Snippet.ResourceId.VideoId);
                        }

                        nextPageToken = playlistItemsListResponse.NextPageToken;
                    }
                }


                Console.ReadLine();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
    }
}
