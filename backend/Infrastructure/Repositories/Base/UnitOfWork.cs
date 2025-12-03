using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using backend.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore.Storage;

namespace backend.Infrastructure.Repositories.Base;

/*
The unit of work needs to implement all the functionalities
from its interface contract. 

Unit of Work Pattern : created to ensure data consistancy and
integrity by grouping database requests and operations into a units.
that can be commited, or disposed. if it affects the db in negative way we can dispose of the transactions.

Keeping track of objects/business trasactions that can affect the database.
Hepls optimize preformance.

*/


public class UnitOfWork : IUnitOfWork {
    // add the database connection here, as readonly
    private readonly AppDbContext _context;
    // and context transaction.
    private IDbContextTransaction? _transaction;
    private readonly IEntityFileLogger _entityLogger;

    // Registering all the repositories as Dependancy Injections.
    public IUserRepository UserRepository { get; }
    public IStoryNodeRepository StoryNodeRepository { get; }
    public ICharacterRepository CharacterRepository { get; }
    public IChoiceRepository ChoiceRepository { get; }
    public IDialogueRepository DialogueRepository { get; }
    public IPlayerCharacterRepository PlayerCharacterRepository { get; }
    public IGameRepository GameRepository { get; }

    public UnitOfWork(AppDbContext context, IEntityFileLogger entityLogger) {
        _context = context;
        _entityLogger = entityLogger;

        // entity repositories
        UserRepository = new UserRepository(_context, _entityLogger);
        StoryNodeRepository = new StoryNodeRepository(_context, _entityLogger);
        CharacterRepository = new CharacterRepository(_context, _entityLogger);
        ChoiceRepository = new ChoiceRepository(_context, _entityLogger);
        DialogueRepository = new DialogueRepository(_context, _entityLogger);
        PlayerCharacterRepository = new PlayerCharacterRepository(_context, _entityLogger);
        GameRepository = new GameRepository(_context, _entityLogger);
    }

    // method to get the repository we are looking for.
    public IGenericRepository<T> GetRepository<T>() where T : class
    {
        return new GenericRepository<T>(_context, _entityLogger);
    }

    // save changes;
    public async Task SaveAsync() {

        // when saving consolelog to see if it is working
        Console.WriteLine("UnitOfWork Started");

        // when the db context is created we save the data.
        await _context.SaveChangesAsync();
    }

    // start a transaction
    public async Task BeginAsync() {
         if (_context.Database.CurrentTransaction == null)
         {
             _transaction = await _context.Database.BeginTransactionAsync();
         }

    }

    public async Task CommitAsync() {
        // if sucessfull it goes through else we rollback the transaction.

        try {
             // relies on the db connection
             await _transaction!.CommitAsync();
        } catch {
            await RollBackAsync();
            // throw invalidException
            await _entityLogger.LogAsync(
                "Transaction failed to commit, rolling back.",
                LogCategories.System
            );
            throw new InvalidOperationException("Transaction failed to commit.");
        }
       
    }

    // rollback a transaction.
    public async Task RollBackAsync(){
        
        // we check if the transactions are null
        if (_transaction != null) {
            // if not null we roll back the transaction and dispose of it.
            await _transaction.RollbackAsync();
            _transaction.Dispose();

            // we set the transaction to null.
            _transaction = null;
        }

        // we then check the state of the entries
        // and set them to detached.
        foreach (var entry in _context.ChangeTracker.Entries()) {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.State = EntityState.Detached;
                    break;

                case EntityState.Modified:
                    entry.State = EntityState.Detached;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Detached;
                    break;
            }
        }
    }

    
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
