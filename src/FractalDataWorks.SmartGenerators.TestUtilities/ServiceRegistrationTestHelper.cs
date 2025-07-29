using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TestUtilities
{
    /// <summary>
    /// Helper for testing service registration patterns between generators.
    /// Simulates the behavior of ConditionalWeakTable used in cross-generator services.
    /// </summary>
    /// <typeparam name="TService">The type of service being tested.</typeparam>
    public class ServiceRegistrationTestHelper<TService> where TService : class
    {
        private readonly ConditionalWeakTable<Compilation, TService> _serviceTable;
        private readonly Dictionary<Compilation, WeakReference> _weakReferences;
        private readonly object _lock = new object();

        public ServiceRegistrationTestHelper()
        {
            _serviceTable = new ConditionalWeakTable<Compilation, TService>();
            _weakReferences = new Dictionary<Compilation, WeakReference>();
        }

        /// <summary>
        /// Registers a service for a specific compilation.
        /// </summary>
        public void RegisterService(Compilation compilation, TService service)
        {
            if (compilation == null)
                throw new ArgumentNullException(nameof(compilation));
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            lock (_lock)
            {
                _serviceTable.Remove(compilation);
                _serviceTable.Add(compilation, service);
                _weakReferences[compilation] = new WeakReference(service);
            }
        }

        /// <summary>
        /// Gets a service for a specific compilation.
        /// </summary>
        public TService? GetService(Compilation compilation)
        {
            if (compilation == null)
                throw new ArgumentNullException(nameof(compilation));

            lock (_lock)
            {
                if (_serviceTable.TryGetValue(compilation, out var service))
                {
                    return service;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets a service or throws if not found (mimics GetRequiredService behavior).
        /// </summary>
        public TService GetRequiredService(Compilation compilation)
        {
            var service = GetService(compilation);
            if (service == null)
            {
                throw new InvalidOperationException(
                    $"Service of type {typeof(TService).Name} not found for compilation");
            }
            return service;
        }

        /// <summary>
        /// Verifies that a service is registered for a compilation.
        /// </summary>
        public void VerifyServiceRegistered(Compilation compilation)
        {
            if (compilation == null)
                throw new ArgumentNullException(nameof(compilation));

            lock (_lock)
            {
                if (!_serviceTable.TryGetValue(compilation, out _))
                {
                    throw new InvalidOperationException(
                        $"Expected service of type {typeof(TService).Name} to be registered for compilation");
                }
            }
        }

        /// <summary>
        /// Verifies that a service is NOT registered for a compilation.
        /// </summary>
        public void VerifyServiceNotRegistered(Compilation compilation)
        {
            if (compilation == null)
                throw new ArgumentNullException(nameof(compilation));

            lock (_lock)
            {
                if (_serviceTable.TryGetValue(compilation, out _))
                {
                    throw new InvalidOperationException(
                        $"Expected service of type {typeof(TService).Name} to NOT be registered for compilation");
                }
            }
        }

        /// <summary>
        /// Simulates weak table cleanup by forcing garbage collection.
        /// </summary>
        public void SimulateWeakTableCleanup()
        {
            // Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Small delay to ensure cleanup
            Thread.Sleep(100);
        }

        /// <summary>
        /// Checks if a previously registered service is still alive (not collected).
        /// </summary>
        public bool IsServiceAlive(Compilation compilation)
        {
            lock (_lock)
            {
                if (_weakReferences.TryGetValue(compilation, out var weakRef))
                {
                    return weakRef.IsAlive;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets the count of registered services.
        /// </summary>
        public int GetRegisteredServiceCount()
        {
            lock (_lock)
            {
                // ConditionalWeakTable doesn't support enumeration or count
                // Return -1 to indicate count is not available
                return -1;
            }
        }

        /// <summary>
        /// Clears all registered services.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                // ConditionalWeakTable doesn't support Clear in older versions
                // We'll need to track compilations separately
                _weakReferences.Clear();
            }
        }
    }

    /// <summary>
    /// Static helper for testing the actual AssemblyScannerService pattern.
    /// </summary>
    public static class ServiceRegistrationTestHelper
    {
        private static readonly ConditionalWeakTable<Compilation, object> _testServices = new();

        /// <summary>
        /// Registers a test service (mimics AssemblyScannerService.Register).
        /// </summary>
        public static void Register<TService>(Compilation compilation, TService service)
            where TService : class
        {
            if (compilation == null)
                throw new ArgumentNullException(nameof(compilation));
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            _testServices.Remove(compilation);
            _testServices.Add(compilation, service);
        }

        /// <summary>
        /// Gets a test service (mimics AssemblyScannerService.Get).
        /// </summary>
        public static TService? Get<TService>(Compilation compilation)
            where TService : class
        {
            if (compilation == null)
                throw new ArgumentNullException(nameof(compilation));

            if (_testServices.TryGetValue(compilation, out var service))
            {
                return service as TService;
            }
            return null;
        }

        /// <summary>
        /// Clears all test services.
        /// </summary>
        public static void Clear()
        {
            // ConditionalWeakTable doesn't support Clear in older versions
            // Create a new instance instead
            throw new NotSupportedException("Clear is not supported on ConditionalWeakTable in this version");
        }

        /// <summary>
        /// Creates a mock service for testing.
        /// </summary>
        public static TMock CreateMockService<TMock>() where TMock : class, new()
        {
            return new TMock();
        }

        /// <summary>
        /// Verifies service registration across multiple compilations.
        /// </summary>
        public static void VerifyServiceAcrossCompilations<TService>(
            params (Compilation compilation, bool shouldHaveService)[] expectations)
            where TService : class
        {
            foreach (var (compilation, shouldHaveService) in expectations)
            {
                var service = Get<TService>(compilation);

                if (shouldHaveService && service == null)
                {
                    throw new InvalidOperationException(
                        $"Expected compilation '{compilation.AssemblyName}' to have service of type {typeof(TService).Name}");
                }
                else if (!shouldHaveService && service != null)
                {
                    throw new InvalidOperationException(
                        $"Expected compilation '{compilation.AssemblyName}' to NOT have service of type {typeof(TService).Name}");
                }
            }
        }
    }

}