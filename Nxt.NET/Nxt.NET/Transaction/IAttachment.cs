namespace Nxt.NET.Transaction
{
    public interface IAttachment
    {
        int Length { get; }
        byte[] GetBytes();
    }
}