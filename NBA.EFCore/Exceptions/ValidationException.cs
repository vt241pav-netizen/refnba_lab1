
using System;

namespace NBA.EFCore.Exceptions
{

    public class ValidationException : Exception
    {
        public ValidationException() : base() { }
        
        public ValidationException(string message) : base(message) { }
        
        public ValidationException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
    

    public class TransactionException : Exception
    {
        public TransactionException() : base() { }
        
        public TransactionException(string message) : base(message) { }
        
        public TransactionException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}