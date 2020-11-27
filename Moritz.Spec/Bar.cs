using System;
using System.Collections.Generic;
using System.Diagnostics;

using MNX.Globals;

namespace Moritz.Spec
{
	public class Bar : ITrksContainer
    {
		/// <summary>
		/// All constructors in this class are protected, so Bars can only be created by subclasses.
		/// <para>A Bar contains a list of voiceDefs that are all Trks. A Seq only contains Trks.
		/// Bars do not contain barlines. They are implicit, at the beginning and end of the Bar. 
		/// This constructor uses its arguments' voiceDefs directly, so, if the arguments need to be used again, pass a clone.</para>
		/// <para>Seq.AssertConsistency must succeed.</para>
		/// <para>The Bar's AbsMsPosition is set to the seq's AbsMsPosition. If initialClefPerChannel is not null, the initial ClefDef is
		/// inserted at the beginning of each voice.</para>
		/// <para>When complete, this constructor calls the bar.AssertConsistency() function (see that its documentation).</para>
		/// </summary>
		/// <param name="seq">Cannot be null, and must have Trks</param>
		public Bar(Seq seq, Tuple<bool, bool> repeatBarlineTypes)
        {
			#region conditions
            seq.AssertConsistency();
			#endregion

			AbsMsPosition = seq.AbsMsPosition;

			int clefIndex = 0;
			int msDuration = seq.MsDuration;
			foreach(Trk trk in seq.Trks)
            {
                trk.Container = this;
				M.Assert(trk.MsDuration == msDuration); // cannot be 0 here.
                _voiceDefs.Add(trk);
				clefIndex++;
            }

            AssertConsistency();

            RepeatBeginBarline = repeatBarlineTypes.Item1;
            RepeatEndBarline = repeatBarlineTypes.Item2;
        }

		public void Concat(Bar bar2)
        {
            M.Assert(_voiceDefs.Count == bar2._voiceDefs.Count);

            for(int i = 0; i < _voiceDefs.Count; ++i)
            {
                VoiceDef vd1 = _voiceDefs[i];
                VoiceDef vd2 = bar2._voiceDefs[i];

                vd1.Container = null;
                vd2.Container = null;
                vd1.AddRange(vd2);
                vd1.RemoveDuplicateClefDefs();
                vd1.AgglomerateRests();
                vd1.Container = this;
            }

            AssertConsistency();
        }


		/// <summary>
		/// Trk.AssertConsistency() is called on each VoiceDef (each VoiceDef is a Trk).
		/// Then the following checks ae also made:
		/// <para>1. The first VoiceDef in a Bar must be a Trk.</para>
		/// <para>2. All voiceDefs have the same MsDuration.</para>
		/// <para>3. At least one Trk must start with a MidiChordDef, possibly preceded by a ClefDef.</para>
		/// </summary> 
		public virtual void AssertConsistency()
        {
			#region trk consistent in bar
			foreach(VoiceDef voiceDef in _voiceDefs)
			{
				if(voiceDef is Trk trk)
				{
					trk.AssertConsistency();
				}
				else
				{
					M.Assert(false, "Type error.");
				}
			}
			#endregion

			int barMsDuration = MsDuration;

			#region 1. The first VoiceDef in a Bar must be a Trk.
			M.Assert(_voiceDefs[0] is Trk, "The first VoiceDef in a Bar must be a Trk.");
			#endregion

			#region 2. All voiceDefs have the same MsDuration.
			foreach(VoiceDef voiceDef in _voiceDefs)
			{
				M.Assert(voiceDef.MsDuration == barMsDuration, "All Trks in a block must have the same duration.");
			}
			#endregion

			#region 3. At least one Trk must start with a MidiChordDef, possibly preceded by a ClefDef.
			//IReadOnlyList<Trk> trks = Trks;
			//bool startFound = false;
			//foreach(Trk trk in trks)
			//{
			//	List<IUniqueDef> iuds = trk.UniqueDefs;
			//	IUniqueDef firstIud = iuds[0];
			//	if(firstIud is MidiChordDef || (iuds.Count > 1 && firstIud is ClefDef && iuds[1] is MidiChordDef))
			//	{
			//		startFound = true;
			//		break;
			//	} 				
			//}
			//M.Assert(startFound, "MidiChordDef not found at start.");
			#endregion
		}

		#region envelopes
		/// <summary>
		/// This function does not change the MsDuration of the Bar.
		/// See Envelope.TimeWarp() for a description of the arguments.
		/// </summary>
		/// <param name="envelope"></param>
		/// <param name="distortion"></param>
		public void TimeWarp(Envelope envelope, double distortion)
        {
            AssertConsistency();
            int originalMsDuration = MsDuration;
            List<int> originalMsPositions = GetMsPositions();
            Dictionary<int, int> warpDict = new Dictionary<int, int>();
            #region get warpDict
            List<int> newMsPositions = envelope.TimeWarp(originalMsPositions, distortion);

            for(int i = 0; i < newMsPositions.Count; ++i)
            {
                warpDict.Add(originalMsPositions[i], newMsPositions[i]);
            }
            #endregion get warpDict

            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                List<IUniqueDef> iuds = voiceDef.UniqueDefs;
                IUniqueDef iud = null;
                int msPos = 0;
                for(int i = 1; i < iuds.Count; ++i)
                {
                    iud = iuds[i - 1];
                    msPos = warpDict[iud.MsPositionReFirstUD];
                    iud.MsPositionReFirstUD = msPos;
                    iud.MsDuration = warpDict[iuds[i].MsPositionReFirstUD] - msPos;
                    msPos += iud.MsDuration;
                }
                iud = iuds[iuds.Count - 1];
                iud.MsPositionReFirstUD = msPos;
                iud.MsDuration = originalMsDuration - msPos;
            }

            M.Assert(originalMsDuration == MsDuration);

            AssertConsistency();
        }

        /// <summary>
        /// Returns a list containing the msPositions of all IUniqueDefs plus the endMsPosition of the final object.
        /// </summary>
        private List<int> GetMsPositions()
        {
            int originalMsDuration = MsDuration;

            List<int> originalMsPositions = new List<int>();
            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                foreach(IUniqueDef iud in voiceDef)
                {
                    int msPos = iud.MsPositionReFirstUD;
                    if(!originalMsPositions.Contains(msPos))
                    {
                        originalMsPositions.Add(msPos);
                    }
                }
                originalMsPositions.Sort();
            }
            originalMsPositions.Add(originalMsDuration);
            return originalMsPositions;
        }

        public void SetPitchWheelSliders(Envelope envelope)
        {
            #region condition
            if(envelope.Domain != 127)
            {
                throw new ArgumentException($"{nameof(envelope.Domain)} must be 127.");
            }
            #endregion condition

            List<int> msPositions = GetMsPositions();
            Dictionary<int, int> pitchWheelValuesPerMsPosition = envelope.GetValuePerMsPosition(msPositions);

            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                if(voiceDef is Trk trk)
                {
                    trk.SetPitchWheelSliders(pitchWheelValuesPerMsPosition);
                }
            }
        }

        #endregion envelopes

        public void SetMsPositionsReFirstUD()
        {
            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                int msPosition = 0;
                foreach(IUniqueDef iud in voiceDef)
                {
                    iud.MsPositionReFirstUD = msPosition;
                    msPosition += iud.MsDuration;
                }
            }
        }

        /// <summary>
        /// Setting this value stretches or compresses the msDurations of all the voiceDefs and their contained UniqueDefs.
        /// </summary>
        public int MsDuration
        {
            get
            {
				return _voiceDefs[0].MsDuration;
            }
            set
            {
                AssertConsistency();
                int currentDuration = MsDuration;
                double factor = ((double)value) / currentDuration;
                foreach(VoiceDef voiceDef in _voiceDefs)
                {
                    voiceDef.MsDuration = (int)Math.Round(voiceDef.MsDuration * factor);
                }
                int roundingError = value - MsDuration;
                if(roundingError != 0)
                {
                    foreach(VoiceDef voiceDef in _voiceDefs)
                    {
                        if((voiceDef.EndMsPositionReFirstIUD + roundingError) == value)
                        {
                            voiceDef.EndMsPositionReFirstIUD += roundingError;
                        }
                    }
                }
				foreach(VoiceDef voiceDef in _voiceDefs)
				{
					M.Assert(voiceDef.MsDuration == value);
				}				
            }
        }

        public int AbsMsPosition
        {
            get { return _absMsPosition; }
            set
            {
                M.Assert(value >= 0);
                _absMsPosition = value;
            }
        }

		private int _absMsPosition = 0;

		public IReadOnlyList<Trk> Trks
        {
            get
            {
                List<Trk> trks = new List<Trk>();
                foreach(VoiceDef voiceDef in _voiceDefs)
                {
                    if(voiceDef is Trk trk)
                    {
                        trks.Add(trk);
                    }
                }
                return trks.AsReadOnly();
            }
        }

		public override string ToString()
		{
			return $"AbsMsPosition={AbsMsPosition}, MsDuration={MsDuration}, nVoiceDefs={VoiceDefs.Count}";
		}

		public List<VoiceDef> VoiceDefs
		{
			get => _voiceDefs;
			set
			{
				_voiceDefs = value;
				AssertConsistency();
			}
		}

        public bool RepeatBeginBarline { get; }
        public bool RepeatEndBarline { get; }

        protected List<VoiceDef> _voiceDefs = new List<VoiceDef>();
	}
}
