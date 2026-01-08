namespace JournalApp.Services
{
    public class LockState
    {
        public bool IsLocked { get; private set; } = false;

        public event Action? OnChange;

        public void Lock()
        {
            IsLocked = true;
            OnChange?.Invoke();
        }

        public void Unlock()
        {
            IsLocked = false;
            OnChange?.Invoke();
        }
    }
}
