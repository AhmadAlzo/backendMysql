using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;

using backendTest.Infrastructure.Models;
using backendTest.Infrastructure.Exceptions;

namespace backendTest.Infrastructure.Services
{
    

    /// <summary>
    /// In order to use this method you have to enable Sign-in provider 
    /// in Firebase Console -> Authentication -> Sign-In Method
    /// https://console.firebase.google.com/u/0/project/[PROJECT_ID]/authentication/providers
    /// </summary>
    public interface IFirebaseService
    {
        
        /// In order to use this method you have to enable Anonymous Sign-in provider 
        
        Task<FirebaseUserToken> SignInAnonymously();
        
        /// In order to use this method you have to enable Email/Password Sign-in provider 
        
        Task<FirebaseUserToken> SignUpWithEmailAndPassword(string email, string password);

        
        /// In order to use this method you have to enable to delet acount

        Task DeleteAccount(string email, string password);

        /// In order to use this method you have to enable Email/Password Sign-in provider
        Task<FirebaseUserToken> SignInWithEmailAndPassword(string email, string password);

        /// In order to use this method you can Reset Password By Email 
        Task ResetPasswordByEmail(string email);

        /// In order to use this method you have to enable Google Sign-in provider 
        Task<FirebaseOAuthUserToken> SignInWithGoogleAccessToken(string googleIdToken);
    }

    public class FirebaseService : IFirebaseService
    {
        private readonly AppSettings _appSettings;

        private readonly IHttpClientFactory _httpClientFactory;

        public FirebaseService(IOptions<AppSettings> appSettings, IHttpClientFactory httpClientFactory)
        {
            _appSettings = appSettings.Value;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<FirebaseUserToken> SignInAnonymously()
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "returnSecureToken", "true" }
            });

            HttpClient httpClient = _httpClientFactory.CreateClient();

            HttpResponseMessage httpResponseMessage = 
                await httpClient.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_appSettings.WebAPIKey}", content);

            using Stream contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();

            if (httpResponseMessage.IsSuccessStatusCode == false)
            {
                var contentError = await JsonSerializer.DeserializeAsync<FirebaseContentError>(contentStream);
                if (contentError == null)
                {
                    throw new ArgumentNullException(nameof(contentError));
                }
                throw new FirebaseException(contentError);
            }
            
            return await JsonSerializer.DeserializeAsync<FirebaseUserToken>(contentStream);
        }

        public async Task<FirebaseUserToken> SignInWithEmailAndPassword(string email, string password)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "email", email },
                { "password", password },
                { "returnSecureToken", "true" }
            });

            var httpClient = _httpClientFactory.CreateClient();

            var httpResponseMessage = await httpClient.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_appSettings.WebAPIKey}", content);

            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();

            if (httpResponseMessage.IsSuccessStatusCode == false)
            {
                var contentError = await JsonSerializer.DeserializeAsync<FirebaseContentError>(contentStream);
                if (contentError == null)
                {
                    throw new ArgumentNullException(nameof(contentError));
                }
                throw new FirebaseException(contentError);
            }
 
            return await JsonSerializer.DeserializeAsync<FirebaseUserToken>(contentStream);
        }

        public async Task<FirebaseUserToken> SignUpWithEmailAndPassword(string email, string password)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "email", email },
                { "password", password },
                { "returnSecureToken", "true" }
            });

            var httpClient = _httpClientFactory.CreateClient();

            var httpResponseMessage = await httpClient.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_appSettings.WebAPIKey}", content);

            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();

            if (httpResponseMessage.IsSuccessStatusCode == false)
            {
                var contentError = await JsonSerializer.DeserializeAsync<FirebaseContentError>(contentStream);
                if (contentError == null)
                {
                    throw new ArgumentNullException(nameof(contentError));
                }
                throw new FirebaseException(contentError);
            }
            
            return await JsonSerializer.DeserializeAsync<FirebaseUserToken>(contentStream);
        }

        public async Task ResetPasswordByEmail(string email)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "email", email },
                { "requestType", "PASSWORD_RESET" }
            });

            var httpClient = _httpClientFactory.CreateClient();

            var httpResponseMessage = await httpClient.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_appSettings.WebAPIKey}", content);

            if (httpResponseMessage.IsSuccessStatusCode == false)
            {
                using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync(); // Declare and initialize contentStream here

                var contentError = await JsonSerializer.DeserializeAsync<FirebaseContentError>(contentStream);
                if (contentError == null)
                {
                    throw new ArgumentNullException(nameof(contentError));
                }
                throw new FirebaseException(contentError);
            }
        }

        public async Task DeleteAccount(string email, string password)
        {
            // You can use the Firebase Authentication API to delete the user's account.
            // Here, we'll make a request to Firebase for deleting the account.

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "email", email },
                { "password", password }
            });

            var httpClient = _httpClientFactory.CreateClient();

            var httpResponseMessage = await httpClient.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:delete?key={_appSettings.WebAPIKey}", content);

            if (httpResponseMessage.IsSuccessStatusCode == false)
            {
                using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();

                var contentError = await JsonSerializer.DeserializeAsync<FirebaseContentError>(contentStream);
                if (contentError == null)
                {
                    throw new ArgumentNullException(nameof(contentError));
                }
                throw new FirebaseException(contentError);
            }

            // If the request is successful, the user account has been deleted.
        }

        public async Task<FirebaseOAuthUserToken> SignInWithGoogleAccessToken(string googleAccessToken)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "autoCreate", "true" },
                { "postBody", $"providerId=google.com&access_token={googleAccessToken}&nonce=nonce" },
                { "requestUri", "http://localhost" },
                { "returnIdpCredential", "true" },
                { "returnSecureToken", "true" }
            });

            var httpClient = _httpClientFactory.CreateClient();

            var httpResponseMessage = await httpClient.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:signInWithIdp?key={_appSettings.WebAPIKey}", content);

            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();

            if (httpResponseMessage.IsSuccessStatusCode == false)
            {
                var contentError = await JsonSerializer.DeserializeAsync<FirebaseContentError>(contentStream);
                if (contentError == null)
                {
                    throw new ArgumentNullException(nameof(contentError));
                }
                throw new FirebaseException(contentError);
            }

            return await JsonSerializer.DeserializeAsync<FirebaseOAuthUserToken>(contentStream)!;
        }
    }
}
