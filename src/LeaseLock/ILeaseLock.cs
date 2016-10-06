using System;
using System.Threading.Tasks;

public interface ILeaseLock : IDisposable
{
    Task<bool> TryClaim();
    Task Release();
}
