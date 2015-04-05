using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using Nxt.NET.Block;
using Nxt.NET.Transaction;

namespace Nxt.NETTest.Block
{
    [TestClass]
    public class BlockParserTest
    {
        private BlockParser _blockParser;
        private Mock<ITransactionParser> _transactionParserMock;
        private Mock<IBlockFactory> _blockFactoryMock;
        private const string EmptyJson = "{}";

        // First actual block after the genesis block
        private const string OneBlockJson =
            "{" +
                "\"previousBlock\": \"2680262203532249785\"," +
                "\"timestamp\": 793," +
                "\"transactions\": []," +
                "\"payloadHash\": \"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\"," +
                "\"totalAmountNQT\": 0," +
                "\"blockSignature\": \"f518e6badb70dececcc9f3da1cf149c602ce4a2c21cdece152c066a205cf630ba177b42da1be6fbd757ff728eec11c7f3731c61d2612df2914ace06ad1a2422f\"," +
                "\"totalFeeNQT\": 0," +
                "\"generationSignature\": \"2195f938e5c94cb4a42197d1cfb6a6465949bc1678d8d294d1935df2ed6df80d8a83df15b6a222a905fcf286365cd4e9baeca95c3d4ad64b9b430e4a076f97b5\"," +
                "\"payloadLength\": 0," +
                "\"generatorPublicKey\": \"015772aead6002e8ca65663df03d90daee0e3c4d3cecc637ae34fb273fc2fb55\"," +
                "\"version\": 1" +
            "}";

        [TestInitialize]
        public void TestInit()
        {
            _transactionParserMock = new Mock<ITransactionParser>();
            _blockFactoryMock = new Mock<IBlockFactory>();

            _blockParser = new BlockParser(_blockFactoryMock.Object, _transactionParserMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ParsingEmptyJsonShouldThrowException()
        {
            _blockParser.ParseBlock(JObject.Parse(EmptyJson));
        }

        [TestMethod]
        public void ParsingOneBlockShouldReturnOneBlock()
        {
            var expectedTransactions = new List<ITransaction>();
            _transactionParserMock.Setup(tp => tp.ParseTransactions(It.IsAny<JToken>()))
                .Returns(expectedTransactions);

            _blockParser.ParseBlock(JObject.Parse(OneBlockJson));

            _blockFactoryMock.Verify(
                bf =>
                    bf.Create(
                        It.Is<int>(version => version == 1),
                        It.Is<int>(timestamp => timestamp == 793),
                        It.Is<long?>(previousBlockId => previousBlockId == 2680262203532249785L),
                        It.Is<long>(totalAmount => totalAmount == 0),
                        It.Is<long>(totalFee => totalFee == 0),
                        It.Is<int>(payloadLength => payloadLength == 0),
                        It.Is<byte[]>(payloadHash => payloadHash.Length == 32 && payloadHash[0] == 227),
                        It.Is<byte[]>(generatorPublicKey => generatorPublicKey.Length == 32 && generatorPublicKey[0] == 1),
                        It.Is<byte[]>(generationSignature => generationSignature.Length == 64 && generationSignature[0] == 33),
                        It.Is<byte[]>(blockSignature => blockSignature.Length == 64 && blockSignature[0] == 245),
                        It.Is<byte[]>(previousBlockHash => previousBlockHash == null),
                        It.Is<IList<ITransaction>>(transactions => transactions == expectedTransactions)));
        }
    }
}
