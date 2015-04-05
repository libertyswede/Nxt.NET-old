using System.Linq;
using System.Threading.Tasks;
using NLog;
using Nxt.NET.Transaction;
using StructureMap;

namespace Nxt.NET.Block
{
    public interface IBlockchainScanner
    {
        Task Scan(bool forceValidate);
    }

    public class BlockchainScanner : IBlockchainScanner
    {
        private readonly IBlockRepository _blockRepository;
        private readonly IBlockVerifyer _blockVerifyer;
        private readonly IAccountContainer _accountContainer;
        private readonly ITransactionVerifyer _transactionVerifyer;
        private readonly IBlockchainProcessor _blockchainProcessor;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public BlockchainScanner(IBlockRepository blockRepository,
            IBlockVerifyer blockVerifyer, IAccountContainer accountContainer,
            ITransactionVerifyer transactionVerifyer, IBlockchainProcessor blockchainProcessor)
        {
            _blockRepository = blockRepository;
            _blockVerifyer = blockVerifyer;
            _accountContainer = accountContainer;
            _transactionVerifyer = transactionVerifyer;
            _blockchainProcessor = blockchainProcessor;
        }

        public async Task Scan(bool forceValidate)
        {
            Logger.Info("Scanning blockchain...");
            _accountContainer.Clear();

            _accountContainer.GetOrAddAccount(Genesis.CreatorId).ApplyPublicKey(Genesis.CreatorPublicKey, 0);
            var blocks = await _blockRepository.GetAllBlocks();
            _blockRepository.LastBlock = blocks.First();
            var currentBlockId = Genesis.GenesisBlockId;
            IBlock previousBlock = null;

            foreach (var block in blocks)
            {
                if (block.Id != currentBlockId)
                    throw new NxtException("Database blocks in the wrong order!");
                if (forceValidate && block.Id != Genesis.GenesisBlockId)
                {
                    VerifyBlock(block, previousBlock);
                    VerifyTransactions(block);
                }
                ApplyUnconfirmedTransactions(block);
                
                previousBlock = block;
                _blockRepository.LastBlock = block;

                _blockchainProcessor.ApplyBlock(block);

                if (block.NextBlockId.HasValue) 
                    currentBlockId = block.NextBlockId.Value;

                if (block.Height % 5000 == 0)
                    Logger.Info("processed block " + block.Height);
            }
            Logger.Info("...done");
        }

        private static void ApplyUnconfirmedTransactions(IBlock block)
        {
            using (var applier = ObjectFactory.GetInstance<IUnconfirmedTransactionApplier>())
            {
                if (block.Transactions.All(applier.ApplyUnconfirmedTransaction))
                    applier.Commit();
            }
        }

        private void VerifyTransactions(IBlock block)
        {
            foreach (var transaction in block.Transactions)
            {
                _transactionVerifyer.VerifySignature(transaction);
                transaction.ValidateAttachment();
            }
        }

        private void VerifyBlock(IBlock block, IBlock previousBlock)
        {
            _blockVerifyer.VerifyBlockSignature(block);
            _blockVerifyer.VerifyGenerationSignature(block, previousBlock);
            _blockVerifyer.VerifyVersion(block, block.Height);
        }
    }
}
