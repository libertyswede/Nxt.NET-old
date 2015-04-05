namespace Nxt.NET.Crypto
{
    public interface ICryptoFactory
    {
        ICrypto Create();
    }

    public class CryptoFactory : ICryptoFactory
    {
        public ICrypto Create()
        {
            return new Crypto();
        }
    }
}
