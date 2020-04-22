using System;
using System.Collections.Generic;
using System.Xml;
using System.Diagnostics;

using MNX.AGlobals;

namespace Moritz.Spec
{
	public abstract class BasicDurationDef
	{
		public BasicDurationDef(int msDuration)
		{
			//A.Assert(msDuration > 0, "msDuration out of range");
			if(msDuration <= 0)
			{
				throw new ApplicationException("msDuration out of range");
			}

			MsDuration = msDuration;
		}
		public int MsDuration { get; set; }
	}

	public class BasicMidiRestDef : BasicDurationDef
	{
		public BasicMidiRestDef(int msDuration)
			:base(msDuration)
		{
		}
	}

	public class BasicMidiChordDef : BasicDurationDef
    {
        public BasicMidiChordDef(int msDuration, byte? bank, byte? patch, bool hasChordOff, List<byte> pitches, List<byte> velocities)
			:base(msDuration)
        {
            BankIndex = bank;
            PatchIndex = patch;
            HasChordOff = hasChordOff;
            Pitches = new List<byte>(pitches);
            Velocities = new List<byte>(velocities);

            #region check values
            A.Assert(BankIndex == null || BankIndex == C.SetRange0_127((int)BankIndex), "Bank out of range.");
            A.Assert(PatchIndex == null || PatchIndex == C.SetRange0_127((int)PatchIndex), "Patch out of range.");
            A.Assert(Pitches.Count == Velocities.Count, "There must be the same number of pitches and velocities.");
            foreach(byte pitch in Pitches)
                A.Assert(pitch >= 0 && pitch <= 127);
            foreach(byte velocity in Velocities)
                C.AssertIsVelocityValue(velocity);     
            #endregion
        }

		internal void AssertConsistency()
		{
			//A.Assert(Pitches != null && Pitches.Count > 0);
			//A.Assert(Velocities != null && Velocities.Count == Pitches.Count);
			if(Pitches == null || Pitches.Count == 0 || Velocities == null || Velocities.Count != Pitches.Count)
			{
				throw new ApplicationException();
			}
		}

		#region Inversion
		/// <summary>
		/// Creates a BasicMidiChordDef having the original base pitch,
		/// but in which the top-bottom order of the prime intervals is reversed. 
		/// Velocities remain in the same order, bottom to top. They are not inverted. 
		/// </summary>
		/// <returns></returns>
		public BasicMidiChordDef Inversion()
        {
            List<byte> pitches = Pitches; // default if Pitches.Count == 1

            if(Pitches.Count > 1)
            {
                List<byte> intervals = new List<byte>();

                for(int i = 1; i < Pitches.Count; ++i)
                {
                    intervals.Add((byte)(Pitches[i] - Pitches[i - 1]));
                }
                intervals.Reverse();
                pitches = new List<byte>() { Pitches[0] };
                for(int i = 0; i < intervals.Count; ++i)
                {
                    byte interval = intervals[i];
                    pitches.Add((byte)(pitches[pitches.Count - 1] + interval));
                }
            }

            BasicMidiChordDef invertedBMCD = new BasicMidiChordDef(MsDuration, BankIndex, PatchIndex, HasChordOff, pitches, Velocities);

            return invertedBMCD;
        }
        #endregion Inversion

        /// <summary>
        /// Writes a single moment element which may contain
        /// NoteOffs, bank, patch, pitchWheelDeviation, NoteOns.
        /// Moritz never writes SysEx messages.
        /// </summary>
        public void WriteSVG(XmlWriter w, int channel, CarryMsgs carryMsgs)
        {
            w.WriteStartElement("moment");
            w.WriteAttributeString("msDuration", MsDuration.ToString());

            if(carryMsgs.Count > 0)
            {
                carryMsgs.WriteSVG(w);
                carryMsgs.Clear();
            }

            if(carryMsgs.IsStartOfSwitches)
            {
                BankIndex = GetValue(BankIndex, 0);
                PatchIndex = GetValue(PatchIndex, 0);
                PitchWheelDeviation = GetValue(PitchWheelDeviation, 2);
                carryMsgs.IsStartOfSwitches = false;
            }

            if((BankIndex != null && BankIndex != carryMsgs.BankState)
            || (PatchIndex != null && PatchIndex != carryMsgs.PatchState)
            || (PitchWheelDeviation != null && PitchWheelDeviation != carryMsgs.PitchWheelDeviationState))
            {
                w.WriteStartElement("switches");
                if(BankIndex != null && BankIndex != carryMsgs.BankState)
                {
                    MidiMsg msg = new MidiMsg(C.CMD_CONTROL_CHANGE_0xB0 + channel, C.CTL_BANK_CHANGE_0, BankIndex);
                    msg.WriteSVG(w);
                    carryMsgs.BankState = (byte) BankIndex;
                }
                if(PatchIndex != null && PatchIndex != carryMsgs.PatchState)
                {
                    MidiMsg msg = new MidiMsg(C.CMD_PATCH_CHANGE_0xC0 + channel, (int)PatchIndex, null);
                    msg.WriteSVG(w);
                    carryMsgs.PatchState = (byte) PatchIndex;
                }
                if(PitchWheelDeviation != null && PitchWheelDeviation != carryMsgs.PitchWheelDeviationState)
                {
                    MidiMsg msg1 = new MidiMsg(C.CMD_CONTROL_CHANGE_0xB0 + channel, C.CTL_REGISTEREDPARAMETER_COARSE_101, C.SELECT_PITCHBEND_RANGE_0);
                    msg1.WriteSVG(w);
                    MidiMsg msg2 = new MidiMsg(C.CMD_CONTROL_CHANGE_0xB0 + channel, C.CTL_DATAENTRY_COARSE_6, PitchWheelDeviation);
                    msg2.WriteSVG(w);
                    carryMsgs.PitchWheelDeviationState = (byte) PitchWheelDeviation;
                }
                w.WriteEndElement(); // switches
            }

            if(Pitches != null)
            {
                A.Assert(Velocities != null && Pitches.Count == Velocities.Count);
                w.WriteStartElement("noteOns");
                int status = C.CMD_NOTE_ON_0x90 + channel; // NoteOn
                for(int i = 0; i < Pitches.Count; ++i)
                {
                    MidiMsg msg = new MidiMsg(status, Pitches[i], Velocities[i]);
                    msg.WriteSVG(w);
                }
                w.WriteEndElement(); // end of noteOns

                if(HasChordOff)
                {
                    status = C.CMD_NOTE_OFF_0x80 + channel;
                    int data2 = C.DEFAULT_NOTEOFF_VELOCITY_64;
                    foreach(byte pitch in Pitches)
                    {
                        carryMsgs.Add(new MidiMsg(status, pitch, data2));
                    }
                }
            }

            w.WriteEndElement(); // end of moment
        }

        /// <summary>
        /// If value is null return defaultValue, otherwise return value.
        /// </summary>
        private byte? GetValue(byte? value, byte defaultValue)
        {
            if(value == null)
            {
                value = defaultValue;
            }
            return value;
        }

        /// <summary>
        /// The argument contains a list of 12 velocity values (range [1..127] in order of absolute pitch.
        /// For example: If the MidiChordDef contains one or more C#s, they will be given velocity velocityPerAbsolutePitch[1].
        /// Middle-C is midi pitch 60 (60 % 12 == absolute pitch 0), middle-C# is midi pitch 61 (61 % 12 == absolute pitch 1), etc.
        /// </summary>
        /// <param name="velocityPerAbsolutePitch">A list of 12 velocity values (range [1..127] in order of absolute pitch</param>
        public void SetVelocityPerAbsolutePitch(IReadOnlyList<byte> velocityPerAbsolutePitch)
        {
            for(int pitchIndex = 0; pitchIndex < Pitches.Count; ++pitchIndex)
            {
                int absPitch = Pitches[pitchIndex] % 12;
                byte newVelocity = velocityPerAbsolutePitch[absPitch];
                C.AssertIsVelocityValue(newVelocity);
                Velocities[pitchIndex] = C.VelocityValue(newVelocity);
            }
        }

        /// <summary>
        /// Individual velocities will be set in the range 1..127
        /// </summary>
        /// <param name="factor"></param>
        internal void AdjustVelocities(double factor)
        {
            A.Assert(factor > 0.0);
            for(int i = 0; i < Velocities.Count; ++i)
            {
                byte velocity = (byte)Math.Round((Velocities[i] * factor));
                Velocities[i] = C.VelocityValue(velocity); 
            }
        }

		/// <summary>
		/// Velocities having originalVelocity are changed to newVelocity.
		/// Velocity values above originalVelocity are changed proportionally with max possible velocity at 127.
		/// Velocity values below originalVelocity are changed proportionally with min possible velocity at 1.
		/// </summary>
		/// <param name="factor">greater than 0</param>
		public void AdjustVelocities(byte originalVelocity, byte newVelocity)
		{
			C.AssertIsVelocityValue(originalVelocity);
			C.AssertIsVelocityValue(newVelocity);
			double upperFactor = ((double)127 - originalVelocity) / ((double)127 - newVelocity);
			double lowerFactor = ((double)newVelocity) / ((double)originalVelocity);
			for(int i = 0; i < Velocities.Count; i++)
			{
				byte oldVel = Velocities[i];
				byte newVel = 0;
				if(oldVel > originalVelocity)
				{
					newVel = (byte)(originalVelocity + Math.Round((oldVel - originalVelocity) * upperFactor));
				}
				else
				{
					newVel = (byte)(Math.Round(oldVel * lowerFactor));
				}
				newVel = C.VelocityValue(newVel);
				Velocities[i] = newVel;
			}
		}

        /// <summary>
        /// The arguments must both be in range [1..127].
        /// If the basicMidiChordDef contains more than 1 note (=velocity), the velocities of the root and top notes in the
        /// chord are set to the argument values, and the other velocities are interpolated linearly. 
        /// </summary>
        public void SetVerticalVelocityGradient(byte rootVelocity, byte topVelocity)
        {
            #region conditions
            C.AssertIsVelocityValue(rootVelocity);
            C.AssertIsVelocityValue(topVelocity);
            #endregion conditions
            
            if(Velocities.Count > 1)
            {
                double increment = (((double)(topVelocity - rootVelocity)) / (Velocities.Count - 1));
                double newVelocity = rootVelocity;
                for(int velocityIndex = 0; velocityIndex < Velocities.Count; ++velocityIndex)
                {
                    Velocities[velocityIndex] = C.VelocityValue((int)Math.Round(newVelocity));
                    newVelocity += increment;
                }
            }
        }

        public override string ToString() => $"BasicMidiChordDef: MsDuration={MsDuration.ToString()} BasePitch={Pitches[0]} ";

        public List<byte> Pitches = new List<byte>();
        public List<byte> Velocities = new List<byte>();
        public byte? BankIndex = null; // optional. If null, bank commands are not sent
        public byte? PatchIndex = null; // optional. If null, patch commands are not sent
        public byte? PitchWheelDeviation = null; // optional. If null, PitchWheelDeviation commands are not sent
        public bool HasChordOff = true;
	}
}
