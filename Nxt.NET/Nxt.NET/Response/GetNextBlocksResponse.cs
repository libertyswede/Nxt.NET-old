using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json.Linq;
using NLog;
using Nxt.NET.Block;

namespace Nxt.NET.Response
{
    public interface IGetNextBlocksResponse
    {
        IReadOnlyCollection<IBlock> Blocks { get; }
    }

    public class GetNextBlocksResponse : IGetNextBlocksResponse
    {
        private readonly IBlockParser _blockParser;
        public IReadOnlyCollection<IBlock> Blocks { get; private set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public GetNextBlocksResponse(IBlockParser blockParser, string json)
        {
            _blockParser = blockParser;
            Parse(json);
        }

        private void Parse(string json)
        {
            var blockList = new List<IBlock>();
            var blocksToken = JObject.Parse(json).SelectToken("nextBlocks");
            var currentBlock = blocksToken.FirstOrDefault();

            while (currentBlock != null)
            {
                blockList.Add(_blockParser.ParseBlock(currentBlock));
                currentBlock = currentBlock.Next;
            }
            Blocks = new ReadOnlyCollection<IBlock>(blockList);
        }
    }
}
