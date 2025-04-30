using DerbyDash.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DerbyDash.Services
{
    public class RacerService
    {
        private List<Racer> _racers = new List<Racer>();
        private Racer? _currentRacer;
        
        // Event that components can subscribe to for updates
        public event Action? OnRacerChanged;

        public RacerService()
        {
            // Initialize with sample data for demonstration
            // In a real application, you would load this from a database
            _racers = new List<Racer>
            {
                new Racer { Id = "1", Name = "Alice", UserName = "user1" },
                new Racer { Id = "2", Name = "Bob", UserName = "user1" },
                new Racer { Id = "3", Name = "Charlie", UserName = "user1" }
            };
            
            // Set default current racer
            if (_racers.Any())
            {
                _currentRacer = _racers.First();
            }
        }

        public List<Racer> GetRacers()
        {
            return _racers;
        }

        public Racer? GetCurrentRacer()
        {
            return _currentRacer;
        }

        public void SetCurrentRacer(string racerId)
        {
            var racer = _racers.FirstOrDefault(r => r.Id == racerId);
            if (racer != null)
            {
                _currentRacer = racer;
                OnRacerChanged?.Invoke();
            }
        }

        public async Task<List<Racer>> GetRacersForUserAsync(string userName)
        {
            // In a real application, you would fetch this from a database
            // For now, we'll just return all racers
            return _racers.Where(r => r.UserName == userName).ToList();
        }

        public async Task AddRacerAsync(Racer racer)
        {
            // Generate a new ID for the racer
            racer.Id = Guid.NewGuid().ToString();
            
            // Add the racer to the list
            _racers.Add(racer);
            
            // If this is the first racer, set it as the current racer
            if (_currentRacer == null)
            {
                _currentRacer = racer;
                OnRacerChanged?.Invoke();
            }
        }
    }
}