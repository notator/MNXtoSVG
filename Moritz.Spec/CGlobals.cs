using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MNX.AGlobals;

namespace Moritz.Spec
{
    internal static class C
    {
        /// <summary>
        /// Fails if the argument is outside the range 1..127.
        /// </summary>
        public static void AssertIsVelocityValue(int velocity)
        {
            A.Assert(velocity >= 1 && velocity <= 127);
        }

        /// <summary>
        /// Throws an ApplicationException if condition is false,
        /// and displays a custom message.
        /// </summary>
        /// <param name="v"></param>
        public static void Assert(bool condition, string message)
        {
            if(condition == false)
            {
                throw new ApplicationException(message);
            }
        }
        /// <summary>
        /// Throws an ApplicationException if condition is false
        /// </summary>
        /// <param name="v"></param>
        public static void Assert(bool condition)
        {
            if(condition == false)
            {
                throw new ApplicationException($"Condition failed.");
            }
        }
        /// <summary>
        /// Used by AssertRange0_11(int) and AssertRange0_127(int)
        /// </summary>
        private static void AssertRange0_(int maxVal, int value)
        {
            if(value < 0 || value > maxVal)
            {
                throw new ApplicationException($"Value must be in range 0..{maxVal}");
            }
        }
        /// <summary>
        /// Used by AssertRange0_11(byte) and AssertRange0_127(byte)
        /// </summary>
        private static void AssertRange0_(int maxVal, byte value)
        {
            if(value < 0 || value > maxVal)
            {
                throw new ApplicationException($"Value must be in range 0..{maxVal}");
            }
        }
        /// <summary>
        /// Throws an ApplicationException if the argument is not in range 0..11. 
        /// </summary>
        public static void AssertRange0_11(int value)
        {
            AssertRange0_(11, value);
        }
        /// <summary>
        /// Throws an ApplicationException if the argument is not in range 0..11. 
        /// </summary>
        public static void AssertRange0_11(byte value)
        {
            AssertRange0_(11, value);
        }
        /// <summary>
        /// Throws an ApplicationException if the argument is not in range 0..127. 
        /// </summary>
        public static void AssertRange0_127(int value)
        {
            AssertRange0_(127, value);
        }
        /// <summary>
        /// Throws an ApplicationException if the argument is not in range 0..127. 
        /// </summary>
        public static void AssertRange0_127(byte value)
        {
            AssertRange0_(127, value);
        }
        /// <summary>
        /// Used by AssertRange0_11(int collection) and AssertRange0_127(int collection) 
        /// </summary>
        private static void AssertRange0_(int maxVal, ICollection<int> collection)
        {
            foreach(int value in collection)
            {
                if(value < 0 || value > maxVal)
                {
                    throw new ApplicationException($"Value must be in range 0..{maxVal}");
                }
            }
        }
        /// <summary>
        /// Used by AssertRange0_11(byte collection) and AssertRange0_127(byte collection) 
        /// </summary>
        private static void AssertRange0_(int maxVal, ICollection<byte> collection)
        {
            foreach(int value in collection)
            {
                if(value < 0 || value > maxVal)
                {
                    throw new ApplicationException($"Value must be in range 0..{maxVal}");
                }
            }
        }
        /// <summary>
        /// Throws an ApplicationException if an int in the argument is not in range 0..11. 
        /// </summary>
        public static void AssertRange0_11(ICollection<int> collection)
        {
            AssertRange0_(11, collection);
        }
        /// <summary>
        /// Throws an ApplicationException if a byte in the argument is not in range 0..11. 
        /// </summary>
        public static void AssertRange0_11(ICollection<byte> collection)
        {
            AssertRange0_(11, collection);
        }
        /// <summary>
        /// Throws an ApplicationException if an int in the argument is not in range 0..127. 
        /// </summary>
        public static void AssertRange0_127(ICollection<int> collection)
        {
            AssertRange0_(127, collection);
        }
        /// <summary>
        /// Throws an ApplicationException if a byte in the argument is not in range 0..127. 
        /// </summary>
        public static void AssertRange0_127(ICollection<byte> collection)
        {
            AssertRange0_(127, collection);
        }

        /// <summary>
        /// Returns the value argument as a byte, coerced to the range [0..127] 
        /// </summary>
        public static byte SetRange0_127(int value)
        {
            int rval;

            if(value > 127)
                rval = 127;
            else if(value < 0)
                rval = 0;
            else
                rval = value;

            return (byte)rval;
        }

        /// <summary>
        /// Returns the argument as a byte coerced to the range 1..127.
        /// </summary>
        public static byte VelocityValue(int velocity)
        {
            velocity = (velocity >= 1) ? velocity : 1;
            velocity = (velocity <= 127) ? velocity : 127;
            return (byte)velocity;
        }

        /// <summary>
        /// This function divides total into divisor parts, returning a List of ints whose:
        ///     * Count is equal to divisor.
        ///     * sum is exactly equal to total
        ///     * members are as equal as possible. 
        /// </summary>
        public static List<int> IntDivisionSizes(int total, int divisor)
        {
            List<int> relativeSizes = new List<int>();
            for(int i = 0; i < divisor; ++i)
            {
                relativeSizes.Add(1);
            }
            return IntDivisionSizes(total, relativeSizes);

        }

        public static List<int> IntDivisionSizes(int total, List<int> relativeSizes)
        {
            int divisor = relativeSizes.Count;
            int sumRelative = 0;
            for(int i = 0; i < divisor; ++i)
            {
                sumRelative += relativeSizes[i];
            }
            float factor = ((float)total / (float)sumRelative);
            float fPos = 0;
            List<int> intPositions = new List<int>();
            for(int i = 0; i < divisor; ++i)
            {
                intPositions.Add((int)(Math.Floor(fPos)));
                fPos += (relativeSizes[i] * factor);
            }
            intPositions.Add((int)Math.Floor(fPos));

            List<int> intDivisionSizes = new List<int>();
            for(int i = 0; i < divisor; ++i)
            {
                int intDuration = (int)(intPositions[i + 1] - intPositions[i]);
                intDivisionSizes.Add(intDuration);
            }

            int intSum = 0;
            foreach(int i in intDivisionSizes)
            {
                //A.Assert(i >= 0);
                if(i < 0)
                {
                    throw new ApplicationException();
                }
                intSum += i;
            }
            A.Assert(intSum <= total);
            if(intSum < total)
            {
                int lastDuration = intDivisionSizes[intDivisionSizes.Count - 1];
                lastDuration += (total - intSum);
                intDivisionSizes.RemoveAt(intDivisionSizes.Count - 1);
                intDivisionSizes.Add(lastDuration);
            }
            return intDivisionSizes;
        }

        #region MIDI
        /// <summary>
        /// Commands
        /// </summary>
        public static readonly int CMD_NOTE_OFF_0x80 = 0x80;
        public static readonly int CMD_NOTE_ON_0x90 = 0x90;
        public static readonly int CMD_AFTERTOUCH_0xA0 = 0xA0;
        public static readonly int CMD_CONTROL_CHANGE_0xB0 = 0xB0;
        public static readonly int CMD_PATCH_CHANGE_0xC0 = 0xC0;
        public static readonly int CMD_CHANNEL_PRESSURE_0xD0 = 0xD0;
        public static readonly int CMD_PITCH_WHEEL_0xE0 = 0xE0;

        /// <summary>
        /// Control Numbers (These are just the ones Moritz uses.)
        /// </summary>
        public static readonly int CTL_BANK_CHANGE_0 = 0;
        public static readonly int CTL_MODWHEEL_1 = 1;
        public static readonly int CTL_PAN_10 = 10;
        public static readonly int CTL_EXPRESSION_11 = 11;
        public static readonly int CTL_REGISTEREDPARAMETER_COARSE_101 = 101;
        public static readonly int CTL_REGISTEREDPARAMETER_FINE_100 = 100;
        public static readonly int CTL_DATAENTRY_COARSE_6 = 6;
        public static readonly int CTL_DATAENTRY_FINE_38 = 38;
        public static readonly int SELECT_PITCHBEND_RANGE_0 = 0;
        public static readonly int DEFAULT_NOTEOFF_VELOCITY_64 = 64;
        /// <summary>
        /// The following values are (supposed to be) set by AllControllersOff.
        /// </summary>
        public static readonly byte DEFAULT_BANKAndPATCH_0 = 0;
        public static readonly byte DEFAULT_VOLUME_100 = 100;
        public static readonly byte DEFAULT_EXPRESSION_127 = 127;
        public static readonly byte DEFAULT_PITCHWHEELDEVIATION_2 = 2;
        public static readonly byte DEFAULT_PITCHWHEEL_64 = 64;
        public static readonly byte DEFAULT_PAN_64 = 64;
        public static readonly byte DEFAULT_MODWHEEL_0 = 0;
        /// <summary>
        /// Constants that are set when palette fields are empty
        /// </summary>
        public static readonly bool DEFAULT_HAS_CHORDOFF_true = true;
        public static readonly bool DEFAULT_CHORDREPEATS_false = false;
        public static readonly int DEFAULT_ORNAMENT_MINIMUMDURATION_1 = 1;

        /// <summary>
        /// Called in the above constructor
        /// </summary>
        private static void SetMidiPitchDict()
        {
            string[] alphabet = { "C", "D", "E", "F", "G", "A", "B" };
            int midiPitch = 0;
            for(int octave = 0; octave < 11; octave++)
            {
                foreach(string letter in alphabet)
                {
                    switch(letter)
                    {
                        case "A":
                            midiPitch = 9;
                            break;
                        case "B":
                            midiPitch = 11;
                            break;
                        case "C":
                            midiPitch = 0;
                            break;
                        case "D":
                            midiPitch = 2;
                            break;
                        case "E":
                            midiPitch = 4;
                            break;
                        case "F":
                            midiPitch = 5;
                            break;
                        case "G":
                            midiPitch = 7;
                            break;
                    }

                    midiPitch += (octave * 12); // C5 (middle letter) is 60
                    if(midiPitch >= 0 && midiPitch <= 119)
                    {
                        string pitchString = letter + octave.ToString();
                        MidiPitchDict.Add(pitchString, midiPitch);
                    }
                    else if(midiPitch > 119 && midiPitch < 128)
                    {
                        string pitchString = letter + ":";
                        MidiPitchDict.Add(pitchString, midiPitch);
                    }
                }
            }
        }
        /// <summary>
        /// capella pitch strings and their equivalent midi pitch numbers.
        /// the strings are absolute diatonic pitch. middle C (c'): "C5"
        /// Chromatic pitches are found using the head.alteration field (-2..+2)
        /// This dictionary is
        /// </summary>
        static public Dictionary<string, int> MidiPitchDict = new Dictionary<string, int>();

        #endregion MIDI
    }
}
