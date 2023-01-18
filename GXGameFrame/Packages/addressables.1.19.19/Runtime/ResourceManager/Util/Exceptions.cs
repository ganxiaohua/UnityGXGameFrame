using System;
using System.Runtime.Serialization;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.ResourceManagement.Exceptions
{
    /// <summary>
    /// Base class for all ResourceManager related exceptions.
    /// </summary>
    public class ResourceManagerException : Exception
    {
        /// <summary>
        /// Construct a new ResourceManagerException.
        /// </summary>
        public ResourceManagerException() {}
        /// <summary>
        /// Construct a new ResourceManagerException.
        /// </summary>
        /// <param name="message">Message to describe the exception.</param>
        public ResourceManagerException(string message) : base(message) {}
        /// <summary>
        /// Construct a new ResourceManagerException.
        /// </summary>
        /// <param name="message">Message to describe the exception.</param>
        /// <param name="innerException">Inner exception that caused this exception.</param>
        public ResourceManagerException(string message, Exception innerException) : base(message, innerException) {}
        /// <summary>
        /// Construct a new ResourceManagerException.
        /// </summary>
        /// <param name="message">Message to describe the exception.</param>
        /// <param name="context">Context related to the exception.</param>
        protected ResourceManagerException(SerializationInfo message, StreamingContext context) : base(message, context) {}

        /// <summary>Provides a new string object describing the exception.</summary>
        /// <returns>A newly allocated managed string.</returns>
        public override string ToString() => $"{GetType().Name} : {base.Message}\n{InnerException}";
    }
    /// <summary>
    /// Exception returned when the IResourceProvider is not found for a location.
    /// </summary>
    public class UnknownResourceProviderException : ResourceManagerException
    {
        /// <summary>
        /// The location that contains the provider id that was not found.
        /// </summary>
        public IResourceLocation Location { get; private set; }
        /// <summary>
        /// Construct a new UnknownResourceProviderException
        /// </summary>
        /// <param name="location">The location that caused the exception to be created.</param>
        public UnknownResourceProviderException(IResourceLocation location)
        {
            Location = location;
        }

        /// <summary>
        ///  Construct a new UnknownResourceProviderException
        /// </summary>
        public UnknownResourceProviderException() {}
        /// <summary>
        /// Construct a new UnknownResourceProviderException
        /// </summary>
        /// <param name="message">Message to describe the exception.</param>
        public UnknownResourceProviderException(string message) : base(message) {}
        /// <summary>
        /// Construct a new UnknownResourceProviderException
        /// </summary>
        /// <param name="message">Message to describe the exception.</param>
        /// <param name="innerException">Inner exception that caused this exception.</param>
        public UnknownResourceProviderException(string message, Exception innerException) : base(message, innerException) {}
        /// <summary>
        /// Construct a new UnknownResourceProviderException
        /// </summary>
        /// <param name="message">Message to describe the exception.</param>
        /// <param name="context">Context related to the exception.</param>
        protected UnknownResourceProviderException(SerializationInfo message, StreamingContext context) : base(message, context) {}

        /// <summary>
        /// Returns a string describing this exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return base.Message + ", ProviderId=" + Location.ProviderId + ", Location=" + Location;
            }
        }
        /// <summary>
        /// Returns string representation of exception.
        /// </summary>
        /// <returns>String representation of exception.</returns>
        public override string ToString()
        {
            return Message;
        }
    }

    /// <summary>
    /// Class that represent an error that occured during an AsyncOperation.
    /// </summary>
    public class OperationException : Exception
    {
        /// <summary>
        /// Creates a new instance of <see cref="OperationException"/>.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        /// <param name="innerException">The exception that caused the error, if any.</param>
        public OperationException(string message, Exception innerException = null) : base(message, innerException) {}

        /// <summary>Provides a new string object describing the exception.</summary>
        /// <returns>A newly allocated managed string.</returns>
        public override string ToString() => $"{GetType().Name} : {base.Message}\n{InnerException}";
    }

    /// <summary>
    /// Class that represent an error that occured during a ProviderOperation.
    /// </summary>
    public class ProviderException : OperationException
    {
        /// <summary>
        /// Creates a new instance of <see cref="ProviderException"/>.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        /// <param name="location">The resource location that the operation was trying to provide.</param>
        /// <param name="innerException">The exception that caused the error, if any.</param>
        public ProviderException(string message, IResourceLocation location = null, Exception innerException = null)
            : base(message, innerException)
        {
            Location = location;
        }

        /// <summary>
        /// The resource location that the operation was trying to provide.
        /// </summary>
        public IResourceLocation Location { get; }
    }

    /// <summary>
    /// Class representing an error occured during an operation that remotely fetch data.
    /// </summary>
    public class RemoteProviderException : ProviderException
    {
        /// <summary>
        /// Creates a new instance of <see cref="ProviderException"/>.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        /// <param name="location">The resource location that the operation was trying to provide.</param>
        /// <param name="uwrResult">The result of the unity web request, if any.</param>
        /// <param name="innerException">The exception that caused the error, if any.</param>
        public RemoteProviderException(string message, IResourceLocation location = null, UnityWebRequestResult uwrResult = null, Exception innerException = null)
            : base(message, location, innerException)
        {
            WebRequestResult = uwrResult;
        }

        ///<inheritdoc/>
        public override string Message => this.ToString();

        /// <summary>
        /// The result of the unity web request, if any.
        /// </summary>
        public UnityWebRequestResult WebRequestResult { get; }

        /// <summary>Provides a new string object describing the exception.</summary>
        /// <returns>A newly allocated managed string.</returns>
        public override string ToString()
        {
            if (WebRequestResult != null)
                return $"{GetType().Name} : {base.Message}\nUnityWebRequest result : {WebRequestResult}\n{InnerException}";
            else
                return base.ToString();
        }
    }
}
