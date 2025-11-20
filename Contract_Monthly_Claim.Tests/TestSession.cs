using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class TestSession : ISession
{
    private readonly Dictionary<string, byte[]> _store = new();

    public IEnumerable<string> Keys => _store.Keys;

    public string Id => "TestSessionId";
    public bool IsAvailable => true;
    public void Clear() => _store.Clear();
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void Remove(string key) => _store.Remove(key);
    public void Set(string key, byte[] value) => _store[key] = value;
    public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
}

public static class TestHelper
{
    public static DefaultHttpContext BuildHttpContextWithSession()
    {
        var context = new DefaultHttpContext();
        context.Features.Set<ISessionFeature>(new SessionFeature { Session = new TestSession() });
        return context;
    }

    public static IFormFile CreateTestFormFile(string content = "Hello", string fileName = "test.txt")
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };
    }

    // small helper to set typed session entries (string)
    public static void SetSessionString(ISession session, string key, string value)
    {
        session.Set(key, Encoding.UTF8.GetBytes(value));
    }

    public static string GetSessionString(ISession session, string key)
    {
        if (session.TryGetValue(key, out var data)) return Encoding.UTF8.GetString(data);
        return null;
    }

    private class SessionFeature : ISessionFeature
    {
        public ISession Session { get; set; }
    }
}
