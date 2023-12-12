using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;

namespace WebApplication2
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        internal DbSet<T> _entities;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _entities = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _entities.ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _entities.FindAsync(id);
        }

        public async Task CreateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await _entities.AddAsync(entity);
        }

        public Task UpdateAsync(T entity)
        {
            // EF Core automatically tracks changes based on the primary key value
            // So, you don't have to explicitly call an update method as long as you fetched
            // the entity from the database in the same context.
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            _entities.Update(entity);

            return Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            T entity = await _entities.FindAsync(id);
            if (entity != null)
            {
                _entities.Remove(entity);
            }
            else
            {
                throw new ArgumentNullException(nameof(entity), "Entity not found");
            }
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
