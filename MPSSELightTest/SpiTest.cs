using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPSSELight;
using System.Linq;

namespace MPSSELightTest
{
    [TestClass]
    public class SpiTest
    {
        [TestMethod]
        public void OpenCloseTest()
        {
            using (MpsseDevice mpsse = new FT2232H(""))
            {
                SpiDevice spi = new SpiDevice(mpsse);
            }
        }

        [TestMethod]
        public void LoopbackTest()
        {
            using (MpsseDevice mpsse = new FT2232H(""))
            {
                SpiDevice spi = new SpiDevice(mpsse);
                mpsse.Loopback = true;

                byte[] tData = { 0x0A, 0x01, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0xFF };
                byte[] rData = spi.readWrite(tData);

                Assert.IsTrue(tData.SequenceEqual(rData));
            }
        }

        [TestMethod]
        public void TransmitTest()
        {
            using (MpsseDevice mpsse = new FT2232H(""))
            {
                SpiDevice spi = new SpiDevice(mpsse);

                byte[] tData = { 0x0D, 0x01, 0x0F };
                spi.write(tData);
            }
        }

        [TestMethod]
        public void LoopbackBigTest()
        {
            Random r = new Random();
            MpsseDevice.MpsseParams mpsseParams = new MpsseDevice.MpsseParams();
            mpsseParams.clockDevisor = 0;
            //mpsseParams.DataWriteEvent = new FtdiDevice.DataTransferEvent();
            const uint size = 60000;

            using (MpsseDevice mpsse = new FT2232H("", mpsseParams))
            {
                SpiDevice spi = new SpiDevice(mpsse);
                mpsse.Loopback = true;

                byte[] tData = new byte[size];
                r.NextBytes(tData);

                byte[] rData = spi.readWrite(tData);

                Assert.IsTrue(tData.SequenceEqual(rData));
            }
        }
        [TestMethod]
        public void Read()
        {
            Random r = new Random();

            const uint size = 60000;
            using (MpsseDevice mpsse = new FT2232H(""))
            {
                SpiDevice spi = new SpiDevice(mpsse);
                mpsse.Loopback = true;

                byte[] rData = mpsse.read();

                Assert.AreNotEqual(rData.Length, 0);
            }
        }

    }
}
