using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SGX.Data.DataClasses.SPOC
{
    public class BinData
    {
        public uint Intensity;
        public double Microns;
    }

    public class SpocReaderData
    {
        private byte[] _rawfocusdata;
        private List<BinData> _focusdata;
        private List<BinData> _readdata;
        private byte[] _rawreaddata;
        private string _focusEncodedMicron;
        private string _rawfocusdataencoded;
        public int NumOfBytesToRead;
        public int ClockDevisor;

        public int FocusStartPosition { get; set; }

        [XmlIgnore]
        public byte[] RawReadData
        {
            get { return this._rawreaddata; }
            set
            {
                this._rawreaddata = value;
                this.RawReadDataEncoded = Convert.ToBase64String(_rawreaddata);
            }
        }
        [XmlIgnore]
        public byte[] RawFocusData
        {
            get { return this._rawfocusdata; }
            set
            {
                this._rawfocusdata = value;
                this.RawFocusDataEncoded = Convert.ToBase64String(_rawfocusdata);
            }
        }
        [XmlIgnore]
        public List<BinData> FocusData
        {
            get { return this._focusdata; }
            set
            {
                this._focusdata = value;
                var tempIntensity = new byte[this.FocusData.Count() * 4];


                var tempMicrons = new List<byte>();
                var microns = FocusData.Select(data => data.Microns).ToList();
                foreach (var item in microns)
                    tempMicrons.AddRange(BitConverter.GetBytes(item));

                for (int i = 1; i < this.FocusData.Count + 1; i++)
                {
                    tempIntensity[i * 4 - 4] = (byte)(this.FocusData[i - 1].Intensity >> 24);
                    tempIntensity[i * 4 - 3] = (byte)(this.FocusData[i - 1].Intensity >> 16);
                    tempIntensity[i * 4 - 2] = (byte)(this.FocusData[i - 1].Intensity >> 8);
                    tempIntensity[i * 4 - 1] = (byte)(this.FocusData[i - 1].Intensity);
                }
                this.FocusEncodedIntensity = Convert.ToBase64String(tempIntensity);
                this._focusEncodedMicron = Convert.ToBase64String(tempMicrons.ToArray());

            }
        }
        [XmlIgnore]
        public List<BinData> ReadData
        {
            get { return this._readdata; }
            set
            {
                this._readdata = value;
                var tempIntensity = new byte[this.ReadData.Count() * 4];

                var tempMicrons = new List<byte>();
                var microns = ReadData.Select(data => data.Microns).ToList();
                foreach (var item in microns)
                    tempMicrons.AddRange(BitConverter.GetBytes(item));

                for (int i = 1; i < this.ReadData.Count + 1; i++)
                {
                    tempIntensity[i * 4 - 4] = (byte)(this.ReadData[i - 1].Intensity >> 24);
                    tempIntensity[i * 4 - 3] = (byte)(this.ReadData[i - 1].Intensity >> 16);
                    tempIntensity[i * 4 - 2] = (byte)(this.ReadData[i - 1].Intensity >> 8);
                    tempIntensity[i * 4 - 1] = (byte)(this.ReadData[i - 1].Intensity);

                }

                this.ReadEncodedIntensity = Convert.ToBase64String(tempIntensity);
                this.ReadEncodedMicron = Convert.ToBase64String(tempMicrons.ToArray());

            }
        }
        public string RawReadDataEncoded { get; set; }
        public String RawFocusDataEncoded
        {
            get
            {
                return this._rawfocusdataencoded;
            }
            set
            {
                this._rawfocusdataencoded = value;

                //Decode the encoded data                 
                _rawfocusdata = Convert.FromBase64String(value);
            }
        }
        public String ReadEncodedIntensity { get; set; }

        public String FocusEncodedIntensity
        {
            get; set;
            /*    get
                {
                    return this._focusEncodedIntensity;
                }
                set
                {
                    this._focusEncodedIntensity = value;

                    //Decode the encoded data 
                    byte[] decodedFocusDataIntensity = Convert.FromBase64String(value);

                    byte[] decodedFocusDataMicrons = Convert.FromBase64String(FocusEncodedMicron);
                    var tempMicron = new double[decodedFocusDataMicrons.Count() / 8];

                    //Format the data to restore in FocusData 
                    var tempIntensity = new uint[decodedFocusDataIntensity.Count() / 4];

                    //putting back to the focusData
                    for (int i = 1; i < tempIntensity.Count(); i++)
                    {
                        tempIntensity[i - 1] = (uint)decodedFocusDataIntensity[i * 4 - 4] << 24 |
                            (uint)decodedFocusDataIntensity[i * 4 - 3] << 16 |
                            (uint)decodedFocusDataIntensity[i * 4 - 2] << 8 |
                            (uint)decodedFocusDataIntensity[i * 4 - 1];

                        tempMicron[i - 1] = BitConverter.ToDouble(decodedFocusDataMicrons, 8 * i);
                    }

                    _focusdata = new List<BinData>();
                    for (int i = 0; i < tempIntensity.Count(); i++)
                        _focusdata.Add(new BinData { Intensity = tempIntensity[i], Microns = tempMicron[i] });

                }*/
        }

        public String ReadEncodedMicron { get; set; }

        public String FocusEncodedMicron
        {
            get
            {
                return this._focusEncodedMicron;
            }
            set
            {
                this._focusEncodedMicron = value;

                //Decode the encoded data 
                byte[] decodedFocusDataIntensity = Convert.FromBase64String(FocusEncodedIntensity);

                byte[] decodedFocusDataMicrons = Convert.FromBase64String(value);
                var tempMicron = new double[decodedFocusDataMicrons.Count() / 8];

                //Format the data to restore in FocusData 
                var tempIntensity = new uint[decodedFocusDataIntensity.Count() / 4];

                //putting back to the focusData
                for (int i = 1; i < tempIntensity.Count(); i++)
                {
                    tempIntensity[i - 1] = (uint)decodedFocusDataIntensity[i * 4 - 4] << 24 |
                        (uint)decodedFocusDataIntensity[i * 4 - 3] << 16 |
                        (uint)decodedFocusDataIntensity[i * 4 - 2] << 8 |
                        (uint)decodedFocusDataIntensity[i * 4 - 1];

                    tempMicron[i - 1] = BitConverter.ToDouble(decodedFocusDataMicrons, 8 * i);
                }

                _focusdata = new List<BinData>();
                for (int i = 0; i < tempIntensity.Count(); i++)
                    _focusdata.Add(new BinData { Intensity = tempIntensity[i], Microns = tempMicron[i] });

            }
        }

        public string Deprime { get; set; }
        public string EPPrime { get; set; }
        public String FileName { get; set; }

        public string RetestWell { get; set; }
    }
}
