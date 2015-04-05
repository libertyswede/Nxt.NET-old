using System.Collections.Generic;
using System.Collections.ObjectModel;
using Moq;
using Nxt.NET;
using Nxt.NET.Block;
using Nxt.NET.Peer;
using Nxt.NET.Request;
using Nxt.NET.Response;

namespace Nxt.NETTest
{
    public class PeerTestHelper
    {
        public Mock<IPeer> PeerMock { get; private set; }
        public Mock<IGetCumulativeDifficultyResponse> CumulativeDifficultyMock { get; private set; }
        public Mock<IGetNextBlockIdsResponse> NextBlockIdsMock { get; private set; }
        public Mock<IGetInfoResponse> InfoMock { get; private set; }
        public Mock<IGetMilestoneBlockIdsResponse> MilestoneBlockIdsMock { get; private set; }
        public Mock<IGetNextBlocksResponse> NextBlocksMock { get; private set; }
        public Mock<IGetPeersResponse> PeersResponseMock { get; private set; }
        public Mock<IGetUnconfirmedTransactionsResponse> UnconfirmedTransactionsMock { get; private set; }

        public PeerTestHelper()
        {
            PeerMock = new Mock<IPeer>();
            CumulativeDifficultyMock = new Mock<IGetCumulativeDifficultyResponse>();
            NextBlockIdsMock = new Mock<IGetNextBlockIdsResponse>();
            InfoMock = new Mock<IGetInfoResponse>();
            MilestoneBlockIdsMock = new Mock<IGetMilestoneBlockIdsResponse>();
            NextBlocksMock = new Mock<IGetNextBlocksResponse>();
            PeersResponseMock = new Mock<IGetPeersResponse>();
            UnconfirmedTransactionsMock = new Mock<IGetUnconfirmedTransactionsResponse>();

            PeerMock.Setup(p => p.SendRequest(It.IsAny<GetCumulativeDifficultyRequest>()))
                .ReturnsAsync(CumulativeDifficultyMock.Object);
            PeerMock.Setup(p => p.SendRequest(It.IsAny<GetNextBlockIdsRequest>()))
                .ReturnsAsync(NextBlockIdsMock.Object);
            PeerMock.Setup(p => p.SendRequest(It.IsAny<GetInfoRequest>()))
                .ReturnsAsync(InfoMock.Object);
            PeerMock.Setup(p => p.SendRequest(It.IsAny<GetMilestoneBlockIdsRequestBase>()))
                .ReturnsAsync(MilestoneBlockIdsMock.Object);
            PeerMock.Setup(p => p.SendRequest(It.IsAny<GetNextBlocksRequest>()))
                .ReturnsAsync(NextBlocksMock.Object);
            PeerMock.Setup(p => p.SendRequest(It.IsAny<GetPeersRequest>()))
                .ReturnsAsync(PeersResponseMock.Object);
            PeerMock.Setup(p => p.SendRequest(It.IsAny<GetUnconfirmedTransactionsRequest>()))
                .ReturnsAsync(UnconfirmedTransactionsMock.Object);
        }

        public void SetupNextBlocks(long blockId, IList<IBlock> nextBlocks)
        {
            var responseMock = new Mock<IGetNextBlocksResponse>();
            responseMock.SetupGet(r => r.Blocks).Returns(new ReadOnlyCollection<IBlock>(nextBlocks));
            PeerMock.Setup(p => p.SendRequest(It.Is<GetNextBlocksRequest>(r => (long)r.BlockId == blockId)))
                .ReturnsAsync(responseMock.Object);
        }
    }
}