using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nxt.NET.Block;
using Nxt.NET.Crypto;
using Nxt.NET.Peer;
using Nxt.NET.Response;
using Nxt.NET.Transaction;
using Nxt.NET.Transaction.Types;
using StructureMap;

namespace Nxt.NET
{
    public class Nxt
    {
        private Task _listenTask;
        private IPeerContainer _peerContainer;
        private DbVersion _dbVersion;
        private IConfiguration _configuration;

        public Task Start()
        {
            lock (this)
            {
                return _listenTask ?? (_listenTask = Task.Factory.StartNew(Run));
            }
        }

        private void Run()
        {
            Init();

            var cts = new CancellationTokenSource();
            var morePeersFetcher = ObjectFactory.GetInstance<IPeerAddressFetcher>();
            var peerConnector = ObjectFactory.GetInstance<IPeerConnector>();
            var blockchainProcessor = ObjectFactory.GetInstance<IBlockchainProcessor>();

            var connectTask = peerConnector.PeerConnectingTask(cts.Token);
            var morePeersTask = morePeersFetcher.GetMorePeersTask(cts.Token);
            var moreBlocksTask = blockchainProcessor.GetNextBlocks(cts.Token);
            var writeBlockInfoTask = WriteBlockInfo();
            Task.WaitAll(connectTask, morePeersTask, moreBlocksTask, writeBlockInfoTask);
        }

        private void Init()
        {
            InitObjectFactory();
            EchoSettings();
            _dbVersion.Init();
            InitPeerContainer();
            InitBlockchain();
        }

        private void InitPeerContainer()
        {
            var peerRepository = ObjectFactory.GetInstance<IPeerRepository>();
            var peerAddressTask = peerRepository.GetPeerAddresses();
            _peerContainer.Init(peerAddressTask.Result);
        }

        private void InitObjectFactory()
        {
            // Util
            ObjectFactory.Configure(x => x.For<IHttpClientFactory>().Singleton().Use(new HttpClientFactory()));
            ObjectFactory.Configure(x => x.For<IConfiguration>().Singleton().Use<Configuration>());
            ObjectFactory.Configure(x => x.For<IDnsWrapper>().Use<DnsWrapper>());
            ObjectFactory.Configure(x => x.For<IConvert>().Use<Convert>());
            ObjectFactory.Configure(x => x.For<ICryptoFactory>().Use<CryptoFactory>());

            // Peer
            ObjectFactory.Configure(x => x.For<IPeerContainer>().Singleton().Use<PeerContainer>());
            ObjectFactory.Configure(x => x.For<IPeerRepository>().Singleton().Use<PeerRepository>());
            ObjectFactory.Configure(x => x.For<IPeerAddressFetcher>().Singleton().Use<PeerAddressFetcher>());
            ObjectFactory.Configure(x => x.For<IPeerConnector>().Singleton().Use<PeerConnector>());
            ObjectFactory.Configure(x => x.For<IPeerSaver>().Use<PeerSaver>());

            // Block
            ObjectFactory.Configure(x => x.For<IBlockRepository>().Singleton().Use<BlockRepository>());
            ObjectFactory.Configure(x => x.For<IBlockchainProcessor>().Singleton().Use<BlockchainProcessor>());
            ObjectFactory.Configure(x => x.For<IBlockParser>().Singleton().Use<BlockParser>());
            ObjectFactory.Configure(x => x.For<IGetNextBlocksResponse>().Singleton().Use<GetNextBlocksResponse>());
            ObjectFactory.Configure(x => x.For<IBlockGenerationSignatureVerifyer>().Singleton().Use<BlockGenerationSignatureVerifyer>());
            ObjectFactory.Configure(x => x.For<IBlockchainScanner>().Singleton().Use<BlockchainScanner>());
            ObjectFactory.Configure(x => x.For<IBlockSignatureVerifyer>().Use<BlockSignatureVerifyer>());
            ObjectFactory.Configure(x => x.For<IBlockFactory>().Use<BlockFactory>());
            ObjectFactory.Configure(x => x.For<IBlockVerifyer>().Use<BlockVerifyer>());
            ObjectFactory.Configure(x => x.For<IBlock>().Use<Block.Block>());
            ObjectFactory.Configure(x => x.For<IBlockByteSerializer>().Use<BlockByteSerializer>());
            ObjectFactory.Configure(x => x.For<IBaseTargetCalculator>().Use<BaseTargetCalculator>());
            ObjectFactory.Configure(x => x.For<INextBlocksProcessor>().Use<NextBlocksProcessor>());

            // Transaction
            ObjectFactory.Configure(x => x.For<ITransactionRepository>().Singleton().Use<TransactionRepository>());
            ObjectFactory.Configure(x => x.For<ITransactionProcessor>().Singleton().Use<TransactionProcessor>());
            ObjectFactory.Configure(x => x.For<ITransactionParser>().Singleton().Use<TransactionParser>());
            ObjectFactory.Configure(x => x.For<ITransactionTypeFactory>().Singleton().Use<TransactionTypeFactory>());
            ObjectFactory.Configure(x => x.For<ITransactionVerifyer>().Use<TransactionVerifyer>());
            ObjectFactory.Configure(x => x.For<ITransactionsChecksumCalculator>().Use<TransactionsChecksumCalculator>());
            ObjectFactory.Configure(x => x.For<ITransactionFactory>().Use<TransactionFactory>());
            ObjectFactory.Configure(x => x.For<ITransactionSignatureVerifyer>().Use<TransactionSignatureVerifyer>());
            ObjectFactory.Configure(x => x.For<ITransactionByteSerializer>().Use<TransactionByteSerializer>());
            ObjectFactory.Configure(x => x.For<ITransaction>().Use<Transaction.Transaction>());
            ObjectFactory.Configure(x => x.For<IUnconfirmedTransactionApplier>().Use<UnconfirmedTransactionApplier>());

            // TransactionTypes
            ObjectFactory.Configure(x => x.For<AliasAssignment>().Singleton().Use<AliasAssignment>());
            ObjectFactory.Configure(x => x.For<OrdinaryPayment>().Singleton().Use<OrdinaryPayment>());

            // Other
            ObjectFactory.Configure(x => x.For<IAccountContainer>().Singleton().Use<AccountContainer>());
            ObjectFactory.Configure(x => x.For<IAliasContainer>().Singleton().Use<AliasContainer>());
            ObjectFactory.Configure(x => x.For<IDbController>().Singleton().Use<DbController>());
            ObjectFactory.Configure(x => x.For<IGenesis>().Use<Genesis>());
            
            _peerContainer = ObjectFactory.GetInstance<IPeerContainer>();
            _dbVersion = new DbVersion(ObjectFactory.GetInstance<IDbController>(), ObjectFactory.GetInstance<IConfiguration>());
            _configuration = ObjectFactory.GetInstance<IConfiguration>();
        }

        private void EchoSettings()
        {
            // ReSharper disable UnusedVariable
            var isTestnet = _configuration.IsTestnet;
            var maxNumberOfConnectedPublicPeers = _configuration.MaxNumberOfConnectedPublicPeers;
            // ReSharper restore UnusedVariable
        }

        private void InitBlockchain()
        {
            var blockchainProcessor = ObjectFactory.GetInstance<IBlockchainProcessor>();
            blockchainProcessor.AfterApplyBlock += BlockchainProcessorOnAfterApplyBlock;
            Task.WaitAll(blockchainProcessor.AddGenesisBlockIfNeeded());

            var blockchainScanner = ObjectFactory.GetInstance<IBlockchainScanner>();
            Task.WaitAll(blockchainScanner.Scan(_configuration.ForceValidate));
        }

        private static readonly ConcurrentQueue<string> BlockInfo = new ConcurrentQueue<string>();

        private static void BlockchainProcessorOnAfterApplyBlock(IBlockchainProcessor blockchainProcessor, ApplyBlockEventArgs eventArgs)
        {
            if (true)
                return;

            var block = eventArgs.Block;
            var crypto = ObjectFactory.GetInstance<ICryptoFactory>().Create();

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("---------------------------------------");
            stringBuilder.AppendLine(string.Format("BlockID: {0}", block.PresentationId));
            stringBuilder.AppendLine(string.Format("Height: {0}", block.Height));
            stringBuilder.AppendLine(string.Format("BaseTarget: {0}", block.BaseTarget));
            stringBuilder.AppendLine(string.Format("BlockSignature: {0}", ByteToString(block.BlockSignature)));
            stringBuilder.AppendLine(string.Format("GenerationSignature: {0}", ByteToString(block.GenerationSignature)));
            stringBuilder.AppendLine(string.Format("Generator: {0}", (ulong)block.GeneratorId));
            stringBuilder.AppendLine(string.Format("GeneratorRS: NXT-{0}", crypto.ReedSolomonEncode(block.GeneratorId)));
            stringBuilder.AppendLine(string.Format("NextBlock: {0}", (ulong?)block.NextBlockId));
            stringBuilder.AppendLine(string.Format("NumberOfTransactions: {0}", block.Transactions.Count));
            stringBuilder.AppendLine(string.Format("PayloadHash: {0}", ByteToString(block.PayloadHash)));
            stringBuilder.AppendLine(string.Format("PayloadLength: {0}", block.PayloadLength));
            stringBuilder.AppendLine(string.Format("PreviousBlock: {0}", (ulong?)block.PreviousBlockId));
            stringBuilder.AppendLine(string.Format("PreviousBlockHash: {0}", ByteToString(block.PreviousBlockHash)));
            stringBuilder.AppendLine(string.Format("TimeStamp: {0}", block.Timestamp));
            stringBuilder.AppendLine(string.Format("TotalAmountNQT: {0}", block.TotalAmount));
            stringBuilder.AppendLine(string.Format("TotalFeeNQT: {0}", block.TotalFee));

            foreach (var transaction in block.Transactions.OrderBy(t => t.PresentationId))
                stringBuilder.AppendLine(string.Format("TransactionID: {0}", transaction.PresentationId));

            BlockInfo.Enqueue(stringBuilder.ToString());
        }

        private static string ByteToString(IEnumerable<byte> bytes)
        {
            return string.Join("", bytes.Select(b => System.Convert.ToString(b, 16).PadLeft(2, '0')));
        }

        private static async Task WriteBlockInfo()
        {
            using (var fileStream = File.AppendText(@"c:\temp\blocks.net.txt"))
            {
                while (true)
                {
                    string blockInfo;
                    if (BlockInfo.TryDequeue(out blockInfo))
                        await fileStream.WriteAsync(blockInfo);
                    else
                    {
                        await fileStream.FlushAsync();
                        await Task.Delay(1000);
                    }
                }
            }
        }
    }
}
