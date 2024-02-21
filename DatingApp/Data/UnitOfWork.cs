using AutoMapper;
using DatingApp.Interfaces;

namespace DatingApp.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext context;
        private readonly IMapper mapper;

        public UnitOfWork(DataContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }
        public IUserRepository UserRepository => new UserRepository(context, mapper);

        public IMessageRepository MessageRepository => new MessageRepository(context, mapper);

        public ILikesRepository LikesRepository => new LikesRepository(context);

        public async Task<bool> Complete()
        {
            return await context.SaveChangesAsync() > 0;
        }

        public bool hasChanges()
        {
            return context.ChangeTracker.HasChanges();
        }
    }
}
