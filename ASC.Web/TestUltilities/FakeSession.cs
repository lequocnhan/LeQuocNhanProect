using ASC.Utilities;

namespace ASC.Web.TestUltilities
{
    public class FakeSession : ISession

    {
        public bool IsAvailable => throw new NotImplementedException();
        public string Id => throw new NotImplementedException();
        public IEnumerable<string> Keys => throw new NotImplementedException();

        private Dictionary<string, byte[]> sessionFactory = new Dictionary<string, byte[]>();

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        public void Remove(string key)
        {
            sessionFactory.Remove(key);
        }
        public void Set(string key, byte[] value)
        {
            if (value != null)
            {
                sessionFactory[key] = value;
            }
        }


        public bool TryGetValue(string key, out byte[] value)
        {
            if (sessionFactory.ContainsKey(key) && sessionFactory[key] != null)
            {
                value = sessionFactory[key];
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
    }
}
