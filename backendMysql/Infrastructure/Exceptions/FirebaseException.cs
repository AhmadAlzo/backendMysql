using System;
using backendTest.Infrastructure.Models;

namespace backendTest.Infrastructure.Exceptions
{
    

    public class FirebaseException : Exception
    { 
        public int Code { get; set; }

        public FirebaseException(FirebaseContentError contentError) : base(contentError.error.message)
        {
            Code = contentError.error.code;
        }
    }
}
