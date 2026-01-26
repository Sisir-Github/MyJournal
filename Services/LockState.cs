namespace MyJournal.Services
{
    public class LockState
    {
        public bool IsLocked { get; private set; } = false;
        public bool IsSessionUnlocked { get; private set; } = false;

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

        public void UnlockSession()
        {
            IsSessionUnlocked = true;
            IsLocked = false;
            OnChange?.Invoke();
        }

        public void ResetSession()
        {
            IsSessionUnlocked = false;
        }
    }
}
