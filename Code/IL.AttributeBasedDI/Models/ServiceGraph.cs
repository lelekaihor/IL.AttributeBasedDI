using Microsoft.Extensions.DependencyInjection;

namespace IL.AttributeBasedDI.Models
{
    /// <summary>
    /// Represents a service node in the dependency injection graph
    /// </summary>
    public sealed class ServiceNode
    {
        /// <summary>
        /// The service type
        /// </summary>
        public Type? ServiceType { get; set; } = null;
        
        /// <summary>
        /// The implementation type
        /// </summary>
        public Type? ImplementationType { get; set; } = null;
        
        /// <summary>
        /// Service key for keyed services
        /// </summary>
        public string? Key { get; set; } = null;
        
        /// <summary>
        /// Service lifetime
        /// </summary>
        public ServiceLifetime Lifetime { get; set; }
        
        /// <summary>
        /// List of decorator types applied to this service
        /// </summary>
        public List<Type> Decorators { get; set; } = [];
        
        /// <summary>
        /// Features that this service is associated with
        /// </summary>
        public HashSet<Enum> Features { get; set; } = [];
    }
    
    /// <summary>
    /// Represents a graph of all services registered via AttributeBasedDI
    /// </summary>
    public sealed class ServiceGraph
    {
        public Dictionary<Type, List<ServiceNode>> ServicesByType { get; } = new();
    }
}
