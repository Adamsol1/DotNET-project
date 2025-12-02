namespace backend.Infrastructure.Logging;

/// <summary>
/// Centralized logging categories matching the API structure.
/// Use nested categories for detailed organization or flat constants 
/// for simpler categorization.
/// </summary>
public static class LogCategories
{
    // Root categories (flat structure)
    public const string Auth = "Auth";
    public const string Account = "Account";
    public const string Admin = "Admin";
    public const string Game = "Game";
    public const string Story = "Story";
    public const string System = "System";

    /// <summary>
    /// Authentication-related logs (login, register, logout, sessions)
    /// </summary>
    public static class Authentication
    {
        public const string Login = "Auth/Login";
        public const string Registration = "Auth/Registration";
        public const string Logout = "Auth/Logout";
        public const string Session = "Auth/Session";
        public const string TokenRefresh = "Auth/TokenRefresh";
    }

    /// <summary>
    /// Account management logs (profile updates, password changes, deletion)
    /// </summary>
    public static class AccountManagement
    {
        public const string Profile = "Account/Profile";
        public const string Username = "Account/Username";
        public const string Password = "Account/Password";
        public const string Deletion = "Account/Deletion";
    }

    /// <summary>
    /// Admin action logs (user management, system operations)
    /// </summary>
    public static class Administration
    {
        public const string UserRetrieval = "Admin/UserRetrieval";
        public const string UserModification = "Admin/UserModification";
        public const string UserDeletion = "Admin/UserDeletion";
    }

    /// <summary>
    /// Game-related logs (saves, sessions, creation)
    /// </summary>
    public static class GamePlay
    {
        public const string Session = "Game/Session";
        public const string Creation = "Game/Creation";
        public const string Saves = "Game/Saves";
        public const string Load = "Game/Load";
        public const string Delete = "Game/Delete";
        public const string Update = "Game/Update";
    }


    /// <summary>
    /// System-level logs (errors, performance, security)
    /// </summary>
    public static class SystemLevel
    {
        public const string Errors = "System/Errors";
        public const string Database = "System/Database";
    }
}