using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.Media
{
    /// <summary>
    /// This class holds audio format information.
    /// </summary>
    public class AudioFormat
    {
        public static readonly AudioFormat PCMU = new AudioFormat("PCMU",0,0,1,1,0);

        public static readonly AudioFormat PCMA = new AudioFormat("PCMA",0,0,1,1,0);

        public AudioFormat(string encoding,int sampleRate,int bitsPerSample,int channels,int frameSize,float frameRate)
        {
        }


        #region Properties implementatation

        public string Encoding
        {
            get{ return ""; }
        }

        /// <summary>
        /// Gets audio samples per second(Hz).
        /// </summary>
        public int SampleRate
        {
            get{ return 0; }
        }

        /// <summary>
        /// Gets number of bites per sample.
        /// </summary>
        public int BitsPerSample
        {
            get{ return 0; }
        }

        /// <summary>
        /// Gets number of audio channels.
        /// </summary>
        public int Channels
        {
            get{ return 0; }
        }

        public int FrameSize
        {
            get{ return 0; }
        }

        public int FrameRate
        {
            get{ return 0; }
        }

        #endregion
    }
}
