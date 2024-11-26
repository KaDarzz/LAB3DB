using Microsoft.Extensions.Caching.Memory;
using Lab3.Data;
using Lab3.Models;

namespace Lab3.Service
{
    public class CachedDataService
    {
        private readonly HairdressingContext _context;
        private readonly IMemoryCache _cache;
        private const int RowCount = 20;

        public CachedDataService(HairdressingContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _cache = memoryCache;
        }

        public IEnumerable<Client> GetClient()
        {
            if (!_cache.TryGetValue("Client", out IEnumerable<Client> client))
            {
                client = _context.Clients.Take(RowCount).ToList();
                _cache.Set("Client", client, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2 * 11 + 240)
                });
            }
            return client;
        }

        public IEnumerable<Employee> GetEmployees()
        {
            if (!_cache.TryGetValue("Employees", out IEnumerable<Employee> employees))
            {
                employees = _context.Employees.Take(RowCount).ToList();
                _cache.Set("Employees", employees, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2 * 11 + 240)
                });
            }
            return employees;
        }

        public IEnumerable<EmployeeSchedule> GetEmployeeSchedules()
        {
            if (!_cache.TryGetValue("EmployeeSchedules", out IEnumerable<EmployeeSchedule> employeeSchedule))
            {
                employeeSchedule = _context.EmployeeSchedules.Take(RowCount).ToList();
                _cache.Set("EmployeeSchedules", employeeSchedule, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2 * 11 + 240)
                });
            }
            return employeeSchedule;
        }

        public IEnumerable<PerformedService> GetPerformedServices()
        {
            if (!_cache.TryGetValue("PerformedServices", out IEnumerable<PerformedService> performedService))
            {
                performedService = _context.PerformedServices.Take(RowCount).ToList();
                _cache.Set("PerformedServices", performedService, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2 * 11 + 240)
                });
            }
            return performedService;
        }


        public IEnumerable<Review> GetReviews()
        {
            if (!_cache.TryGetValue("Reviews", out IEnumerable<Review> review))
            {
                review = _context.Reviews.Take(RowCount).ToList();
                _cache.Set("Reviews", review, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2 * 11 + 240)
                });
            }
            return review;
        }

        public IEnumerable<Services> GetServices()
        {
            if (!_cache.TryGetValue("Services", out IEnumerable<Services> service))
            {
                service = _context.Services.Take(RowCount).ToList();
                _cache.Set("Services", service, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2 * 11 + 240)
                });
            }
            return service;
        }

        public IEnumerable<ServiceType> GetServiceTypes()
        {
            if (!_cache.TryGetValue("ServiceTypes", out IEnumerable<ServiceType> serviceType))
            {
                serviceType = _context.ServiceTypes.Take(RowCount).ToList();
                _cache.Set("ServiceTypes", serviceType, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2 * 11 + 240)
                });
            }
            return serviceType;
        }
    }

}
