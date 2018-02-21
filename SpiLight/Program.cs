/* The MIT License (MIT)

Copyright(c) 2016 Stanislav Zhelnio

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPSSELight;
using System.Threading;
using Fclp;
using System.Diagnostics;
using System.IO;
using SGX.Data.DataClasses.SPOC;

namespace SpiLight
{
    class Program
    {
        class ReadInfo
        {
            public SpocReaderData readerData;
            public MpsseDevice.MpsseParams parameters;
            public UInt16 numOfBytesToRead;
        }

        static void Main(string[] args)
        {
            try
            {

                for (int repeat = 1; repeat < 11; repeat++)
                {
                    SpocReaderData spocReaderData = new SpocReaderData();
                    List<byte> RawData = new List<byte>();
                    MpsseDevice.MpsseParams mpsseParams = new MpsseDevice.MpsseParams();
                    mpsseParams.clockDevisor = 1;
                    UInt16 NumOfBytesToRead = 6500;                    


                    String csvFilename = "ReadData_" + DateTime.Now.ToString("ddMMMyy--HH.mm.ss") + ".csv";
                    String xmlFilename = "ReadData_" + DateTime.Now.ToString("ddMMMyy--HH.mm.ss") + ".xml";

                    using (var mpsse = new FT2232H("", mpsseParams))
                    {

                        // configure FTDI port for MPSSE Use
                        mpsse.AdaptiveClocking = false;
                        mpsse.ThreePhaseDataClocking = false;
                        mpsse.ClkDivideBy5 = false;

                        Stopwatch stopWatch = new Stopwatch();
                        stopWatch.Start();
                        
                        while (stopWatch.ElapsedMilliseconds < 5000)
                        {
                            EnableLine(mpsse);
                            var OutputData = mpsse.BytesInOnMinusEdgeWithMsbFirst(NumOfBytesToRead);
                            DisableLine(mpsse);

                            Console.Write(".");
                            RawData.AddRange(OutputData);
                        }                        

                        int HighBit = 30;
                        List<List<BinData>> ReadBuffer = TruncateData(RawData, HighBit);

                        spocReaderData.RawFocusData = RawData.ToArray();

                        // write to files
                        ReadInfo readInfo = new ReadInfo { readerData = spocReaderData, parameters = mpsseParams, numOfBytesToRead = NumOfBytesToRead };
                        //WriteToCsv(ReadBuffer[0], csvFilename);
                        WriteToXML(readInfo, xmlFilename);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        static private void WriteToCsv(List<BinData> ReadBuffer, String fileName)
        {
            string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ReadData");
            Directory.CreateDirectory(directoryPath);
            var filePath = Path.Combine(directoryPath, fileName);
            string delimiter = ",";
            // write the whole array to file                        
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(filePath))
            {
                for (int i = 1; i < ReadBuffer.Count; i++)
                {
                    int difference = (int)ReadBuffer[i].Word - (int)ReadBuffer[i - 1].Word;

                    String data = BitConverter.ToString(ReadBuffer[i].ByteArray.ToArray());
                    file.Write(data + delimiter + ReadBuffer[i].Word + delimiter);
                    file.Write(difference);
                    file.Write("\n");
                }
            }
        }

        static private void WriteToXML(ReadInfo readInfo, string fileName)
        {
            var spocReaderData = readInfo.readerData;
            if (spocReaderData == null)
                return;

            string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ReadData");
            Directory.CreateDirectory(directoryPath);
            var filePath = Path.Combine(directoryPath, fileName);

            //Dummy data
            spocReaderData.Deprime = 1.ToString();
            spocReaderData.EPPrime = 2.ToString();
            spocReaderData.RetestWell = 3.ToString();
            spocReaderData.ClockDevisor = readInfo.parameters.clockDevisor;
            spocReaderData.NumOfBytesToRead = readInfo.numOfBytesToRead;

            //End of dummy code

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(spocReaderData.GetType());
                xmlSerializer.Serialize(fs, spocReaderData);
            }
        }

        private static void DisableLine(FT2232H mpsse)
        {
            FtdiPin PinDirection = (FtdiPin)(FtdiPin.TCK | FtdiPin.TDI | FtdiPin.TMS | FtdiPin.GPIOL0 | FtdiPin.GPIOL1);
            FtdiPin PinState = (FtdiPin)(FtdiPin.CS | FtdiPin.GPIOL0 | FtdiPin.GPIOL1);
            mpsse.SetDataBitsLowByte(PinState, PinDirection);
        }

        private static void EnableLine(MpsseDevice mpsse)
        {
            FtdiPin PinState = FtdiPin.GPIOL0 | FtdiPin.GPIOL1;
            FtdiPin PinDirection = (FtdiPin)(FtdiPin.TCK | FtdiPin.TDI | FtdiPin.TMS | FtdiPin.GPIOL0 | FtdiPin.GPIOL1);
            mpsse.SetDataBitsLowByte(PinState, PinDirection);
        }

        static void rawInputToScreen(byte[] data)
        {
            if (data.Length > 0)
                writeToScreen("raw input:  ", data);
            else
                writeToScreen("raw input:  ", "nothing");
        }

        static void rawOutputToScreen(byte[] data)
        {
            writeToScreen("raw output: ", data);
        }

        static byte[] readBinaryFile(string fileName)
        {
            return File.ReadAllBytes(fileName);
        }

        static byte[] readTextFile(string fileName)
        {
            string hex = File.ReadAllText(fileName);
            return StringToByteArray(hex);
        }

        static void writeToScreen(string header, byte[] data)
        {
            Console.Write(header);
            string hex = BitConverter.ToString(data).Replace("-", "");
            Console.WriteLine(hex);
        }

        static void writeToScreen(string header, string data)
        {
            Console.WriteLine(header + data);
        }

        static void writeToBinary(string fileName, byte[] data)
        {
            File.WriteAllBytes(fileName, data);
        }

        static void writeToText(string fileName, byte[] data)
        {
            string hex = BitConverter.ToString(data).Replace("-", "");
            File.WriteAllText(fileName, hex);
        }

        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        static List<List<BinData>> TruncateData(List<byte> RawData, int HighBit)
        {


            var outputData = RawData;
            //var temp = new List<UInt32>(outputData.Count / 4);
            var rawOutput = new List<List<BinData>>();
            List<int> readSize = new List<int>();
            List<BinData> binData = new List<BinData>();
            bool continuous = false;
            for (int wordIndex = 0, byteIndex = 3; byteIndex < outputData.Count; wordIndex++, byteIndex += 4)
            {

                List<byte> temp = outputData.GetRange(byteIndex - 3, 4);
                //UInt32 currentValue = ByteArrayToWord(temp);
                var currentValue = new BinData { Word = ByteArrayToWord(outputData.GetRange(byteIndex - 3, 4)), ByteArray = outputData.GetRange(byteIndex - 3, 4) };
                if (!WordCheck(temp))
                {
                    int alignmentShift = 0;
                    for (int shift = 1; shift < 6 && byteIndex - 3 + shift < outputData.Count; shift++)
                    {
                        temp = outputData.GetRange(byteIndex - 3 + shift, 4);
                        if (WordCheck(temp))
                        {
                            alignmentShift = shift;
                            break;
                        }
                    }

                    byteIndex += alignmentShift;
                    currentValue = new BinData { Word = ByteArrayToWord(outputData.GetRange(byteIndex - 3, 4)), ByteArray = outputData.GetRange(byteIndex - 3, 4) };
                }

                if ((currentValue.Word & 1 << HighBit) > 0)
                {
                    if (continuous)
                    {
                        rawOutput[rawOutput.Count - 1].Add(currentValue);
                    }
                    else
                    {
                        continuous = true;
                        // readSize.Add(1);
                        rawOutput.Add(new List<BinData>());
                        rawOutput[rawOutput.Count - 1].Add(currentValue);
                    }
                }
                else
                {
                    continuous = false;
                }

            }
            return rawOutput;
        }

        static UInt32 ByteArrayToWord(List<byte> byteArray)
        {
            UInt32 temp = (UInt32)byteArray[0] << 24 |
                (UInt32)byteArray[1] << 16 |
                (UInt32)byteArray[2] << 8 |
                (UInt32)byteArray[3];

            temp = (temp & 0xFFFFFF00) | ((temp & 0x7F) << 1);
            temp = (temp & 0xFFFF0000) | ((temp & 0x7FFF) << 1);
            temp = (temp & 0xFF000000) | ((temp & 0x7FFFFF) << 1);
            temp = (temp & 0x40000000) | ((temp & 0x3FFFFFFF) >> 3);
            return temp;
        }

        static bool WordCheck(List<byte> byteArray)
        {
            return (byteArray[0] & 0x80) > 0 &&
                (byteArray[1] ^ 0x80) > 0 &&
                (byteArray[2] ^ 0x80) > 0 &&
                (byteArray[3] ^ 0x80) > 0;
        }
    }

    class BinData
    {
        public List<byte> ByteArray;
        public UInt32 Word;
    }
}
