using System;
using System.Threading.Tasks;

namespace backend.Application.Interfaces.Repositories;

/*
Unit of Work Pattern : created to ensure data consistancy and
integrity by grouping database requests and operations into a units.
that can be commited, or disposed. if it affects the db in negative way we can dispose of the transactions.

Keeping track of objects/business trasactions that can affect the database.
Hepls optimize preformance.

*/


public interface IUnitOfWork : IDisposable
{
    // generic repository
    IGenericRepository<T> GetRepository<T>() where T : class;
    // entity repositories here.
    IUserRepository UserRepository { get; }
    IStoryNodeRepository StoryNodeRepository { get; }
    ICharacterRepository CharacterRepository { get; }
    IChoiceRepository ChoiceRepository { get; }
    IDialogueRepository DialogueRepository { get; }
    IPlayerCharacterRepository PlayerCharacterRepository { get; }
    IGameRepository GameRepository { get; }


    //Methods that the unit of work has.

    // begin a transaction asyncronously.
    Task BeginAsync();
    //Save change to the db.
    Task SaveAsync();
    // commit the transaction.
    Task CommitAsync();
    // rollback a transaction. that will be disposed.
    Task RollBackAsync();
}