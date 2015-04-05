using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Nxt.NET.Peer;
using Nxt.NET.Transaction;
using StructureMap;

namespace Nxt.NET.Block
{
    public delegate void ApplyBlockHandler(IBlockchainProcessor blockchainProcessor, ApplyBlockEventArgs eventArgs);

    public interface IBlockchainProcessor
    {
        event ApplyBlockHandler BeforeApplyBlock;
        event ApplyBlockHandler AfterApplyBlock;
        Task PushBlock(IBlock block);
        Task AddGenesisBlockIfNeeded();
        Task ProcessFork(IPeer peer, List<IBlock> forkBlocks, IBlock commonBlock);
        Task GetNextBlocks(CancellationToken token);
        void ApplyBlock(IBlock block);
    }

    public class BlockchainProcessor : IBlockchainProcessor
    {
        private readonly IBlockRepository _blockRepository;
        private readonly ITransactionProcessor _transactionProcessor;
        private readonly IPeerContainer _peerContainer;
        private readonly IBlockVerifyer _blockVerifyer;
        private readonly IGenesis _genesis;
        private readonly IConfiguration _configuration;
        private readonly IAccountContainer _accountContainer;
        private readonly IBaseTargetCalculator _baseTargetCalculator;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public event ApplyBlockHandler BeforeApplyBlock;
        public event ApplyBlockHandler AfterApplyBlock;

        public BlockchainProcessor(IBlockRepository blockRepository, ITransactionProcessor transactionProcessor,
            IPeerContainer peerContainer,
            IBlockVerifyer blockVerifyer, IGenesis genesis, IConfiguration configuration,
            IAccountContainer accountContainer,
            IBaseTargetCalculator baseTargetCalculator)
        {
            _blockRepository = blockRepository;
            _transactionProcessor = transactionProcessor;
            _peerContainer = peerContainer;
            _blockVerifyer = blockVerifyer;
            _genesis = genesis;
            _configuration = configuration;
            _accountContainer = accountContainer;
            _baseTargetCalculator = baseTargetCalculator;
        }

        public async Task PushBlock(IBlock block)
        {
            await VerifyBlock(block);
            await AddBlock(block);
        }

        private async Task VerifyBlock(IBlock block)
        {
            await _blockVerifyer.VerifyBlock(block);
            await _transactionProcessor.VerifyTransactions(block);
        }

        private async Task AddBlock(IBlock block)
        {
            if (_blockRepository.LastBlock != null)
            {
                block.PreviousBlockId = _blockRepository.LastBlock.Id;
                block.Height = _blockRepository.LastBlock.Height + 1;
                _baseTargetCalculator.CalculateAndSetBaseTarget(_blockRepository.LastBlock, block);
            }
            else
            {
                block.Height = 0;
            }
            ConnectTransactionsToBlock(block);
            _blockRepository.LastBlock = block;
            ApplyBlock(block);
            await _blockRepository.AddBlock(block);
        }

        private static void ConnectTransactionsToBlock(IBlock block)
        {
            foreach (var transaction in block.Transactions)
            {
                transaction.BlockId = block.Id;
                transaction.Height = block.Height;
                transaction.BlockTimestamp = block.Timestamp;
            }
        }

        public async Task AddGenesisBlockIfNeeded()
        {
            if (await _blockRepository.HasBlock(Genesis.GenesisBlockId))
                return;

            var genesisTransactions = _transactionProcessor.GetGenesisTransactions();
            var genesisBlock = _genesis.GetGenesisBlock(ObjectFactory.GetInstance<IBlockFactory>(),
                _baseTargetCalculator, genesisTransactions);
            await AddBlock(genesisBlock);
        }

        public void ApplyBlock(IBlock block)
        {
            if (BeforeApplyBlock != null) 
                BeforeApplyBlock(this, new ApplyBlockEventArgs(block));
            var account = _accountContainer.GetOrAddAccount(block.GeneratorId);
            account.ApplyPublicKey(block.GeneratorPublicKey, block.Height);
            account.Balance.AddToBalanceAndUnconfirmedBalanceNQT(block.TotalFee);
            account.Balance.AddToForgedBalanceNQT(block.TotalFee);
            _transactionProcessor.ApplyTransactions(block);
            if (AfterApplyBlock != null) 
                AfterApplyBlock(this, new ApplyBlockEventArgs(block));
        }

        public async Task ProcessFork(IPeer peer, List<IBlock> forkBlocks, IBlock commonBlock)
        {
            throw new NotImplementedException();
        }

        public async Task GetNextBlocks(CancellationToken token)
        {
            Logger.Info("Starting Next blocks task");
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(_configuration.TaskSleepInterval, token);
                var peer = _peerContainer.GetAllPeers().RandomizedSingleOrDefault(p => p.IsConnected);
                if (peer == null)
                    continue;

                var nextBlocksProcessor = ObjectFactory.GetInstance<INextBlocksProcessor>();
                try
                {
                    await nextBlocksProcessor.GetNextBlocks(peer);
                }
                catch (Exception e)
                {
                    Logger.FatalException("CRITICAL ERROR. PLEASE REPORT TO THE DEVELOPERS.", e);
                    Environment.Exit(-1);
                }
            }
        }
    }
}
