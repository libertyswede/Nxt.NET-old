using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using NLog;
using Nxt.NET.Peer;
using Nxt.NET.Request;
using Nxt.NET.Response;

namespace Nxt.NET.Block
{
    public interface INextBlocksProcessor
    {
        Task GetNextBlocks(IPeer peer);
    }

    public class NextBlocksProcessor : INextBlocksProcessor
    {
        public IPeer LastBlockchainFeeder { get; set; }

        private readonly IBlockchainProcessor _blockchainProcessor;
        private readonly IBlockRepository _blockRepository;
        private IPeer _peer;
        private bool _peerHasMore;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public NextBlocksProcessor(IBlockchainProcessor blockchainProcessor, IBlockRepository blockRepository)
        {
            _blockchainProcessor = blockchainProcessor;
            _blockRepository = blockRepository;
        }

        public async Task GetNextBlocks(IPeer peer)
        {
            _peer = peer;
            CheckPeerConnection();
            _peerHasMore = true;
            var lastBlock = _blockRepository.LastBlock;
            long? commonBlockId;

            if (!(await CompareCumulativeDifficultyWithPeer(lastBlock.CumulativeDifficulty)))
                return;

            if (!(commonBlockId = await GetCommonBlockId(lastBlock)).HasValue)
                return;

            var commonBlock = await _blockRepository.GetBlock(commonBlockId.Value);

            if (lastBlock.Height - commonBlock.Height >= 720)
                return;

            var currentBlockId = commonBlockId.Value;
            var forkBlocks = new List<IBlock>();
            while (true)
            {
                var nextBlocksResponse = await _peer.SendRequest(new GetNextBlocksRequest(currentBlockId));
                if (nextBlocksResponse == null || nextBlocksResponse.Blocks.Count == 0)
                    break;
                foreach (var block in nextBlocksResponse.Blocks)
                {
                    currentBlockId = block.Id;
                    if (lastBlock.Id == block.PreviousBlockId)
                    {
                        await _blockchainProcessor.PushBlock(block);
                        lastBlock = block;
                    }
                    else if (!(await _blockRepository.HasBlock(block.Id)))
                        forkBlocks.Add(block);
                }
            }
            if (forkBlocks.Any() && lastBlock.Height - commonBlock.Height < 720)
                await _blockchainProcessor.ProcessFork(_peer, forkBlocks, commonBlock);
        }

        private void CheckPeerConnection()
        {
            if (!_peer.IsConnected)
                throw new ArgumentException("Peer must be connected");
        }

        private async Task<long?> GetCommonBlockId(IBlock lastBlock)
        {
            var commonBlockId = (long?)Genesis.GenesisBlockId;
            if (lastBlock.Id != Genesis.GenesisBlockId)
            {
                commonBlockId = await GetCommonMilestoneBlockId(lastBlock);
            }
            if (!commonBlockId.HasValue || !_peerHasMore)
                return null;

            commonBlockId = await RequestCommonBlockIdFromPeer(commonBlockId.Value);

            if (!commonBlockId.HasValue || !_peerHasMore)
                return null;

            return commonBlockId;
        }

        private async Task<bool> CompareCumulativeDifficultyWithPeer(BigInteger localCumulativeDifficulty)
        {
            var response = await _peer.SendRequest(new GetCumulativeDifficultyRequest());
            if (response == null)
                return false;

            var peerCumulativeDifficulty = response.CumulativeDifficulty;
            return peerCumulativeDifficulty != null && peerCumulativeDifficulty.Value.CompareTo(localCumulativeDifficulty) > 0;
        }

        private async Task<long?> GetCommonMilestoneBlockId(IBlock lastBlock)
        {
            long? lastMilestoneBlockId = null;

            while (true)
            {
                IGetMilestoneBlockIdsResponse response;
                if (lastMilestoneBlockId == null)
                    response = await _peer.SendRequest(new GetMilestoneBlockIdsRequestFromLastBlock(lastBlock.Id));
                else
                    response = await _peer.SendRequest(new GetMilestoneBlockIdsRequestFromMilestoneBlock(lastMilestoneBlockId.Value));
                if (response == null || !response.MilestonBlockIds.Any())
                    return Genesis.GenesisBlockId;
                if (response.MilestonBlockIds.Count > 20)
                {
                    Logger.Debug("Obsolete or rogue peer " + _peer.Address + " sends too many milestoneBlockIds, blacklisting");
                    _peer.Blacklist();
                    return null;
                }
                if (response.IsLast)
                    _peerHasMore = false;
                foreach (var milestonBlockId in response.MilestonBlockIds)
                {
                    if (await _blockRepository.HasBlock(milestonBlockId))
                    {
                        if (!lastMilestoneBlockId.HasValue && response.MilestonBlockIds.Count > 1)
                        {
                            _peerHasMore = false;
                        }
                        return milestonBlockId;
                    }
                    lastMilestoneBlockId = milestonBlockId;
                }
            }
        }

        private async Task<long?> RequestCommonBlockIdFromPeer(long commonBlockId)
        {
            while (true)
            {
                var response = await _peer.SendRequest(new GetNextBlockIdsRequest(commonBlockId));
                if (response == null || !response.NextBlockIds.Any())
                    return null;
                if (response.NextBlockIds.Count > 1440)
                {
                    Logger.Debug("Obsolete or rogue peer " + _peer.Address + " sends too many nextBlockIds, blacklisting");
                    _peer.Blacklist();
                    return null;
                }
                foreach (var blockId in response.NextBlockIds)
                {
                    if (!(await _blockRepository.HasBlock(blockId)))
                    {
                        return commonBlockId;
                    }
                    commonBlockId = blockId;
                }
            }
        }
    }
}
