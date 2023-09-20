using Microsoft.Extensions.DependencyInjection;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace backendTest.Infrastructure.Extensions
{  
    

    public static class FirebaseExtensions
    {
        public static void AddFirebaseAdminWithCredentialFromFile(this IServiceCollection _, string path)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(path)
            });
        }
    }
}
